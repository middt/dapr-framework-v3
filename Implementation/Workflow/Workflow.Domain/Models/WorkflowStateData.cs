using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;

public class WorkflowStateData : BaseEntity
{
    [Required]
    public Guid StateId { get; set; }

    [Required]
    public Guid WorkflowInstanceId { get; set; }

    [Column(TypeName = "jsonb")]
    public string _data = "{}";

    public DateTime EnteredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(StateId))]
    public virtual WorkflowState? State { get; set; }

    [ForeignKey(nameof(WorkflowInstanceId))]
    public virtual WorkflowInstance? WorkflowInstance { get; set; }
}