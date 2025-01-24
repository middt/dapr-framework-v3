using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workflow.Domain.Models;

namespace Workflow.Infrastructure.Data.Configurations;

public class SubflowConfigConfiguration : IEntityTypeConfiguration<SubflowConfig>
{
    public void Configure(EntityTypeBuilder<SubflowConfig> builder)
    {
        builder.Property(x => x._inputMapping)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(x => x._outputMapping)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");
    }
} 