using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobAutomationPlatform.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ExecutionsController : ControllerBase
{
    private readonly IExecutionService _executionService;

    public ExecutionsController(IExecutionService executionService)
    {
        _executionService = executionService;
    }

    [HttpGet("jobs/{jobId:guid}/executions")]
    public async Task<ActionResult<IReadOnlyList<ExecutionSummaryDto>>> ListByJob(Guid jobId, CancellationToken cancellationToken)
    {
        return Ok(await _executionService.ListByJobAsync(jobId, cancellationToken));
    }

    [HttpPost("jobs/{jobId:guid}/run")]
    public async Task<ActionResult<ExecutionDetailDto>> RunNow(Guid jobId, CancellationToken cancellationToken)
    {
        var execution = await _executionService.RunNowAsync(jobId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = execution.Id }, execution);
    }

    [HttpGet("executions/{id:guid}")]
    public async Task<ActionResult<ExecutionDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var execution = await _executionService.GetAsync(id, cancellationToken);
        return execution is null ? NotFound() : Ok(execution);
    }

    [HttpPost("executions/{id:guid}/retry")]
    public async Task<ActionResult<ExecutionDetailDto>> Retry(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _executionService.RetryAsync(id, cancellationToken));
    }
}
