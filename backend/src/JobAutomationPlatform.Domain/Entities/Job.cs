using System.ComponentModel.DataAnnotations;

namespace JobAutomationPlatform.Domain.Entities;

public sealed class Job
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string TargetUrl { get; set; } = string.Empty;

    [MaxLength(16)]
    public string HttpMethod { get; set; } = "POST";

    public string? PayloadJson { get; set; }

    public bool IsEnabled { get; set; }

    public int? ScheduleEveryMinutes { get; set; }

    public DateTimeOffset? NextRunAtUtc { get; set; }

    public int MaxAttempts { get; set; } = 3;

    public long Version { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<ExecutionRequest> ExecutionRequests { get; set; } = new List<ExecutionRequest>();
}
