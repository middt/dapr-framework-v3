using Dapr.Framework.Domain.Repositories;
using Workflow.Domain.Models;

namespace Workflow.Domain.Repositories;

public interface IWorkflowDefinitionRepository : ICRUDRepository<WorkflowDefinition>
{
    Task<WorkflowDefinition?> GetLatestVersionByNameAsync(string name);
    Task<WorkflowDefinition?> GetVersionByNameAsync(string name, string version);
    Task<IEnumerable<WorkflowDefinition>> GetAllVersionsByNameAsync(string name);
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
}