using Microsoft.EntityFrameworkCore;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Dapr.Framework.Infrastructure.Repositories;
using Dapr.Framework.Domain.Services;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowTaskAssignmentRepository : EfCRUDRepository<WorkflowTaskAssignment>, IWorkflowTaskAssignmentRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowTaskAssignmentRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowTaskAssignment>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.Set<WorkflowTaskAssignment>()
            .Include(ta => ta.Task)
            .Where(ta => ta.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowTaskAssignment>> GetByStateIdAsync(Guid stateId)
    {
        return await _context.Set<WorkflowTaskAssignment>()
            .Include(ta => ta.Task)
            .Where(ta => ta.StateId == stateId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowTaskAssignment>> GetByTransitionIdAsync(Guid transitionId)
    {
        return await _context.Set<WorkflowTaskAssignment>()
            .Include(ta => ta.Task)
            .Where(ta => ta.TransitionId == transitionId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowTaskAssignment>> GetByFunctionIdAsync(Guid functionId)
    {
        return await _context.Set<WorkflowTaskAssignment>()
            .Include(ta => ta.Task)
            .Where(ta => ta.FunctionId == functionId)
            .ToListAsync();
    }
} 