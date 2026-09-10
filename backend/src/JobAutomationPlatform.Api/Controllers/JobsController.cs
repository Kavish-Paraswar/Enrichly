using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobAutomationPlatform.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public sealed class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobSummaryDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _jobService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<ActionResult<JobDetailDto>> Create([FromBody] CreateJobRequest request, CancellationToken cancellationToken)
    {
        var created = await _jobService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> Update(Guid id, [FromBody] UpdateJobRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/enable")]
    public async Task<ActionResult<JobDetailDto>> Enable(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.SetEnabledAsync(id, true, cancellationToken));
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult<JobDetailDto>> Disable(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _jobService.SetEnabledAsync(id, false, cancellationToken));
    }
}
