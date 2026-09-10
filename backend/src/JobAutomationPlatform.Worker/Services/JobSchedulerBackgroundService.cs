using JobAutomationPlatform.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace JobAutomationPlatform.Worker.Services;

public sealed class JobSchedulerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<JobSchedulerBackgroundService> _logger;
    private readonly WorkerOptions _options;

    public JobSchedulerBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<JobSchedulerBackgroundService> logger, IOptions<WorkerOptions> options)
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
                var scheduler = scope.ServiceProvider.GetRequiredService<IJobSchedulerService>();
                var created = await scheduler.EnqueueDueJobsAsync(stoppingToken);

                if (created > 0)
                {
                    _logger.LogInformation("Enqueued {Count} scheduled executions.", created);
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.SchedulerIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(_options.SchedulerIntervalSeconds), stoppingToken);
            }
        }
    }
}
