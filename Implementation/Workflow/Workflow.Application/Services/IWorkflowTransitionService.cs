using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Application.Services;

public interface IWorkflowTransitionService : ICRUDDataService<WorkflowTransition>
{
    Task<WorkflowTransition?> GetByIdWithViewsAsync(Guid id);
    Task<IEnumerable<WorkflowTransition>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowTransition>> GetByFromStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowTransition>> GetByToStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowView>> GetViewsByTransitionIdAsync(Guid transitionId);
    Task ExecuteTransitionAsync(Guid instanceId, Guid transitionId);
}