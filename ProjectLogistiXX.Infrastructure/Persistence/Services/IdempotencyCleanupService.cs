using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProjectLogistiXX.Infrastructure.Persistence.Services;

public sealed class IdempotencyCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdempotencyCleanupService> _logger;
    private readonly IdempotencyOptions _options;
    private readonly TimeProvider _timeProvider;

    public IdempotencyCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<IdempotencyCleanupService> logger,
        IOptions<IdempotencyOptions> options,
        TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.CleanupInterval, _timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var cutoffUtc = _timeProvider.GetUtcNow().UtcDateTime - _options.Retention;

                var deleted = await dbContext.IdempotentRequests
                    .Where(request => request.CreatedAtUtc < cutoffUtc)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                {
                    _logger.LogInformation("Removed {Count} expired idempotency records.", deleted);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to remove expired idempotency records.");
            }
        }
    }
}
