using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Workflow.Application.Services;
using Workflow.Domain.Models;
using Dapr.Framework.Api.Controllers.Base;

namespace Workflow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/workflow-functions")]
public class WorkflowFunctionController : CRUDController<WorkflowFunction, IWorkflowFunctionService>
{
        public WorkflowFunctionController(IWorkflowFunctionService service) : base(service)
    {

    }

    [HttpPost("{name}/execute")]
    public async Task<ActionResult<object>> ExecuteFunction(string name, [FromBody] object? data = null)
    {
        try
        {
            var jsonData = data != null ? JsonDocument.Parse(JsonSerializer.Serialize(data)) : null;
            var result = await _service.ExecuteFunctionAsync(name, jsonData);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<WorkflowFunction>>> GetActiveFunctions()
    {
        var functions = await _service.GetActiveFunctionsAsync();
        return Ok(functions);
    }
} 