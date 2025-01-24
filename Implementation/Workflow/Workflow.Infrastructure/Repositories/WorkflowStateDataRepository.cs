using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Workflow.Domain.Models.Views;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowStateDataRepository : EfCRUDRepository<WorkflowStateData>, IWorkflowStateDataRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowStateDataRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowStateData>> GetByInstanceIdAsync(Guid instanceId)
    {
        return _context.WorkflowStateData
            .AsNoTracking()
            .Include(s => s.WorkflowInstance)
            .Where(s => s.WorkflowInstanceId == instanceId)
            .OrderByDescending(s => s.EnteredAt);
    }

    public async Task<IEnumerable<WorkflowStateData>> GetByStateIdAsync(Guid stateId)
    {
        return _context.WorkflowStateData
            .AsNoTracking()
            .Include(s => s.WorkflowInstance)
            .Where(s => s.StateId == stateId)
            .OrderByDescending(s => s.EnteredAt);
    }

    public async Task<WorkflowStateData?> GetLatestByInstanceIdAsync(Guid instanceId)
    {
        return await _context.WorkflowStateData
            .AsNoTracking()
            .Include(s => s.WorkflowInstance)
            .Where(s => s.WorkflowInstanceId == instanceId)
            .OrderByDescending(s => s.EnteredAt)
            .FirstOrDefaultAsync();
    }

    public async Task<WorkflowStateData?> GetByInstanceAndStateAsync(Guid instanceId, Guid stateId)
    {
        return await _context.WorkflowStateData
            .AsNoTracking()
            .Include(s => s.WorkflowInstance)
            .Where(s => s.WorkflowInstanceId == instanceId && s.StateId == stateId)
            .OrderByDescending(s => s.EnteredAt)
            .FirstOrDefaultAsync();
    }
}