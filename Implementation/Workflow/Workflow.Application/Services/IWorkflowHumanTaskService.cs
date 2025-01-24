using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;

namespace Workflow.Application.Services;

public interface IWorkflowHumanTaskService : ICRUDDataService<WorkflowHumanTask>
{
    Task<IEnumerable<WorkflowHumanTask>> GetByInstanceIdAsync(Guid instanceId);
    Task<IEnumerable<WorkflowHumanTask>> GetByAssigneeAsync(string assignee);
    Task<IEnumerable<WorkflowHumanTask>> GetByStatusAsync(string status);
    Task<IEnumerable<WorkflowHumanTask>> GetPendingTasksAsync();
    Task<IEnumerable<WorkflowHumanTask>> GetCompletedTasksAsync();
}