using JobAutomationPlatform.Domain.Enums;

namespace JobAutomationPlatform.Domain.Rules;

public static class ExecutionStateRules
{
    public static bool CanClaim(ExecutionRequestStatus status, DateTimeOffset readyAtUtc, DateTimeOffset utcNow)
    {
        if (readyAtUtc > utcNow)
        {
            return false;
        }

        return status is ExecutionRequestStatus.Queued or ExecutionRequestStatus.RetryScheduled;
    }

    public static bool IsActive(ExecutionRequestStatus status)
    {
        return status is ExecutionRequestStatus.Queued or ExecutionRequestStatus.Running or ExecutionRequestStatus.RetryScheduled;
    }

    public static bool CanRetry(ExecutionRequestStatus status)
    {
        return status is ExecutionRequestStatus.Failed or ExecutionRequestStatus.Canceled;
    }
}
