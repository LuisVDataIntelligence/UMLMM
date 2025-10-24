using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Configuration;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");
        
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        
        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.HasIndex(s => s.Name)
            .IsUnique();
    }
}
