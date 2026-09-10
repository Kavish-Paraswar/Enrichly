using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Infrastructure.Services;

internal static class DtoMapper
{
    public static JobSummaryDto ToSummaryDto(this Job job)
        => new(job.Id, job.Name, job.IsEnabled, job.TargetUrl, job.HttpMethod, job.ScheduleEveryMinutes, job.NextRunAtUtc, job.MaxAttempts, job.Version, job.CreatedAtUtc, job.UpdatedAtUtc);

    public static JobDetailDto ToDetailDto(this Job job)
        => new(job.Id, job.Name, job.Description, job.IsEnabled, job.TargetUrl, job.HttpMethod, job.PayloadJson, job.ScheduleEveryMinutes, job.NextRunAtUtc, job.MaxAttempts, job.Version, job.CreatedAtUtc, job.UpdatedAtUtc);

    public static ExecutionAttemptDto ToAttemptDto(this ExecutionAttempt attempt)
        => new(attempt.Id, attempt.AttemptNumber, attempt.Status, attempt.WorkerName, attempt.StartedAtUtc, attempt.HeartbeatAtUtc, attempt.CompletedAtUtc, attempt.ErrorSummary, attempt.CreatedAtUtc, attempt.UpdatedAtUtc);

    public static ExecutionSummaryDto ToSummaryDto(this ExecutionRequest request)
        => new(request.Id, request.JobId, request.Job?.Name ?? string.Empty, request.Source, request.Status, request.RetryCount, request.RequestedAtUtc, request.ReadyAtUtc, request.StartedAtUtc, request.CompletedAtUtc, request.LastErrorSummary, request.CreatedAtUtc, request.UpdatedAtUtc);

    public static ExecutionDetailDto ToDetailDto(this ExecutionRequest request)
        => new(request.Id, request.JobId, request.Job?.Name ?? string.Empty, request.Source, request.Status, request.RetryCount, request.RequestedAtUtc, request.ReadyAtUtc, request.StartedAtUtc, request.CompletedAtUtc, request.LastErrorSummary, request.Attempts.OrderBy(x => x.AttemptNumber).Select(x => x.ToAttemptDto()).ToList(), request.CreatedAtUtc, request.UpdatedAtUtc);
}
