using System.ComponentModel.DataAnnotations;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;

public class WorkflowCorrelation : BaseEntity
{
    [Required]
    public Guid ParentInstanceId { get; set; }

    [Required]
    public Guid ParentStateId { get; set; }

    [Required]
    public Guid SubflowInstanceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual WorkflowInstance ParentInstance { get; set; } = null!;
    public virtual WorkflowState ParentState { get; set; } = null!;
    public virtual WorkflowInstance SubflowInstance { get; set; } = null!;
} 