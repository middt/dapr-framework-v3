using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Dapr.Framework.Domain.Services;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowInstanceTaskRepository : EfCRUDRepository<WorkflowInstanceTask>, IWorkflowInstanceTaskRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowInstanceTaskRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowInstanceTask>> GetByInstanceIdAsync(Guid instanceId)
    {
        return await _context.Set<WorkflowInstanceTask>()
            .Where(t => t.WorkflowInstanceId == instanceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowInstanceTask>> GetByStateIdAsync(Guid stateId)
    {
        return await _context.Set<WorkflowInstanceTask>()
            .Where(t => t.StateId == stateId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowInstanceTask>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.Set<WorkflowInstanceTask>()
            .Where(t => t.WorkflowTaskId == taskId)
            .ToListAsync();
    }

    public async Task<WorkflowInstanceTask> CreateAsync(WorkflowInstanceTask task)
    {
        await _context.Set<WorkflowInstanceTask>().AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }
} 