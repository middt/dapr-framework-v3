using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Application.Services;

public interface IWorkflowStateService : ICRUDDataService<WorkflowState>
{
    Task<WorkflowState?> GetByIdWithViewsAsync(Guid id);
    Task<IEnumerable<WorkflowState>> GetByDefinitionIdAsync(Guid definitionId);
    Task<IEnumerable<WorkflowView>> GetViewsByStateIdAsync(Guid stateId);
}