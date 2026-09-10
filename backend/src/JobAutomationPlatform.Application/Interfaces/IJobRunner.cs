using JobAutomationPlatform.Domain.Entities;

namespace JobAutomationPlatform.Application.Interfaces;

public sealed record JobRunResult(bool Succeeded, string? Output, string? FailureSummary, string? FailureDetailsJson);

public interface IJobRunner
{
    Task<JobRunResult> RunAsync(Job job, CancellationToken cancellationToken);
}
