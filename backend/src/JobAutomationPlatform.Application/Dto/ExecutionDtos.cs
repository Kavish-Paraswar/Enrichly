using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Application.Dto;

public sealed record ExecutionAttemptDto(
    Guid Id,
    int AttemptNumber,
    ExecutionAttemptStatus Status,
    string? WorkerName,
    string? WorkerInstanceId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset HeartbeatAtUtc,
    int? HttpStatusCode,
    long? DurationMilliseconds,
    string? ResponseHeadersJson,
    string? ResponseBody,
    DateTimeOffset? CompletedAtUtc,
    string? ErrorSummary,
    string? ErrorDetailsJson,
    bool IsTimedOut,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ExecutionSummaryDto(
    Guid Id,
    Guid JobId,
    Guid OwnerUserId,
    string JobName,
    ExecutionRequestSource Source,
    ExecutionRequestStatus Status,
    int RetryCount,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ReadyAtUtc,
    DateTimeOffset? RetryAtUtc,
    string? IdempotencyKey,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? LastErrorSummary,
    string? LastErrorDetailsJson,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ExecutionDetailDto(
    Guid Id,
    Guid JobId,
    Guid OwnerUserId,
    string JobName,
    ExecutionRequestSource Source,
    ExecutionRequestStatus Status,
    int RetryCount,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ReadyAtUtc,
    DateTimeOffset? RetryAtUtc,
    string? IdempotencyKey,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? LastErrorSummary,
    string? LastErrorDetailsJson,
    IReadOnlyList<ExecutionAttemptDto> Attempts,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
