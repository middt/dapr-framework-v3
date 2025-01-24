using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Application.Services;

public interface IWorkflowViewService : ICRUDDataService<WorkflowView>
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