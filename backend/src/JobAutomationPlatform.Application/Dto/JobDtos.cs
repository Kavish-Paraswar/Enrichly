namespace JobAutomationPlatform.Application.Dto;

public sealed record JobSummaryDto(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    bool IsEnabled,
    string TargetUrl,
    string HttpMethod,
    int TimeoutSeconds,
    int? ScheduleEveryMinutes,
    DateTimeOffset? NextRunAtUtc,
    int MaxAttempts,
    long Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record JobDetailDto(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string? Description,
    bool IsEnabled,
    string TargetUrl,
    string HttpMethod,
    string? RequestHeadersJson,
    string? PayloadJson,
    int TimeoutSeconds,
    int? ScheduleEveryMinutes,
    DateTimeOffset? NextRunAtUtc,
    int MaxAttempts,
    long Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
