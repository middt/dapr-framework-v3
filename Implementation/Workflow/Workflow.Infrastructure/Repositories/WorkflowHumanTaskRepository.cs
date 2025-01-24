using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowHumanTaskRepository : EfCRUDRepository<WorkflowHumanTask>, IWorkflowHumanTaskRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowHumanTaskRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetByInstanceIdAsync(Guid instanceId)
    {
        return _context.WorkflowHumanTasks
            .AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.WorkflowInstanceId == instanceId)
            .OrderByDescending(t => t.AssignedAt);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetByAssigneeAsync(string assignee)
    {
        return _context.WorkflowHumanTasks
            .AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.Assignee == assignee)
            .OrderByDescending(t => t.AssignedAt);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetByStatusAsync(string status)
    {
        return _context.WorkflowHumanTasks
            .AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.Result == status)
            .OrderByDescending(t => t.AssignedAt);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetPendingTasksAsync()
    {
        return _context.WorkflowHumanTasks
            .AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.CompletedAt == null)
            .OrderByDescending(t => t.AssignedAt);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetCompletedTasksAsync()
    {
        return _context.WorkflowHumanTasks
            .AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.CompletedAt != null)
            .OrderByDescending(t => t.AssignedAt);
    }
}