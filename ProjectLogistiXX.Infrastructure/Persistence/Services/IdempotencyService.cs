using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Domain.Entities;
using System.Text.Json;


namespace ProjectLogistiXX.Infrastructure.Persistence.Services
{
    public class IdempotencyService<TResponse> : IIdempotencyService<TResponse> where TResponse : class
    {
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        private readonly ILogger<IdempotencyService<TResponse>> _logger;
        private readonly IdempotencyOptions _options;
        private readonly TimeProvider _timeProvider;

        private readonly ApplicationDbContext _dbContext;

        public IdempotencyService(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            ApplicationDbContext dbContext,
            ILogger<IdempotencyService<TResponse>> logger,
            IOptions<IdempotencyOptions> options,
            TimeProvider timeProvider)
        {
            _dbContextFactory = dbContextFactory;
            _dbContext = dbContext;
            _logger = logger;
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        public async Task<IdempotencyReservationResult<TResponse>> TryReserveAsync(
            IdempotencyKeyId id,
            UserId userId,
            string name,
            CancellationToken cancellationToken = default)
        {

            var time = _timeProvider.GetUtcNow().UtcDateTime;

            var reservationResult = IdempotentRequest.Reserve(id, userId, name, time);

            if (reservationResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to build reservation...");
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            dbContext.IdempotentRequests.Add(reservationResult.Value);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return IdempotencyReservationResult<TResponse>.NewReservation(reservationResult.Value.LockToken);

            }
            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                return await InspectExistingAsync(id, userId, name, cancellationToken);
            }
        }

        public async Task CompleteAsync(
            IdempotencyKeyId id,
            UserId userId,
            Guid lockToken,
            TResponse response,
            CancellationToken cancellationToken = default)
        {
            var serializedResponse = JsonSerializer.Serialize(response, _jsonOptions);

            var updated = await _dbContext.IdempotentRequests
                .Where(r => r.Id == id
                        && r.UserId == userId
                        && r.LockToken == lockToken
                        && r.Status == IdempotencyStatus.InProgress)
                .ExecuteUpdateAsync(r => r
                    .SetProperty(req => req.Status, IdempotencyStatus.Completed)
                    .SetProperty(req => req.Response, serializedResponse)
                    .SetProperty(req => req.CompletedAt, _timeProvider.GetUtcNow().UtcDateTime),
                    cancellationToken);

            if (updated == 0)
            {
                _logger.LogWarning("Idempotency key {Key} could not be completed. It may not exist or may not be in progress.", id.Value);

                throw new IdempotencyFatalException(
                    $"Idempotency reservation for key {id.Value} could not be completed. Please retry.");
            }
        }

        public Task<bool> ReleaseAsync(IdempotencyKeyId id, UserId userId, Guid lockToken, CancellationToken cancellationToken = default)
            => ReleaseInternalAsync(id, userId, lockToken, cancellationToken);

        public Task<bool> ReleaseStaleAsync(IdempotencyKeyId id, UserId userId, CancellationToken cancellationToken = default)
            => ReleaseInternalAsync(id, userId, lockToken: null, cancellationToken);

        private async Task<bool> ReleaseInternalAsync(
            IdempotencyKeyId id,
            UserId userId,
            Guid? lockToken,
            CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = dbContext.IdempotentRequests
                .Where(r => r.Id == id
                    && r.UserId == userId
                    && r.Status == IdempotencyStatus.InProgress);

            if (lockToken.HasValue)
            {
                query = query.Where(r => r.LockToken == lockToken.Value);
            }
            else
            {
                var cutoffUtc = _timeProvider.GetUtcNow().UtcDateTime - _options.Ttl;
                query = query.Where(r => r.CreatedAtUtc < cutoffUtc);
            }

            var deleted = await query.ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation(
                    "Idempotency key {Key} release: {Deleted} row(s) deleted.", id.Value, deleted);

            return deleted > 0;
        }

        private static bool IsDuplicateKeyException(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                return sqlEx.Number is 2601 or 2627;
            }

            if (ex.InnerException is PostgresException pgEx)
            {
                return pgEx.SqlState == PostgresErrorCodes.UniqueViolation;
            }

            return false;
        }

        private async Task<IdempotencyReservationResult<TResponse>> InspectExistingAsync(
            IdempotencyKeyId id,
            UserId userId,
            string name,
            CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existingRequest = await dbContext.IdempotentRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (existingRequest is null)
            {
                _logger.LogWarning("Idempotency key {Key} raised a duplicate but no row found on inspect.", id.Value);
                throw new IdempotencyTransientException(
                    $"Idempotency reservation for key {id.Value} vanished before inspection. Please retry.");
            }

            if (existingRequest.Name != name)
            {
                _logger.LogError("Idempotency key {Key} has a name mismatch. Expected: {Expected}, Actual: {Actual}.", 
                    id.Value, name, existingRequest.Name);
                throw new IdempotencyFatalException(
                    $"Idempotency reservation for key {id.Value} has a name mismatch. Expected: {name}, Actual: {existingRequest.Name}.");
            }

            return existingRequest.Status switch
            {
                IdempotencyStatus.Completed => HandleCompleted(existingRequest),
                IdempotencyStatus.InProgress => await HandleInProgressAsync(existingRequest, userId, cancellationToken),
                _ => throw new IdempotencyFatalException($"Unknown idempotency status {existingRequest.Status}.")
            };
        }

        private async Task<IdempotencyReservationResult<TResponse>> HandleInProgressAsync(
            IdempotentRequest existingRequest,
            UserId userId,
            CancellationToken cancellationToken)
        {
            var time = _timeProvider.GetUtcNow().UtcDateTime;

            if (existingRequest.CreatedAtUtc >= time - _options.Ttl)
            {
                return IdempotencyReservationResult<TResponse>.NewInProgress();
            }

            _logger.LogWarning("Idempotency key {Key} is stale. Attempting to release.", existingRequest.Id.Value);

            await ReleaseStaleAsync(existingRequest.Id, existingRequest.UserId, cancellationToken);

            throw new IdempotencyTransientException(
                $"Idempotency reservation for key {existingRequest.Id.Value} is stale and was released. Please retry.");
        }

        private IdempotencyReservationResult<TResponse> HandleCompleted(
            IdempotentRequest existingRequest)
        {
            if (string.IsNullOrEmpty(existingRequest.Response))
            {
                _logger.LogError("Idempotency key {Key} is marked as completed but has no response.", existingRequest.Id.Value);
                throw new IdempotencyFatalException(
                   $"Stored idempotency response for key {existingRequest.Id.Value} is completely missing.");
            }

            try
            {
                var response = JsonSerializer.Deserialize<TResponse>(existingRequest.Response, _jsonOptions)
                    ?? throw new IdempotencyFatalException(
                        $"Deserialized response resulted in null for completed idempotency key {existingRequest.Id.Value}.");

                return IdempotencyReservationResult<TResponse>.NewCompleted(response);

            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Idempotency key {Key} failed to deserialize response.", existingRequest.Id.Value);

                throw new IdempotencyFatalException(
                    $"Stored idempotency response for key {existingRequest.Id.Value} could not be deserialized.", ex);
            }
        }
    }
}
