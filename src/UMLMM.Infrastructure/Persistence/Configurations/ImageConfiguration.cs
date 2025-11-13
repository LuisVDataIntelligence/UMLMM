using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("images");

        builder.HasKey(i => i.ImageId);
        builder.Property(i => i.ImageId).HasColumnName("image_id");

        builder.Property(i => i.ModelId)
            .HasColumnName("model_id");

        builder.Property(i => i.ModelVersionId)
            .HasColumnName("model_version_id");

        builder.Property(i => i.Url)
            .HasColumnName("url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.Rating)
            .HasColumnName("rating")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Width)
            .HasColumnName("width");

        builder.Property(i => i.Height)
            .HasColumnName("height");

        builder.Property(i => i.Sha256)
            .HasColumnName("sha256")
            .HasMaxLength(64);

        builder.Property(i => i.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(i => i.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(i => i.Model)
            .WithMany(m => m.Images)
            .HasForeignKey(i => i.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ModelVersion)
            .WithMany(mv => mv.Images)
            .HasForeignKey(i => i.ModelVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.ModelId);
        builder.HasIndex(i => i.ModelVersionId);
        builder.HasIndex(i => i.Sha256);
    }
}
