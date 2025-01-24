using System.Text.Json;
using Dapr.Framework.Application.Services;
using Microsoft.Extensions.Logging;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Workflow.Application.Services;

public class WorkflowFunctionService : CRUDDataService<WorkflowFunction, IWorkflowFunctionRepository>, IWorkflowFunctionService
{
    private readonly IWorkflowFunctionRepository _repository;

    private readonly IWorkflowTaskRepository _taskRepository;
    private readonly ILogger<WorkflowFunctionService> _logger;
    private readonly WorkflowTaskProcessor _taskProcessor;

    public WorkflowFunctionService(
        IWorkflowFunctionRepository repository,
         IWorkflowTaskRepository taskRepository,
        ILogger<WorkflowFunctionService> logger,
        WorkflowTaskProcessor taskProcessor) : base(repository)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        
        _logger = logger;
        _taskProcessor = taskProcessor;

    }

    public async Task<object?> ExecuteFunctionAsync(string functionName, JsonDocument? data = null)
    {
        var function = await _repository.GetByNameAsync(functionName);
        if (function == null)
            throw new KeyNotFoundException($"Function '{functionName}' not found");

        if (!function.IsActive)
            throw new InvalidOperationException($"Function '{functionName}' is not active");

        if (function.StateId.HasValue || function.WorkflowDefinitionId.HasValue)
            throw new InvalidOperationException($"Function '{functionName}' is bound to a specific state or workflow");

        try
        {
            function.Task = await _taskRepository.GetByIdAsync(function.TaskId);
            return await _taskProcessor.ExecuteTaskAsync(function.Task, data, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function {FunctionName}", functionName);
            throw;
        }
    }

    public async Task<WorkflowFunction?> GetByNameAsync(string name)
    {
        return await _repository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<WorkflowFunction>> GetActiveFunctionsAsync()
    {
        return await _repository.GetActiveFunctionsAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        return await _repository.IsNameUniqueAsync(name, excludeId);
    }
} 