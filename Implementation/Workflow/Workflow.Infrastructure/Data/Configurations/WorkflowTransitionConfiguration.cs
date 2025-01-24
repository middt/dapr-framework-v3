using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workflow.Domain.Models;

namespace Workflow.Infrastructure.Data.Configurations;

public class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.Property(x => x._triggerConfig)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");
    }
} 