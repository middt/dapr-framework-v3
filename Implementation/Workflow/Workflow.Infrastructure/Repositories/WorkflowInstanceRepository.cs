using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowInstanceRepository : EfCRUDRepository<WorkflowInstance>, IWorkflowInstanceRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowInstanceRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<WorkflowInstance?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.WorkflowInstances
            .Include(i => i.CurrentState)
            .Include(i => i.WorkflowDefinition)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetAvailableTransitionsAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances
            .Include(i => i.CurrentState)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance?.CurrentState == null)
            return Enumerable.Empty<WorkflowTransition>();

        return await _context.WorkflowTransitions
            .AsNoTracking()
            .Where(t => t.FromStateId == instance.CurrentStateId)
            .ToListAsync();
    }

    public async Task<WorkflowInstance?> UpdateAsync(WorkflowInstance instance)
    {
        var entry = _context.Entry(instance);
        if (entry.State == EntityState.Detached)
        {
            var existingInstance = await _context.WorkflowInstances.FindAsync(instance.Id);
            if (existingInstance == null)
                return null;

            _context.Entry(existingInstance).CurrentValues.SetValues(instance);
            instance = existingInstance;
        }

        entry.State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return instance;
    }

    public async Task<IEnumerable<WorkflowInstance>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.WorkflowDefinitionId == definitionId)
            .OrderByDescending(i => i.CreatedAt);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetByStateIdAsync(Guid stateId)
    {
        return _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.CurrentStateId == stateId)
            .OrderByDescending(i => i.CreatedAt);
    }

    public async Task<WorkflowStateData?> GetCurrentStateDataAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.StateData)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        return instance?.StateData
            .OrderByDescending(s => s.EnteredAt)
            .FirstOrDefault();
    }

    public async Task<IEnumerable<WorkflowStateData>> GetStateHistoryAsync(Guid instanceId)
    {
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.StateData)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        return instance?.StateData
            .OrderByDescending(s => s.EnteredAt)
            .ToList() ?? new List<WorkflowStateData>();
    }

    public async Task<WorkflowHumanTask?> GetTaskByIdAsync(Guid taskId)
    {
        return await _context.WorkflowHumanTasks.AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetTasksByInstanceAsync(Guid instanceId)
    {
        return _context.WorkflowHumanTasks.AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.WorkflowInstanceId == instanceId)
            .OrderByDescending(t => t.AssignedAt);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetTasksByAssigneeAsync(string assignee)
    {
        return _context.WorkflowHumanTasks.AsNoTracking()
            .Include(t => t.WorkflowInstance)
            .Where(t => t.Assignee == assignee)
            .OrderByDescending(t => t.AssignedAt);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetActiveInstancesAsync()
    {
        return _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.CompletedAt == null)
            .OrderByDescending(i => i.CreatedAt);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetCompletedInstancesAsync()
    {
        return _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.CompletedAt != null)
            .OrderByDescending(i => i.CreatedAt);
    }

    public async Task<WorkflowInstance?> GetLatestInstanceAsync(Guid definitionId)
    {
        return await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.WorkflowDefinitionId == definitionId)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<WorkflowInstance?> GetLatestActiveInstanceAsync(Guid definitionId)
    {
        return await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.WorkflowDefinitionId == definitionId && i.CompletedAt == null)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<WorkflowInstance?> GetLatestCompletedInstanceAsync(Guid definitionId)
    {
        return await _context.WorkflowInstances.AsNoTracking()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentState)
            .Where(i => i.WorkflowDefinitionId == definitionId && i.CompletedAt != null)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetCompatibleDefinitionsAsync(string clientVersion)
    {
        var definitions = await _context.WorkflowDefinitions
            .AsNoTracking()
            .Include(d => d.States)
            .Where(d => d.ArchivedAt == null)
            .ToListAsync();

        return definitions.Where(d => d.IsClientVersionCompatible(clientVersion));
    }

    public async Task<WorkflowInstance> AddAsync(WorkflowInstance instance)
    {
        await _context.WorkflowInstances.AddAsync(instance);
        await _context.SaveChangesAsync();
        return instance;
    }
}