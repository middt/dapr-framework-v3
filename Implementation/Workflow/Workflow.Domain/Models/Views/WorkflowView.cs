using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Dapr.Framework.Domain.Common;
using System.Text.RegularExpressions;

namespace Workflow.Domain.Models.Views;

public class WorkflowView : BaseEntity
{

    [Required]
    public ViewType Type { get; set; }

    [Required]
    public ViewTarget Target { get; set; }

    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0+0";

    public string SemanticVersion => Version.Split('+')[0];

    [Required]
    [MaxLength(20)]
    public string WorkflowVersion { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    [NotMapped]
    public JsonDocument? JsonContent
    {
        get => Type == ViewType.Json && !string.IsNullOrEmpty(Content) ? JsonDocument.Parse(Content) : null;
        set => Content = value?.RootElement.ToString() ?? string.Empty;
    }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Foreign keys
    public Guid? StateId { get; set; }
    public Guid? TransitionId { get; set; }
    public Guid WorkflowDefinitionId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StateId))]
    public virtual WorkflowState? State { get; set; }

    [ForeignKey(nameof(TransitionId))]
    public virtual WorkflowTransition? Transition { get; set; }

    [ForeignKey(nameof(WorkflowDefinitionId))]
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    // Helper method to check if view is compatible with a workflow version
    public bool IsCompatibleWithWorkflowVersion(string workflowVersion)
    {
        // Universal match
        if (WorkflowVersion == "*" || workflowVersion == "*")
            return true;

        // Handle wildcard patterns
        if (WorkflowVersion.Contains('*'))
        {
            var pattern = "^" + Regex.Escape(WorkflowVersion).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(workflowVersion, pattern);
        }

        // Handle version ranges
        if (WorkflowVersion.StartsWith(">="))
        {
            var minVersion = System.Version.Parse(WorkflowVersion[2..]);
            var currentVersion = System.Version.Parse(workflowVersion);
            return currentVersion >= minVersion;
        }

        if (WorkflowVersion.StartsWith("<"))
        {
            var maxVersion = System.Version.Parse(WorkflowVersion[1..]);
            var currentVersion = System.Version.Parse(workflowVersion);
            return currentVersion < maxVersion;
        }

        // Exact match
        return WorkflowVersion == workflowVersion;
    }
}