using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Application.Dto;

public sealed record ExecutionAttemptDto(
    Guid Id,
    int AttemptNumber,
    ExecutionAttemptStatus Status,
    string? WorkerName,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset HeartbeatAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? ErrorSummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ExecutionSummaryDto(
    Guid Id,
    Guid JobId,
    string JobName,
    ExecutionRequestSource Source,
    ExecutionRequestStatus Status,
    int RetryCount,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ReadyAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? LastErrorSummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ExecutionDetailDto(
    Guid Id,
    Guid JobId,
    string JobName,
    ExecutionRequestSource Source,
    ExecutionRequestStatus Status,
    int RetryCount,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ReadyAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? LastErrorSummary,
    IReadOnlyList<ExecutionAttemptDto> Attempts,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
