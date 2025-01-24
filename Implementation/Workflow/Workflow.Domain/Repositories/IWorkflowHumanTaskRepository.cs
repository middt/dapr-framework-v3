using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowHumanTaskRepository : ICRUDRepository<WorkflowHumanTask>
{
    Task<IEnumerable<WorkflowHumanTask>> GetByInstanceIdAsync(Guid instanceId);
    Task<IEnumerable<WorkflowHumanTask>> GetByAssigneeAsync(string assignee);
    Task<IEnumerable<WorkflowHumanTask>> GetByStatusAsync(string status);
    Task<IEnumerable<WorkflowHumanTask>> GetPendingTasksAsync();
    Task<IEnumerable<WorkflowHumanTask>> GetCompletedTasksAsync();
}