using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Infrastructure.Persistence;
using ProjectLogistiXX.Infrastructure.Persistence.Services;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator
{
    [Collection(SqlServerCollection.Name)]
    public sealed class IdempotencyCleanupServiceIntegrationTests(SqlServerFixture fixture)
    {
        private static readonly TimeSpan Retention = TimeSpan.FromHours(1);
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        [Fact]
        public async Task Cleanup_DeletesOnlyRecordsOlderThanRetention()
        {
            var time = new FakeTimeProvider();
            var options = Options.Create(new IdempotencyOptions { Retention = Retention, CleanupInterval = Interval });

            var oldKeyA = IdempotencyKeyId.NewId();
            var oldKeyB = IdempotencyKeyId.NewId();
            var freshKey = IdempotencyKeyId.NewId();

            // t0: two records that will expire
            await SeedAsync(time, options, oldKeyA, UserId.NewId());
            await SeedAsync(time, options, oldKeyB, UserId.NewId());

            // t0 + 2h: one record that is still within retention
            time.Advance(TimeSpan.FromHours(2));
            await SeedAsync(time, options, freshKey, UserId.NewId());

            var services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(fixture.ConnectionString))
                .BuildServiceProvider();

            var cleanup = new IdempotencyCleanupService(
                services.GetRequiredService<IServiceScopeFactory>(),
                NullLogger<IdempotencyCleanupService>.Instance,
                options,
                time);

            await cleanup.StartAsync(CancellationToken.None);
            try
            {
                time.Advance(Interval); // fires the PeriodicTimer tick

                await WaitUntilAsync(async () => await CountAsync(oldKeyA, oldKeyB) == 0);

                Assert.Equal(0, await CountAsync(oldKeyA, oldKeyB));
                Assert.Equal(1, await CountAsync(freshKey));
            }
            finally
            {
                await cleanup.StopAsync(CancellationToken.None);
                await services.DisposeAsync();
            }
        }

        private async Task SeedAsync(
            FakeTimeProvider time, IOptions<IdempotencyOptions> options, IdempotencyKeyId key, UserId userId)
        {
            await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            var service = new IdempotencyService<TestResponse>(
                fixture.DbContextFactory,
                dbContext,
                NullLogger<IdempotencyService<TestResponse>>.Instance,
                options,
                time);

            await service.TryReserveAsync(key, userId, "CleanupTest");
        }

        private async Task<int> CountAsync(params IdempotencyKeyId[] keys)
        {
            await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
            return await dbContext.IdempotentRequests.CountAsync(r => keys.Contains(r.Id));
        }

        private static async Task WaitUntilAsync(Func<Task<bool>> condition)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline && !await condition())
            {
                await Task.Delay(100);
            }
        }
    }
}
