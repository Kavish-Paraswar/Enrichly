using JobAutomationPlatform.Domain.Entities;

namespace JobAutomationPlatform.Application.Interfaces;

public sealed record JobRunResult(
    bool Succeeded,
    string? Output,
    string? FailureSummary,
    string? FailureDetailsJson,
    int? HttpStatusCode,
    long? DurationMilliseconds,
    string? ResponseHeadersJson,
    string? ResponseBody,
    bool IsTimedOut);

public interface IJobRunner
{
    Task<JobRunResult> RunAsync(Job job, QueuedExecutionClaim claim, CancellationToken cancellationToken);
}
