using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Infrastructure.Repositories;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Data;
using Dapr.Framework.Domain.Services;

namespace Workflow.Infrastructure.Repositories;

public class WorkflowInstanceDataRepository : EfCRUDRepository<WorkflowInstanceData>, IWorkflowInstanceDataRepository
{
    private new readonly WorkflowDbContext _context;

    public WorkflowInstanceDataRepository(WorkflowDbContext context, ITransactionService transactionService)
        : base(context, transactionService)
    {
        _context = context;
    }

    public async Task<WorkflowInstanceData?> GetByInstanceIdAsync(Guid instanceId)
    {
        return await _context.WorkflowInstanceData
            .FirstOrDefaultAsync(d => d.WorkflowInstanceId == instanceId);
    }
} 