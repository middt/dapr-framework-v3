using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Workflow.Domain.Models.Views;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowTransitionRepository : EfCRUDRepository<WorkflowTransition>, IWorkflowTransitionRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowTransitionRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<WorkflowTransition?> GetByIdWithViewsAsync(Guid id)
    {
        return await _context.WorkflowTransitions
            .AsNoTracking()
            .Include(t => t.Views)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return _context.WorkflowTransitions
            .AsNoTracking()
            .Include(t => t.FromState)
            .Include(t => t.ToState)
            .Where(t => t.WorkflowDefinitionId == definitionId);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetByFromStateIdAsync(Guid stateId)
    {
        return _context.WorkflowTransitions.AsNoTracking()
            .Include(t => t.Views)
            .Include(t => t.ToState)
            .Where(t => t.FromStateId == stateId);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetByToStateIdAsync(Guid stateId)
    {
        return _context.WorkflowTransitions.AsNoTracking()
            .Include(t => t.Views)
            .Include(t => t.FromState)
            .Where(t => t.ToStateId == stateId);
    }

    public async Task<IEnumerable<WorkflowView>> GetViewsByTransitionIdAsync(Guid transitionId)
    {
        var transition = await _context.WorkflowTransitions.AsNoTracking()
            .Include(t => t.Views)
            .FirstOrDefaultAsync(t => t.Id == transitionId);

        return transition?.Views ?? new List<WorkflowView>();
    }
}