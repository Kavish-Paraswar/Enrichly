using JobAutomationPlatform.Application.Dto;

namespace JobAutomationPlatform.Application.Interfaces;

public interface IJobService
{
    Task<IReadOnlyList<JobSummaryDto>> ListAsync(CancellationToken cancellationToken);

    Task<JobDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<JobDetailDto> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken);

    Task<JobDetailDto> UpdateAsync(Guid id, UpdateJobRequest request, CancellationToken cancellationToken);

    Task<JobDetailDto> SetEnabledAsync(Guid id, bool isEnabled, CancellationToken cancellationToken);
}
