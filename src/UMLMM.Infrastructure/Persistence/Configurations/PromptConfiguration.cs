using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.ToTable("prompts");

        builder.HasKey(p => p.PromptId);
        builder.Property(p => p.PromptId).HasColumnName("prompt_id");

        builder.Property(p => p.ModelId)
            .HasColumnName("model_id");

        builder.Property(p => p.ModelVersionId)
            .HasColumnName("model_version_id");

        builder.Property(p => p.SourceId)
            .HasColumnName("source_id")
            .IsRequired();

        builder.Property(p => p.Text)
            .HasColumnName("text")
            .IsRequired();

        builder.Property(p => p.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(p => p.Model)
            .WithMany(m => m.Prompts)
            .HasForeignKey(p => p.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ModelVersion)
            .WithMany(mv => mv.Prompts)
            .HasForeignKey(p => p.ModelVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Source)
            .WithMany(s => s.Prompts)
            .HasForeignKey(p => p.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ModelId);
        builder.HasIndex(p => p.ModelVersionId);
        builder.HasIndex(p => p.SourceId);
    }
}
