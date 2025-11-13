using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class ArtifactConfiguration : IEntityTypeConfiguration<Artifact>
{
    public void Configure(EntityTypeBuilder<Artifact> builder)
    {
        builder.ToTable("artifacts");

        builder.HasKey(a => a.ArtifactId);
        builder.Property(a => a.ArtifactId).HasColumnName("artifact_id");

        builder.Property(a => a.ModelVersionId)
            .HasColumnName("model_version_id")
            .IsRequired();

        builder.Property(a => a.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.SizeBytes)
            .HasColumnName("size_bytes")
            .IsRequired();

        builder.Property(a => a.Sha256)
            .HasColumnName("sha256")
            .HasMaxLength(64);

        builder.Property(a => a.Url)
            .HasColumnName("url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(a => a.ModelVersion)
            .WithMany(mv => mv.Artifacts)
            .HasForeignKey(a => a.ModelVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.ModelVersionId);
        builder.HasIndex(a => a.Sha256);
    }
}
