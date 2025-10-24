using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class ModelVersionConfiguration : IEntityTypeConfiguration<ModelVersion>
{
    public void Configure(EntityTypeBuilder<ModelVersion> builder)
    {
        builder.ToTable("model_versions");

        builder.HasKey(mv => mv.ModelVersionId);
        builder.Property(mv => mv.ModelVersionId).HasColumnName("model_version_id");

        builder.Property(mv => mv.ModelId)
            .HasColumnName("model_id")
            .IsRequired();

        builder.Property(mv => mv.VersionLabel)
            .HasColumnName("version_label")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(mv => mv.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(mv => mv.Checksum)
            .HasColumnName("checksum")
            .HasMaxLength(255);

        builder.Property(mv => mv.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(mv => mv.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(mv => mv.Model)
            .WithMany(m => m.ModelVersions)
            .HasForeignKey(mv => mv.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mv => mv.ModelId);
        builder.HasIndex(mv => new { mv.ModelId, mv.VersionLabel }).IsUnique();
    }
}
