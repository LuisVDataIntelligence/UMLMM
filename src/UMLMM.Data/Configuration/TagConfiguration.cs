using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Configuration;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");
        
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        
        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(t => t.Category)
            .HasColumnName("category")
            .HasMaxLength(50);
        
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
