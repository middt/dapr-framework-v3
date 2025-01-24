using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowInstanceRepository : ICRUDRepository<WorkflowInstance>
{
    Task<WorkflowInstance?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<WorkflowTransition>> GetAvailableTransitionsAsync(Guid instanceId);
    Task<IEnumerable<WorkflowStateData>> GetStateHistoryAsync(Guid instanceId);
    Task<WorkflowStateData?> GetCurrentStateDataAsync(Guid instanceId);
    Task<IEnumerable<WorkflowInstance>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowInstance>> GetByStateIdAsync(Guid stateId);
    Task<WorkflowHumanTask?> GetTaskByIdAsync(Guid taskId);
    Task<IEnumerable<WorkflowHumanTask>> GetTasksByInstanceAsync(Guid instanceId);
    Task<IEnumerable<WorkflowHumanTask>> GetTasksByAssigneeAsync(string assignee);
    Task<IEnumerable<WorkflowInstance>> GetActiveInstancesAsync();
    Task<IEnumerable<WorkflowInstance>> GetCompletedInstancesAsync();
    Task<WorkflowInstance?> GetLatestInstanceAsync(Guid definitionId);
    Task<WorkflowInstance?> GetLatestActiveInstanceAsync(Guid definitionId);
    Task<WorkflowInstance?> GetLatestCompletedInstanceAsync(Guid definitionId);
    Task<IEnumerable<WorkflowDefinition>> GetCompatibleDefinitionsAsync(string clientVersion);
    Task<WorkflowInstance> AddAsync(WorkflowInstance instance);
}