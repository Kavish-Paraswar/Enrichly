using System.ComponentModel.DataAnnotations;

namespace JobAutomationPlatform.Application.Dto;

public sealed class CreateJobRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Required, Url, MaxLength(500)]
    public string TargetUrl { get; init; } = string.Empty;

    [Required, MaxLength(16)]
    public string HttpMethod { get; init; } = "POST";

    [MaxLength(4000)]
    public string? RequestHeadersJson { get; init; }

    public string? PayloadJson { get; init; }

    [Range(1, 600)]
    public int TimeoutSeconds { get; init; } = 30;

    public bool IsEnabled { get; init; } = true;

    [Range(1, 10080)]
    public int? ScheduleEveryMinutes { get; init; }

    [Range(1, 10)]
    public int MaxAttempts { get; init; } = 3;
}
