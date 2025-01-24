using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-states")]
public class WorkflowStateController : CRUDController<WorkflowState, IWorkflowStateService>
{
    public WorkflowStateController(IWorkflowStateService service) : base(service)
    {
    }

    [HttpGet("{id}/with-views")]
    public async Task<IActionResult> GetByIdWithViewsAsync(Guid id)
    {
        var state = await _service.GetByIdWithViewsAsync(id);
        if (state == null)
            return NotFound();

        return Ok(state);
    }

    [HttpGet("by-definition/{definitionId}")]
    public async Task<IActionResult> GetByDefinitionIdAsync(Guid definitionId)
    {
        var states = await _service.GetByDefinitionIdAsync(definitionId);
        return Ok(states);
    }

    [HttpGet("{stateId}/views")]
    public async Task<IActionResult> GetViewsByStateIdAsync(Guid stateId)
    {
        var views = await _service.GetViewsByStateIdAsync(stateId);
        return Ok(views);
    }
}