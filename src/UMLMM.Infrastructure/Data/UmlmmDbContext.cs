using Microsoft.EntityFrameworkCore;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Data;

public class UmlmmDbContext : DbContext
{
    public UmlmmDbContext(DbContextOptions<UmlmmDbContext> options) : base(options)
    {
    }

    public DbSet<Image> Images { get; set; } = default!;
    public DbSet<Tag> Tags { get; set; } = default!;
    public DbSet<ImageTag> ImageTags { get; set; } = default!;
    public DbSet<FetchRun> FetchRuns { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Image configuration
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceId, e.ExternalId }).IsUnique();
            entity.HasIndex(e => e.Sha256);
            entity.Property(e => e.SourceId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sha256).HasMaxLength(64);
            entity.Property(e => e.Rating).HasMaxLength(20);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ImageTag configuration (many-to-many)
        modelBuilder.Entity<ImageTag>(entity =>
        {
            entity.HasKey(e => new { e.ImageId, e.TagId });
            
            entity.HasOne(e => e.Image)
                .WithMany(i => i.ImageTags)
                .HasForeignKey(e => e.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ImageTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // FetchRun configuration
        modelBuilder.Entity<FetchRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RunId);
            entity.HasIndex(e => new { e.SourceId, e.StartedAt });
            entity.Property(e => e.RunId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Parameters).HasColumnType("jsonb");
        });
    }
}
