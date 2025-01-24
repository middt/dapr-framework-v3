using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Workflow.Domain.Models.Views;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowStateRepository : EfCRUDRepository<WorkflowState>, IWorkflowStateRepository
{
    private new readonly WorkflowDbContext _context;

    public WorkflowStateRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public override async Task<WorkflowState?> GetByIdAsync(Guid id)
    {
        return await _context.Set<WorkflowState>()
            .Include(s => s.Tasks)
            .Include(s => s.SubflowConfig)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<WorkflowState?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.WorkflowStates
            .Include(s => s.SubflowConfig)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<WorkflowState?> GetByIdWithViewsAsync(Guid id)
    {
        return await _context.WorkflowStates
            .AsNoTracking()
            .Include(s => s.Views)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<WorkflowState>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return await _context.WorkflowStates
            .AsNoTracking()
            .Include(s => s.Views)
            .Where(s => s.WorkflowDefinitionId == definitionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowView>> GetViewsByStateIdAsync(Guid stateId)
    {
        var state = await _context.WorkflowStates
            .AsNoTracking()
            .Include(s => s.Views)
            .FirstOrDefaultAsync(s => s.Id == stateId);

        return state?.Views ?? new List<WorkflowView>();
    }
}