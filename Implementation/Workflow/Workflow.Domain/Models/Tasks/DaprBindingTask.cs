using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Domain.Models.Tasks;

public class DaprBindingTask : WorkflowTask
{
    [Required]
    [MaxLength(100)]
    public string BindingName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Operation { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    [Required]
    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}";

    public override void Configure(JsonElement config)
    {
        base.Configure(config);

        if (config.TryGetProperty("bindingName", out var bindingName))
            BindingName = bindingName.GetString();

        if (config.TryGetProperty("operation", out var operation))
            Operation = operation.GetString();

        if (config.TryGetProperty("metadata", out var metadata))
            Metadata = metadata.ToString();

        if (config.TryGetProperty("data", out var data))
            Data = data.ToString();
    }
}