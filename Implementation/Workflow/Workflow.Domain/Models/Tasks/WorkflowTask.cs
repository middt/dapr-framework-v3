using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
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
    public TaskType Type { get; set; }

    [Column(TypeName = "jsonb")]
    public string Config { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public virtual void Configure(JsonElement config)
    {
        Config = config.ToString();
    }

    // Add collection navigation property for task assignments
    public virtual ICollection<WorkflowTaskAssignment> TaskAssignments { get; set; } = new List<WorkflowTaskAssignment>();
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
    Manual,
    OnExecute
}
public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}