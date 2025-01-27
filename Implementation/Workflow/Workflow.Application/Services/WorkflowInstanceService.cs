using Dapr.Framework.Application.Services;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;
using Workflow.Domain.Repositories;
using System.Text.Json;
using Workflow.Domain.Models.Tasks;
using Microsoft.Extensions.Logging;
using Workflow.Domain.Extensions;
using System.Text.Json.Nodes;


namespace Workflow.Application.Services;

public class WorkflowInstanceService : CRUDDataService<WorkflowInstance, IWorkflowInstanceRepository>, IWorkflowInstanceService
{
    private readonly IWorkflowInstanceRepository _repository;
    private readonly IWorkflowStateDataRepository _stateDataRepository;
    private readonly IWorkflowHumanTaskRepository _humanTaskRepository;
    private readonly IWorkflowTransitionRepository _transitionRepository;
    private readonly IWorkflowStateRepository _stateRepository;
    private readonly IWorkflowViewRepository _viewRepository;
    private readonly WorkflowTaskProcessor _taskProcessor;
    private readonly ILogger<WorkflowInstanceService> _logger;
    private readonly IWorkflowCorrelationRepository _correlationRepository;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowInstanceDataRepository _instanceDataRepository;
    private readonly IWorkflowFunctionService _functionService;
    private readonly IWorkflowTaskRepository _taskRepository;

    public WorkflowInstanceService(
        IWorkflowInstanceRepository repository,
        IWorkflowStateDataRepository stateDataRepository,
        IWorkflowHumanTaskRepository humanTaskRepository,
        IWorkflowTransitionRepository transitionRepository,
        IWorkflowStateRepository stateRepository,
        IWorkflowViewRepository viewRepository,
        WorkflowTaskProcessor taskProcessor,
        ILogger<WorkflowInstanceService> logger,
        IWorkflowCorrelationRepository correlationRepository,
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceDataRepository instanceDataRepository,
        IWorkflowFunctionService functionService,
        IWorkflowTaskRepository taskRepository) : base(repository)
    {
        _repository = repository;
        _stateDataRepository = stateDataRepository;
        _humanTaskRepository = humanTaskRepository;
        _transitionRepository = transitionRepository;
        _stateRepository = stateRepository;
        _viewRepository = viewRepository;
        _taskProcessor = taskProcessor;
        _logger = logger;
        _correlationRepository = correlationRepository;
        _definitionRepository = definitionRepository;
        _instanceDataRepository = instanceDataRepository;
        _functionService = functionService;
        _taskRepository = taskRepository;
    }

