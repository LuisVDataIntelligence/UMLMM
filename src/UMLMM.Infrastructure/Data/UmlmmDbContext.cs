using Microsoft.EntityFrameworkCore;
using UMLMM.Core.Entities;

namespace UMLMM.Infrastructure.Data;

public class UmlmmDbContext : DbContext
{
    public UmlmmDbContext(DbContextOptions<UmlmmDbContext> options) : base(options)
    {
    }

    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<ModelVersion> ModelVersions => Set<ModelVersion>();
    public DbSet<ModelArtifact> ModelArtifacts => Set<ModelArtifact>();
    public DbSet<FetchRun> FetchRuns => Set<FetchRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Source configuration
        modelBuilder.Entity<Source>(entity =>
        {
            entity.ToTable("sources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Model configuration
        modelBuilder.Entity<Model>(entity =>
        {
            entity.ToTable("models");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasIndex(e => new { e.SourceId, e.ExternalId }).IsUnique();
            
            entity.HasOne(e => e.Source)
                .WithMany()
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ModelVersion configuration
        modelBuilder.Entity<ModelVersion>(entity =>
        {
            entity.ToTable("model_versions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tag).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ParentModel).HasMaxLength(300);
            entity.Property(e => e.Parameters).HasColumnType("jsonb");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasIndex(e => new { e.ModelId, e.Tag }).IsUnique();
            entity.HasIndex(e => e.ExternalId).IsUnique();
            
            entity.HasOne(e => e.Model)
                .WithMany(m => m.Versions)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ModelArtifact configuration
        modelBuilder.Entity<ModelArtifact>(entity =>
        {
            entity.ToTable("model_artifacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Digest).HasMaxLength(100);
            entity.Property(e => e.MediaType).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasIndex(e => e.Digest);
            
            entity.HasOne(e => e.ModelVersion)
                .WithMany(v => v.Artifacts)
                .HasForeignKey(e => e.ModelVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FetchRun configuration
        modelBuilder.Entity<FetchRun>(entity =>
        {
            entity.ToTable("fetch_runs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            entity.HasIndex(e => e.RunId).IsUnique();
            entity.HasIndex(e => e.StartedAt);
            
            entity.HasOne(e => e.Source)
                .WithMany()
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
