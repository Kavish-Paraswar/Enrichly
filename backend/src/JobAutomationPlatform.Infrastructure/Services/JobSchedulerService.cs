using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobAutomationPlatform.Infrastructure.Services;

public sealed class JobSchedulerService : IJobSchedulerService
{
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public JobSchedulerService(AppDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<int> EnqueueDueJobsAsync(CancellationToken cancellationToken)
    {
        var utcNow = _clock.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var dueJobs = await _dbContext.Jobs
            .FromSqlInterpolated($"""
                select *
                from jobs
                where is_enabled = true
                  and schedule_every_minutes is not null
                  and next_run_at_utc is not null
                  and next_run_at_utc <= {utcNow}
                order by next_run_at_utc
                for update skip locked
                limit 25
            """)
            .ToListAsync(cancellationToken);

        var enqueuedCount = 0;

        foreach (var job in dueJobs)
        {
            var hasActiveRequest = await _dbContext.ExecutionRequests
                .AnyAsync(x => x.JobId == job.Id && x.CompletedAtUtc == null, cancellationToken);

            if (hasActiveRequest)
            {
                continue;
            }

            var request = new ExecutionRequest
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                Source = ExecutionRequestSource.Schedule,
                Status = ExecutionRequestStatus.Queued,
                RetryCount = 0,
                RequestedAtUtc = utcNow,
                ReadyAtUtc = utcNow,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow,
            };

            job.NextRunAtUtc = utcNow.AddMinutes(job.ScheduleEveryMinutes ?? 0);
            job.UpdatedAtUtc = utcNow;

            _dbContext.ExecutionRequests.Add(request);
            enqueuedCount += 1;
        }

        if (enqueuedCount > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return enqueuedCount;
    }
}
