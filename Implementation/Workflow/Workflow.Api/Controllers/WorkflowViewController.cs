using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-views")]
public class WorkflowViewController : CRUDController<WorkflowView, IWorkflowViewService>
{
    public WorkflowViewController(IWorkflowViewService service) : base(service)
    {

    }

    [HttpGet("by-state/{stateId}")]
    public async Task<IActionResult> GetByStateIdAsync(Guid stateId)
    {
        var views = await _service.GetByStateIdAsync(stateId);
        return Ok(views);
    }

    [HttpGet("by-transition/{transitionId}")]
    public async Task<IActionResult> GetByTransitionIdAsync(Guid transitionId)
    {
        var views = await _service.GetByTransitionIdAsync(transitionId);
        return Ok(views);
    }

    [HttpGet("by-definition/{definitionId}")]
    public async Task<IActionResult> GetByDefinitionIdAsync(Guid definitionId)
    {
        var views = await _service.GetByDefinitionIdAsync(definitionId);
        return Ok(views);
    }

    [HttpGet("by-version/{viewVersion}")]
    public async Task<IActionResult> GetByVersionAsync(string viewVersion)
    {
        var views = await _service.GetByVersionAsync(viewVersion);
        return Ok(views);
    }

    [HttpGet("by-workflow-version/{workflowVersion}")]
    public async Task<IActionResult> GetByWorkflowVersionAsync(string workflowVersion)
    {
        var views = await _service.GetByWorkflowVersionAsync(workflowVersion);
        return Ok(views);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestVersionAsync()
    {
        var view = await _service.GetLatestVersionAsync();
        if (view == null)
            return NotFound();

        return Ok(view);
    }

    [HttpGet("instance/{instanceId}/state")]
    public async Task<IActionResult> GetInstanceStateViewsAsync(Guid instanceId)
    {
        var views = await _service.GetInstanceStateViewsAsync(instanceId);
        return Ok(views);
    }

    [HttpGet("instance/{instanceId}/transitions")]
    public async Task<IActionResult> GetInstanceTransitionViewsAsync(Guid instanceId)
    {
        var views = await _service.GetInstanceTransitionViewsAsync(instanceId);
        return Ok(views);
    }

    [HttpGet("instance/{instanceId}/available")]
    public async Task<IActionResult> GetInstanceAvailableViewsAsync(Guid instanceId)
    {
        var views = await _service.GetInstanceAvailableViewsAsync(instanceId);
        return Ok(views);
    }
}