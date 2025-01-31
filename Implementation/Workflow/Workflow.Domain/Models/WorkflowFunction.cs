using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Workflow.Domain.Models.Tasks;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;

public class WorkflowFunction : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? StateId { get; set; }  // If null, applies to all states
    public Guid? WorkflowDefinitionId { get; set; }  // If null, applies to all workflows
    public string? ResponseMapping { get; set; }  // JSON path mapping for response data
    public bool EnrichStateData { get; set; } = true;  // Whether to include the API response in state data
    public int Order { get; set; }  // Order of execution when multiple APIs are configured

    // Navigation properties
    public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
    public virtual ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
}