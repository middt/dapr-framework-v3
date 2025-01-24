using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Common;

namespace Workflow.Domain.Models;
public class SubWorkflow : BaseEntity
{
    [Required]
    public Guid ParentWorkflowId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public string? Parameters { get; set; }
    
    [ForeignKey(nameof(ParentWorkflowId))]
    public virtual WorkflowInstance ParentWorkflow { get; set; }
} 