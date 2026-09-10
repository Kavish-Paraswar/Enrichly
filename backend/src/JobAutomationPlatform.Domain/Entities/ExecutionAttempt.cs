using System.ComponentModel.DataAnnotations;
using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Domain.Entities;

public sealed class ExecutionAttempt
{
    public Guid Id { get; set; }

    public Guid ExecutionRequestId { get; set; }

    public ExecutionRequest? ExecutionRequest { get; set; }

    public int AttemptNumber { get; set; }

    public ExecutionAttemptStatus Status { get; set; }

    [MaxLength(200)]
    public string? WorkerName { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset HeartbeatAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    [MaxLength(2000)]
    public string? ErrorSummary { get; set; }

    public string? ErrorDetailsJson { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
