using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Configuration;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("images");
        
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        
        builder.Property(i => i.PostId)
            .HasColumnName("post_id")
            .IsRequired();
        
        builder.Property(i => i.Sha256)
            .HasColumnName("sha256")
            .HasMaxLength(64);
        
        builder.Property(i => i.Url)
            .HasColumnName("url")
            .HasMaxLength(500);
        
        builder.Property(i => i.SampleUrl)
            .HasColumnName("sample_url")
            .HasMaxLength(500);
        
        builder.Property(i => i.Width)
            .HasColumnName("width");
        
        builder.Property(i => i.Height)
            .HasColumnName("height");
        
        builder.Property(i => i.FileSize)
            .HasColumnName("file_size");
        
        builder.Property(i => i.FileExtension)
            .HasColumnName("file_extension")
            .HasMaxLength(10);
        
        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.HasOne(i => i.Post)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(i => i.Sha256);
    }
}
