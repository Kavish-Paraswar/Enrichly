namespace JobAutomationPlatform.Application.Dto;

public sealed record JobSummaryDto(
    Guid Id,
    string Name,
    bool IsEnabled,
    string TargetUrl,
    string HttpMethod,
    int? ScheduleEveryMinutes,
    DateTimeOffset? NextRunAtUtc,
    int MaxAttempts,
    long Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record JobDetailDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsEnabled,
    string TargetUrl,
    string HttpMethod,
    string? PayloadJson,
    int? ScheduleEveryMinutes,
    DateTimeOffset? NextRunAtUtc,
    int MaxAttempts,
    long Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
