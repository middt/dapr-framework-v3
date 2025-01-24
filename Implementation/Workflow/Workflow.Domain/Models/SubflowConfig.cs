using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;

public class SubflowConfig : BaseEntity
{
    [Required]
    public Guid StateId { get; set; }

    [Required]
    public Guid SubflowDefinitionId { get; set; }

    [Column(TypeName = "jsonb")]
    public string _inputMapping = "{}";
    
    [NotMapped]
    public JsonDocument InputMapping
    {
        get => JsonDocument.Parse(_inputMapping);
        set => _inputMapping = value?.RootElement.ToString() ?? "{}";
    }

    [Column(TypeName = "jsonb")]
    public string _outputMapping = "{}";
    
    [NotMapped]
    public JsonDocument OutputMapping
    {
        get => JsonDocument.Parse(_outputMapping);
        set => _outputMapping = value?.RootElement.ToString() ?? "{}";
    }

    public bool WaitForCompletion { get; set; } = true;

    // Navigation properties
    [ForeignKey(nameof(StateId))]
    public virtual WorkflowState State { get; set; } = null!;

    [ForeignKey(nameof(SubflowDefinitionId))]
    public virtual WorkflowDefinition SubflowDefinition { get; set; } = null!;
} 