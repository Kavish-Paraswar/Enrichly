namespace JobAutomationPlatform.Infrastructure.Security;

public static class RetryMath
{
    public static TimeSpan GetRetryDelay(int retryCount, TimeSpan baseDelay, TimeSpan maxDelay)
    {
        var factor = Math.Pow(2, Math.Max(0, retryCount - 1));
        var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * factor);
        return delay <= maxDelay ? delay : maxDelay;
    }
}
