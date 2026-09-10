using JobAutomationPlatform.Application.Dto;

namespace JobAutomationPlatform.Application.Interfaces;

public interface IExecutionService
{
    Task<IReadOnlyList<ExecutionSummaryDto>> ListByJobAsync(Guid jobId, CancellationToken cancellationToken);

    Task<ExecutionDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<ExecutionDetailDto> RunNowAsync(Guid jobId, CancellationToken cancellationToken);

    Task<ExecutionDetailDto> RetryAsync(Guid executionRequestId, CancellationToken cancellationToken);
}
