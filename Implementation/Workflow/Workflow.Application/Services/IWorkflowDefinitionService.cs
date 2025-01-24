using Dapr.Framework.Domain.Services;
using Workflow.Domain.Models;

namespace Workflow.Application.Services;

public interface IWorkflowDefinitionService : ICRUDDataService<WorkflowDefinition>
{
    Task<WorkflowDefinition?> GetLatestVersionByNameAsync(string name);
    Task<WorkflowDefinition?> GetVersionByNameAsync(string name, string version);
    Task<IEnumerable<WorkflowDefinition>> GetAllVersionsByNameAsync(string name);
    Task<bool> IsNameUniqueAsync(string name, string? excludeId = null);
    Task<WorkflowDefinition> CreateNewVersionAsync(Guid id);
    Task<WorkflowDefinition> CloneAsync(Guid id, string newName);
}