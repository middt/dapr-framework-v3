using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Dapr.Framework.Domain.Common;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Models.Views;

namespace Workflow.Domain.Models;

public class WorkflowDefinition : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0+0";

    [Required]
    [MaxLength(20)]
    public string ClientVersion { get; set; } = "*";

    public string SemanticVersion => Version.Contains('+') ? Version.Split('+')[0] : Version;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ArchivedAt { get; set; }

    public bool IsArchived => ArchivedAt.HasValue;

    // Navigation properties
    public virtual ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
    public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
    public virtual ICollection<WorkflowView> Views { get; set; } = new List<WorkflowView>();
    public virtual ICollection<WorkflowFunction> Functions { get; set; } = new List<WorkflowFunction>();

    public WorkflowState? GetInitialState()
    {
        return States.FirstOrDefault(s => s.StateType == StateType.Initial);
    }

    public WorkflowState? GetFinalState()
    {
        return States.FirstOrDefault(s => s.StateType == StateType.Finish);
    }

    public IEnumerable<WorkflowState> GetErrorStates()
    {
        return States.Where(s => s.StateType == StateType.Finish && s.SubType == StateSubType.Error);
    }

    public IEnumerable<WorkflowState> GetStatesByType(StateType type)
    {
        return States.Where(s => s.StateType == type);
    }

    public IEnumerable<WorkflowState> GetStatesBySubType(StateSubType subType)
    {
        return States.Where(s => s.SubType == subType);
    }

    public bool IsClientVersionCompatible(string clientVersion)
    {
        // Universal match
        if (ClientVersion == "*" || clientVersion == "*")
            return true;

        // Handle wildcard patterns
        if (ClientVersion.Contains('*'))
        {
            var pattern = "^" + Regex.Escape(ClientVersion).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(clientVersion, pattern);
        }

        // Handle version ranges
        if (ClientVersion.StartsWith(">="))
        {
            var minVersion = System.Version.Parse(ClientVersion[2..]);
            var currentVersion = System.Version.Parse(clientVersion);
            return currentVersion >= minVersion;
        }

        if (ClientVersion.StartsWith("<"))
        {
            var maxVersion = System.Version.Parse(ClientVersion[1..]);
            var currentVersion = System.Version.Parse(clientVersion);
            return currentVersion < maxVersion;
        }

        // Exact match
        return ClientVersion == clientVersion;
    }
}