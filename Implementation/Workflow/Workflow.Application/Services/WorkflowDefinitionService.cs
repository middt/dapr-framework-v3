using Dapr.Framework.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;

namespace Workflow.Application.Services;

public class WorkflowDefinitionService : CRUDDataService<WorkflowDefinition, IWorkflowDefinitionRepository>, IWorkflowDefinitionService
{
    private readonly IWorkflowDefinitionRepository _repository;

    public WorkflowDefinitionService(IWorkflowDefinitionRepository repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<WorkflowDefinition?> GetLatestVersionByNameAsync(string name)
    {
        return await _repository.GetLatestVersionByNameAsync(name);
    }

    public async Task<WorkflowDefinition?> GetVersionByNameAsync(string name, string version)
    {
        return await _repository.GetVersionByNameAsync(name, version);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAllVersionsByNameAsync(string name)
    {
        return await _repository.GetAllVersionsByNameAsync(name);
    }

    public async Task<bool> IsNameUniqueAsync(string name, string? excludeId = null)
    {
        return await _repository.IsNameUniqueAsync(name, excludeId != null ? new Guid(excludeId) : null);
    }

    public async Task<WorkflowDefinition> CreateNewVersionAsync(Guid id)
    {
        var currentVersion = await GetByIdAsync(id) ?? throw new InvalidOperationException("Workflow definition not found");
        var newVersion = new WorkflowDefinition
        {
            Name = currentVersion.Name,
            Description = currentVersion.Description,
            Version = IncrementVersion(currentVersion.Version),
            CreatedAt = DateTime.UtcNow
        };

        return await CreateAsync(newVersion);
    }

    public async Task<WorkflowDefinition> CloneAsync(Guid id, string newName)
    {
        var source = await GetByIdAsync(id) ?? throw new InvalidOperationException("Workflow definition not found");
        var clone = new WorkflowDefinition
        {
            Name = newName,
            Description = source.Description,
            Version = "1.0.0+0",
            CreatedAt = DateTime.UtcNow
        };

        return await CreateAsync(clone);
    }

    private static string IncrementVersion(string version)
    {
        var parts = version.Split('+');
        if (parts.Length != 2)
            throw new InvalidOperationException("Invalid version format");

        var semanticVersion = parts[0];
        var build = int.Parse(parts[1]);

        return $"{semanticVersion}+{build + 1}";
    }
}