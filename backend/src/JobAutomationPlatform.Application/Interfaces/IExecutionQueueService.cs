using JobAutomationPlatform.Domain.Entities;

namespace JobAutomationPlatform.Application.Interfaces;

public sealed record QueuedExecutionClaim(
    Guid ExecutionRequestId,
    Guid JobId,
    Guid OwnerUserId,
    Guid ExecutionAttemptId,
    string JobName,
    string TargetUrl,
    string HttpMethod,
    string? RequestHeadersJson,
    string? PayloadJson,
    int TimeoutSeconds,
    int AttemptNumber,
    int MaxAttempts,
    string? IdempotencyKey);

public interface IExecutionQueueService
{
    Task<QueuedExecutionClaim?> ClaimNextAsync(string workerName, CancellationToken cancellationToken);

    Task UpdateHeartbeatAsync(Guid executionAttemptId, DateTimeOffset heartbeatAtUtc, CancellationToken cancellationToken);

    Task CompleteSucceededAsync(Guid executionAttemptId, DateTimeOffset completedAtUtc, JobRunResult result, CancellationToken cancellationToken);

    Task CompleteFailedAsync(Guid executionAttemptId, DateTimeOffset completedAtUtc, JobRunResult result, bool scheduleRetry, DateTimeOffset? retryAtUtc, CancellationToken cancellationToken);

    Task<int> EnqueueDueJobsAsync(DateTimeOffset utcNow, CancellationToken cancellationToken);

    Task<int> ReapStaleExecutionsAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken);
}
