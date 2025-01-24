using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models.Views;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowTransitionRepository : ICRUDRepository<WorkflowTransition>
{
    Task<WorkflowTransition?> GetByIdWithViewsAsync(Guid id);
    Task<IEnumerable<WorkflowTransition>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowTransition>> GetByFromStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowTransition>> GetByToStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowView>> GetViewsByTransitionIdAsync(Guid transitionId);
}