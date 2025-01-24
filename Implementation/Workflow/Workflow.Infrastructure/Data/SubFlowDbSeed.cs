using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Models.Views;
using Workflow.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Workflow.Infrastructure.Data;

public static class SubFlowDbSeed
{
    public static async Task SeedAsync(WorkflowDbContext context)
    {
        if (await context.WorkflowDefinitions.AnyAsync(w => w.Name == "Document Review" || w.Name == "Document Approval"))
            return;

        // Review Subflow Definition
        var reviewWorkflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Document Review",
            Description = "Generic review process that can be used as a subflow",
            Version = "1.0",
            ClientVersion = "1.*",
            States = new List<WorkflowState>(),  // Initialize empty, will add later
            Transitions = new List<WorkflowTransition>()  // Initialize empty, will add later
        };

        // Save the workflow definition first
        context.WorkflowDefinitions.Add(reviewWorkflow);
        await context.SaveChangesAsync();

        // Create states with views
        var reviewStates = new List<WorkflowState>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Under Review",
                Description = "Initial review state",
                StateType = StateType.Initial,
                WorkflowDefinitionId = reviewWorkflow.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Review Complete",
                Description = "Review has been completed",
                StateType = StateType.Finish,
                WorkflowDefinitionId = reviewWorkflow.Id
            }
        };

        // Add states to context
        context.WorkflowStates.AddRange(reviewStates);
        await context.SaveChangesAsync();
/*
        // Add views separately
        var reviewViews = new List<WorkflowView>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = ViewType.Json,
                Target = ViewTarget.State,
                Version = "1.0.0+0",
                WorkflowVersion = "1.*",
                WorkflowDefinitionId = reviewWorkflow.Id,
                StateId = reviewStates[0].Id,
                Content = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { name = "reviewer", type = "text", label = "Reviewer Name", required = true },
                        new { name = "decision", type = "select", label = "Decision", required = true,
                            options = new object[]
                            {
                                new { value = "approve", label = "Approve" },
                                new { value = "reject", label = "Reject" },
                                new { value = "needsWork", label = "Needs Work" }
                            }
                        },
                        new { name = "comments", type = "textarea", label = "Comments", required = true }
                    }
                })
            }
        };

        context.WorkflowViews.AddRange(reviewViews);
        await context.SaveChangesAsync();
*/
        // Create and add transitions for review workflow
        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Approve",
                Description = "Approve the item",
                FromStateId = reviewStates[0].Id,
                ToStateId = reviewStates[1].Id,
                TriggerType = TriggerType.Manual,
                WorkflowDefinitionId = reviewWorkflow.Id
            }.Apply(t => t.SetTriggerConfig(new
            {
                condition = new
                {
                    field = "decision",
                    @operator = "equals",
                    value = "approve"
                }
            })),
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Reject",
                Description = "Reject the item",
                FromStateId = reviewStates[0].Id,
                ToStateId = reviewStates[1].Id,
                TriggerType = TriggerType.Manual,
                WorkflowDefinitionId = reviewWorkflow.Id
            }.Apply(t => t.SetTriggerConfig(new
            {
                condition = new
                {
                    field = "decision",
                    @operator = "equals",
                    value = "reject"
                }
            })),
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Request Changes",
                Description = "Request changes to the item",
                FromStateId = reviewStates[0].Id,
                ToStateId = reviewStates[1].Id,
                TriggerType = TriggerType.Manual,
                WorkflowDefinitionId = reviewWorkflow.Id
            }.Apply(t => t.SetTriggerConfig(new
            {
                condition = new
                {
                    field = "decision",
                    @operator = "equals",
                    value = "needsWork"
                }
            }))
        };

        // Add transitions to context
        context.WorkflowTransitions.AddRange(transitions);
        await context.SaveChangesAsync();

        // Main Approval Workflow Definition
        var approvalWorkflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Document Approval",
            Description = "Main approval process with review subflow",
            Version = "1.0",
            ClientVersion = "1.*",
            States = new List<WorkflowState>()  // Initialize empty, will add later
        };

        // Save approval workflow first
        context.WorkflowDefinitions.Add(approvalWorkflow);
        await context.SaveChangesAsync();

        // Create approval workflow states
        var approvalStates = new List<WorkflowState>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Draft",
                Description = "Initial draft state",
                StateType = StateType.Initial,
                WorkflowDefinitionId = approvalWorkflow.Id
            },
            new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "In Review",
                Description = "Item is being reviewed",
                StateType = StateType.Subflow,
                WorkflowDefinitionId = approvalWorkflow.Id,
                SubflowConfig = new SubflowConfig
                {
                    Id = Guid.NewGuid(),
                    SubflowDefinitionId = reviewWorkflow.Id,
                    WaitForCompletion = true
                }
            }.Apply(state =>
            {
                state.SubflowConfig!.StateId = state.Id;
                state.SubflowConfig.InputMapping = JsonDocument.Parse("""
                {
                    "title": "$.parentData.title",
                    "description": "$.parentData.description"
                }
                """);
                state.SubflowConfig.OutputMapping = JsonDocument.Parse("""
                {
                    "reviewDecision": "$.childData.decision",
                    "reviewComments": "$.childData.comments"
                }
                """);
            }),
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Approved",
                Description = "Item approved",
                StateType = StateType.Finish,
                SubType = StateSubType.Success,
                WorkflowDefinitionId = approvalWorkflow.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Rejected",
                Description = "Item rejected",
                StateType = StateType.Finish,
                SubType = StateSubType.Error,
                WorkflowDefinitionId = approvalWorkflow.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Revision Needed",
                Description = "Item needs revision",
                StateType = StateType.Intermediate,
                WorkflowDefinitionId = approvalWorkflow.Id
            }
        };

        // Add states to context
        context.WorkflowStates.AddRange(approvalStates);
        await context.SaveChangesAsync();
