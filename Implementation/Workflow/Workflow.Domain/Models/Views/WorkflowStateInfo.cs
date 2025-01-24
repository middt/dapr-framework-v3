using System.Text.Json;
using Workflow.Domain.Models;

namespace Workflow.Domain.Models.Views;

public class WorkflowStateInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StateType StateType { get; set; }
    public StateSubType SubType { get; set; }
    public bool RequiresHumanTask { get; set; }
    public string? HumanTaskName { get; set; }
    public string? HumanTaskDescription { get; set; }
    public IEnumerable<WorkflowView> Views { get; set; } = new List<WorkflowView>();
    public IEnumerable<TransitionInfo> AvailableTransitions { get; set; } = new List<TransitionInfo>();

    // Human Task Information
    public WorkflowHumanTask? PendingTask { get; set; }
    public string? TasksUrl { get; set; }
}

