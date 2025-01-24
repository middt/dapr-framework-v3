using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models.Views;

namespace Workflow.Domain.Repositories;

public interface IWorkflowViewRepository : ICRUDRepository<WorkflowView>
{
    Task<IEnumerable<WorkflowView>> GetByStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowView>> GetByTransitionIdAsync(Guid transitionId);
    Task<IEnumerable<WorkflowView>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowView>> GetByVersionAsync(string version);
    Task<IEnumerable<WorkflowView>> GetByWorkflowVersionAsync(string workflowVersion);
    Task<WorkflowView?> GetLatestVersionAsync();
    Task<IEnumerable<WorkflowView>> GetInstanceStateViewsAsync(Guid instanceId);
    Task<IEnumerable<WorkflowView>> GetInstanceTransitionViewsAsync(Guid instanceId);
    Task<IEnumerable<WorkflowView>> GetInstanceAvailableViewsAsync(Guid instanceId);
}