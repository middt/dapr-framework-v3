using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Domain.Models.Tasks;

public class DaprServiceTask : WorkflowTask
{
    [Required]
    [MaxLength(100)]
    public string AppId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MethodName { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string HttpVerb { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}";

    [MaxLength(1000)]
    public string? QueryString { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public override void Configure(JsonElement config)
    {
        base.Configure(config);

        if (config.TryGetProperty("appId", out var appId))
            AppId = appId.GetString();

        if (config.TryGetProperty("methodName", out var methodName))
            MethodName = methodName.GetString();

        if (config.TryGetProperty("httpVerb", out var httpVerb))
            HttpVerb = httpVerb.GetString();

        if (config.TryGetProperty("data", out var data))
            Data = data.ToString();

        if (config.TryGetProperty("queryString", out var queryString))
            QueryString = queryString.GetString();

        if (config.TryGetProperty("timeoutSeconds", out var timeout))
            TimeoutSeconds = timeout.GetInt32();
    }
}