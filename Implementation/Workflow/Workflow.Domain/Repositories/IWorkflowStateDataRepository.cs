using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowStateDataRepository : ICRUDRepository<WorkflowStateData>
{
    Task<IEnumerable<WorkflowStateData>> GetByInstanceIdAsync(Guid instanceId);
    Task<IEnumerable<WorkflowStateData>> GetByStateIdAsync(Guid stateId);
    Task<WorkflowStateData?> GetLatestByInstanceIdAsync(Guid instanceId);
    Task<WorkflowStateData?> GetByInstanceAndStateAsync(Guid instanceId, Guid stateId);
}