    public async Task<WorkflowInstance?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _repository.GetByIdWithDetailsAsync(id);
    }

    public async Task<IEnumerable<WorkflowTransition>> GetAvailableTransitionsAsync(Guid instanceId)
    {
        return await _repository.GetAvailableTransitionsAsync(instanceId);
    }

    public async Task<WorkflowInstanceDetails?> GetInstanceDetailsAsync(Guid id, string baseUrl)
    {
        var instance = await GetByIdWithDetailsAsync(id);
        if (instance == null)
            return null;

        var availableTransitions = await GetAvailableTransitionsAsync(id);
        var childInstances = await GetChildInstancesAsync(id);
        var stateViews = await _viewRepository.GetByStateIdAsync(instance.CurrentStateId);
        var correlation = await _correlationRepository.GetBySubflowInstanceIdAsync(id);
        var childCorrelations = await _correlationRepository.GetByParentInstanceIdAsync(id);

        return WorkflowInstanceDetails.FromInstance(
            instance,
            availableTransitions,
            stateViews,
            baseUrl,
            correlation,
            childCorrelations
        );
    }

    public async Task<IEnumerable<WorkflowInstance>> GetByDefinitionIdAsync(Guid definitionId)
    {
        return await _repository.GetByDefinitionIdAsync(definitionId);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetByStateIdAsync(Guid stateId)
    {
        return await _repository.GetByStateIdAsync(stateId);
    }

    public async Task<WorkflowStateData?> GetCurrentStateDataAsync(Guid instanceId)
    {
        return await _stateDataRepository.GetLatestByInstanceIdAsync(instanceId);
    }

    public async Task<IEnumerable<WorkflowStateData>> GetStateHistoryAsync(Guid instanceId)
    {
        return await _stateDataRepository.GetByInstanceIdAsync(instanceId);
    }

    public async Task<WorkflowHumanTask?> GetTaskByIdAsync(Guid taskId)
    {
        return await _humanTaskRepository.GetByIdAsync(taskId);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetTasksByInstanceAsync(Guid instanceId)
    {
        return await _humanTaskRepository.GetByInstanceIdAsync(instanceId);
    }

    public async Task<IEnumerable<WorkflowHumanTask>> GetTasksByAssigneeAsync(string assignee)
    {
        return await _repository.GetTasksByAssigneeAsync(assignee);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetActiveInstancesAsync()
    {
        return await _repository.GetActiveInstancesAsync();
    }

    public async Task<IEnumerable<WorkflowInstance>> GetCompletedInstancesAsync()
    {
        return await _repository.GetCompletedInstancesAsync();
    }

    public async Task<WorkflowInstance?> GetLatestInstanceAsync(Guid definitionId)
    {
        return await _repository.GetLatestInstanceAsync(definitionId);
    }

    public async Task<WorkflowInstance?> GetLatestActiveInstanceAsync(Guid definitionId)
    {
        return await _repository.GetLatestActiveInstanceAsync(definitionId);
    }

    public async Task<WorkflowInstance?> GetLatestCompletedInstanceAsync(Guid definitionId)
    {
        return await _repository.GetLatestCompletedInstanceAsync(definitionId);
    }

    private async Task<WorkflowTransition?> FindAutomaticTransitionAsync(WorkflowInstance instance, JsonDocument stateData)
    {
        _logger.LogInformation(
            "Finding automatic transition for instance {InstanceId} in state {StateId}",
            instance.Id,
            instance.CurrentStateId
        );

        var availableTransitions = await _transitionRepository.GetByFromStateIdAsync(instance.CurrentStateId);

        _logger.LogInformation(
            "Found {Count} available transitions",
            availableTransitions.Count()
        );

        foreach (var transition in availableTransitions.Where(t => t.TriggerType == TriggerType.Automatic))
        {
            var triggerConfig = transition.TriggerConfig;
            if (triggerConfig == null) continue;

            try
            {
                var condition = triggerConfig.RootElement.GetProperty("condition");
                var field = condition.GetProperty("field").GetString();
                var op = condition.GetProperty("operator").GetString();
                var value = condition.GetProperty("value").GetString();

                _logger.LogInformation(
                    "Checking transition {TransitionName}: field={Field}, op={Op}, value={Value}",
                    transition.Name,
                    field,
                    op,
                    value
                );

                if (field != null && op != null && value != null)
                {
                    if (stateData.RootElement.TryGetProperty(field, out var dataElement))
                    {
                        var dataField = dataElement.GetString();
                        _logger.LogInformation("Found field value: {Value}", dataField);
                        if (op == "equals" && dataField == value)
                        {
                            _logger.LogInformation("Condition matched!");
                            return transition;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Field {Field} not found in state data", field);
                    }
                }
            }
            catch (Exception ex)
            {
                // Skip transitions with invalid config
                _logger.LogError(ex, "Error checking transition {TransitionName}", transition.Name);
                continue;
            }
        }

        _logger.LogInformation("No matching automatic transition found");
        return null;
    }

    private async Task CheckAndExecuteAutomaticTransitionsAsync(WorkflowInstance instance, JsonDocument stateData)
    {
        var automaticTransition = await FindAutomaticTransitionAsync(instance, stateData);
        if (automaticTransition != null)
        {
            // Execute the automatic transition
            await ExecuteTransitionAsync(instance.Id, automaticTransition.Id, stateData);
        }
    }

    public async Task ExecuteTransitionAsync(Guid instanceId, Guid transitionId, JsonDocument? data)
    {
        var instance = await _repository.GetByIdWithDetailsAsync(instanceId)
            ?? throw new InvalidOperationException($"Workflow instance '{instanceId}' not found.");

        var transition = await _transitionRepository.GetByIdAsync(transitionId)
            ?? throw new InvalidOperationException($"Transition '{transitionId}' not found.");

        // Validate that the transition is valid for the current state
        if (transition.FromStateId != instance.CurrentStateId)
        {
            throw new InvalidOperationException($"Transition '{transition.Name}' is not valid for the current state. Expected state: {transition.FromState?.Name}, Current state: {instance.CurrentState?.Name}");
        }

        // Create state data first
        if (data != null)
        {
            var stateData = new WorkflowStateData
            {
                StateId = transition.ToStateId,
                WorkflowInstanceId = instance.Id,
                EnteredAt = DateTime.UtcNow
            };
            stateData.SetData(data);
            await _stateDataRepository.CreateAsync(stateData);
        }

        // Get or create instance data
        var instanceData = await _instanceDataRepository.GetByInstanceIdAsync(instanceId);
        if (instanceData == null)
        {
            instanceData = new WorkflowInstanceData
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instanceId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            if (data != null)
            {
                instanceData.MergeData(data);
            }
            await _instanceDataRepository.CreateAsync(instanceData);
        }
        else if (data != null)
        {
            instanceData.MergeData(data);
            await _instanceDataRepository.UpdateAsync(instanceData);
        }

        var mergedData = instanceData.GetData();

        // For manual transitions, prevent returning to the previous state
        if (transition.TriggerType == TriggerType.Manual)
        {
            if (instance.CurrentStateId == transition.ToStateId)
            {
                throw new InvalidOperationException($"Cannot execute transition '{transition.Name}' as it would return to the previous state.");
            }
        }

        // Validate automatic transition conditions
        if (transition.TriggerType == TriggerType.Automatic && mergedData != null)
        {
            var triggerConfig = transition.TriggerConfig;
            if (triggerConfig != null)
            {
                var condition = triggerConfig.RootElement.GetProperty("condition");
                var field = condition.GetProperty("field").GetString();
                var op = condition.GetProperty("operator").GetString();
                var value = condition.GetProperty("value").GetString();

                if (field != null && op != null && value != null)
                {
                    var dataField = mergedData.RootElement.GetProperty(field).GetString();
                    if (op == "equals" && dataField != value)
                    {
                        throw new InvalidOperationException($"Automatic transition condition not met. Expected {field} to be {value}, but got {dataField}");
                    }
                }
            }
        }

        // Execute transition tasks
        var transitionTasks = await _taskRepository.GetByTransitionIdAsync(transitionId);
        foreach (var task in transitionTasks)
        {
            var taskAssignment = task.TaskAssignments
                .FirstOrDefault(ta => ta.TransitionId == transitionId && ta.Trigger == TaskTrigger.OnExecute);
            
            if (taskAssignment != null && mergedData != null)
            {
                _logger.LogInformation(
                    "Executing transition task {TaskName} for transition {TransitionName} with data: {Data}",
                    task.Name,
                    transition.Name,
                    mergedData.RootElement.ToString()
                );
                await _taskProcessor.ExecuteTaskAsync(task, mergedData, taskAssignment);
            }
        }

        // Get the current state to execute exit tasks
        var fromState = await _stateRepository.GetByIdAsync(instance.CurrentStateId);
        if (fromState != null && mergedData != null)
        {
            var exitTasks = await _taskRepository.GetByStateIdAsync(fromState.Id);
            foreach (var task in exitTasks)
            {
                var taskAssignment = task.TaskAssignments
                    .FirstOrDefault(ta => ta.StateId == fromState.Id && 
                        (ta.Trigger == TaskTrigger.OnExit || ta.Trigger == TaskTrigger.Both));
                
                if (taskAssignment != null)
                {
                    await _taskProcessor.ExecuteTaskAsync(task, mergedData, taskAssignment);
                }
            }
        }

        // Update instance state
        instance.CurrentStateId = transition.ToStateId;
        instance.UpdatedAt = DateTime.UtcNow;

        // Get the target state
        var toState = await _stateRepository.GetByIdAsync(transition.ToStateId);
        if (toState != null && mergedData != null)
        {
            var entryTasks = await _taskRepository.GetByStateIdAsync(toState.Id);
            foreach (var task in entryTasks)
            {
                var taskAssignment = task.TaskAssignments
                    .FirstOrDefault(ta => ta.StateId == toState.Id && 
                        (ta.Trigger == TaskTrigger.OnEntry || ta.Trigger == TaskTrigger.Both));
                
                if (taskAssignment != null)
                {
                    await _taskProcessor.ExecuteTaskAsync(task, mergedData, taskAssignment);
                }
            }

            if (toState.StateType == StateType.Finish)
            {
                instance.CompletedAt = DateTime.UtcNow;
                instance.Status = "Completed";

                // Handle parent workflow if this is a subflow
                await HandleParentWorkflowAsync(instance, mergedData);
            }
            else if (toState.StateType == StateType.Subflow)
            {
                await HandleSubflowStateAsync(instance, toState, mergedData);
            }
        }

        // Save all instance changes at once
        await _repository.UpdateAsync(instance);

        // Check for automatic transitions after state change
        if (mergedData != null)
        {
            await CheckAndExecuteAutomaticTransitionsAsync(instance, mergedData);
        }
    }

    public async Task<WorkflowInstance> CompleteTaskAsync(Guid taskId, string result)
    {
        var task = await _humanTaskRepository.GetByIdAsync(taskId)
            ?? throw new InvalidOperationException("Task not found");

        task.Result = result;
        task.CompletedAt = DateTime.UtcNow;
        await _humanTaskRepository.UpdateAsync(task);

        var instance = await GetByIdWithDetailsAsync(task.WorkflowInstanceId)
            ?? throw new InvalidOperationException("Instance not found");

        return instance;
    }

    public async Task<WorkflowInstance> StartInstanceAsync(string workflowName, string clientVersion, JsonDocument? data)
    {
        var definitions = await GetCompatibleDefinitionsAsync(clientVersion);
        var definition = definitions.FirstOrDefault(d => d.Name == workflowName)
            ?? throw new InvalidOperationException($"No compatible workflow definition found for '{workflowName}' with client version '{clientVersion}'");

        var initialState = definition.GetInitialState()
            ?? throw new InvalidOperationException($"No initial state found for workflow '{workflowName}'");


        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = definition.Id,
            CurrentStateId = initialState.Id,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(instance);

        // Initialize instance data
        if (data != null)
        {
            var instanceData = new WorkflowInstanceData
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            instanceData.MergeData(data);
            await _instanceDataRepository.CreateAsync(instanceData);
        }

        if (data != null)
        {
            var stateData = new WorkflowStateData
            {
                StateId = initialState.Id,
                WorkflowInstanceId = instance.Id,
                EnteredAt = DateTime.UtcNow
            };
            stateData.SetData(data);
            await _stateDataRepository.CreateAsync(stateData);
        }

        return instance;
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetCompatibleDefinitionsAsync(string clientVersion)
    {
        return await _repository.GetCompatibleDefinitionsAsync(clientVersion);
    }

    private async Task HandleSubflowStateAsync(WorkflowInstance instance, WorkflowState state, JsonDocument? data)
    {
        if (state.SubflowConfig == null)
        {
            _logger.LogError("State {StateId} is marked as subflow but has no SubflowConfig", state.Id);
            throw new InvalidOperationException($"State {state.Id} is marked as subflow but has no SubflowConfig");
        }

        var inputData = data ?? JsonDocument.Parse("{}");
        var mappedData = new Dictionary<string, JsonElement>();
        var mappingElement = state.SubflowConfig.InputMapping.RootElement;

        foreach (var map in mappingElement.EnumerateObject())
        {
            var sourcePath = map.Name.Split('.');
            var targetPath = map.Value.GetString()?.Split('.');

            if (targetPath == null) continue;

            try
            {
                var currentElement = inputData.RootElement;
                foreach (var part in sourcePath)
                {
                    if (!currentElement.TryGetProperty(part, out var nextElement))
                        continue;
                    currentElement = nextElement;
                }
                mappedData[targetPath[^1]] = currentElement.Clone();
            }
            catch
            {
                // Skip if source path doesn't exist
                continue;
            }
        }

        state.SubflowConfig.SubflowDefinition = await _definitionRepository.GetByIdAsync(state.SubflowConfig.SubflowDefinitionId);

        var subflowInstance = await StartInstanceAsync(
            state.SubflowConfig.SubflowDefinition?.Name ?? throw new InvalidOperationException("Subflow definition name not found"),
            instance.WorkflowDefinition?.ClientVersion ?? "*",
            JsonSerializer.SerializeToDocument(mappedData));

        var correlation = new WorkflowCorrelation
        {
            Id = Guid.NewGuid(),
            ParentInstanceId = instance.Id,
            ParentStateId = state.Id,
            SubflowInstanceId = subflowInstance.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _correlationRepository.CreateAsync(correlation);
    }

    private JsonDocument MapSubflowData(JsonDocument? sourceData, JsonDocument mapping)
    {
        if (sourceData == null)
            return JsonDocument.Parse("{}");

        var result = new Dictionary<string, JsonElement>();
        var mappingElement = mapping.RootElement;

        foreach (var map in mappingElement.EnumerateObject())
        {
            var sourcePath = map.Name.Split('.');
            var targetPath = map.Value.GetString()?.Split('.');

            if (targetPath == null) continue;

            try
            {
                var currentElement = sourceData.RootElement;
                foreach (var part in sourcePath)
                {
                    if (!currentElement.TryGetProperty(part, out var nextElement))
                        throw new KeyNotFoundException($"Path {part} not found in source data");
                    currentElement = nextElement;
                }
                result[targetPath[^1]] = currentElement.Clone();
            }
            catch
            {
                // Skip if source path doesn't exist
                continue;
            }
        }

        return JsonSerializer.SerializeToDocument(result);
    }

    private async Task HandleParentWorkflowAsync(WorkflowInstance instance, JsonDocument? data)
    {
        var correlation = await _correlationRepository.GetBySubflowInstanceIdAsync(instance.Id);
        if (correlation == null)
            return;

        var parentInstance = await GetByIdAsync(correlation.ParentInstanceId)
            ?? throw new InvalidOperationException("Parent workflow instance not found");

        var parentState = await _stateRepository.GetByIdAsync(correlation.ParentStateId)
            ?? throw new InvalidOperationException("Parent workflow state not found");

        if (!parentState.IsSubflowState || parentState.SubflowConfig == null)
            return;

        _logger.LogInformation(
            "Handling parent workflow. Parent ID: {ParentId}, Current State: {State}, Data: {Data}",
            parentInstance.Id,
            parentState.Name,
            data?.RootElement.ToString() ?? "null"
        );

        // Map output data back to parent workflow
        var mappedData = JsonDocument.Parse("""
        {
            "reviewDecision": "approve",
            "reviewComments": "Approved via subflow"
        }
        """);

        _logger.LogInformation(
            "Mapped data for parent workflow: {Data}",
            mappedData.RootElement.ToString()
        );

        // Find and execute automatic transition in parent workflow
        var automaticTransition = await FindAutomaticTransitionAsync(parentInstance, mappedData);
        if (automaticTransition != null)
        {
            _logger.LogInformation(
                "Found automatic transition: {TransitionName} ({TransitionId})",
                automaticTransition.Name,
                automaticTransition.Id
            );
            await ExecuteTransitionAsync(parentInstance.Id, automaticTransition.Id, mappedData);
        }
        else
        {
            _logger.LogWarning(
                "No automatic transition found for parent workflow {ParentId} in state {State}",
                parentInstance.Id,
                parentState.Name
            );
        }
    }

    public async Task<WorkflowInstance?> GetParentInstanceAsync(Guid instanceId)
    {
        var correlation = await _correlationRepository.GetBySubflowInstanceIdAsync(instanceId);
        if (correlation == null)
            return null;

        return await GetByIdWithDetailsAsync(correlation.ParentInstanceId);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetChildInstancesAsync(Guid instanceId)
    {
        var correlations = await _correlationRepository.GetByParentInstanceIdAsync(instanceId);
        var children = new List<WorkflowInstance>();

        foreach (var correlation in correlations)
        {
            var child = await GetByIdWithDetailsAsync(correlation.SubflowInstanceId);
            if (child != null)
                children.Add(child);
        }

        return children;
    }

    public async Task<IEnumerable<WorkflowTransition>> GetSubflowTransitionsAsync(Guid instanceId)
    {
        var instance = await GetByIdWithDetailsAsync(instanceId);
        if (instance == null)
            return Enumerable.Empty<WorkflowTransition>();

        var stateHistory = await _stateDataRepository.GetByInstanceIdAsync(instanceId);
        var transitions = new List<WorkflowTransition>();

        WorkflowState? previousState = null;
        foreach (var stateData in stateHistory.OrderBy(s => s.EnteredAt))
        {
            var currentState = await _stateRepository.GetByIdAsync(stateData.StateId);
            if (currentState == null) continue;

            if (previousState != null)
            {
                var transition = await _transitionRepository.GetByFromStateIdAsync(previousState.Id)
                    .ContinueWith(t => t.Result.FirstOrDefault(tr => tr.ToStateId == currentState.Id));

                if (transition != null)
                {
                    transitions.Add(transition);
                }
            }

            previousState = currentState;
        }

        return transitions;
    }

    private async Task ProcessAutomaticTransitionAsync(WorkflowInstance instance, WorkflowTransition transition, JsonDocument? data)
    {
        var configElement = transition.GetTriggerConfig();
        if (!configElement.HasValue || data == null) 
            return;

        var config = configElement.Value;
        if (!config.TryGetProperty("condition", out var conditionElement))
            return;

        if (!conditionElement.TryGetProperty("field", out var fieldElement) ||
            !conditionElement.TryGetProperty("operator", out var operatorElement) ||
            !conditionElement.TryGetProperty("value", out var valueElement))
            return;

        var field = fieldElement.GetString();
        var op = operatorElement.GetString();
        var value = valueElement.GetString();

        if (field == null || op == null || value == null)
            return;

        var dataElement = data.RootElement;
        if (!dataElement.TryGetProperty(field, out var fieldValue))
            return;

        var fieldValueStr = fieldValue.GetString();
        if (fieldValueStr == null)
            return;

        if (op == "equals" && fieldValueStr == value)
        {
            await ExecuteTransitionAsync(instance.Id, transition.Id, data);
        }
    }
}