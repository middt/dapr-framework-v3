using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflow.Domain.Models.Tasks;

public class HumanTask : WorkflowTask
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AssignedTo { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string Form { get; set; } = "{}";

    public int ReminderIntervalMinutes { get; set; }
    public int EscalationTimeoutMinutes { get; set; }

    [Required]
    [MaxLength(100)]
    public string EscalationAssignee { get; set; } = string.Empty;

    public override void Configure(JsonElement config)
    {
        base.Configure(config);

        if (config.TryGetProperty("title", out var title))
            Title = title.GetString() ?? throw new ArgumentNullException(nameof(title));

        if (config.TryGetProperty("instructions", out var instructions))
            Instructions = instructions.GetString() ?? throw new ArgumentNullException(nameof(instructions));

        if (config.TryGetProperty("assignedTo", out var assignedTo))
            AssignedTo = assignedTo.GetString() ?? throw new ArgumentNullException(nameof(assignedTo));

        if (config.TryGetProperty("dueDate", out var dueDate))
            DueDate = dueDate.GetDateTime();

        if (config.TryGetProperty("form", out var form))
            Form = form.ToString();

        if (config.TryGetProperty("reminderIntervalMinutes", out var reminder))
            ReminderIntervalMinutes = reminder.GetInt32();

        if (config.TryGetProperty("escalationTimeoutMinutes", out var timeout))
            EscalationTimeoutMinutes = timeout.GetInt32();

        if (config.TryGetProperty("escalationAssignee", out var escalation))
            EscalationAssignee = escalation.GetString() ?? throw new ArgumentNullException(nameof(escalation));
    }
}