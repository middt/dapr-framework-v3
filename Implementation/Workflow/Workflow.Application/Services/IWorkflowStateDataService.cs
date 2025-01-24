using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;

namespace Workflow.Application.Services;

public interface IWorkflowStateDataService : ICRUDDataService<WorkflowStateData>
{
    Task<IEnumerable<WorkflowStateData>> GetByInstanceIdAsync(Guid instanceId);
    Task<IEnumerable<WorkflowStateData>> GetByStateIdAsync(Guid stateId);
    Task<WorkflowStateData?> GetLatestByInstanceIdAsync(Guid instanceId);
    Task<WorkflowStateData?> GetByInstanceAndStateAsync(Guid instanceId, Guid stateId);
    Task<WorkflowStateData?> GetStateDataAsync(Guid instanceId, Guid stateId);
}