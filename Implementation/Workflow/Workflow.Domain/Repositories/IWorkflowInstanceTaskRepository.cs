using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models.Tasks;

namespace Workflow.Domain.Repositories;

public interface IWorkflowInstanceTaskRepository : ICRUDRepository<WorkflowInstanceTask>
{
    Task<IEnumerable<WorkflowInstanceTask>> GetByInstanceIdAsync(Guid instanceId);
    Task<IEnumerable<WorkflowInstanceTask>> GetByStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowInstanceTask>> GetByTaskIdAsync(Guid taskId);
} 