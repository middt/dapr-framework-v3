using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Workflow.Domain.Models.Tasks;

public class DaprHttpEndpointTask : WorkflowTask
{
    [Required]
    [MaxLength(100)]
    public string EndpointName { get; set; } = string.Empty;  // References the HTTPEndpoint component name

    [Required]
    [MaxLength(100)]
    public string Path { get; set; } = string.Empty;  // Path to append to baseUrl

    [Required]
    [MaxLength(10)]
    public string Method { get; set; } = "GET";  // HTTP method to use

    public string? Headers { get; set; }  // Additional headers as JSON
} 