using System.Text.Json;
using Dapr.Framework.Domain.Entities;

namespace Workflow.Domain.Models.Tasks;

/// <summary>
/// Execute olan task'ların kaydın tutulduğu domain
/// </summary>
public class WorkflowInstanceTask : IEntity
{
    public Guid Id { get; set; }
    public Guid? WorkflowInstanceId { get; set; }
    public Guid WorkflowTaskId { get; set; }
    public Guid? StateId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public TaskType TaskType { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
    public string? Result { get; set; }

    // IEntity implementation
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
    public virtual WorkflowTask WorkflowTask { get; set; } = null!;
    public virtual WorkflowState State { get; set; } = null!;
} 