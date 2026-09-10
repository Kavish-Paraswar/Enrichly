using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Domain.Enums;
using JobAutomationPlatform.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace JobAutomationPlatform.Infrastructure.Services;

public sealed class ExecutionService : IExecutionService
{
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public ExecutionService(AppDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ExecutionSummaryDto>> ListByJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        return await _dbContext.ExecutionRequests
            .AsNoTracking()
            .Include(x => x.Job)
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.RequestedAtUtc)
            .Select(x => x.ToSummaryDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<ExecutionDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.ExecutionRequests
            .AsNoTracking()
            .Include(x => x.Job)
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return request?.ToDetailDto();
    }

    public async Task<ExecutionDetailDto> RunNowAsync(Guid jobId, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var job = await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new NotFoundException($"Job '{jobId}' was not found.");

        var activeExists = await _dbContext.ExecutionRequests
            .AnyAsync(x => x.JobId == jobId && x.CompletedAtUtc == null, cancellationToken);

        if (activeExists)
        {
            throw new ConflictException("This job already has an active execution request.");
        }

        var utcNow = _clock.UtcNow;
        var request = new ExecutionRequest
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            Source = ExecutionRequestSource.Manual,
            Status = ExecutionRequestStatus.Queued,
            RetryCount = 0,
            RequestedAtUtc = utcNow,
            ReadyAtUtc = utcNow,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };

        _dbContext.ExecutionRequests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetAsync(request.Id, cancellationToken) ?? throw new InvalidOperationException("Execution request was not persisted.");
    }

    public async Task<ExecutionDetailDto> RetryAsync(Guid executionRequestId, CancellationToken cancellationToken)
    {
        var request = await _dbContext.ExecutionRequests
            .Include(x => x.Job)
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(x => x.Id == executionRequestId, cancellationToken)
            ?? throw new NotFoundException($"Execution request '{executionRequestId}' was not found.");

        if (!ExecutionStateRules.CanRetry(request.Status))
        {
            throw new ConflictException("Only failed or canceled executions can be retried.");
        }

        if (request.Job is null)
        {
            throw new InvalidOperationException("Execution request is missing its job reference.");
        }

        if (request.RetryCount >= request.Job.MaxAttempts)
        {
            throw new ConflictException("Retry limit has been reached for this execution request.");
        }

        request.RetryCount += 1;
        request.Status = ExecutionRequestStatus.Queued;
        request.ReadyAtUtc = _clock.UtcNow;
        request.CompletedAtUtc = null;
        request.LastErrorSummary = null;
        request.UpdatedAtUtc = _clock.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return request.ToDetailDto();
    }
}
