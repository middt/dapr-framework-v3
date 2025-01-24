using System.Text.Json;
using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;
using Microsoft.AspNetCore.Routing;
using Workflow.Domain.Models.Views;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-instances")]
public class WorkflowInstanceController : CRUDController<WorkflowInstance, IWorkflowInstanceService>
{
    private readonly IWorkflowInstanceService _instanceService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkflowTransitionService _transitionService;
    private readonly LinkGenerator _linkGenerator;

    public WorkflowInstanceController(
        IWorkflowInstanceService service,
        IHttpContextAccessor httpContextAccessor,
        IWorkflowTransitionService transitionService,
        LinkGenerator linkGenerator)
        : base(service)
    {
        _instanceService = service;
        _httpContextAccessor = httpContextAccessor;
        _transitionService = transitionService;
        _linkGenerator = linkGenerator;
    }

    [HttpGet("{id}/with-details")]
    public async Task<ActionResult<WorkflowInstanceDetails>> GetWithDetails(Guid id)
    {
        var instance = await _instanceService.GetInstanceDetailsAsync(id, GetBaseUrl());
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    [HttpGet("{id}/state-history")]
    public async Task<ActionResult<IEnumerable<WorkflowStateData>>> GetStateHistory(Guid id)
    {
        var history = await _instanceService.GetStateHistoryAsync(id);
        return Ok(history);
    }

    [HttpGet("{id}/state-data")]
    public async Task<ActionResult<WorkflowStateData>> GetStateData(Guid id)
    {
        var data = await _instanceService.GetCurrentStateDataAsync(id);
        if (data == null)
            return NotFound();
        return Ok(data);
    }

    [HttpGet("by-definition/{definitionId}")]
    public async Task<IActionResult> GetByDefinitionIdAsync(Guid definitionId)
    {
        var instances = await _instanceService.GetByDefinitionIdAsync(definitionId);
        return Ok(instances);
    }

    [HttpGet("by-state/{stateId}")]
    public async Task<IActionResult> GetByStateIdAsync(Guid stateId)
    {
        var instances = await _instanceService.GetByStateIdAsync(stateId);
        return Ok(instances);
    }

    [HttpGet("tasks/{taskId}")]
    public async Task<IActionResult> GetTaskByIdAsync(Guid taskId)
    {
        var task = await _instanceService.GetTaskByIdAsync(taskId);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpGet("{instanceId}/tasks")]
    public async Task<IActionResult> GetTasksByInstanceAsync(Guid instanceId)
    {
        var tasks = await _instanceService.GetTasksByInstanceAsync(instanceId);
        return Ok(tasks);
    }

    [HttpGet("tasks/by-assignee/{assignee}")]
    public async Task<IActionResult> GetTasksByAssigneeAsync(string assignee)
    {
        var tasks = await _instanceService.GetTasksByAssigneeAsync(assignee);
        return Ok(tasks);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveInstancesAsync()
    {
        var instances = await _instanceService.GetActiveInstancesAsync();
        return Ok(instances);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedInstancesAsync()
    {
        var instances = await _instanceService.GetCompletedInstancesAsync();
        return Ok(instances);
    }

    [HttpGet("latest/by-definition/{definitionId}")]
    public async Task<IActionResult> GetLatestInstanceAsync(Guid definitionId)
    {
        var instance = await _instanceService.GetLatestInstanceAsync(definitionId);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    [HttpGet("latest/active/by-definition/{definitionId}")]
    public async Task<IActionResult> GetLatestActiveInstanceAsync(Guid definitionId)
    {
        var instance = await _instanceService.GetLatestActiveInstanceAsync(definitionId);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    [HttpGet("latest/completed/by-definition/{definitionId}")]
    public async Task<IActionResult> GetLatestCompletedInstanceAsync(Guid definitionId)
    {
        var instance = await _instanceService.GetLatestCompletedInstanceAsync(definitionId);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    [HttpPost("{instanceId}/transitions/{transitionId}")]
    public async Task<IActionResult> ExecuteTransitionAsync(Guid instanceId, Guid transitionId, [FromBody] JsonDocument? data = null)
    {
        // var instance = await _instanceService.ExecuteTransitionAsync(instanceId, transitionId, data);
        // return Ok(instance);
        await _instanceService.ExecuteTransitionAsync(instanceId, transitionId, data);
        return Ok();
    }

    [HttpPost("tasks/{taskId}/complete")]
    public async Task<IActionResult> CompleteTaskAsync(Guid taskId, [FromBody] string result)
    {
        var instance = await _instanceService.CompleteTaskAsync(taskId, result);
        return Ok(instance);
    }

    [HttpPost("start/{workflowName}")]
    public async Task<IActionResult> StartInstanceAsync(string workflowName, [FromHeader(Name = "X-Client-Version")] string clientVersion, [FromBody] JsonDocument? data = null)
    {
        var instance = await _instanceService.StartInstanceAsync(workflowName, clientVersion, data);
        return Ok(instance);
    }

    [HttpGet("compatible-definitions")]
    public async Task<IActionResult> GetCompatibleDefinitionsAsync([FromHeader(Name = "X-Client-Version")] string clientVersion)
    {
        var definitions = await _instanceService.GetCompatibleDefinitionsAsync(clientVersion);
        return Ok(definitions);
    }

    [HttpGet("{instanceId}/subflow-info")]
    public async Task<IActionResult> GetSubflowInfoAsync(Guid instanceId)
    {
        var instance = await _instanceService.GetInstanceDetailsAsync(instanceId, GetBaseUrl());
        if (instance == null)
            return NotFound();
            
        return Ok(instance);  // WorkflowInstanceDetails already contains parent/child info
    }

    private string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return $"{request?.Scheme}://{request?.Host}";
    }

    [HttpGet("{instanceId}/parent")]
    public async Task<IActionResult> GetParentInstanceAsync(Guid instanceId)
    {
        var parent = await _instanceService.GetParentInstanceAsync(instanceId);
        if (parent == null)
            return NotFound();
            
        return Ok(parent);
    }

    [HttpGet("{instanceId}/children")]
    public async Task<IActionResult> GetChildInstancesAsync(Guid instanceId)
    {
        var children = await _instanceService.GetChildInstancesAsync(instanceId);
        return Ok(children);
    }

    [HttpGet("{instanceId}/subflow-transitions")]
    public async Task<IActionResult> GetSubflowTransitionsAsync(Guid instanceId)
    {
        var transitions = await _instanceService.GetSubflowTransitionsAsync(instanceId);
        return Ok(transitions);
    }
}