using System.Text.Json;
using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Application.Services;

public interface IWorkflowInstanceService : ICRUDDataService<WorkflowInstance>
{
    Task<WorkflowInstance?> GetByIdWithDetailsAsync(Guid id);
    Task<WorkflowInstanceDetails?> GetInstanceDetailsAsync(Guid id, string baseUrl);
    Task<IEnumerable<WorkflowInstance>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowInstance>> GetByStateIdAsync(Guid stateId);
    Task<WorkflowStateData?> GetCurrentStateDataAsync(Guid instanceId);
    Task<IEnumerable<WorkflowStateData>> GetStateHistoryAsync(Guid instanceId);
    Task<WorkflowHumanTask?> GetTaskByIdAsync(Guid taskId);
    Task<IEnumerable<WorkflowHumanTask>> GetTasksByInstanceAsync(Guid instanceId);
    Task<IEnumerable<WorkflowHumanTask>> GetTasksByAssigneeAsync(string assignee);
    Task<IEnumerable<WorkflowInstance>> GetActiveInstancesAsync();
    Task<IEnumerable<WorkflowInstance>> GetCompletedInstancesAsync();
    Task<WorkflowInstance?> GetLatestInstanceAsync(Guid definitionId);
    Task<WorkflowInstance?> GetLatestActiveInstanceAsync(Guid definitionId);
    Task<WorkflowInstance?> GetLatestCompletedInstanceAsync(Guid definitionId);
    Task ExecuteTransitionAsync(Guid instanceId, Guid transitionId, JsonDocument? data);
    Task<WorkflowInstance> CompleteTaskAsync(Guid taskId, string result);
    Task<WorkflowInstance> StartInstanceAsync(string workflowName, string clientVersion, JsonDocument? data = null);
    Task<IEnumerable<WorkflowDefinition>> GetCompatibleDefinitionsAsync(string clientVersion);
    Task<WorkflowInstance?> GetParentInstanceAsync(Guid instanceId);
    Task<IEnumerable<WorkflowInstance>> GetChildInstancesAsync(Guid instanceId);
    Task<IEnumerable<WorkflowTransition>> GetSubflowTransitionsAsync(Guid instanceId);
}