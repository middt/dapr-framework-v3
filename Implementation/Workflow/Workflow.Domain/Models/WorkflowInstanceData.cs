using System.Text.Json;
using Dapr.Framework.Domain.Entities;

namespace Workflow.Domain.Models;

public class WorkflowInstanceData : IEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string Data { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; }

    // IEntity implementation
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public void MergeData(JsonDocument newData)
    {
        var existingData = JsonDocument.Parse(Data);
        var mergedDict = new Dictionary<string, JsonElement>();

        // Copy existing data
        foreach (var property in existingData.RootElement.EnumerateObject())
        {
            mergedDict[property.Name] = property.Value.Clone();
        }

        // Merge new data (overwriting existing values)
        foreach (var property in newData.RootElement.EnumerateObject())
        {
            mergedDict[property.Name] = property.Value.Clone();
        }

        Data = JsonSerializer.Serialize(mergedDict);
        UpdatedAt = DateTime.UtcNow;
    }

    public JsonDocument GetData()
    {
        return JsonDocument.Parse(Data);
    }
} 