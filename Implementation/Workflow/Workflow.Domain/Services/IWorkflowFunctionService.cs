using System.Text.Json;
using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;

namespace Workflow.Domain.Services;

public interface IWorkflowFunctionService : ICRUDDataService<WorkflowFunction>
{
    Task<object?> ExecuteFunctionAsync(string functionName, JsonDocument? data = null);
    Task<object?> ExecuteFunctionAsync(Guid functionId, JsonDocument data);
    Task<WorkflowFunction?> GetByNameAsync(string name);
    Task<IEnumerable<WorkflowFunction>> GetActiveFunctionsAsync();
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
} 