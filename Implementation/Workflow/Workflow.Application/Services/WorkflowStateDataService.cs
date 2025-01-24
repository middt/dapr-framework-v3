using Dapr.Framework.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;

namespace Workflow.Application.Services;

public class WorkflowStateDataService : CRUDDataService<WorkflowStateData, IWorkflowStateDataRepository>, IWorkflowStateDataService
{
    private readonly IWorkflowStateDataRepository _stateDataRepository;

    public WorkflowStateDataService(IWorkflowStateDataRepository repository) : base(repository)
    {
        _stateDataRepository = repository;
    }

    public async Task<IEnumerable<WorkflowStateData>> GetByInstanceIdAsync(Guid instanceId)
    {
        return await _stateDataRepository.GetByInstanceIdAsync(instanceId);
    }

    public async Task<IEnumerable<WorkflowStateData>> GetByStateIdAsync(Guid stateId)
    {
        return await _stateDataRepository.GetByStateIdAsync(stateId);
    }

    public async Task<WorkflowStateData?> GetLatestByInstanceIdAsync(Guid instanceId)
    {
        return await _stateDataRepository.GetLatestByInstanceIdAsync(instanceId);
    }

    public async Task<WorkflowStateData?> GetStateDataAsync(Guid instanceId, Guid stateId)
    {
        return await GetByInstanceAndStateAsync(instanceId, stateId);
    }

    public async Task<WorkflowStateData?> GetByInstanceAndStateAsync(Guid instanceId, Guid stateId)
    {
        return await _stateDataRepository.GetByInstanceAndStateAsync(instanceId, stateId);
    }
}