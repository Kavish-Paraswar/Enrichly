namespace JobAutomationPlatform.Worker.Services;

public sealed class WorkerOptions
{
    public int PollingIntervalSeconds { get; set; } = 5;

    public int HeartbeatIntervalSeconds { get; set; } = 5;

    public int StaleAfterMinutes { get; set; } = 2;

    public int RetryBaseDelaySeconds { get; set; } = 30;

    public int RetryMaxDelaySeconds { get; set; } = 600;

    public int SchedulerIntervalSeconds { get; set; } = 15;
}