/*
        // Add approval workflow views
        var approvalViews = new List<WorkflowView>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = ViewType.Json,
                Target = ViewTarget.State,
                Version = "1.0.0+0",
                WorkflowVersion = "1.*",
                WorkflowDefinitionId = approvalWorkflow.Id,
                StateId = approvalWorkflow.States.First().Id,
                Content = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { name = "title", type = "text", label = "Title", required = true },
                        new { name = "description", type = "textarea", label = "Description", required = true },
                        new { name = "author", type = "text", label = "Author", required = true }
                    }
                })
            }
        };

        context.WorkflowViews.AddRange(approvalViews);
        await context.SaveChangesAsync();
*/
        // Add states and transitions for approval workflow
        var approvalWorkflowTransitions = new List<WorkflowTransition>
        {
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Submit for Review",
                Description = "Submit item for review",
                FromStateId = approvalStates[0].Id,
                ToStateId = approvalStates[1].Id,
                TriggerType = TriggerType.Manual,
                WorkflowDefinitionId = approvalWorkflow.Id
            },
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Auto Approve",
                Description = "Automatically approve when review is approved",
                FromStateId = approvalStates[1].Id,
                ToStateId = approvalStates[2].Id,
                TriggerType = TriggerType.Automatic,
                WorkflowDefinitionId = approvalWorkflow.Id
            }.Apply(t => t.SetTriggerConfig(new
            {
                condition = new
                {
                    field = "reviewDecision",
                    @operator = "equals",
                    value = "approve"
                }
            })),
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Auto Reject",
                Description = "Automatically reject when review is rejected",
                FromStateId = approvalStates[1].Id,
                ToStateId = approvalStates[3].Id,
                TriggerType = TriggerType.Automatic,
                WorkflowDefinitionId = approvalWorkflow.Id
            }.Apply(t => t.SetTriggerConfig(new
            {
                condition = new
                {
                    field = "reviewDecision",
                    @operator = "equals",
                    value = "reject"
                }
            })),
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Request Revision",
                Description = "Request revision when review needs work",
                FromStateId = approvalStates[1].Id,
                ToStateId = approvalStates[4].Id,
                TriggerType = TriggerType.Automatic,
                WorkflowDefinitionId = approvalWorkflow.Id
            }.Apply(t => t.SetTriggerConfig(new
            {
                condition = new
                {
                    field = "reviewDecision",
                    @operator = "equals",
                    value = "needsWork"
                }
            })),
            new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                Name = "Resubmit",
                Description = "Resubmit after revision",
                FromStateId = approvalStates[4].Id,
                ToStateId = approvalStates[1].Id,
                TriggerType = TriggerType.Manual,
                WorkflowDefinitionId = approvalWorkflow.Id
            }
        };

        // Add transitions to context
        context.WorkflowTransitions.AddRange(approvalWorkflowTransitions);
        await context.SaveChangesAsync();
    }
}