namespace JobAutomationPlatform.Application.Interfaces;

public interface IJobSchedulerService
{
    Task<int> EnqueueDueJobsAsync(CancellationToken cancellationToken);
}
