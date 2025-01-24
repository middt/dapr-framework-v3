using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Tasks;
using Workflow.Domain.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace Workflow.Infrastructure.Data;

public static class TaskDbSeed
{
    public static async Task SeedAsync(WorkflowDbContext context)
    {
        if (await context.Set<WorkflowTask>().OfType<HumanTask>().AnyAsync() ||
            await context.Set<WorkflowTask>().OfType<DaprBindingTask>().AnyAsync() ||
            await context.Set<WorkflowTask>().OfType<DaprServiceTask>().AnyAsync())
            return;

        if (await context.WorkflowDefinitions.AnyAsync(w => w.Name == "Document Processing"))
            return;

        // Document Processing Workflow - Version 1.0
        var documentWorkflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Document Processing",
            Description = "Document processing workflow with manual, automatic, and scheduled transitions",
            Version = "1.0",
            ClientVersion = "1.*"
        };

        // Create states without adding them to the workflow definition yet
        var states = new List<WorkflowState>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Draft",
                Description = "Initial document draft",
                StateType = StateType.Initial,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Review",
                Description = "Document under review",
                StateType = StateType.Intermediate,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Approved",
                Description = "Document approved and ready for processing",
                StateType = StateType.Intermediate,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Processing",
                Description = "Document is being processed",
                StateType = StateType.Intermediate,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Published",
                Description = "Document has been published",
                StateType = StateType.Intermediate,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Archived",
                Description = "Document has been archived",
                StateType = StateType.Finish,
                WorkflowDefinition = documentWorkflow
            }
        };

        // Create views for states
        foreach (var state in states)
        {
            var view = new WorkflowView
            {
                Id = Guid.NewGuid(),
                Type = ViewType.Json,
                Target = ViewTarget.State,
                Version = "1.0.0+0",
                WorkflowVersion = "1.*",
                Content = JsonSerializer.Serialize(new
                {
                    fields = new object[]
                    {
                        new { name = "title", type = "text", label = "Document Title", required = true },
                        new { name = "content", type = "textarea", label = "Document Content", required = true },
                        new { name = "author", type = "text", label = "Author", required = true }
                    }
                }),
                CreatedAt = DateTime.UtcNow,
                WorkflowDefinition = documentWorkflow
            };
            state.Views.Add(view);
        }

        // Create transitions
        var transitions = new List<WorkflowTransition>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Submit for Review",
                Description = "Submit document for review",
                FromState = states[0],
                ToState = states[1],
                TriggerType = TriggerType.Manual,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Return for Revision",
                Description = "Return document for revision",
                FromState = states[1],
                ToState = states[0],
                TriggerType = TriggerType.Manual,
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Auto Approve",
                Description = "Automatically approve document when review status is approved",
                FromState = states[1],
                ToState = states[2],
                TriggerType = TriggerType.Automatic,
                TriggerConfig = JsonSerializer.SerializeToDocument(new
                {
                    type = "condition",
                    condition = new
                    {
                        field = "status",
                        @operator = "equals",
                        value = "approved",
                        path = "$.data"
                    }
                }),
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Start Processing",
                Description = "Start processing approved document after 10 seconds",
                FromState = states[2],
                ToState = states[3],
                TriggerType = TriggerType.Scheduled,
                TriggerConfig = JsonSerializer.SerializeToDocument(new
                {
                    type = "schedule",
                    schedule = new
                    {
                        type = "delay",
                        duration = "PT10S",
                        timeZone = "UTC"
                    }
                }),
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Publish Document",
                Description = "Publish processed document after 30 seconds",
                FromState = states[3],
                ToState = states[4],
                TriggerType = TriggerType.Scheduled,
                TriggerConfig = JsonSerializer.SerializeToDocument(new
                {
                    type = "schedule",
                    schedule = new
                    {
                        type = "delay",
                        duration = "PT30S",
                        timeZone = "UTC"
                    }
                }),
                WorkflowDefinition = documentWorkflow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Archive Inactive",
                Description = "Archive document after 5 minutes of inactivity",
                FromState = states[0],
                ToState = states[5],
                TriggerType = TriggerType.Scheduled,
                TriggerConfig = JsonSerializer.SerializeToDocument(new
                {
                    type = "schedule",
                    schedule = new
                    {
                        type = "delay",
                        duration = "PT5M",
                        timeZone = "UTC"
                    }
                }),
                WorkflowDefinition = documentWorkflow
            }
        };

        // Add tasks to the Review state
        var reviewState = states.First(s => s.Name == "Review");
        var publishedState = states.First(s => s.Name == "Published");
        
        var reviewTask = new HumanTask
        {
            Id = Guid.NewGuid(),
            Name = "Review Document",
            Description = "Review the submitted document",
            Trigger = TaskTrigger.OnEntry,
            Type = Workflow.Domain.Models.Tasks.TaskType.Human,
            CreatedAt = DateTime.UtcNow,
            Title = "Document Review Required",
            Instructions = "Please review the submitted document and approve or reject it.",
            AssignedTo = "reviewer",
            Form = JsonDocument.Parse(@"{
                ""fields"": [
                    {
                        ""name"": ""status"",
                        ""type"": ""select"",
                        ""options"": [""approved"", ""rejected""]
                    },
                    {
                        ""name"": ""comments"",
                        ""type"": ""textarea""
                    }
                ]
            }").RootElement.ToString(),
            EscalationAssignee = "supervisor",
            ReminderIntervalMinutes = 60,
            EscalationTimeoutMinutes = 1440,
            Config = JsonDocument.Parse(@"{
                ""title"": ""Document Review Required"",
                ""instructions"": ""Please review the submitted document and approve or reject it."",
                ""assignedTo"": ""reviewer"",
                ""form"": {
                    ""fields"": [
                        {
                            ""name"": ""status"",
                            ""type"": ""select"",
                            ""options"": [""approved"", ""rejected""]
                        },
                        {
                            ""name"": ""comments"",
                            ""type"": ""textarea""
                        }
                    ]
                },
                ""reminderIntervalMinutes"": 60,
                ""escalationTimeoutMinutes"": 1440,
                ""escalationAssignee"": ""supervisor""
            }").RootElement.ToString()
        };
        reviewState.Tasks.Add(reviewTask);

        // Add email notification task to Published state
        var emailTask = new DaprBindingTask
        {
            Id = Guid.NewGuid(),
            Name = "Send Publication Notification",
            Description = "Send email notification when document is published",
            Trigger = TaskTrigger.OnEntry,
            Type = Workflow.Domain.Models.Tasks.TaskType.DaprBinding,
            CreatedAt = DateTime.UtcNow,
            BindingName = "smtp",
            Operation = "create",
            Metadata = JsonSerializer.Serialize(new
            {
                emailFrom = "workflow@example.com",
                emailTo = "{{data.author}}",
                subject = "Document Published: {{data.title}}"
            }),
            Config = JsonSerializer.Serialize(new
            {
                data = """
                    Dear {{data.author}},

                    Your document '{{data.title}}' has been successfully published.

                    Document Content:
                    {{data.content}}

                    Regards,
                    Workflow System
                    """
            })
        };
        publishedState.Tasks.Add(emailTask);

        // Add standalone HTTP task using httpbin external service
        var httpTask = new HttpTask
        {
            Id = Guid.NewGuid(),
            Name = "Test External API",
            Description = "Test external API call using httpbin",
            Type = Workflow.Domain.Models.Tasks.TaskType.Http,
            Trigger = TaskTrigger.Manual,
            CreatedAt = DateTime.UtcNow,
            WorkflowStateId = null,
            Url = "https://httpbin.org/post",
            Method = "POST",
            Config = JsonSerializer.Serialize(new
            {
                headers = new Dictionary<string, string>
                {
                    { "x-custom-header", "value" },
                    { "Content-Type", "application/json" }
                },
                data = new
                {
                    message = "Hello from standalone task",
                    timestamp = DateTime.UtcNow.ToString("O")
                }
            })
        };

        var workflowFunction = new WorkflowFunction
        {
            Id = Guid.NewGuid(),
            Name = "Test External API Function",
            Description = "Function wrapping the external API call",
            TaskId = httpTask.Id,  // Reference to the HttpTask
            IsActive = true,
            EnrichStateData = true,
            Order = 1
        };

        // Add external service task using Dapr binding
        var externalServiceTask = new DaprBindingTask
        {
            Id = Guid.NewGuid(),
            Name = "External Service Call",
            Description = "Call external service through Dapr binding",
            Type = Workflow.Domain.Models.Tasks.TaskType.DaprBinding,
            Trigger = TaskTrigger.Manual,
            CreatedAt = DateTime.UtcNow,
            WorkflowStateId = null,
            BindingName = "http-output",
            Operation = "get",
            Metadata = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }),
            Config = JsonSerializer.Serialize(new
            {
                data = new
                {
                    message = "Hello from Dapr binding",
                    timestamp = "{{workflow.timestamp}}"
                }
            })
        };

        var externalServiceFunction = new WorkflowFunction
        {
            Id = Guid.NewGuid(),
            Name = "External Service Function",
            Description = "Function wrapping the external service call through Dapr binding",
            TaskId = externalServiceTask.Id,
            IsActive = true,
            EnrichStateData = true,
            Order = 1
        };

        // Add HTTPEndpoint task example
        var httpEndpointTask = new DaprHttpEndpointTask
        {
            Id = Guid.NewGuid(),
            Name = "External API via HTTPEndpoint",
            Description = "Call external API through Dapr HTTPEndpoint",
            Type = Workflow.Domain.Models.Tasks.TaskType.DaprHttpEndpoint,
            Trigger = TaskTrigger.Manual,
            CreatedAt = DateTime.UtcNow,
            WorkflowStateId = null,
            EndpointName = "httpbin",
            Path = "/post",
            Method = "POST",
            Headers = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "x-custom-header", "value" }
            }),
            Config = JsonSerializer.Serialize(new
            {
                data = new
                {
                    message = "Hello from HTTPEndpoint",
                    timestamp = "{{workflow.timestamp}}"
                }
            })
        };

        var httpEndpointFunction = new WorkflowFunction
        {
            Id = Guid.NewGuid(),
            Name = "External Service via HTTPEndpoint",
            Description = "Function wrapping the external service call through HTTPEndpoint",
            TaskId = httpEndpointTask.Id,
            IsActive = true,
            EnrichStateData = true,
            Order = 1
        };

        // Save everything
        documentWorkflow.States = states;
        documentWorkflow.Transitions = transitions;
        await context.WorkflowDefinitions.AddAsync(documentWorkflow);
        await context.Set<DaprBindingTask>().AddAsync(emailTask);
        await context.Set<HttpTask>().AddAsync(httpTask);
        await context.Set<WorkflowFunction>().AddAsync(workflowFunction);
        await context.Set<DaprBindingTask>().AddAsync(externalServiceTask);
        await context.Set<WorkflowFunction>().AddAsync(externalServiceFunction);
        await context.Set<DaprHttpEndpointTask>().AddAsync(httpEndpointTask);
        await context.Set<WorkflowFunction>().AddAsync(httpEndpointFunction);
        await context.SaveChangesAsync();
    }

    // Keep the sync version for backward compatibility
    public static void Initialize(WorkflowDbContext context)
    {
        SeedAsync(context).GetAwaiter().GetResult();
    }
}