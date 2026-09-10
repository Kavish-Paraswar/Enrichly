namespace JobAutomationPlatform.Domain.Enums;

public enum ExecutionRequestStatus
{
    Queued = 0,
    Running = 1,
    RetryScheduled = 2,
    Succeeded = 3,
    Failed = 4,
    Canceled = 5,
}
