using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Repositories;
using System.Net.Http;

namespace Workflow.Application.Services;

public class WorkflowTaskProcessor
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<WorkflowTaskProcessor> _logger;
    private readonly IWorkflowInstanceTaskRepository _instanceTaskRepository;
    private readonly HttpClient _httpClient;
    private readonly IWorkflowTaskRepository _taskRepository;
    private readonly IWorkflowTaskAssignmentRepository _taskAssignmentRepository;

    public WorkflowTaskProcessor(
        DaprClient daprClient, 
        ILogger<WorkflowTaskProcessor> logger,
        IWorkflowInstanceTaskRepository instanceTaskRepository,
        HttpClient httpClient,
        IWorkflowTaskRepository taskRepository,
        IWorkflowTaskAssignmentRepository taskAssignmentRepository)
    {
        _daprClient = daprClient;
        _logger = logger;
        _instanceTaskRepository = instanceTaskRepository;
        _httpClient = httpClient;
        _taskRepository = taskRepository;
        _taskAssignmentRepository = taskAssignmentRepository;
    }

    public async Task<object?> ExecuteTaskAsync(WorkflowTask task, JsonDocument data, WorkflowInstance? instance = null)
    {
        var taskAssignment = new WorkflowTaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Status = Domain.Models.Tasks.TaskStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            object? result = null;

            switch (task)
            {
                case HumanTask humanTask:
                    result = await ExecuteHumanTaskAsync(humanTask, taskAssignment, data);
                    break;
                case DaprBindingTask daprBindingTask:
                    result = await ExecuteDaprBindingTaskAsync(daprBindingTask, instance, data);
                    break;
                case DaprServiceTask daprServiceTask:
                    result = await ExecuteDaprServiceTaskAsync(daprServiceTask, instance, data);
                    break;
                case DaprPubSubTask daprPubSubTask:
                    result = await ExecuteDaprPubSubTaskAsync(daprPubSubTask, instance, data);
                    break;
                case HttpTask httpTask:
                    result = await ExecuteHttpTaskAsync(httpTask, instance, data);
                    break;
                case DaprHttpEndpointTask daprHttpEndpointTask:
                    result = await ExecuteDaprHttpEndpointTaskAsync(daprHttpEndpointTask, instance, data);
                    break;
            }

            taskAssignment.Status = Domain.Models.Tasks.TaskStatus.Completed;
            taskAssignment.CompletedAt = DateTime.UtcNow;
            taskAssignment.Result = result?.ToString();
            await _taskAssignmentRepository.UpdateAsync(taskAssignment);

            return result;
        }
        catch (Exception ex)
        {
            taskAssignment.Status = Domain.Models.Tasks.TaskStatus.Failed;
            taskAssignment.Result = ex.Message;
            await _taskAssignmentRepository.UpdateAsync(taskAssignment);
            throw;
        }
    }

    private async Task<object?> ExecuteDaprBindingTaskAsync(DaprBindingTask? task, WorkflowInstance? instance, JsonDocument stateData)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(task.Metadata);
        var config = JsonSerializer.Deserialize<JsonDocument>(task.Config);
        var data = config.RootElement.GetProperty("data");

        // Replace placeholders in metadata and data with actual values from state data
        metadata = ReplacePlaceholders(metadata, stateData, instance);
        var jsonData = data.GetRawText();
        var replacedData = ReplacePlaceholders(jsonData, stateData, instance);
        var requestData = JsonSerializer.Deserialize<object>(replacedData);

        await _daprClient.InvokeBindingAsync(task.BindingName, task.Operation, requestData, metadata);
        return new { operation = task.Operation, binding = task.BindingName, metadata, data = requestData };    }

    private async Task<object?> ExecuteDaprServiceTaskAsync(DaprServiceTask? task, WorkflowInstance? instance, JsonDocument stateData)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var dataJson = JsonSerializer.Deserialize<JsonDocument>(task.Data);
        var jsonData = dataJson.RootElement.GetRawText();
        var replacedData = ReplacePlaceholders(jsonData, stateData, instance);
        var requestData = JsonSerializer.Deserialize<object>(replacedData);

        var httpMethod = new HttpMethod(task.HttpVerb);
        var response = await _daprClient.InvokeMethodAsync<object, object>(httpMethod, task.AppId, task.MethodName, requestData);
        return response;
    }

    private async Task<object?> ExecuteDaprPubSubTaskAsync(DaprPubSubTask? task, WorkflowInstance? instance, JsonDocument stateData)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var dataJson = JsonSerializer.Deserialize<JsonDocument>(task.Data);
        var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(task.Metadata);

        var jsonData = dataJson.RootElement.GetRawText();
        var replacedData = ReplacePlaceholders(jsonData, stateData, instance);
        var requestData = JsonSerializer.Deserialize<object>(replacedData);
        metadata = ReplacePlaceholders(metadata, stateData, instance);

        await _daprClient.PublishEventAsync(task.PubSubName, task.Topic, requestData, metadata);
        return new { pubsub = task.PubSubName, topic = task.Topic, data = requestData, metadata };
    }

    private async Task<object?> ExecuteHumanTaskAsync(HumanTask task, WorkflowTaskAssignment assignment, JsonDocument data)
    {
        // Human task execution logic
        _logger.LogInformation("Executing human task: {TaskName}", task.Name);
        // ... implementation
        return null;
    }

    private async Task<object?> ExecuteHttpTaskAsync(HttpTask? task, WorkflowInstance? instance, JsonDocument stateData)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        // Create the HTTP request
        var request = new HttpRequestMessage(new HttpMethod(task.Method), task.Url);

        // Parse the config
        var config = JsonSerializer.Deserialize<JsonDocument>(task.Config);

        // Add headers from Config
        if (config?.RootElement.TryGetProperty("headers", out var headersElement) == true)
        {
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersElement.ToString());
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Add request body if provided
        if (config?.RootElement.TryGetProperty("data", out var dataElement) == true)
        {
            var jsonData = dataElement.GetRawText();
            var replacedData = ReplacePlaceholders(jsonData, stateData, instance);
            var requestData = JsonSerializer.Deserialize<object>(replacedData);
            var jsonContent = JsonSerializer.Serialize(requestData);
            request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        }

        // Send the request
        _logger.LogInformation("Executing HTTP task {TaskId} with URL {Url}", task.Id, task.Url);
        var response = await _httpClient.SendAsync(request);

        // Handle the response
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<object?> ExecuteDaprHttpEndpointTaskAsync(DaprHttpEndpointTask? task, WorkflowInstance? instance, JsonDocument stateData)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        // Process request body and headers if provided in Config
        var config = JsonSerializer.Deserialize<JsonDocument>(task.Config);
        object? requestData = null;
        if (config?.RootElement.TryGetProperty("data", out var dataElement) == true)
        {
            var jsonData = dataElement.GetRawText();
            var replacedData = ReplacePlaceholders(jsonData, stateData, instance);
            requestData = JsonSerializer.Deserialize<object>(replacedData);
        }

        // Invoke the endpoint through Dapr
        var response = await _daprClient.InvokeMethodAsync<object?, object>(
            new HttpMethod(task.Method),
            task.EndpointName,
            task.Path,
            requestData);

        return response;
    }

    private T ReplacePlaceholders<T>(T obj, JsonDocument stateData, WorkflowInstance? instance)
    {
        var json = JsonSerializer.Serialize(obj);
        _logger.LogInformation("Replacing placeholders in: {Json}", json);
        _logger.LogInformation("State data: {StateData}", stateData.RootElement.ToString());

        // Replace workflow placeholders
        if (instance != null)
        {
            json = json.Replace("{{workflow.instanceId}}", instance.Id.ToString());
            json = json.Replace("{{workflow.timestamp}}", DateTime.UtcNow.ToString("O"));
        }

        // Replace data placeholders
        foreach (var property in stateData.RootElement.EnumerateObject())
        {
            var placeholder = $"{{{{data.{property.Name}}}}}";
            if (json.Contains(placeholder))
            {
                _logger.LogInformation("Replacing {Placeholder} with {Value}", 
                    placeholder, property.Value.ToString());
                json = json.Replace(placeholder, property.Value.ToString());
            }
        }

        _logger.LogInformation("Final json after replacements: {Json}", json);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}