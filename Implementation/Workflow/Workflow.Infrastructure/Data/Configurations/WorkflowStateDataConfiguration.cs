using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workflow.Domain.Models;

namespace Workflow.Infrastructure.Data.Configurations;

public class WorkflowStateDataConfiguration : IEntityTypeConfiguration<WorkflowStateData>
{
    public void Configure(EntityTypeBuilder<WorkflowStateData> builder)
    {
        builder.Property(x => x._data)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");
    }
} 