using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowCorrelationRepository : EfCRUDRepository<WorkflowCorrelation>, IWorkflowCorrelationRepository
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<WorkflowCorrelationRepository> _logger;

    public WorkflowCorrelationRepository(WorkflowDbContext context, ITransactionService transactionService, ILogger<WorkflowCorrelationRepository> logger)
        : base(context, transactionService)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowCorrelation?> GetBySubflowInstanceIdAsync(Guid subflowInstanceId)
    {
        return await _context.WorkflowCorrelations
            .Include(c => c.ParentState)
            .Include(c => c.ParentState.SubflowConfig)
            .FirstOrDefaultAsync(c => c.SubflowInstanceId == subflowInstanceId);
    }

    public async Task<IEnumerable<WorkflowCorrelation>> GetByParentInstanceIdAsync(Guid parentInstanceId)
    {
        return await _context.WorkflowCorrelations
            .Include(c => c.ParentState)
            .Include(c => c.ParentState.SubflowConfig)
            .Include(c => c.SubflowInstance)
            .Include(c => c.SubflowInstance.CurrentState)
            .Where(c => c.ParentInstanceId == parentInstanceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowCorrelation>> GetByParentStateIdAsync(Guid parentStateId)
    {
        return await _context.WorkflowCorrelations
            .Include(c => c.ParentState)
            .Include(c => c.ParentState.SubflowConfig)
            .Include(c => c.SubflowInstance)
            .Where(c => c.ParentStateId == parentStateId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowCorrelation>> GetActiveCorrelationsAsync()
    {
        return await _context.WorkflowCorrelations
            .Include(c => c.ParentState)
            .Include(c => c.ParentState.SubflowConfig)
            .Include(c => c.SubflowInstance)
            .Where(c => c.CompletedAt == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowInstance>> GetChildrenAsync(Guid parentInstanceId)
    {
        var correlations = await _context.WorkflowCorrelations
            .Include(c => c.SubflowInstance)
            .Where(c => c.ParentInstanceId == parentInstanceId)
            .ToListAsync();

        _logger.LogInformation("Found {Count} children for parent instance {ParentId}", 
            correlations.Count, parentInstanceId);

        return correlations.Select(c => c.SubflowInstance!).ToList();
    }
} 