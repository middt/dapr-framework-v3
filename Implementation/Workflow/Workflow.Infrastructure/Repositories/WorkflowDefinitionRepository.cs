using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowDefinitionRepository : EfCRUDRepository<WorkflowDefinition>, IWorkflowDefinitionRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowDefinitionRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<WorkflowDefinition?> GetLatestVersionByNameAsync(string name)
    {
        return await _context
            .WorkflowDefinitions
            .AsNoTracking()
            .Include(w => w.States)
            .Include(w => w.Transitions)
            .Where(w => w.Name == name)
            .OrderByDescending(w => w.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<WorkflowDefinition?> GetVersionByNameAsync(string name, string version)
    {
        return await _context
            .WorkflowDefinitions
            .AsNoTracking()
            .Include(w => w.States)
            .Include(w => w.Transitions)
            .FirstOrDefaultAsync(w => w.Name == name && w.Version == version);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAllVersionsByNameAsync(string name)
    {
        return _context
            .WorkflowDefinitions
            .AsNoTracking()
            .Include(w => w.States)
            .Include(w => w.Transitions)
            .Where(w => w.Name == name)
            .OrderByDescending(w => w.Version);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _context.WorkflowDefinitions.AsNoTracking().Where(w => w.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }
}