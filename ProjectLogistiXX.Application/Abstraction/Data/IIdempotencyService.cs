using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Domain.Abstraction;

namespace ProjectLogistiXX.Application.Abstraction.Data
{
    public interface IIdempotencyService<TResponse> where TResponse : class
    {
        Task<IdempotencyReservationResult<TResponse>> TryReserveAsync(
            IdempotencyKeyId id, UserId userId, string name, CancellationToken cancellationToken = default);

        Task CompleteAsync(IdempotencyKeyId id,
            UserId userId,
            Guid lockToken,
            TResponse response, 
            CancellationToken cancellationToken = default);

        Task<bool> ReleaseAsync(IdempotencyKeyId id,
            UserId userId,
            Guid lockToken,
            CancellationToken cancellationToken = default);

        Task<bool> ReleaseStaleAsync(IdempotencyKeyId id,
            UserId userId,
            CancellationToken cancellationToken = default);
    }
}
