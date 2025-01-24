using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models.Tasks;

public class WorkflowTaskAssignment : BaseEntity
{
    [Required]
    public Guid TaskId { get; set; }

    public Guid? StateId { get; set; }
    
    public Guid? TransitionId { get; set; }
    
    public Guid? FunctionId { get; set; }

    [Required]
    public TaskTrigger Trigger { get; set; }

    public Domain.Models.Tasks.TaskStatus Status { get; set; } = Domain.Models.Tasks.TaskStatus.Pending;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? Result { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TaskId))]
    public virtual WorkflowTask Task { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual WorkflowState? State { get; set; }

    [ForeignKey(nameof(TransitionId))]
    public virtual WorkflowTransition? Transition { get; set; }

    [ForeignKey(nameof(FunctionId))]
    public virtual WorkflowFunction? Function { get; set; }
}
