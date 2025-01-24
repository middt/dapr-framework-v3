using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Workflow.Domain.Models;
using Workflow.Domain.Repositories;
using Workflow.Domain.Extensions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Workflow.Application.Services;

public class WorkflowTransitionProcessor : BackgroundService
{
    private readonly ILogger<WorkflowTransitionProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);


    public WorkflowTransitionProcessor(
        ILogger<WorkflowTransitionProcessor> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTransitionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Exit gracefully when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing workflow transitions");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Exit gracefully when cancellation is requested during delay
                break;
            }
        }
    }

    private async Task ProcessTransitionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var instanceRepo = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
        var stateDataRepo = scope.ServiceProvider.GetRequiredService<IWorkflowStateDataRepository>();
        var transitionRepo = scope.ServiceProvider.GetRequiredService<IWorkflowTransitionRepository>();
        var instanceService = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceService>();

        // Get all active instances
        var activeInstances = await instanceRepo.GetActiveInstancesAsync();

        foreach (var instance in activeInstances)
        {
            // Check for cancellation between instances
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                // Get current state data
                var currentStateData = await stateDataRepo.GetLatestByInstanceIdAsync(instance.Id);
                if (currentStateData == null) continue;

                // Get available transitions
                var transitions = await transitionRepo.GetByFromStateIdAsync(instance.CurrentStateId);

                foreach (var transition in transitions)
                {
                    // Check for cancellation between transitions
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await ProcessTransitionAsync(instance, transition, currentStateData, instanceService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing transition {TransitionId} for instance {InstanceId}",
                            transition.Id, instance.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing instance {InstanceId}", instance.Id);
            }
        }
    }

    private async Task ProcessTransitionAsync(
        WorkflowInstance instance,
        WorkflowTransition transition,
        WorkflowStateData stateData,
        IWorkflowInstanceService instanceService)
    {
        using var scope = _serviceProvider.CreateScope();
        var stateDataService = scope.ServiceProvider.GetRequiredService<IWorkflowStateDataService>();
        var transitionService = scope.ServiceProvider.GetRequiredService<IWorkflowTransitionService>();

        _logger.LogInformation(
            "Processing transition {TransitionId} of type {TriggerType} with config: {Config}",
            transition.Id,
            transition.TriggerType,
            transition.TriggerConfig?.RootElement.ToString() ?? "null");

        switch (transition.TriggerType)
        {
            case TriggerType.Automatic:
                await ProcessAutomaticTransitionAsync(transition, instance, stateDataService, transitionService);
                break;

            case TriggerType.Scheduled:
                await ProcessScheduledTransitionAsync(transition, instance, transitionService);
                break;
        }
    }

    private async Task ProcessAutomaticTransitionAsync(
        WorkflowTransition transition, 
        WorkflowInstance instance,
        IWorkflowStateDataService stateDataService,
        IWorkflowTransitionService transitionService)
    {
        var config = transition.TriggerConfig;
        if (!config.RootElement.TryGetProperty("condition", out var conditionElement))
        {
            _logger.LogWarning("Invalid automatic transition config for transition {TransitionId}", transition.Id);
            return;
        }

        var stateData = await stateDataService.GetStateDataAsync(instance.Id, instance.CurrentStateId);
        if (stateData == null)
        {
            _logger.LogWarning("No state data found for instance {InstanceId}", instance.Id);
            return;
        }

        var field = conditionElement.GetProperty("field").GetString();
        var op = conditionElement.GetProperty("operator").GetString();
        var value = conditionElement.GetProperty("value").GetString();
        var path = conditionElement.GetProperty("path").GetString() ?? "$.data";

        // Parse the state data JSON
        var stateDataJson = stateData.GetDataAsJson();
        
        // Use JsonPath to get the field value
        var fieldValue = stateDataJson.GetProperty(field).GetString();

        bool conditionMet = op switch
        {
            "equals" => fieldValue == value,
            "notEquals" => fieldValue != value,
            "contains" => fieldValue?.Contains(value) == true,
            "startsWith" => fieldValue?.StartsWith(value) == true,
            "endsWith" => fieldValue?.EndsWith(value) == true,
            _ => false
        };

        if (conditionMet)
        {
            await transitionService.ExecuteTransitionAsync(instance.Id, transition.Id);
        }
    }

    private async Task ProcessScheduledTransitionAsync(
        WorkflowTransition transition, 
        WorkflowInstance instance,
        IWorkflowTransitionService transitionService)
    {
        var config = transition.TriggerConfig;
        if (!config.RootElement.TryGetProperty("schedule", out var scheduleElement))
        {
            _logger.LogWarning("Invalid schedule transition config for transition {TransitionId}", transition.Id);
            return;
        }

        var scheduleType = scheduleElement.GetProperty("type").GetString();
        if (scheduleType != "delay")
        {
            _logger.LogWarning("Invalid schedule type {ScheduleType} for transition {TransitionId}", scheduleType, transition.Id);
            return;
        }

        var duration = scheduleElement.GetProperty("duration").GetString();
        var timeZone = scheduleElement.GetProperty("timeZone").GetString() ?? "UTC";
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

        if (ShouldTransitionForDelay(duration, instance, tz))
        {
            await transitionService.ExecuteTransitionAsync(instance.Id, transition.Id);
        }
    }

    private bool ShouldTransitionForDelay(string duration, WorkflowInstance instance, TimeZoneInfo tz)
    {
        var referenceTime = instance.UpdatedAt;
        var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        
        try
        {
            var timeSpan = ParseIsoDuration(duration);
            return currentTime >= referenceTime.Add(timeSpan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing duration: {Duration}", duration);
            return false;
        }
    }

    private static readonly Regex DurationRegex = new(
        @"^P(?:(\d+)D)?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?)?$",
        RegexOptions.Compiled);

    private TimeSpan ParseIsoDuration(string duration)
    {
        var match = DurationRegex.Match(duration);
        if (!match.Success)
        {
            _logger.LogWarning("Invalid ISO 8601 duration format: {Duration}", duration);
            throw new ArgumentException($"Invalid ISO 8601 duration format: {duration}");
        }

        var days = ParseGroup(match.Groups[1]);
        var hours = ParseGroup(match.Groups[2]);
        var minutes = ParseGroup(match.Groups[3]);
        var seconds = ParseGroup(match.Groups[4]);

        return new TimeSpan(
            days,    // days
            hours,   // hours
            minutes, // minutes
            seconds  // seconds
        );
    }

    private static int ParseGroup(Group group)
    {
        return group.Success ? int.Parse(group.Value) : 0;
    }
}