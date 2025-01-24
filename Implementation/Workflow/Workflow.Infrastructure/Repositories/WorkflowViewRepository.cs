using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using System.Text.RegularExpressions;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Workflow.Domain.Models.Views;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowViewRepository : EfCRUDRepository<WorkflowView>, IWorkflowViewRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowViewRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowView>> GetByStateIdAsync(Guid stateId)
    {
        return _context.WorkflowViews
            .AsNoTracking()
            .Include(v => v.State)
            .Where(v => v.StateId == stateId);
    }

    public async Task<IEnumerable<WorkflowView>> GetByTransitionIdAsync(Guid transitionId)
    {
        return _context.WorkflowViews.AsNoTracking()
            .Include(v => v.Transition)
            .Where(v => v.TransitionId == transitionId);
    }

    public async Task<IEnumerable<WorkflowView>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return _context.WorkflowViews.AsNoTracking()
            .Include(v => v.State)
            .Include(v => v.Transition)
            .Where(v => v.WorkflowDefinitionId == definitionId);
    }

    public async Task<IEnumerable<WorkflowView>> GetByVersionAsync(string version)
    {
        return _context.WorkflowViews.AsNoTracking()
            .Where(v => v.Version == version);
    }

    public async Task<IEnumerable<WorkflowView>> GetByWorkflowVersionAsync(string workflowVersion)
    {
        var allViews = _context.WorkflowViews.AsNoTracking();
        return allViews.Where(v => v.IsCompatibleWithWorkflowVersion(workflowVersion));
    }

    public async Task<WorkflowView?> GetLatestVersionAsync()
    {
        return await _context.WorkflowViews.AsNoTracking()
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WorkflowView>> GetInstanceStateViewsAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.CurrentState)
                .ThenInclude(s => s!.Views)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        return instance?.CurrentState?.Views ?? new List<WorkflowView>();
    }

    public async Task<IEnumerable<WorkflowView>> GetInstanceTransitionViewsAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.CurrentState)
            .Include(i => i.WorkflowDefinition)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            return Enumerable.Empty<WorkflowView>();

        var transitions = await _context.WorkflowTransitions.AsNoTracking()
            .Where(t => t.FromStateId == instance.CurrentStateId)
            .Select(t => t.Id)
            .ToListAsync();

        var allViews = await _context.WorkflowViews
            .Include(v => v.Transition)
            .Where(v => v.TransitionId.HasValue && transitions.Contains(v.TransitionId.Value))
             .ToListAsync();

        return allViews.Where(v => v.IsCompatibleWithWorkflowVersion(instance.WorkflowDefinition.Version));
    }

    public async Task<IEnumerable<WorkflowView>> GetInstanceAvailableViewsAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.CurrentState)
            .Include(i => i.WorkflowDefinition)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
            return Enumerable.Empty<WorkflowView>();

        var transitions = await _context.WorkflowTransitions.AsNoTracking()
            .Where(t => t.FromStateId == instance.CurrentStateId)
            .Select(t => t.Id)
            .ToListAsync();

        var allViews = await _context.WorkflowViews
            .Include(v => v.State)
            .Include(v => v.Transition)
            .Where(v => v.StateId == instance.CurrentStateId ||
                       (v.TransitionId.HasValue && transitions.Contains(v.TransitionId.Value)))
            .ToListAsync();

        return allViews.Where(v => v.IsCompatibleWithWorkflowVersion(instance.WorkflowDefinition.Version));
    }
}