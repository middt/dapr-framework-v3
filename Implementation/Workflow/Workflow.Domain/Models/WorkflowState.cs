using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapr.Framework.Domain.Common;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Models.Views;
using Workflow.Domain.Models;

namespace Workflow.Domain.Models;

public class WorkflowState : BaseEntity
{
    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public StateType StateType { get; set; }

    [Required]
    public StateSubType SubType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ArchivedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(WorkflowDefinitionId))]
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    public virtual ICollection<WorkflowView> Views { get; set; } = new List<WorkflowView>();
    public virtual ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();

    public virtual SubflowConfig? SubflowConfig { get; set; }

    public bool IsSubflowState => StateType == StateType.Subflow && SubflowConfig != null;
}