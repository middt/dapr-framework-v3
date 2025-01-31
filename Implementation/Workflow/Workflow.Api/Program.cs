using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Infrastructure.Repositories;
using Dapr.Client;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Application.Services;
using Dapr.Framework.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Dapr.Framework.Telemetry.Configuration;
using Dapr.Framework.Api.Configuration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Workflow.Infrastructure.Data;
using Workflow.Domain.Repositories;
using Workflow.Infrastructure.Repositories;
using Workflow.Application.Services;
using Workflow.Application;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Configure telemetry using framework's configuration
builder.Services.AddHttpContextAccessor();
var serviceProvider = builder.Services.BuildServiceProvider();
builder.Services.AddFrameworkTelemetry(builder.Configuration, (loggerConfig, telemetryOptions) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    //loggerConfig.Enrich.With(new ProductsLogEnricher(
    //    customName: "x-custom-app-name",
    //    configuration: builder.Configuration,
    //    httpContextAccessor: httpContextAccessor
    //));
});

// Option 1: Use .NET Core Distributed Cache
builder.Services.AddNetCoreDistributedCache(services =>
{
    // Configure your preferred distributed cache implementation
    services.AddDistributedMemoryCache(); // Default in-memory
});

// Option 2: Use Dapr State Store Cache
// builder.Services.AddDaprStateStoreCache();

// Add Redis Configuration
builder.Services.AddRedis(builder.Configuration);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add API Versioning using the custom configuration
builder.Services.AddCustomApiVersioning(apiTitle: "Workflow API");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Replace the in-memory database with PostgreSQL
builder.Services.AddDbContext<WorkflowDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Host=localhost;Database=workflowdb;Username=workflow;Password=workflow123";
    
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("Workflow.Infrastructure");
        // Enable retrying failed database operations
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

// Register DbContext as base type for generic repository
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<WorkflowDbContext>());

// Configure Dapr
builder.Services.AddDaprClient();
builder.Services.AddScoped<DaprClient>(sp => new DaprClientBuilder().Build());

// Register repositories
builder.Services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
builder.Services.AddScoped<IWorkflowStateRepository, WorkflowStateRepository>();
builder.Services.AddScoped<IWorkflowStateDataRepository, WorkflowStateDataRepository>();
builder.Services.AddScoped<IWorkflowTransitionRepository, WorkflowTransitionRepository>();
builder.Services.AddScoped<IWorkflowViewRepository, WorkflowViewRepository>();
builder.Services.AddScoped<IWorkflowHumanTaskRepository, WorkflowHumanTaskRepository>();
builder.Services.AddScoped<IWorkflowFunctionRepository, WorkflowFunctionRepository>();
builder.Services.AddScoped<IWorkflowCorrelationRepository, WorkflowCorrelationRepository>();
builder.Services.AddScoped<IWorkflowInstanceDataRepository, WorkflowInstanceDataRepository>();
builder.Services.AddScoped<IWorkflowInstanceTaskRepository, WorkflowInstanceTaskRepository>();
builder.Services.AddScoped<IWorkflowTaskRepository, WorkflowTaskRepository>();
builder.Services.AddScoped<IWorkflowTaskAssignmentRepository, WorkflowTaskAssignmentRepository>();
// Register services
builder.Services.AddScoped<IWorkflowDefinitionService, WorkflowDefinitionService>();
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowInstanceService>();
builder.Services.AddScoped<IWorkflowFunctionService, WorkflowFunctionService>();
builder.Services.AddScoped<IWorkflowStateService, WorkflowStateService>();
builder.Services.AddScoped<IWorkflowStateDataService, WorkflowStateDataService>();
builder.Services.AddScoped<IWorkflowTransitionService, WorkflowTransitionService>();
builder.Services.AddScoped<IWorkflowViewService, WorkflowViewService>();
builder.Services.AddScoped<IWorkflowHumanTaskService, WorkflowHumanTaskService>();
builder.Services.AddScoped<WorkflowTaskProcessor>();
builder.Services.AddHttpClient();

// Register Framework Services
builder.Services.AddScoped<IDistributedLockService, DaprDistributedLockService>();
// Register distributed lock service
// builder.services.AddScoped<IDistributedLockService, RedisDistributedLockService>();

builder.Services.AddScoped<ITransactionService, EfTransactionService>();


// Add hosted services
builder.Services.AddHostedService<WorkflowTransitionProcessor>();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrated successfully");
        }
        
        // Seed initial data
        await TaskDbSeed.SeedAsync(dbContext);
        await SubFlowDbSeed.SeedAsync(dbContext);
        logger.LogInformation("Database seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

app.UseCustomApiVersioning();

app.UseCloudEvents();
app.MapSubscribeHandler();

// Map Controllers
app.MapControllers();

app.Run();
