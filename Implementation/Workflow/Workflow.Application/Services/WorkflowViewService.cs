using Dapr.Framework.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Domain.Models.Views;

namespace Workflow.Application.Services;

public class WorkflowViewService : CRUDDataService<WorkflowView, IWorkflowViewRepository>, IWorkflowViewService
{
    private readonly IWorkflowViewRepository _repository;

    public WorkflowViewService(IWorkflowViewRepository repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WorkflowView>> GetByStateIdAsync(Guid stateId)
    {
        return await _repository.GetByStateIdAsync(stateId);
    }

    public async Task<IEnumerable<WorkflowView>> GetByTransitionIdAsync(Guid transitionId)
    {
        return await _repository.GetByTransitionIdAsync(transitionId);
    }

    public async Task<IEnumerable<WorkflowView>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return await _repository.GetByDefinitionIdAsync(definitionId);
    }

    public async Task<IEnumerable<WorkflowView>> GetByVersionAsync(string version)
    {
        return await _repository.GetByVersionAsync(version);
    }

    public async Task<IEnumerable<WorkflowView>> GetByWorkflowVersionAsync(string workflowVersion)
    {
        return await _repository.GetByWorkflowVersionAsync(workflowVersion);
    }

    public async Task<WorkflowView?> GetLatestVersionAsync()
    {
        return await _repository.GetLatestVersionAsync();
    }

    public async Task<IEnumerable<WorkflowView>> GetInstanceStateViewsAsync(Guid instanceId)
    {
        return await _repository.GetInstanceStateViewsAsync(instanceId);
    }

    public async Task<IEnumerable<WorkflowView>> GetInstanceTransitionViewsAsync(Guid instanceId)
    {
        return await _repository.GetInstanceTransitionViewsAsync(instanceId);
    }

    public async Task<IEnumerable<WorkflowView>> GetInstanceAvailableViewsAsync(Guid instanceId)
    {
        return await _repository.GetInstanceAvailableViewsAsync(instanceId);
    }
}