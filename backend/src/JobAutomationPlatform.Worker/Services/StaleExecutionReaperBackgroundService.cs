using JobAutomationPlatform.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace JobAutomationPlatform.Worker.Services;

public sealed class StaleExecutionReaperBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StaleExecutionReaperBackgroundService> _logger;
    private readonly WorkerOptions _options;

    public StaleExecutionReaperBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<StaleExecutionReaperBackgroundService> logger, IOptions<WorkerOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var queueService = scope.ServiceProvider.GetRequiredService<IExecutionQueueService>();
                var staleBeforeUtc = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(_options.StaleAfterMinutes));
                var reaped = await queueService.ReapStaleExecutionsAsync(staleBeforeUtc, stoppingToken);

                if (reaped > 0)
                {
                    _logger.LogWarning("Reaped {Count} stale executions.", reaped);
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, _options.SchedulerIntervalSeconds)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stale execution reaper failed.");
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, _options.SchedulerIntervalSeconds)), stoppingToken);
            }
        }
    }
}
