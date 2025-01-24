using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;

public class WorkflowInstance : BaseEntity
{
    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    [Required]
    public Guid CurrentStateId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
    public virtual WorkflowState? CurrentState { get; set; }
    public virtual ICollection<WorkflowStateData> StateData { get; set; } = new List<WorkflowStateData>();
    public virtual ICollection<WorkflowHumanTask> HumanTasks { get; set; } = new List<WorkflowHumanTask>();

    // Navigation properties for correlations
    public virtual WorkflowCorrelation? ParentCorrelation { get; set; }
    public virtual ICollection<WorkflowCorrelation> ChildCorrelations { get; set; } = new List<WorkflowCorrelation>();
}