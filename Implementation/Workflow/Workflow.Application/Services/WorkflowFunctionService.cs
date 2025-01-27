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
    private readonly IWorkflowTaskAssignmentRepository _taskAssignmentRepository;

    public WorkflowFunctionService(
        IWorkflowFunctionRepository repository,
         IWorkflowTaskRepository taskRepository,
        ILogger<WorkflowFunctionService> logger,
        WorkflowTaskProcessor taskProcessor,
        IWorkflowTaskAssignmentRepository taskAssignmentRepository) : base(repository)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        
        _logger = logger;
        _taskProcessor = taskProcessor;
        _taskAssignmentRepository = taskAssignmentRepository;
    }

    public async Task<object?> ExecuteFunctionAsync(string functionName, JsonDocument? data = null)
    {
        var function = await _repository.GetByNameAsync(functionName)
            ?? throw new KeyNotFoundException($"Function '{functionName}' not found");

        if (!function.IsActive)
            throw new InvalidOperationException($"Function '{functionName}' is not active");

        if (function.StateId.HasValue || function.WorkflowDefinitionId.HasValue)
            throw new InvalidOperationException($"Function '{functionName}' is bound to a specific state or workflow");

        try
        {
            var taskAssignments = await _taskAssignmentRepository.GetByFunctionIdAsync(function.Id);
            var taskAssignment = taskAssignments.FirstOrDefault()
                ?? throw new InvalidOperationException($"No task assignment found for function {functionName}");

            // Set WorkflowInstanceId to null for standalone function execution
            taskAssignment.WorkflowInstanceId = null;

            return await _taskProcessor.ExecuteTaskAsync(taskAssignment.Task, data ?? JsonDocument.Parse("{}"), taskAssignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function {FunctionName}", functionName);
            throw;
        }
    }

    public async Task<object?> ExecuteFunctionAsync(Guid functionId, JsonDocument data)
    {
        var function = await _repository.GetByIdAsync(functionId)
            ?? throw new InvalidOperationException($"Function {functionId} not found");

        var taskAssignments = await _taskAssignmentRepository.GetByFunctionIdAsync(functionId);
        if (!taskAssignments.Any())
            throw new InvalidOperationException($"No task assignments found for function {functionId}");

        var results = new List<object?>();
        
        // Execute tasks in order
        foreach (var taskAssignment in taskAssignments.OrderBy(ta => ta.Order))
        {
            var result = await _taskProcessor.ExecuteTaskAsync(taskAssignment.Task, data, taskAssignment);
            if (result != null)
            {
                results.Add(result);
            }
        }

        // If no results, return null
        if (!results.Any())
            return null;

        // If single result, return it directly
        if (results.Count == 1)
            return results[0];

        // Merge multiple results
        return JsonSerializer.Deserialize<object>(
            JsonSerializer.Serialize(
                results.Select(r => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(r)))
                      .Aggregate((a, b) => JsonDocument.Parse(MergeJsonElements(a, b).ToString()).RootElement)
            )
        );
    }

    private JsonElement MergeJsonElements(JsonElement a, JsonElement b)
    {
        var merged = new Dictionary<string, JsonElement>();
        
        foreach (var property in a.EnumerateObject())
            merged[property.Name] = property.Value;
            
        foreach (var property in b.EnumerateObject())
            merged[property.Name] = property.Value;

        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(merged));
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