using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Entities;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator
{
    internal class FakeTestIdempotency
    {
        public record TestCommand() : ICommand<TestResponse>;
        public sealed record TestResponse(string Data);

        public sealed class FakeCommandHandler : ICommandHandler<TestCommand, TestResponse>
        {
            public Result<TestResponse> ResultToReturn { get; set; } = default!;
            public int Calls { get; private set; }

            public Task<Result<TestResponse>> Handle(TestCommand command, CancellationToken cancellationToken)
            {
                Calls++;
                return Task.FromResult(ResultToReturn);
            }
        }

        public sealed class FakeIdempotencyService : IIdempotencyService<TestResponse>
        {
            public IdempotencyReservationResult<TestResponse> ReservationToReturn { get; set; } = default!;
            public int TryReserveCallsCount { get; private set; }

            public int CompletCallsCount { get; private set; }
            public Guid? LastCompletedLockToken { get; private set; }

            public int ReleaseCallsCount { get; private set; }
            public Guid? LastReleaseLockToken { get; private set; }



            public Task<IdempotencyReservationResult<TestResponse>> TryReserveAsync(
                IdempotencyKeyId id,
                UserId userId,
                string name,
                CancellationToken cancellationToken = default)
            {
                TryReserveCallsCount++;
                return Task.FromResult(ReservationToReturn);
            }

            public Task CompleteAsync(
                IdempotencyKeyId id,
                UserId userId,
                Guid lockToken,
                TestResponse response,
                CancellationToken cancellationToken = default)
            {
                CompletCallsCount++;
                LastCompletedLockToken = lockToken;
                return Task.CompletedTask;
            }

            public Task<bool> ReleaseAsync(
                IdempotencyKeyId id,
                UserId userId,
                Guid lockToken,
                CancellationToken cancellationToken = default)
            {
                ReleaseCallsCount++;
                LastReleaseLockToken = lockToken;
                return Task.FromResult(true);
            }

            public Task<bool> ReleaseStaleAsync(
                IdempotencyKeyId id,
                UserId userId,
                CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }

        internal sealed class FakeRequestContext : IRequestContext
        {
            public IdempotencyKeyId? IdempotencyKey { get; set; }
            public UserId? UserId { get; set; }
        }

        internal sealed class FakeApplicationDbContext : IApplicationDbContext
        {
            public FakeTransaction? Transaction { get; private set; }

            public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            {
                Transaction = new FakeTransaction();
                return Task.FromResult<IDbContextTransaction>(Transaction);
            }

            public DbSet<User> Users => throw new NotImplementedException();

            public DbSet<IdempotentRequest> IdempotentRequests => throw new NotImplementedException();

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        internal sealed class FakeTransaction : IDbContextTransaction
        {
            public Guid TransactionId { get; } = Guid.NewGuid();
            public bool Commited { get; private set; }

            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;

            public ValueTask DisposeAsync()
            {
                Disposed = true;
                return ValueTask.CompletedTask;
            }

            public void Commit() => Commited = true;
            public Task CommitAsync(CancellationToken cancellationToken = default)
            {
                Commited = true;
                return Task.CompletedTask;
            }
            public Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                // Simulate rolling back the transaction
                return Task.CompletedTask;
            }
            public void Rollback()
            {
                throw new NotImplementedException();
            }
        }
    }
}
