using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Domain.Models.Tasks;

public class DaprPubSubTask : WorkflowTask
{
    [Required]
    [MaxLength(100)]
    public string PubSubName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Topic { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}";

    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    public override void Configure(JsonElement config)
    {
        base.Configure(config);

        if (config.TryGetProperty("pubSubName", out var pubSubName))
            PubSubName = pubSubName.GetString();

        if (config.TryGetProperty("topic", out var topic))
            Topic = topic.GetString();

        if (config.TryGetProperty("data", out var data))
            Data = data.ToString();

        if (config.TryGetProperty("metadata", out var metadata))
            Metadata = metadata.ToString();
    }
}