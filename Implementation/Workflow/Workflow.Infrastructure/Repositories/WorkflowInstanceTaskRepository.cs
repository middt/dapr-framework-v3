using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Dapr.Framework.Domain.Services;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowInstanceTaskRepository : EfCRUDRepository<WorkflowInstanceTask>, IWorkflowInstanceTaskRepository
{
    private new readonly WorkflowDbContext _context;

    public WorkflowInstanceTaskRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowInstanceTask>> GetByInstanceIdAsync(Guid instanceId)
    {
        return await _context.WorkflowInstanceTasks
            .Where(t => t.WorkflowInstanceId == instanceId)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowInstanceTask>> GetByStateIdAsync(Guid stateId)
    {
        return await _context.WorkflowInstanceTasks
            .Where(t => t.StateId == stateId)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowInstanceTask>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.WorkflowInstanceTasks
            .Where(t => t.WorkflowTaskId == taskId)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }
} 