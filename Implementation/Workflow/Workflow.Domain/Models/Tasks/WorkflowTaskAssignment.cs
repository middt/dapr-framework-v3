using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models.Tasks;

/// <summary>
/// Task;
/// Tüm bileşenden bağımsızda çalışabilir fakat state, transition, instance ve function seviyesinde ki atama durumunu bellir eder.
/// </summary>
public class WorkflowTaskAssignment : BaseEntity
{
    [Required]
    public Guid TaskId { get; set; }

    public Guid? StateId { get; set; }
    
    public Guid? TransitionId { get; set; }
    
    public Guid? FunctionId { get; set; }

    public Guid? WorkflowInstanceId { get; set; }

    [Required]
    public TaskTrigger Trigger { get; set; }

    public int Order { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TaskId))]
    public virtual WorkflowTask Task { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual WorkflowState? State { get; set; }

    [ForeignKey(nameof(TransitionId))]
    public virtual WorkflowTransition? Transition { get; set; }

    [ForeignKey(nameof(FunctionId))]
    public virtual WorkflowFunction? Function { get; set; }

    [ForeignKey(nameof(WorkflowInstanceId))]
    public virtual WorkflowInstance? Instance { get; set; }
}
