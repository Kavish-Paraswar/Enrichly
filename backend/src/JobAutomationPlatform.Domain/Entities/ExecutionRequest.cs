using System.ComponentModel.DataAnnotations;
using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Domain.Entities;

public sealed class ExecutionRequest
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Job? Job { get; set; }

    public ExecutionRequestSource Source { get; set; }

    public ExecutionRequestStatus Status { get; set; }

    public int RetryCount { get; set; }

    public DateTimeOffset RequestedAtUtc { get; set; }

    public DateTimeOffset ReadyAtUtc { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    [MaxLength(2000)]
    public string? LastErrorSummary { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<ExecutionAttempt> Attempts { get; set; } = new List<ExecutionAttempt>();
}
