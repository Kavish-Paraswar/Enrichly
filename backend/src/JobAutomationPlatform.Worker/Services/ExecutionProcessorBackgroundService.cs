using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JobAutomationPlatform.Worker.Services;

public sealed class ExecutionProcessorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ExecutionProcessorBackgroundService> _logger;
    private readonly WorkerOptions _options;
    private readonly string _workerName = Environment.MachineName;

    public ExecutionProcessorBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<ExecutionProcessorBackgroundService> logger, IOptions<WorkerOptions> options)
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
                var jobRunner = scope.ServiceProvider.GetRequiredService<IJobRunner>();

                var claim = await queueService.ClaimNextAsync(_workerName, stoppingToken);
                if (claim is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
                    continue;
                }

                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var job = await dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == claim.JobId, stoppingToken);
                if (job is null)
                {
                    _logger.LogWarning("Claimed execution {ExecutionRequestId} but job {JobId} no longer exists.", claim.ExecutionRequestId, claim.JobId);
                    continue;
                }

                await RunClaimAsync(queueService, jobRunner, job, claim, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution processor loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
        }
    }

    private async Task RunClaimAsync(IExecutionQueueService queueService, IJobRunner jobRunner, Job job, QueuedExecutionClaim claim, CancellationToken stoppingToken)
    {
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        var heartbeatTask = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, _options.HeartbeatIntervalSeconds)));
            while (await timer.WaitForNextTickAsync(heartbeatCts.Token))
            {
                await queueService.UpdateHeartbeatAsync(claim.ExecutionAttemptId, DateTimeOffset.UtcNow, heartbeatCts.Token);
            }
        }, heartbeatCts.Token);

        try
        {
            var result = await jobRunner.RunAsync(job, stoppingToken);
            var completedAtUtc = DateTimeOffset.UtcNow;

            if (result.Succeeded)
            {
                await queueService.CompleteSucceededAsync(claim.ExecutionAttemptId, completedAtUtc, result.Output, stoppingToken);
                return;
            }

            var retryAtUtc = completedAtUtc.AddSeconds(_options.RetryDelaySeconds);
            await queueService.CompleteFailedAsync(
                claim.ExecutionAttemptId,
                completedAtUtc,
                result.FailureSummary ?? "Job execution failed.",
                scheduleRetry: claim.AttemptNumber < claim.MaxAttempts,
                retryAtUtc: retryAtUtc,
                cancellationToken: stoppingToken);
        }
        finally
        {
            heartbeatCts.Cancel();
            try
            {
                await heartbeatTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
