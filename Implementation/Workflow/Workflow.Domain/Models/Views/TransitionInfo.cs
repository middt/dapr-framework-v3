using Workflow.Domain.Models;

namespace Workflow.Domain.Models.Views;

public class TransitionInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public WorkflowState ToState { get; set; } = null!;
    public IEnumerable<WorkflowView> Views { get; set; } = new List<WorkflowView>();
}