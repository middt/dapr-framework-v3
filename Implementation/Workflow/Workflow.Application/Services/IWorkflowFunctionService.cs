using System.Text.Json;
using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;

namespace Workflow.Application.Services;

public interface IWorkflowFunctionService : ICRUDDataService<WorkflowFunction>
{
    Task<WorkflowFunction?> GetByNameAsync(string name);
    Task<object?> ExecuteFunctionAsync(string name, JsonDocument? data = null);
    Task<IEnumerable<WorkflowFunction>> GetActiveFunctionsAsync();
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
} 