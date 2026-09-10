namespace JobAutomationPlatform.Domain.Enums;

public enum ExecutionAttemptStatus
{
    Running = 0,
    Succeeded = 1,
    Failed = 2,
    Stale = 3,
}
