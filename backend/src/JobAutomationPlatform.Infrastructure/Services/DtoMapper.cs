using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Infrastructure.Services;

internal static class DtoMapper
{
    public static JobSummaryDto ToSummaryDto(this Job job)
        => new(job.Id, job.OwnerUserId, job.Name, job.IsEnabled, job.TargetUrl, job.HttpMethod, job.TimeoutSeconds, job.ScheduleEveryMinutes, job.NextRunAtUtc, job.MaxAttempts, job.Version, job.CreatedAtUtc, job.UpdatedAtUtc);

    public static JobDetailDto ToDetailDto(this Job job)
        => new(job.Id, job.OwnerUserId, job.Name, job.Description, job.IsEnabled, job.TargetUrl, job.HttpMethod, job.RequestHeadersJson, job.PayloadJson, job.TimeoutSeconds, job.ScheduleEveryMinutes, job.NextRunAtUtc, job.MaxAttempts, job.Version, job.CreatedAtUtc, job.UpdatedAtUtc);

    public static UserDto ToDto(this User user)
        => new(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc, user.UpdatedAtUtc);

    public static ExecutionAttemptDto ToAttemptDto(this ExecutionAttempt attempt)
        => new(attempt.Id, attempt.AttemptNumber, attempt.Status, attempt.WorkerName, attempt.WorkerInstanceId, attempt.StartedAtUtc, attempt.HeartbeatAtUtc, attempt.HttpStatusCode, attempt.DurationMilliseconds, attempt.ResponseHeadersJson, attempt.ResponseBody, attempt.CompletedAtUtc, attempt.ErrorSummary, attempt.ErrorDetailsJson, attempt.IsTimedOut, attempt.CreatedAtUtc, attempt.UpdatedAtUtc);

    public static ExecutionSummaryDto ToSummaryDto(this ExecutionRequest request)
        => new(request.Id, request.JobId, request.OwnerUserId, request.Job?.Name ?? string.Empty, request.Source, request.Status, request.RetryCount, request.RequestedAtUtc, request.ReadyAtUtc, request.RetryAtUtc, request.IdempotencyKey, request.StartedAtUtc, request.CompletedAtUtc, request.LastErrorSummary, request.LastErrorDetailsJson, request.CreatedAtUtc, request.UpdatedAtUtc);

    public static ExecutionDetailDto ToDetailDto(this ExecutionRequest request)
        => new(request.Id, request.JobId, request.OwnerUserId, request.Job?.Name ?? string.Empty, request.Source, request.Status, request.RetryCount, request.RequestedAtUtc, request.ReadyAtUtc, request.RetryAtUtc, request.IdempotencyKey, request.StartedAtUtc, request.CompletedAtUtc, request.LastErrorSummary, request.LastErrorDetailsJson, request.Attempts.OrderBy(x => x.AttemptNumber).Select(x => x.ToAttemptDto()).ToList(), request.CreatedAtUtc, request.UpdatedAtUtc);
}

