using Dapr.Framework.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Domain.Models.Views;

namespace Workflow.Application.Services;

public class WorkflowStateService : CRUDDataService<WorkflowState, IWorkflowStateRepository>, IWorkflowStateService
{
    private readonly IWorkflowStateRepository _repository;
    private readonly IWorkflowFunctionRepository _functionRepository;

    public WorkflowStateService(IWorkflowStateRepository repository, IWorkflowFunctionRepository functionRepository) : base(repository)
    {
        _repository = repository;
        _functionRepository = functionRepository;
    }

    public async Task<WorkflowState?> GetByIdWithViewsAsync(Guid id)
    {
        return await _repository.GetByIdWithViewsAsync(id);
    }

    public async Task<IEnumerable<WorkflowState>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return await _repository.GetByDefinitionIdAsync(definitionId);
    }

    public async Task<IEnumerable<WorkflowView>> GetViewsByStateIdAsync(Guid stateId)
    {
        return await _repository.GetViewsByStateIdAsync(stateId);
    }
}