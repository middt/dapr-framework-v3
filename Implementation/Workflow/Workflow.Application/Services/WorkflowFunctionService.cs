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

            return await _taskProcessor.ExecuteTaskAsync(taskAssignment.Task, data ?? JsonDocument.Parse("{}"));
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
        var taskAssignment = taskAssignments.FirstOrDefault()
            ?? throw new InvalidOperationException($"No task assignment found for function {functionId}");

        return await _taskProcessor.ExecuteTaskAsync(taskAssignment.Task, data);
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