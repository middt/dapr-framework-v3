using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-transitions")]
public class WorkflowTransitionController : CRUDController<WorkflowTransition, IWorkflowTransitionService>
{

    public WorkflowTransitionController(IWorkflowTransitionService service) : base(service)
    {

    }

    [HttpGet("{id}/with-views")]
    public async Task<IActionResult> GetByIdWithViewsAsync(Guid id)
    {
        var transition = await _service.GetByIdWithViewsAsync(id);
        if (transition == null)
            return NotFound();

        return Ok(transition);
    }

    [HttpGet("by-definition/{definitionId}")]
    public async Task<IActionResult> GetByDefinitionIdAsync(Guid definitionId)
    {
        var transitions = await _service.GetByDefinitionIdAsync(definitionId);
        return Ok(transitions);
    }

    [HttpGet("by-from-state/{stateId}")]
    public async Task<IActionResult> GetByFromStateIdAsync(Guid stateId)
    {
        var transitions = await _service.GetByFromStateIdAsync(stateId);
        return Ok(transitions);
    }

    [HttpGet("by-to-state/{stateId}")]
    public async Task<IActionResult> GetByToStateIdAsync(Guid stateId)
    {
        var transitions = await _service.GetByToStateIdAsync(stateId);
        return Ok(transitions);
    }

    [HttpGet("{transitionId}/views")]
    public async Task<IActionResult> GetViewsByTransitionIdAsync(Guid transitionId)
    {
        var views = await _service.GetViewsByTransitionIdAsync(transitionId);
        return Ok(views);
    }
}