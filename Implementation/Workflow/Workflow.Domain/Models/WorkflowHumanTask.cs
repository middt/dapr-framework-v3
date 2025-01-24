using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;

public class WorkflowHumanTask : BaseEntity
{
    [Required]
    public Guid WorkflowInstanceId { get; set; }

    [Required]
    public Guid StateId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Assignee { get; set; } = string.Empty;

    public string? Result { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(WorkflowInstanceId))]
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual WorkflowState State { get; set; } = null!;
}