using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models.Tasks;

namespace Workflow.Domain.Repositories;

public interface IWorkflowTaskAssignmentRepository : ICRUDRepository<WorkflowTaskAssignment>
{
    Task<IEnumerable<WorkflowTaskAssignment>> GetByTaskIdAsync(Guid taskId);
    Task<IEnumerable<WorkflowTaskAssignment>> GetByStateIdAsync(Guid stateId);
    Task<IEnumerable<WorkflowTaskAssignment>> GetByTransitionIdAsync(Guid transitionId);
    Task<IEnumerable<WorkflowTaskAssignment>> GetByFunctionIdAsync(Guid functionId);
} 