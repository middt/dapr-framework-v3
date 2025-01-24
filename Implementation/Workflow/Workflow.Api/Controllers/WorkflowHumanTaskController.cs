using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-human-tasks")]
public class WorkflowHumanTaskController : CRUDController<WorkflowHumanTask, IWorkflowHumanTaskService>
{
    public WorkflowHumanTaskController(IWorkflowHumanTaskService service) : base(service)
    {
    }

    [HttpGet("by-instance/{instanceId}")]
    public async Task<IActionResult> GetByInstanceIdAsync(Guid instanceId)
    {
        var tasks = await _service.GetByInstanceIdAsync(instanceId);
        return Ok(tasks);
    }

    [HttpGet("by-assignee/{assignee}")]
    public async Task<IActionResult> GetByAssigneeAsync(string assignee)
    {
        var tasks = await _service.GetByAssigneeAsync(assignee);
        return Ok(tasks);
    }

    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetByStatusAsync(string status)
    {
        var tasks = await _service.GetByStatusAsync(status);
        return Ok(tasks);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingTasksAsync()
    {
        var tasks = await _service.GetPendingTasksAsync();
        return Ok(tasks);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedTasksAsync()
    {
        var tasks = await _service.GetCompletedTasksAsync();
        return Ok(tasks);
    }
}