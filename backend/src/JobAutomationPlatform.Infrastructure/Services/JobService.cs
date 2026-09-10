using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobAutomationPlatform.Infrastructure.Services;

public sealed class JobService : IJobService
{
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public JobService(AppDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<IReadOnlyList<JobSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.ToSummaryDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<JobDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return job?.ToDetailDto();
    }

    public async Task<JobDetailDto> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken)
    {
        ValidateJob(request.Name, request.TargetUrl, request.HttpMethod, request.ScheduleEveryMinutes, request.MaxAttempts);

        var utcNow = _clock.UtcNow;
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            TargetUrl = NormalizeUrl(request.TargetUrl),
            HttpMethod = NormalizeMethod(request.HttpMethod),
            PayloadJson = request.PayloadJson,
            IsEnabled = request.IsEnabled,
            ScheduleEveryMinutes = request.ScheduleEveryMinutes,
            NextRunAtUtc = request.IsEnabled && request.ScheduleEveryMinutes is not null
                ? utcNow.AddMinutes(request.ScheduleEveryMinutes.Value)
                : null,
            MaxAttempts = request.MaxAttempts,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };

        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return job.ToDetailDto();
    }

    public async Task<JobDetailDto> UpdateAsync(Guid id, UpdateJobRequest request, CancellationToken cancellationToken)
    {
        ValidateJob(request.Name, request.TargetUrl, request.HttpMethod, request.ScheduleEveryMinutes, request.MaxAttempts);

        var job = await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Job '{id}' was not found.");

        if (job.Version != request.Version)
        {
            throw new ConflictException("The job was updated by another request. Reload and try again.");
        }

        job.Name = request.Name.Trim();
        job.Description = request.Description?.Trim();
        job.TargetUrl = NormalizeUrl(request.TargetUrl);
        job.HttpMethod = NormalizeMethod(request.HttpMethod);
        job.PayloadJson = request.PayloadJson;
        job.ScheduleEveryMinutes = request.ScheduleEveryMinutes;
        job.MaxAttempts = request.MaxAttempts;
        job.IsEnabled = request.IsEnabled;
        job.NextRunAtUtc = request.IsEnabled && request.ScheduleEveryMinutes is not null
            ? _clock.UtcNow.AddMinutes(request.ScheduleEveryMinutes.Value)
            : null;
        job.Version += 1;
        job.UpdatedAtUtc = _clock.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return job.ToDetailDto();
    }

    public async Task<JobDetailDto> SetEnabledAsync(Guid id, bool isEnabled, CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Job '{id}' was not found.");

        job.IsEnabled = isEnabled;
        job.Version += 1;
        job.UpdatedAtUtc = _clock.UtcNow;

        if (isEnabled && job.ScheduleEveryMinutes is not null && job.NextRunAtUtc is null)
        {
            job.NextRunAtUtc = _clock.UtcNow.AddMinutes(job.ScheduleEveryMinutes.Value);
        }

        if (!isEnabled)
        {
            job.NextRunAtUtc = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return job.ToDetailDto();
    }

    private static void ValidateJob(string name, string targetUrl, string httpMethod, int? scheduleEveryMinutes, int maxAttempts)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Job name is required.");
        }

        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri) || (uri.Scheme is not "http" and not "https"))
        {
            throw new ValidationException("TargetUrl must be an absolute http or https URL.");
        }

        if (!string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(httpMethod, "PUT", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(httpMethod, "PATCH", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(httpMethod, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("HttpMethod must be one of GET, POST, PUT, PATCH, or DELETE.");
        }

        if (scheduleEveryMinutes is not null && scheduleEveryMinutes < 1)
        {
            throw new ValidationException("ScheduleEveryMinutes must be positive when provided.");
        }

        if (maxAttempts < 1)
        {
            throw new ValidationException("MaxAttempts must be at least 1.");
        }
    }

    private static string NormalizeMethod(string httpMethod) => httpMethod.Trim().ToUpperInvariant();

    private static string NormalizeUrl(string targetUrl) => targetUrl.Trim();
}
