using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Dapr.Framework.Domain.Common;
namespace Workflow.Domain.Models.Tasks;

[Table("WorkflowTasks")]
public abstract class WorkflowTask : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public TaskTrigger Trigger { get; set; }

    [Required]
    public TaskType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public string Config { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    // Task execution properties
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public string? Result { get; set; }

    public Guid? WorkflowStateId { get; set; }

    [ForeignKey(nameof(WorkflowStateId))]
    public virtual WorkflowState? WorkflowState { get; set; }

    public virtual void Configure(JsonElement config)
    {
        Config = config.ToString();
    }
}

public enum TaskType
{
    DaprHttpEndpoint,
    DaprBinding,
    DaprService,
    DaprPubSub,
    Human,
    Http
}

public enum TaskTrigger
{
    OnEntry,
    OnExit,
    Both,
    Manual
}

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}