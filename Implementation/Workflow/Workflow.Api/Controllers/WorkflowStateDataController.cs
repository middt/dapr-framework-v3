using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-state-data")]
public class WorkflowStateDataController : CRUDController<WorkflowStateData, IWorkflowStateDataService>
{
    public WorkflowStateDataController(IWorkflowStateDataService service) : base(service)
    {

    }

    [HttpGet("by-instance/{instanceId}")]
    public async Task<IActionResult> GetByInstanceIdAsync(Guid instanceId)
    {
        var stateData = await _service.GetByInstanceIdAsync(instanceId);
        return Ok(stateData);
    }

    [HttpGet("by-state/{stateId}")]
    public async Task<IActionResult> GetByStateIdAsync(Guid stateId)
    {
        var stateData = await _service.GetByStateIdAsync(stateId);
        return Ok(stateData);
    }

    [HttpGet("latest/by-instance/{instanceId}")]
    public async Task<IActionResult> GetLatestByInstanceIdAsync(Guid instanceId)
    {
        var stateData = await _service.GetLatestByInstanceIdAsync(instanceId);
        if (stateData == null)
            return NotFound();

        return Ok(stateData);
    }

    [HttpGet("by-instance/{instanceId}/state/{stateId}")]
    public async Task<IActionResult> GetByInstanceAndStateAsync(Guid instanceId, Guid stateId)
    {
        var stateData = await _service.GetByInstanceAndStateAsync(instanceId, stateId);
        if (stateData == null)
            return NotFound();

        return Ok(stateData);
    }
}