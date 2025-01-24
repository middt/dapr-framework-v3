using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowInstanceDataRepository : ICRUDRepository<WorkflowInstanceData>
{
    Task<WorkflowInstanceData?> GetByInstanceIdAsync(Guid instanceId);
} 