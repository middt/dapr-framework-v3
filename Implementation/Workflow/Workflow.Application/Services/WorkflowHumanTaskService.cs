using Dapr.Framework.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;

namespace Workflow.Application.Services;

public class WorkflowHumanTaskService : CRUDDataService<WorkflowHumanTask, IWorkflowHumanTaskRepository>, IWorkflowHumanTaskService
{
    private readonly IWorkflowHumanTaskRepository _repository;

    public WorkflowHumanTaskService(IWorkflowHumanTaskRepository repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetByInstanceIdAsync(Guid instanceId)
    {
        return await _repository.GetByInstanceIdAsync(instanceId);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetByAssigneeAsync(string assignee)
    {
        return await _repository.GetByAssigneeAsync(assignee);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetByStatusAsync(string status)
    {
        return await _repository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetPendingTasksAsync()
    {
        return await _repository.GetPendingTasksAsync();
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetCompletedTasksAsync()
    {
        return await _repository.GetCompletedTasksAsync();
    }
}