using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowFunctionRepository : EfCRUDRepository<WorkflowFunction>, IWorkflowFunctionRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowFunctionRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowFunction>> GetByWorkflowDefinitionIdAsync(Guid definitionId)
    {
        return _context.Set<WorkflowTask>()
            .OfType<WorkflowFunction>()
            .AsNoTracking()
            .Where(c => c.WorkflowDefinitionId == definitionId);
    }

    public async Task<WorkflowFunction?> GetByNameAsync(string name)
    {
        return await _context.Set<WorkflowFunction>()
            .FirstOrDefaultAsync(f => f.Name == name);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _context.Set<WorkflowFunction>()
            .Where(f => f.Name == name);

        if (excludeId.HasValue)
            query = query.Where(f => f.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<WorkflowFunction?> GetByStateIdAsync(Guid stateId)
    {
        return await _context.Set<WorkflowTask>()
            .OfType<WorkflowFunction>()
            .AsNoTracking()
            .Include(c => c.States)
            .FirstOrDefaultAsync(c => c.States.Any(s => s.Id == stateId));
    }

    public async Task<IEnumerable<WorkflowFunction>> GetActiveFunctionsAsync()
    {
        return await _context.Set<WorkflowFunction>()
            .Where(f => f.IsActive)
            .ToListAsync();
    }
}