using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Dapr.Framework.Domain.Common;
using Workflow.Domain.Models.Views;

namespace Workflow.Domain.Models;


public class WorkflowTransition : BaseEntity
{
    [Required]
    public Guid WorkflowDefinitionId { get; set; }

    [Required]
    public Guid FromStateId { get; set; }

    [Required]
    public Guid ToStateId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public TriggerType TriggerType { get; set; }

    [Column(TypeName = "jsonb")]
    public string _triggerConfig = "{}";

    // Navigation properties
    [ForeignKey(nameof(WorkflowDefinitionId))]
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    [ForeignKey(nameof(FromStateId))]
    public virtual WorkflowState FromState { get; set; } = null!;

    [ForeignKey(nameof(ToStateId))]
    public virtual WorkflowState ToState { get; set; } = null!;

    public virtual ICollection<WorkflowView> Views { get; set; } = new List<WorkflowView>();


    [NotMapped]
    public JsonDocument TriggerConfig
    {
        get => JsonDocument.Parse(_triggerConfig);
        set => _triggerConfig = value?.RootElement.ToString() ?? "{}";
    }


}