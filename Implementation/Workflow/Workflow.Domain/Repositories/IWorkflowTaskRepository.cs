using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workflow.Domain.Models.Tasks;
using Dapr.Framework.Domain.Repositories;

namespace Workflow.Domain.Repositories
{
    public interface IWorkflowTaskRepository: ICRUDRepository<WorkflowTask>
    {
        Task<IEnumerable<WorkflowTask>> GetByStateIdAsync(Guid stateId);
        Task<WorkflowTask> AddAsync(WorkflowTask task);
        Task UpdateAsync(WorkflowTask task);

    }
} 