using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.ToTable("models");

        builder.HasKey(m => m.ModelId);
        builder.Property(m => m.ModelId).HasColumnName("model_id");

        builder.Property(m => m.SourceId)
            .HasColumnName("source_id")
            .IsRequired();

        builder.Property(m => m.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnName("description");

        builder.Property(m => m.NsfwRating)
            .HasColumnName("nsfw_rating")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(m => m.Raw)
            .HasColumnName("raw")
            .HasColumnType("jsonb");

        builder.HasOne(m => m.Source)
            .WithMany(s => s.Models)
            .HasForeignKey(m => m.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.SourceId, m.ExternalId }).IsUnique();
        builder.HasIndex(m => m.SourceId);
        builder.HasIndex(m => m.Type);
        builder.HasIndex(m => m.CreatedAtUtc);
    }
}
