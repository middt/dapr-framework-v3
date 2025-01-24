using Dapr.Framework.Application.Services;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Common;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Domain.Models.Views;
using Microsoft.Extensions.Logging;

namespace Workflow.Application.Services;

public class WorkflowTransitionService : CRUDDataService<WorkflowTransition, IWorkflowTransitionRepository>, IWorkflowTransitionService
{
    private readonly IWorkflowTransitionRepository _transitionRepository;
    private readonly IWorkflowInstanceService _instanceService;
    private readonly ILogger<WorkflowTransitionService> _logger;

    public WorkflowTransitionService(
        IWorkflowTransitionRepository repository,
        IWorkflowInstanceService instanceService,
        ILogger<WorkflowTransitionService> logger) : base(repository)
    {
        _transitionRepository = repository;
        _instanceService = instanceService;
        _logger = logger;
    }

    public async Task<WorkflowTransition?> GetByIdWithViewsAsync(Guid id)
    {
        return await _transitionRepository.GetByIdWithViewsAsync(id);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return await _transitionRepository.GetByDefinitionIdAsync(definitionId);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetByFromStateIdAsync(Guid stateId)
    {
        return await _transitionRepository.GetByFromStateIdAsync(stateId);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetByToStateIdAsync(Guid stateId)
    {
        return await _transitionRepository.GetByToStateIdAsync(stateId);
    }

    public async Task<IEnumerable<WorkflowView>> GetViewsByTransitionIdAsync(Guid transitionId)
    {
        return await _transitionRepository.GetViewsByTransitionIdAsync(transitionId);
    }

    public async Task ExecuteTransitionAsync(Guid instanceId, Guid transitionId)
    {
        try
        {
            // Get the transition
            var transition = await GetByIdAsync(transitionId);
            if (transition == null)
            {
                _logger.LogWarning("Transition {TransitionId} not found", transitionId);
                return;
            }

            // Execute the transition using the instance service
            await _instanceService.ExecuteTransitionAsync(instanceId, transitionId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing transition {TransitionId} for instance {InstanceId}", 
                transitionId, instanceId);
            throw;
        }
    }

    // Implement any additional methods required by ICRUDDataService<WorkflowTransition>
    public override async Task<WorkflowTransition> CreateAsync(WorkflowTransition entity)
    {
        return await base.CreateAsync(entity);
    }

    public override async Task<WorkflowTransition> UpdateAsync(Guid id, WorkflowTransition entity)
    {
        return await base.UpdateAsync(id, entity);
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        await base.DeleteAsync(id);
        return true;
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await base.ExistsAsync(id);
    }

    public override async Task<IEnumerable<WorkflowTransition>> GetAllAsync()
    {
        return await base.GetAllAsync();
    }

    public override async Task<PagedList<WorkflowTransition>> GetPagedAsync(PaginationParameters parameters)
    {
        return await base.GetPagedAsync(parameters);
    }

    public override async Task<WorkflowTransition> GetByIdAsync(Guid id)
    {
        return await base.GetByIdAsync(id);
    }
}