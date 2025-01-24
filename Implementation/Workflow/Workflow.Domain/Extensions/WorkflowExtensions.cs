using System.Text.Json;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;

namespace Workflow.Domain.Extensions;

public static class WorkflowExtensions
{
    public static JsonElement? GetTriggerConfig(this WorkflowTransition transition)
    {
        if (transition.TriggerConfig == null)
            return null;
        
        return transition.TriggerConfig.RootElement;
    }

    public static void SetTriggerConfig<T>(this WorkflowTransition transition, T config)
    {
        transition.TriggerConfig = JsonSerializer.SerializeToDocument(config);
    }

    public static bool IsClientVersionCompatible(this WorkflowDefinition definition, string clientVersion)
    {
        if (string.IsNullOrEmpty(definition.ClientVersion) || definition.ClientVersion == "*")
            return true;

        var defParts = definition.ClientVersion.Split('.');
        var clientParts = clientVersion.Split('.');

        // Compare major version
        if (defParts[0] != "*" && defParts[0] != clientParts[0])
            return false;

        // If definition only specifies major version
        if (defParts.Length == 1)
            return true;

        // Compare minor version
        if (defParts[1] != "*" && defParts[1] != clientParts[1])
            return false;

        return true;
    }

    public static bool IsCompatibleWithWorkflowVersion(this WorkflowView view, string workflowVersion)
    {
        if (string.IsNullOrEmpty(view.WorkflowVersion) || view.WorkflowVersion == "*")
            return true;

        var viewParts = view.WorkflowVersion.Split('.');
        var workflowParts = workflowVersion.Split('.');

        // Compare major version
        if (viewParts[0] != "*" && viewParts[0] != workflowParts[0])
            return false;

        // If view only specifies major version
        if (viewParts.Length == 1)
            return true;

        // Compare minor version
        if (viewParts[1] != "*" && viewParts[1] != workflowParts[1])
            return false;

        return true;
    }

    public static void SetData(this WorkflowStateData stateData, string data)
    {
        stateData._data = data;
    }

    public static void SetData(this WorkflowStateData stateData, object data)
    {
        stateData._data = JsonSerializer.Serialize(data);
    }

    public static string GetData(this WorkflowStateData stateData)
    {
        return stateData._data;
    }

    public static T? GetData<T>(this WorkflowStateData stateData)
    {
        return JsonSerializer.Deserialize<T>(stateData._data);
    }

    public static JsonElement GetDataAsJson(this WorkflowStateData stateData)
    {
        return JsonDocument.Parse(stateData._data).RootElement;
    }

    public static T Apply<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
} 