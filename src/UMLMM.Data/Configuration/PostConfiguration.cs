using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Core.Domain.Entities;
using UMLMM.Core.Domain.Enums;

namespace UMLMM.Data.Configuration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        
        builder.Property(p => p.SourceId)
            .HasColumnName("source_id")
            .IsRequired();
        
        builder.Property(p => p.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(p => p.Description)
            .HasColumnName("description");
        
        builder.Property(p => p.Rating)
            .HasColumnName("rating")
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(p => p.ExternalCreatedAt)
            .HasColumnName("external_created_at");
        
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder.HasOne(p => p.Source)
            .WithMany(s => s.Posts)
            .HasForeignKey(p => p.SourceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(p => new { p.SourceId, p.ExternalId })
            .IsUnique();
    }
}
