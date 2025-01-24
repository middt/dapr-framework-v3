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

    public WorkflowTaskProcessor(
        DaprClient daprClient, 
        ILogger<WorkflowTaskProcessor> logger,
        IWorkflowInstanceTaskRepository instanceTaskRepository,
        HttpClient httpClient)
    {
        _daprClient = daprClient;
        _logger = logger;
        _instanceTaskRepository = instanceTaskRepository;
        _httpClient = httpClient;
    }

    public async Task<object?> ExecuteTaskAsync(WorkflowTask task, JsonDocument? data = null, WorkflowInstance? instance = null)
    {
        WorkflowInstanceTask? instanceTask = null;
        object? result = null;

        if (instance != null)
        {
            instanceTask = new WorkflowInstanceTask
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                WorkflowTaskId = task.Id,
                StateId = instance.CurrentStateId,
                TaskName = task.Name,
                TaskType = task.Type,
                Status = Workflow.Domain.Models.Tasks.TaskStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            await _instanceTaskRepository.CreateAsync(instanceTask);
        }

        try
        {
            _logger.LogInformation("Executing task {TaskId} of type {TaskType} for instance {InstanceId}",
                task.Id, task.Type, instance?.Id);

            // This switch statement works with TPT since we're using the Type property
            switch (task.Type)
            {
                case TaskType.Human:
                    result = await ExecuteHumanTaskAsync(task as HumanTask, data, instance);
                    break;
                case TaskType.DaprBinding:
                    result = await ExecuteDaprBindingTaskAsync(task as DaprBindingTask, instance, data);
                    break;
                case TaskType.DaprService:
                    result = await ExecuteDaprServiceTaskAsync(task as DaprServiceTask, instance, data);
                    break;
                case TaskType.DaprPubSub:
                    result = await ExecuteDaprPubSubTaskAsync(task as DaprPubSubTask, instance, data);
                    break;
                case TaskType.Http:
                    result = await ExecuteHttpTaskAsync(task as HttpTask, instance, data);
                    break;
                case TaskType.DaprHttpEndpoint:
                    result = await ExecuteDaprHttpEndpointTaskAsync(task as DaprHttpEndpointTask, instance, data);
                    break;
            }

            task.Status = Workflow.Domain.Models.Tasks.TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.Result = result != null ? JsonSerializer.Serialize(result) : null;
            
            if (instanceTask != null)
            {
                instanceTask.Status = Workflow.Domain.Models.Tasks.TaskStatus.Completed;
                instanceTask.CompletedAt = DateTime.UtcNow;
                instanceTask.Result = task.Result;
                await _instanceTaskRepository.UpdateAsync(instanceTask);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing task {TaskId}", task.Id);
            task.Status = Workflow.Domain.Models.Tasks.TaskStatus.Failed;
            
            if (instanceTask != null)
            {
                instanceTask.Status = Workflow.Domain.Models.Tasks.TaskStatus.Failed;
                instanceTask.Error = ex.Message;
                await _instanceTaskRepository.UpdateAsync(instanceTask);
            }
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

    private async Task<object?> ExecuteHumanTaskAsync(HumanTask? task, JsonDocument? data, WorkflowInstance? instance)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        // Human tasks are completed manually, so we just validate and log
        _logger.LogInformation("Human task {TaskId} created and waiting for completion", task.Id);

        if (data != null)
        {
            // Replace any placeholders in the task form or instructions
            task.Form = ReplacePlaceholders(task.Form, data, instance);
            task.Instructions = ReplacePlaceholders(task.Instructions, data, instance);
        }

        // Human tasks start in pending status until manually completed
        task.Status = Workflow.Domain.Models.Tasks.TaskStatus.Pending;

        return new { status = "pending", form = task.Form, instructions = task.Instructions };
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