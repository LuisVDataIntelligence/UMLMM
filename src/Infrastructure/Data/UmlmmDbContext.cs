using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class UmlmmDbContext : DbContext
{
    public UmlmmDbContext(DbContextOptions<UmlmmDbContext> options) : base(options)
    {
    }

    public DbSet<Model> Models { get; set; }
    public DbSet<ModelVersion> ModelVersions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ModelTag> ModelTags { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Run> Runs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Model entity configuration
        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.HasIndex(e => e.Name);
        });

        // ModelVersion entity configuration
        modelBuilder.Entity<ModelVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Model)
                .WithMany(m => m.Versions)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ModelId, e.VersionName });
        });

        // Tag entity configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ModelTag entity configuration (many-to-many)
        modelBuilder.Entity<ModelTag>(entity =>
        {
            entity.HasKey(e => new { e.ModelId, e.TagId });
            entity.HasOne(e => e.Model)
                .WithMany(m => m.ModelTags)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ModelTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Image entity configuration
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Hash).HasMaxLength(64);
            entity.HasOne(e => e.ModelVersion)
                .WithMany(mv => mv.Images)
                .HasForeignKey(e => e.ModelVersionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Hash);
            entity.HasIndex(e => e.Rating);
            entity.HasIndex(e => e.ModelVersionId);
        });

        // Run entity configuration
        modelBuilder.Entity<Run>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WorkflowName).HasMaxLength(200);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
        });
    }
}
