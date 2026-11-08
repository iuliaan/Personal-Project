using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;

namespace ProjectLogistiXX.Application.Abstraction.Decorators
{
    public sealed class IdempotentDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : class
    {
        private readonly ICommandHandler<TCommand, TResponse> _handler;
        private readonly IIdempotencyService<TResponse> _idempotencyService;
        private readonly IRequestContext _requestContext;

        private readonly IApplicationDbContext _dbContext;

        private readonly ILogger<IdempotentDecorator<TCommand, TResponse>> _logger;

        public IdempotentDecorator(
            ICommandHandler<TCommand, TResponse> handler,
            IIdempotencyService<TResponse> idempotencyService,
            IRequestContext requestContext,
            IApplicationDbContext dbContext,
            ILogger<IdempotentDecorator<TCommand, TResponse>> logger)
        {
            _handler = handler;
            _idempotencyService = idempotencyService;
            _requestContext = requestContext;
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var idempotencyKey = _requestContext.IdempotencyKey;

            if (idempotencyKey == null)
            {
                return await _handler.Handle(command, cancellationToken);
            }

            var userId = _requestContext.UserId
                ?? throw new InvalidOperationException("Idempotency requires an authenticated user");

            var reservation = await _idempotencyService.TryReserveAsync(
                idempotencyKey.Value,
                userId,
                typeof(TCommand).Name,
                cancellationToken);

            if (reservation.Status == IdempotencyReservationStatus.Completed)
            {
                return Result<TResponse>.Success(reservation.Response!);
            }

            if (reservation.Status == IdempotencyReservationStatus.InProgress)
            {
                throw new IdempotencyTransientException(
                    "A request with this idempotency key is currently being processed. Please wait.");
            }

            if (reservation.Status != IdempotencyReservationStatus.Reserved)
            {
                throw new IdempotencyFatalException(
                    $"Unexpected idempotency reservation status {reservation.Status}.");
            }

            Result<TResponse> result;

            try
            {
                result = await HandleInTransactionAsync(command, idempotencyKey.Value, userId, reservation.LockToken, cancellationToken);

            }
            catch
            {
                await ReleaseSafelyAsync(idempotencyKey.Value, userId, reservation.LockToken);
                throw;
            }

            if (result.IsFailure)
            {
                await ReleaseSafelyAsync(idempotencyKey.Value, userId, reservation.LockToken);
            }

            return result;
        }

        private async Task<Result<TResponse>> HandleInTransactionAsync(
            TCommand command, IdempotencyKeyId key, UserId userId, Guid lockToken, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

            var result = await _handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return result;
            }

            await _idempotencyService.CompleteAsync(
                key,
                userId,
                lockToken,
                result.Value,
                CancellationToken.None);

            await transaction.CommitAsync(CancellationToken.None);

            return result;
        }

        private async Task ReleaseSafelyAsync(IdempotencyKeyId value, UserId userId, Guid lockToken)
        {
            try
            {
                var released = await _idempotencyService.ReleaseAsync(
                    value,
                    userId,
                    lockToken,
                    CancellationToken.None);

                if (!released)
                {
                    _logger.LogWarning("Failed to release the idempotency reservation.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release the idempotency reservation.");
            }
        }
    }
}
