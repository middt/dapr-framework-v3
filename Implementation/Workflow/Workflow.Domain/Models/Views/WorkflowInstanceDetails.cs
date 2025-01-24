using System.Text.Json;
using Workflow.Domain.Models;

namespace Workflow.Domain.Models.Views;

public class WorkflowInstanceDetails
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string WorkflowVersion { get; set; } = string.Empty;
    public Guid CurrentStateId { get; set; }
    public string CurrentStateName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }  // Keep as non-nullable for the view
    public DateTime? CompletedAt { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowStateInfo CurrentState { get; set; } = null!;

    public string StateDataUrl { get; set; } = string.Empty;
    public string StateHistoryUrl { get; set; } = string.Empty;

    // Workflow hierarchy information
    public bool IsSubflow { get; set; }
    public ParentWorkflowInfo? Parent { get; set; }
    public IEnumerable<ChildWorkflowInfo> Children { get; set; } = new List<ChildWorkflowInfo>();

    public static WorkflowInstanceDetails FromInstance(
        WorkflowInstance instance,
        IEnumerable<WorkflowTransition> availableTransitions,
        IEnumerable<WorkflowView> stateViews,
        string baseUrl,
        WorkflowCorrelation? correlation = null,
        IEnumerable<WorkflowCorrelation> childCorrelations = null)
    {
        /*
        var pendingTask = instance.HumanTasks
            .Where(t => t.StateId == instance.CurrentStateId && t.CompletedAt == null)
            .OrderByDescending(t => t.AssignedAt)
            .FirstOrDefault();
*/
        var stateInfo = new WorkflowStateInfo
        {
            Id = instance.CurrentState!.Id,
            Name = instance.CurrentState.Name,
            Description = instance.CurrentState.Description,
            StateType = instance.CurrentState.StateType,
            SubType = instance.CurrentState.SubType,
            //bViews = stateViews,
            AvailableTransitions = availableTransitions.Select(t => new TransitionInfo
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Trigger = t.TriggerType.ToString(),
                ToState = t.ToState!,
                Views = t.Views
            }),
            // PendingTask = pendingTask,
            // TasksUrl = $"{baseUrl}/api/v1.0/workflow-instances/{instance.Id}/tasks"
        };

        return new WorkflowInstanceDetails
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowName = instance.WorkflowDefinition?.Name ?? string.Empty,
            WorkflowVersion = instance.WorkflowDefinition?.Version ?? string.Empty,
            CurrentStateId = instance.CurrentStateId,
            CurrentStateName = instance.CurrentState?.Name ?? string.Empty,
            Status = instance.Status,
            CreatedAt = instance.CreatedAt,
            UpdatedAt = instance.UpdatedAt, //?? instance.CreatedAt,  // Use CreatedAt as fallback
            CompletedAt = instance.CompletedAt,
            // WorkflowDefinition = instance.WorkflowDefinition!,
            CurrentState = stateInfo,
            StateDataUrl = $"{baseUrl}/api/v1.0/workflow-state-data/latest/by-instance/{instance.Id}",
            StateHistoryUrl = $"{baseUrl}/api/v1.0/workflow-state-data/by-instance/{instance.Id}",
            IsSubflow = correlation != null,
            Parent = correlation != null ? new ParentWorkflowInfo
            {
                InstanceId = correlation.ParentInstanceId,
                StateName = correlation.ParentState?.Name ?? "Unknown",
                StateId = correlation.ParentStateId,
                WaitForCompletion = correlation.ParentState?.SubflowConfig?.WaitForCompletion ?? true
            } : null,
            Children = childCorrelations?.Select(c => new ChildWorkflowInfo
            {
                InstanceId = c.SubflowInstanceId,
                StateId = c.SubflowInstance?.CurrentStateId ?? Guid.Empty,
                StateName = c.SubflowInstance?.CurrentState?.Name ?? "Unknown",
                Status = c.SubflowInstance?.Status ?? "Unknown",
                WaitForCompletion = c.ParentState?.SubflowConfig?.WaitForCompletion ?? true
            }) ?? new List<ChildWorkflowInfo>()
        };
    }
}

public class ParentWorkflowInfo
{
    public Guid InstanceId { get; set; }
    public Guid StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public bool WaitForCompletion { get; set; }
}

public class ChildWorkflowInfo
{
    public Guid InstanceId { get; set; }
    public Guid StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool WaitForCompletion { get; set; }
}