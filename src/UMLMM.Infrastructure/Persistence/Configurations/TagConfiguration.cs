using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.TagId);
        builder.Property(t => t.TagId).HasColumnName("tag_id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.SourceId)
            .HasColumnName("source_id");

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(t => t.Source)
            .WithMany(s => s.Tags)
            .HasForeignKey(t => t.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.NormalizedName);
        builder.HasIndex(t => new { t.NormalizedName, t.SourceId }).IsUnique();
    }
}
