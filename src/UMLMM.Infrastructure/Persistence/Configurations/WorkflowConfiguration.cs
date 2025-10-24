using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("workflows");

        builder.HasKey(w => w.WorkflowId);
        builder.Property(w => w.WorkflowId).HasColumnName("workflow_id");

        builder.Property(w => w.SourceId)
            .HasColumnName("source_id")
            .IsRequired();

        builder.Property(w => w.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(w => w.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(w => w.Graph)
            .HasColumnName("graph")
            .HasColumnType("jsonb");

        builder.Property(w => w.NodesCount)
            .HasColumnName("nodes_count");

        builder.Property(w => w.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(w => w.Source)
            .WithMany(s => s.Workflows)
            .HasForeignKey(w => w.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.SourceId, w.ExternalId }).IsUnique();
        builder.HasIndex(w => w.SourceId);
    }
}
