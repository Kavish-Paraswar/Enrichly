using JobAutomationPlatform.Domain.Entities;

namespace JobAutomationPlatform.Application.Interfaces;

public sealed record QueuedExecutionClaim(
    Guid ExecutionRequestId,
    Guid JobId,
    Guid ExecutionAttemptId,
    string JobName,
    string TargetUrl,
    string HttpMethod,
    string? PayloadJson,
    int AttemptNumber,
    int MaxAttempts);

public interface IExecutionQueueService
{
    Task<QueuedExecutionClaim?> ClaimNextAsync(string workerName, CancellationToken cancellationToken);

    Task UpdateHeartbeatAsync(Guid executionAttemptId, DateTimeOffset heartbeatAtUtc, CancellationToken cancellationToken);

    Task CompleteSucceededAsync(Guid executionAttemptId, DateTimeOffset completedAtUtc, string? output, CancellationToken cancellationToken);

    Task CompleteFailedAsync(Guid executionAttemptId, DateTimeOffset completedAtUtc, string failureSummary, bool scheduleRetry, DateTimeOffset? retryAtUtc, CancellationToken cancellationToken);

    Task<int> EnqueueDueJobsAsync(DateTimeOffset utcNow, CancellationToken cancellationToken);

    Task<int> ReapStaleExecutionsAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken);
}
