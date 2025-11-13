using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class ModelTagConfiguration : IEntityTypeConfiguration<ModelTag>
{
    public void Configure(EntityTypeBuilder<ModelTag> builder)
    {
        builder.ToTable("model_tags");

        builder.HasKey(mt => new { mt.ModelId, mt.TagId });

        builder.Property(mt => mt.ModelId)
            .HasColumnName("model_id")
            .IsRequired();

        builder.Property(mt => mt.TagId)
            .HasColumnName("tag_id")
            .IsRequired();

        builder.HasOne(mt => mt.Model)
            .WithMany(m => m.ModelTags)
            .HasForeignKey(mt => mt.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mt => mt.Tag)
            .WithMany(t => t.ModelTags)
            .HasForeignKey(mt => mt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mt => mt.ModelId);
        builder.HasIndex(mt => mt.TagId);
    }
}
