using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workflow.Domain.Models.Tasks;
using Workflow.Infrastructure.Data;
using Workflow.Domain.Repositories;
using Dapr.Framework.Infrastructure.Repositories;
using Dapr.Framework.Domain.Services;
public class WorkflowTaskRepository : EfCRUDRepository<WorkflowTask>, IWorkflowTaskRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowTaskRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowTask>> GetByStateIdAsync(Guid stateId)
    {
        return await _context.Set<WorkflowTask>()
            .AsNoTracking()
            .Where(t => t.WorkflowStateId == stateId)
            .ToListAsync();
    }

    public override async Task<WorkflowTask?> GetByIdAsync(Guid id)
    {
        // Load the base task first to determine type
        var baseTask = await _context.Set<WorkflowTask>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (baseTask == null)
            return null;

        // Then load the specific type with its properties
        return baseTask.Type switch
        {
            Workflow.Domain.Models.Tasks.TaskType.Human => await _context.Set<HumanTask>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id),
            Workflow.Domain.Models.Tasks.TaskType.DaprBinding => await _context.Set<DaprBindingTask>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id),
            Workflow.Domain.Models.Tasks.TaskType.DaprPubSub => await _context.Set<DaprPubSubTask>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id),
            Workflow.Domain.Models.Tasks.TaskType.DaprService => await _context.Set<DaprServiceTask>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id),
            Workflow.Domain.Models.Tasks.TaskType.Http => await _context.Set<HttpTask>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id),
            _ => baseTask
        };
    }

    public override async Task<IEnumerable<WorkflowTask>> GetAllAsync()
    {
        var tasks = new List<WorkflowTask>();
        
        tasks.AddRange(await _context.Set<HumanTask>().AsNoTracking().ToListAsync());
        tasks.AddRange(await _context.Set<DaprBindingTask>().AsNoTracking().ToListAsync());
        tasks.AddRange(await _context.Set<DaprPubSubTask>().AsNoTracking().ToListAsync());
        tasks.AddRange(await _context.Set<DaprServiceTask>().AsNoTracking().ToListAsync());
        tasks.AddRange(await _context.Set<HttpTask>().AsNoTracking().ToListAsync());

        return tasks;
    }

    public async Task<WorkflowTask> AddAsync(WorkflowTask task)
    {
        await _context.Set<WorkflowTask>().AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(WorkflowTask task)
    {
        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await GetByIdAsync(id);
        if (task != null)
        {
            _context.Set<WorkflowTask>().Remove(task);
            await _context.SaveChangesAsync();
        }
    }
} 