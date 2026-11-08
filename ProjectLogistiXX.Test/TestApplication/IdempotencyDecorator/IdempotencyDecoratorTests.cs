using Microsoft.Extensions.Logging.Abstractions;
using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Decorators;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.SharedKernel;
using static ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.FakeTestIdempotency;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator
{
    public sealed class IdempotencyDecoratorTests
    {
        private readonly IdempotencyKeyId key = new(Guid.NewGuid());
        private readonly UserId userId = UserId.NewId();
        private readonly Guid lockToken = Guid.NewGuid();

        private readonly FakeCommandHandler handler = new();
        private readonly FakeIdempotencyService idempotencyService = new();
        private readonly FakeRequestContext requestContext = new();
        private readonly FakeApplicationDbContext dbContext = new();

        private readonly IdempotentDecorator<FakeTestIdempotency.TestCommand, FakeTestIdempotency.TestResponse>? decorator;

        public IdempotencyDecoratorTests()
        {
            requestContext.IdempotencyKey = key;
            requestContext.UserId = userId;
            idempotencyService.ReservationToReturn = IdempotencyReservationResult<FakeTestIdempotency.TestResponse>.NewReservation(lockToken);

            decorator = new IdempotentDecorator<FakeTestIdempotency.TestCommand, FakeTestIdempotency.TestResponse>(
                handler,
                idempotencyService,
                requestContext,
                dbContext,
                NullLogger<IdempotentDecorator<FakeTestIdempotency.TestCommand, FakeTestIdempotency.TestResponse>>.Instance);
        }

        [Fact]
        public async Task Handle_WhenHandlerSucceeds_CompletesAndCommits()
        {
            var response = new FakeTestIdempotency.TestResponse("ok");
            handler.ResultToReturn = Result<FakeTestIdempotency.TestResponse>.Success(response);

            var result = await decorator!.Handle(new TestCommand(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(response, result.Value);

            Assert.Equal(1, idempotencyService.CompletCallsCount);
            Assert.Equal(lockToken, idempotencyService.LastCompletedLockToken);
            Assert.True(dbContext.Transaction!.Commited);
        }

        [Fact]
        public async Task Handle_WhenHandlerReturnsFailure_ReleasesWithoutComitting()
        {
            handler.ResultToReturn = Result<FakeTestIdempotency.TestResponse>.Failure(new Error("Test.Failure", "Handler failed", ErrorType.NotFound));

            var result = await decorator!.Handle(new TestCommand(), CancellationToken.None);

            Assert.True(result.IsFailure);

            Assert.Equal(0, idempotencyService.CompletCallsCount);
            Assert.Equal(1, idempotencyService.ReleaseCallsCount);
            Assert.Equal(lockToken, idempotencyService.LastReleaseLockToken);

            Assert.False(dbContext.Transaction!.Commited);
            Assert.True(dbContext.Transaction.Disposed);
        }

        [Fact]
        public async Task Handle_WhenNoIdempotencyKey_CallsHandlerDirectly()
        {
            requestContext.IdempotencyKey = null;

            var response = new FakeTestIdempotency.TestResponse("ok");
            handler.ResultToReturn = Result<FakeTestIdempotency.TestResponse>.Success(response);

            var result = await decorator!.Handle(new TestCommand(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(response, result.Value);
            Assert.Equal(1, handler.Calls);
            Assert.Equal(0, idempotencyService.TryReserveCallsCount);
            Assert.Null(dbContext.Transaction);
            Assert.Same(response, result.Value);
        }

        [Fact]
        public async Task Handle_WhenReservationCompleted_ReturnsCachedResponseWithoutCallingHandler()
        {
            var cachedResponse = new FakeTestIdempotency.TestResponse("cached");
            idempotencyService.ReservationToReturn = IdempotencyReservationResult<FakeTestIdempotency.TestResponse>.NewCompleted(cachedResponse);

            var result = await decorator!.Handle(new TestCommand(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(cachedResponse, result.Value);

            Assert.Equal(0, handler.Calls);
            Assert.Equal(0, idempotencyService.CompletCallsCount);
            Assert.Equal(0, idempotencyService.ReleaseCallsCount);
            Assert.Null(dbContext.Transaction);
        }

        [Fact]
        public async Task Handle_WhenKeyPresentButUserMissing_ThrowsAndDoesNotReserve()
        {
            requestContext.UserId = null;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => decorator!.Handle(new TestCommand(), CancellationToken.None));

            Assert.Equal(0, idempotencyService.TryReserveCallsCount);
        }
    }
}
