using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Domain.Repositories;

public interface IWorkflowStateRepository : ICRUDRepository<WorkflowState>
{
    Task<WorkflowState?> GetByIdWithViewsAsync(Guid id);
    Task<IEnumerable<WorkflowState>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowView>> GetViewsByStateIdAsync(Guid stateId);
}