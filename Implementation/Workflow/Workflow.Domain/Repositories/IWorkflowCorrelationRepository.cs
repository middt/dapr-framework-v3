using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowCorrelationRepository : ICRUDRepository<WorkflowCorrelation>
{
    Task<WorkflowCorrelation?> GetBySubflowInstanceIdAsync(Guid subflowInstanceId);
    Task<IEnumerable<WorkflowCorrelation>> GetByParentInstanceIdAsync(Guid parentInstanceId);
    Task<IEnumerable<WorkflowCorrelation>> GetByParentStateIdAsync(Guid parentStateId);
    Task<IEnumerable<WorkflowCorrelation>> GetActiveCorrelationsAsync();
} 