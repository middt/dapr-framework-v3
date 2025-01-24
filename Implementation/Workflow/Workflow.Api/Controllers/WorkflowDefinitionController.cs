using Dapr.Framework.Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-definitions")]
public sealed class WorkflowDefinitionController : CRUDController<WorkflowDefinition, IWorkflowDefinitionService>
{
    public WorkflowDefinitionController(IWorkflowDefinitionService service) : base(service)
    {

    }

    [HttpGet("latest/by-name/{name}")]
    public async Task<IActionResult> GetLatestVersionByNameAsync(string name)
    {
        var definition = await _service.GetLatestVersionByNameAsync(name);
        if (definition == null)
            return NotFound();

        return Ok(definition);
    }

    [HttpGet("by-name/{name}/versions/{workflowVersion}")]
    public async Task<IActionResult> GetVersionByNameAsync(string name, string workflowVersion)
    {
        var definition = await _service.GetVersionByNameAsync(name, workflowVersion);
        if (definition == null)
            return NotFound();

        return Ok(definition);
    }

    [HttpGet("by-name/{name}/versions")]
    public async Task<IActionResult> GetAllVersionsByNameAsync(string name)
    {
        var definitions = await _service.GetAllVersionsByNameAsync(name);
        return Ok(definitions);
    }

    [HttpGet("check-name")]
    public async Task<IActionResult> IsNameUniqueAsync([FromQuery] string name, [FromQuery] string? excludeId = null)
    {
        var isUnique = await _service.IsNameUniqueAsync(name, excludeId);
        return Ok(isUnique);
    }

    [HttpPost("{id}/versions")]
    public async Task<IActionResult> CreateNewVersionAsync(Guid id)
    {
        var definition = await _service.CreateNewVersionAsync(id);
        return Ok(definition);
    }

    [HttpPost("{id}/clone")]
    public async Task<IActionResult> CloneAsync(Guid id, [FromQuery] string newName)
    {
        var definition = await _service.CloneAsync(id, newName);
        return Ok(definition);
    }
}