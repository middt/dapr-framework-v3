using Microsoft.EntityFrameworkCore;
using Workflow.Domain.Models;
using Workflow.Domain.Models.Views;
using Workflow.Domain.Models.Tasks;

namespace Workflow.Infrastructure.Data;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } = null!;
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; } = null!;
    public DbSet<WorkflowState> WorkflowStates { get; set; } = null!;
    public DbSet<WorkflowTransition> WorkflowTransitions { get; set; } = null!;
    public DbSet<WorkflowStateData> WorkflowStateData { get; set; } = null!;
    public DbSet<WorkflowHumanTask> WorkflowHumanTasks { get; set; } = null!;
    public DbSet<WorkflowView> WorkflowViews { get; set; } = null!;
    public DbSet<WorkflowCorrelation> WorkflowCorrelations { get; set; } = null!;
    public DbSet<WorkflowInstanceData> WorkflowInstanceData { get; set; } = null!;
    public DbSet<WorkflowInstanceTask> WorkflowInstanceTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WorkflowTask inheritance using Table-Per-Type (TPT)
        modelBuilder.Entity<WorkflowTask>(entity =>
        {
            entity.ToTable("WorkflowTasks");  // Base table for common properties

            // Configure task execution properties
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.Result)
                .HasColumnType("jsonb");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Config)
                .HasColumnType("jsonb");
        });

        // Configure task-specific tables
        modelBuilder.Entity<HumanTask>()
            .ToTable("HumanTasks")
            .Property(t => t.Form)
            .HasColumnType("jsonb");

        modelBuilder.Entity<DaprBindingTask>()
            .ToTable("DaprBindingTasks")
            .Property(t => t.Metadata)
            .HasColumnType("jsonb");

        modelBuilder.Entity<DaprPubSubTask>()
            .ToTable("DaprPubSubTasks");

        modelBuilder.Entity<DaprServiceTask>()
            .ToTable("DaprServiceTasks")
            .Property(t => t.Data)
            .HasColumnType("jsonb");

        modelBuilder.Entity<HttpTask>()
            .ToTable("HttpTasks");

        modelBuilder.Entity<DaprHttpEndpointTask>()
            .ToTable("DaprHttpEndpointTasks")
            .Property(t => t.Headers)
            .HasColumnType("jsonb");

        // Configure WorkflowFunction as a derived type of HttpTask
        modelBuilder.Entity<WorkflowFunction>()
            .ToTable("WorkflowFunctions");

        // Configure the relationship between WorkflowFunction and WorkflowState
        modelBuilder.Entity<WorkflowFunction>()
            .HasMany(f => f.States)
            .WithMany()
            .UsingEntity(j => j.ToTable("WorkflowFunctionStates"));

        // Configure the relationship between WorkflowFunction and WorkflowDefinition
        modelBuilder.Entity<WorkflowFunction>()
            .HasOne(f => f.WorkflowDefinition)
            .WithMany()
            .HasForeignKey(f => f.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure SubflowConfig relationship
        modelBuilder.Entity<WorkflowState>()
            .HasOne(s => s.SubflowConfig)
            .WithOne(c => c.State)
            .HasForeignKey<SubflowConfig>(c => c.StateId);

        // Configure WorkflowCorrelation relationships
        modelBuilder.Entity<WorkflowCorrelation>()
            .HasOne(c => c.ParentInstance)
            .WithMany(i => i.ChildCorrelations)
            .HasForeignKey(c => c.ParentInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<WorkflowCorrelation>()
            .HasOne(c => c.SubflowInstance)
            .WithOne(i => i.ParentCorrelation)
            .HasForeignKey<WorkflowCorrelation>(c => c.SubflowInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<WorkflowCorrelation>()
            .HasOne(c => c.ParentState)
            .WithMany()
            .HasForeignKey(c => c.ParentStateId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure WorkflowFunction relationship with WorkflowTask
        modelBuilder.Entity<WorkflowFunction>()
            .HasOne(f => f.Task)
            .WithMany()
            .HasForeignKey(f => f.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add any specific model configurations here
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowDbContext).Assembly);

        modelBuilder.Entity<WorkflowInstanceData>()
            .HasOne(d => d.WorkflowInstance)
            .WithOne()
            .HasForeignKey<WorkflowInstanceData>(d => d.WorkflowInstanceId);

        modelBuilder.Entity<WorkflowInstanceTask>()
            .Property(t => t.Result)
            .HasColumnType("jsonb");
    }
}