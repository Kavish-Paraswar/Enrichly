using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Domain.Enums;
using JobAutomationPlatform.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace JobAutomationPlatform.Infrastructure.Services;

public sealed class ExecutionQueueService : IExecutionQueueService
{
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public ExecutionQueueService(AppDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<QueuedExecutionClaim?> ClaimNextAsync(string workerName, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var utcNow = _clock.UtcNow;
        var request = await _dbContext.ExecutionRequests
            .FromSqlInterpolated($"""
                select *
                from execution_requests
                where status in ({ExecutionRequestStatus.Queued}, {ExecutionRequestStatus.RetryScheduled})
                  and ready_at_utc <= {utcNow}
                  and completed_at_utc is null
                order by ready_at_utc, created_at_utc
                for update skip locked
                limit 1
            """)
            .Include(x => x.Job)
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null || request.Job is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var attemptNumber = request.Attempts.Count + 1;
        var attempt = new ExecutionAttempt
        {
            Id = Guid.NewGuid(),
            ExecutionRequestId = request.Id,
            AttemptNumber = attemptNumber,
            Status = ExecutionAttemptStatus.Running,
            WorkerName = workerName,
            WorkerInstanceId = Environment.MachineName,
            StartedAtUtc = utcNow,
            HeartbeatAtUtc = utcNow,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };

        request.Status = ExecutionRequestStatus.Running;
        request.StartedAtUtc ??= utcNow;
        request.UpdatedAtUtc = utcNow;

        _dbContext.ExecutionAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new QueuedExecutionClaim(
            request.Id,
            request.JobId,
            request.OwnerUserId,
            attempt.Id,
            request.Job.Name,
            request.Job.TargetUrl,
            request.Job.HttpMethod,
            request.Job.RequestHeadersJson,
            request.Job.PayloadJson,
            request.Job.TimeoutSeconds,
            attemptNumber,
            request.Job.MaxAttempts,
            request.IdempotencyKey);
    }

    public async Task UpdateHeartbeatAsync(Guid executionAttemptId, DateTimeOffset heartbeatAtUtc, CancellationToken cancellationToken)
    {
        var attempt = await _dbContext.ExecutionAttempts.FirstOrDefaultAsync(x => x.Id == executionAttemptId, cancellationToken)
            ?? throw new NotFoundException($"Execution attempt '{executionAttemptId}' was not found.");

        attempt.HeartbeatAtUtc = heartbeatAtUtc;
        attempt.UpdatedAtUtc = heartbeatAtUtc;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteSucceededAsync(Guid executionAttemptId, DateTimeOffset completedAtUtc, JobRunResult result, CancellationToken cancellationToken)
    {
        var attempt = await _dbContext.ExecutionAttempts
            .Include(x => x.ExecutionRequest)
            .FirstOrDefaultAsync(x => x.Id == executionAttemptId, cancellationToken)
            ?? throw new NotFoundException($"Execution attempt '{executionAttemptId}' was not found.");

        var request = attempt.ExecutionRequest ?? throw new InvalidOperationException("Attempt is missing its execution request.");

        attempt.Status = ExecutionAttemptStatus.Succeeded;
        attempt.CompletedAtUtc = completedAtUtc;
        attempt.UpdatedAtUtc = completedAtUtc;
        attempt.IsTimedOut = false;
        attempt.HttpStatusCode = result.HttpStatusCode;
        attempt.DurationMilliseconds = result.DurationMilliseconds;
        attempt.ResponseHeadersJson = result.ResponseHeadersJson;
        attempt.ResponseBody = result.ResponseBody;
        attempt.ErrorSummary = null;
        attempt.ErrorDetailsJson = null;

        request.Status = ExecutionRequestStatus.Succeeded;
        request.CompletedAtUtc = completedAtUtc;
        request.LastErrorSummary = null;
        request.LastErrorDetailsJson = null;
        request.UpdatedAtUtc = completedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteFailedAsync(Guid executionAttemptId, DateTimeOffset completedAtUtc, JobRunResult result, bool scheduleRetry, DateTimeOffset? retryAtUtc, CancellationToken cancellationToken)
    {
        var attempt = await _dbContext.ExecutionAttempts
            .Include(x => x.ExecutionRequest)
            .ThenInclude(x => x!.Job)
            .FirstOrDefaultAsync(x => x.Id == executionAttemptId, cancellationToken)
            ?? throw new NotFoundException($"Execution attempt '{executionAttemptId}' was not found.");

        var request = attempt.ExecutionRequest ?? throw new InvalidOperationException("Attempt is missing its execution request.");
        var job = request.Job ?? throw new InvalidOperationException("Execution request is missing its job reference.");

        attempt.Status = ExecutionAttemptStatus.Failed;
        attempt.CompletedAtUtc = completedAtUtc;
        attempt.ErrorSummary = result.FailureSummary;
        attempt.ErrorDetailsJson = result.FailureDetailsJson;
        attempt.HttpStatusCode = result.HttpStatusCode;
        attempt.DurationMilliseconds = result.DurationMilliseconds;
        attempt.ResponseHeadersJson = result.ResponseHeadersJson;
        attempt.ResponseBody = result.ResponseBody;
        attempt.IsTimedOut = result.IsTimedOut;
        attempt.UpdatedAtUtc = completedAtUtc;

        request.LastErrorSummary = result.FailureSummary;
        request.LastErrorDetailsJson = result.FailureDetailsJson;
        request.UpdatedAtUtc = completedAtUtc;

        if (scheduleRetry)
        {
            request.Status = ExecutionRequestStatus.RetryScheduled;
            request.RetryCount += 1;
            request.RetryAtUtc = retryAtUtc ?? _clock.UtcNow;
            request.ReadyAtUtc = request.RetryAtUtc.Value;
            request.CompletedAtUtc = null;
        }
        else
        {
            request.Status = ExecutionRequestStatus.Failed;
            request.CompletedAtUtc = completedAtUtc;
        }

        if (request.RetryCount >= job.MaxAttempts)
        {
            request.Status = ExecutionRequestStatus.Failed;
            request.CompletedAtUtc = completedAtUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> EnqueueDueJobsAsync(DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
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

        var created = 0;

        foreach (var job in dueJobs)
        {
            var hasActiveRequest = await _dbContext.ExecutionRequests
                .AnyAsync(x => x.JobId == job.Id && x.CompletedAtUtc == null, cancellationToken);

            if (hasActiveRequest)
            {
                continue;
            }

            job.NextRunAtUtc = utcNow.AddMinutes(job.ScheduleEveryMinutes ?? 0);
            job.UpdatedAtUtc = utcNow;

            _dbContext.ExecutionRequests.Add(new ExecutionRequest
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
            });

            created += 1;
        }

        if (created > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return created;
    }

    public async Task<int> ReapStaleExecutionsAsync(DateTimeOffset staleBeforeUtc, CancellationToken cancellationToken)
    {
        var staleAttempts = await _dbContext.ExecutionAttempts
            .Include(x => x.ExecutionRequest)
            .ThenInclude(x => x!.Job)
            .Where(x => x.Status == ExecutionAttemptStatus.Running && x.HeartbeatAtUtc < staleBeforeUtc)
            .ToListAsync(cancellationToken);

        var reaped = 0;

        foreach (var attempt in staleAttempts)
        {
            var request = attempt.ExecutionRequest;
            var job = request?.Job;

            if (request is null || job is null)
            {
                continue;
            }

            attempt.Status = ExecutionAttemptStatus.Stale;
            attempt.CompletedAtUtc = staleBeforeUtc;
            attempt.UpdatedAtUtc = staleBeforeUtc;
            attempt.IsTimedOut = true;
            attempt.ErrorSummary = "Execution became stale before completing.";

            if (request.RetryCount < job.MaxAttempts)
            {
                request.Status = ExecutionRequestStatus.RetryScheduled;
                request.RetryCount += 1;
                request.RetryAtUtc = staleBeforeUtc.Add(RetryMath.GetRetryDelay(request.RetryCount, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(600)));
                request.ReadyAtUtc = request.RetryAtUtc.Value;
                request.CompletedAtUtc = null;
            }
            else
            {
                request.Status = ExecutionRequestStatus.Failed;
                request.CompletedAtUtc = staleBeforeUtc;
            }

            request.LastErrorSummary = "Execution became stale before completing.";
            request.LastErrorDetailsJson = null;
            request.UpdatedAtUtc = staleBeforeUtc;
            reaped += 1;
        }

        if (reaped > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return reaped;
    }
}
