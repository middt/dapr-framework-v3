using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Domain.Models.Tasks;

public class HttpTask : WorkflowTask
{
    [Required]
    [MaxLength(2000)]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Method { get; set; } = "GET";

    [Column(TypeName = "jsonb")]
    public string Headers { get; set; } = "{}";

    [Column(TypeName = "jsonb")]
    public string Body { get; set; } = "{}";

    public int TimeoutSeconds { get; set; } = 30;
    public bool ValidateSSL { get; set; } = true;

    public override void Configure(JsonElement config)
    {
        base.Configure(config);

        if (config.TryGetProperty("url", out var url))
            Url = url.GetString();

        if (config.TryGetProperty("method", out var method))
            Method = method.GetString();

        if (config.TryGetProperty("headers", out var headers))
            Headers = headers.ToString();

        if (config.TryGetProperty("body", out var body))
            Body = body.ToString();

        if (config.TryGetProperty("timeoutSeconds", out var timeout))
            TimeoutSeconds = timeout.GetInt32();

        if (config.TryGetProperty("validateSSL", out var validateSSL))
            ValidateSSL = validateSSL.GetBoolean();
    }
}