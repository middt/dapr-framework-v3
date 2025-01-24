using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Entities;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowFunctionRepository : ICRUDRepository<WorkflowFunction>
{
    Task<IEnumerable<WorkflowFunction>> GetByWorkflowDefinitionIdAsync(Guid definitionId);
    Task<WorkflowFunction?> GetByNameAsync(string name);
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
    Task<WorkflowFunction?> GetByStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowFunction>> GetActiveFunctionsAsync();
}