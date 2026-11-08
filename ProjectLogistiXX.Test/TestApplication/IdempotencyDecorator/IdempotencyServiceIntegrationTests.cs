using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Decorators;
using ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Application.Abstraction.Users.Register;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Infrastructure.Persistence;
using ProjectLogistiXX.Infrastructure.Persistence.Services;
using ProjectLogistiXX.SharedKernel;
using ProjectLogistiXX.Test.TestApplication.FakeTestRegisterUser;
using ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.IntegrationTestDecorators;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.FakeTestIdempotency;


namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator
{
    public sealed record TestResponse(string Value);
    [Collection(SqlServerCollection.Name)]
    public sealed class IdempotencyServiceIntegrationTests(SqlServerFixture fixture)

    {
        private static readonly TimeSpan TestTtl = TimeSpan.FromMinutes(5);
        private static readonly UserId TestUser = UserId.NewId();

        [Fact]
        public async Task TryReserveAsync_SameKeyDifferentUsers_BothReserved()
        {
            await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            var service = CreateIdempotencyService(dbContext, new FakeTimeProvider());
            var key = new IdempotencyKeyId(Guid.NewGuid());

            var first = await service.TryReserveAsync(key, UserId.NewId(), "TestOperation");
            var second = await service.TryReserveAsync(key, UserId.NewId(), "TestOperation");

            Assert.Equal(IdempotencyReservationStatus.Reserved, first.Status);
            Assert.Equal(IdempotencyReservationStatus.Reserved, second.Status);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            Assert.Equal(2, await verifyDbContext.IdempotentRequests.CountAsync(r => r.Id == key));
        }

        [Fact]
        public async Task TryReserveAsync_SameKeySameUserDifferentName_ThrowsFatal()
        {
            await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            var service = CreateIdempotencyService(dbContext, new FakeTimeProvider());
            var key = new IdempotencyKeyId(Guid.NewGuid());

            await service.TryReserveAsync(key, TestUser, "OperationA");

            await Assert.ThrowsAsync<IdempotencyFatalException>(
                () => service.TryReserveAsync(key, TestUser, "OperationB"));
        }

        [Fact]
        public async Task TryReserveAsync_SameKeyTwice_ReturnsInProgressForSecondCall()
        {
            await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            var idempotencyService = new IdempotencyService<TestResponse>(
                fixture.DbContextFactory,
                dbContext,
                NullLogger<IdempotencyService<TestResponse>>.Instance,
                Options.Create(new IdempotencyOptions { Ttl = TimeSpan.FromMinutes(5) }),
                new FakeTimeProvider());

            var key = new IdempotencyKeyId(Guid.NewGuid());

            var firstTry = await idempotencyService.TryReserveAsync(key, TestUser, "TestOperation");
            var secondTry = await idempotencyService.TryReserveAsync(key, TestUser, "TestOperation");

            Assert.Equal(IdempotencyReservationStatus.Reserved, firstTry.Status);

            Assert.Equal(IdempotencyReservationStatus.InProgress, secondTry.Status);

            Assert.Equal(Guid.Empty, secondTry.LockToken);
            Assert.NotEqual(Guid.Empty, firstTry.LockToken);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            Assert.Equal(1, await verifyDbContext.IdempotentRequests.CountAsync(r => r.Id == key));
        }

        [Fact]
        public async Task TryReserveAsync_WhenKeyCompleted_ReturnsStoredResponse()
        {
            await using var scopedDbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            var service = CreateIdempotencyService(scopedDbContext, new FakeTimeProvider());
            var key = new IdempotencyKeyId(Guid.NewGuid());

            var firstTry = await service.TryReserveAsync(key, TestUser, "TestOperation");
            await service.CompleteAsync(key, TestUser, firstTry.LockToken, new TestResponse("Ok"));

            var secondTry = await service.TryReserveAsync(key, TestUser, "TestOperation");

            Assert.Equal(IdempotencyReservationStatus.Completed, secondTry.Status);
            Assert.Equal("Ok", secondTry.Response?.Value);
        }

        [Fact]
        public async Task TryReserveAsync_WhenInProgressKeyIsStale_ReleasesItAndAllowsNewReservation()
        {
            await using var scopedDbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            var fakeTimeProvider = new FakeTimeProvider();
            var idempotencyService = CreateIdempotencyService(scopedDbContext, fakeTimeProvider);
            var key = new IdempotencyKeyId(Guid.NewGuid());

            var firstTry = await idempotencyService.TryReserveAsync(key, TestUser, "TestOperation");
            fakeTimeProvider.Advance(TestTtl + TimeSpan.FromSeconds(1));

            Task<IdempotencyReservationResult<TestResponse>> secondTry()
                => idempotencyService.TryReserveAsync(key, TestUser, "TestOperation");
            await Assert.ThrowsAsync<IdempotencyTransientException>((Func<Task<IdempotencyReservationResult<TestResponse>>>)secondTry);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            Assert.Equal(0, await verifyDbContext.IdempotentRequests.CountAsync(r => r.Id == key));

            var retryAfterStale = await idempotencyService.TryReserveAsync(key, TestUser, "TestOperation");
            Assert.Equal(IdempotencyReservationStatus.Reserved, retryAfterStale.Status);
            Assert.NotEqual(firstTry.LockToken, retryAfterStale.LockToken);

        }

        [Fact]
        public async Task Handle_WhenHandlerSucceeds_CommitsDataAndCOmpletesReservation()
        {
            var key = new IdempotencyKeyId(Guid.NewGuid());
            var (decorator, requestContext) = await CreateDecoratorAsyns(key);

            var result = await decorator.Handle(new CreateUserTestCommand("John Doe"), CancellationToken.None);

            Assert.True(result.IsSuccess);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            var user = await verifyDbContext.IdempotentRequests.SingleAsync(r => r.Id == key);
            Assert.Equal(IdempotencyStatus.Completed, user.Status);
            Assert.NotNull(user.Response);
        }

        [Fact]
        public async Task Handle_WhenValid_validatesThenReservesThenCommitsUser()
        {
            var key = new IdempotencyKeyId(Guid.NewGuid());
            var pipeline = await CreatePipelineAsync(key);
            var command = TestIntegrationDecorators.CreateValidCommand();

            var result = await pipeline.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            var user = await verifyDbContext.IdempotentRequests.SingleAsync(r => r.Id == key);
            var row = await verifyDbContext.Users.CountAsync(u => u.Username == command.Username);

            Assert.Equal(1, row);
            Assert.Equal(IdempotencyStatus.Completed, user.Status);

            Assert.NotNull(user.Response);
        }

        [Fact]
        public async Task Handle_WhenValidationFails_DoesNotReserveKeyOrCreateUser()
        {
            var key = new IdempotencyKeyId(Guid.NewGuid());
            var pipeline = await CreatePipelineAsync(key);
            var command = TestIntegrationDecorators.CreateValidCommand() with { Email = "invalid-email" };

            var result = await pipeline.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            Assert.Equal(0, await verifyDbContext.IdempotentRequests.CountAsync(r => r.Id == key));
            Assert.Equal(0, await verifyDbContext.Users.CountAsync(u => u.Username == command.Username));
        }

        [Fact]
        public async Task Cleanup_RemoveIdempotencyKeysOfRegisteredUser()
        {
            var firstKey = new IdempotencyKeyId(Guid.NewGuid());
            var secondKey = new IdempotencyKeyId(Guid.NewGuid());

            var command1 = TestIntegrationDecorators.CreateValidCommand();

            var pipeline1 = await CreatePipelineAsync(firstKey);

            var result = await pipeline1.Handle(command1, CancellationToken.None);

            Assert.True(result.IsSuccess);

            await using var verifyDbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            var row = await verifyDbContext.Users.CountAsync(u => u.Username == command1.Username);
            Assert.Equal(1, row);

            var cleanupTime = new FakeTimeProvider();
            var options = Options.Create(new IdempotencyOptions
            {
                Retention = TimeSpan.FromSeconds(5),
                CleanupInterval = TimeSpan.FromSeconds(10)
            });

            var services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(fixture.ConnectionString))
                .BuildServiceProvider();

            var cleanup = new IdempotencyCleanupService(
                services.GetRequiredService<IServiceScopeFactory>(),
                NullLogger<IdempotencyCleanupService>.Instance,
                options,
                cleanupTime);

            await cleanup.StartAsync(CancellationToken.None);
            try
            {
                //cleanupTime.Advance(TimeSpan.FromSeconds(10)); // fires the PeriodicTimer tick
                //await WaitUntilAsync(async () => await CountAsync(firstKey, secondKey) == 0);
                await WaitUntilAsync(async () =>
                {
                    cleanupTime.Advance(TimeSpan.FromSeconds(10)); // tick; se repetă până apucă timer-ul să existe
                    return await CountAsync(firstKey, secondKey) == 0;
                });
            }
            finally
            {
                await cleanup.StopAsync(CancellationToken.None);
                await services.DisposeAsync();
            }

            Assert.Equal(0, await CountAsync(firstKey, secondKey));
            await using var verifyDbContext2 = await fixture.DbContextFactory.CreateDbContextAsync();
            Assert.Equal(1, await verifyDbContext2.Users.CountAsync(u => u.Username == command1.Username));
            Assert.Equal(0, await verifyDbContext2.IdempotentRequests.CountAsync(r => r.Id == firstKey || r.Id == secondKey));
        }

        [Fact]
        public async Task Handle_WhenTwoRequestsWithSameKeyRunConcurrently_CreatesUserOnlyOnce()
        {
            var key = new IdempotencyKeyId(Guid.NewGuid());
            var command = TestIntegrationDecorators.CreateValidCommand();

            var pipeline1 = await CreatePipelineAsync(key);
            var pipeline2 = await CreatePipelineAsync(key);

            var results = await Task.WhenAll(
                RunSafelyAsync(pipeline1, command),
                RunSafelyAsync(pipeline2, command));

            var successes = results.Count(r => r.IsSuccess);
            Assert.Equal(1, successes);

            await using var verify = await fixture.DbContextFactory.CreateDbContextAsync();

            Assert.Equal(1, await verify.Users.CountAsync(u => u.Username == command.Username));
            Assert.Equal(1, await verify.IdempotentRequests.CountAsync(r => r.Id == key));

        }

        [Fact]
        public async Task Handle_WhenDbFailsBeforeUserInsert_RollsBackAndReleasesKey()
        {
            var key = new IdempotencyKeyId(Guid.NewGuid());
            var commnad = TestIntegrationDecorators.CreateValidCommand();
            var interceptor = new FaultInjectionInterceptor { Fault = FaultPoint.BeforeUserInsert };

            var (options, factory) = CreateFaultyDbSetup(interceptor);
            await using var scopedDbContext = new ApplicationDbContext(options);
            var pipeline = await CreatePipelineAsync(key, factory);

            await Assert.ThrowsAnyAsync<InvalidOperationException>(
                () => pipeline.Handle(commnad, CancellationToken.None));

            await using var verify = await fixture.DbContextFactory.CreateDbContextAsync();

            Assert.Equal(0, await verify.Users.CountAsync(u => u.Username == commnad.Username));
            Assert.Equal(0, await verify.IdempotentRequests.CountAsync(r => r.Id == key));
        }

        private (DbContextOptions<ApplicationDbContext> Options, IDbContextFactory<ApplicationDbContext> Factory)
            CreateFaultyDbSetup(FaultInjectionInterceptor interceptor)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(fixture.ConnectionString)
                .AddInterceptors(interceptor)
                .Options;

            return (options, new PooledDbContextFactory<ApplicationDbContext>(options));
        }

        private IdempotencyService<TestResponse> CreateIdempotencyService(
            ApplicationDbContext scopedDbContext, TimeProvider timeProvider)
            => new(
                fixture.DbContextFactory,
                scopedDbContext,
                NullLogger<IdempotencyService<TestResponse>>.Instance,
                Options.Create(new IdempotencyOptions { Ttl = TestTtl }),
                timeProvider);

        private async Task<(IdempotentDecorator<CreateUserTestCommand, TestResponse> Decorator,
            FakeRequestContext RequestContext)> CreateDecoratorAsyns(IdempotencyKeyId key)
        {
            var scopedDbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            var idempotencyService = CreateIdempotencyService(scopedDbContext, new FakeTimeProvider());

            var requestContext = new FakeRequestContext { IdempotencyKey = key, UserId = TestUser };

            var decorator = new IdempotentDecorator<CreateUserTestCommand, TestResponse>(
                new CreateUserTestHandler(scopedDbContext),
                idempotencyService,
                requestContext,
                scopedDbContext,
                NullLogger<IdempotentDecorator<CreateUserTestCommand, TestResponse>>.Instance);

            return (decorator, requestContext);
        }

        private async Task<ICommandHandler
            <RegisterUserCommand, UserId>> CreatePipelineAsync(
            IdempotencyKeyId key,
            IDbContextFactory<ApplicationDbContext>? factory = null)
        {
            factory ??= fixture.DbContextFactory;
            var scopedDbContext = await fixture.DbContextFactory.CreateDbContextAsync();

            var idempotencyService = new IdempotencyService<UserId>(
                factory,
                scopedDbContext,
                NullLogger<IdempotencyService<UserId>>.Instance,
                Options.Create(new IdempotencyOptions { Ttl = TestTtl }),
                new FakeTimeProvider());

            var requestContext = new FakeRequestContext { IdempotencyKey = key, UserId = TestUser };

            var handler = new RegisterUserCommandHandler(scopedDbContext, new FakePasswordHash());

            var idempotentDecorator = new IdempotentDecorator<RegisterUserCommand, UserId>(
                handler,
                idempotencyService,
                requestContext,
                scopedDbContext,
                NullLogger<IdempotentDecorator<RegisterUserCommand, UserId>>.Instance);

            return new ValidationDecorator<RegisterUserCommand, UserId>(
                idempotentDecorator,
                [new RegisterUserCommandValidator()]);
        }

        private static async Task<Result<UserId>> RunSafelyAsync(
            ICommandHandler<RegisterUserCommand, UserId> handler,
            RegisterUserCommand command)
        {
            try
            {
                return await handler.Handle(command, CancellationToken.None);
            }
            catch (IdempotencyTransientException)
            {
                return Result<UserId>.Failure(new Error("Test.InProgress", "In progress.", ErrorType.Conflict));
            }
        }

        private static async Task WaitUntilAsync(Func<Task<bool>> condition)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !await condition())
            {
                await Task.Delay(100);
            }
        }

        private async Task<int> CountAsync(params IdempotencyKeyId[] keys)
        {
            await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            return await dbContext.IdempotentRequests.CountAsync(r => keys.Contains(r.Id));
        }
    }
}
