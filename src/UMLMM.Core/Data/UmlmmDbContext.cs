using Microsoft.EntityFrameworkCore;
using UMLMM.Core.Models;

namespace UMLMM.Core.Data;

public class UmlmmDbContext : DbContext
{
    public UmlmmDbContext(DbContextOptions<UmlmmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<FetchRun> FetchRuns => Set<FetchRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Workflow
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("workflows");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceId).HasColumnName("source_id").IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(500);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GraphJsonb).HasColumnName("graph_jsonb").HasColumnType("jsonb");
            entity.Property(e => e.NodesCount).HasColumnName("nodes_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => new { e.SourceId, e.ExternalId }).IsUnique();
        });

        // Configure Artifact
        modelBuilder.Entity<Artifact>(entity =>
        {
            entity.ToTable("artifacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceId).HasColumnName("source_id").IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(500);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(500);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(100);
            entity.Property(e => e.Path).HasColumnName("path").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => new { e.SourceId, e.ExternalId }).IsUnique();
        });

        // Configure FetchRun
        modelBuilder.Entity<FetchRun>(entity =>
        {
            entity.ToTable("fetch_runs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceId).HasColumnName("source_id").IsRequired().HasMaxLength(100);
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedCount).HasColumnName("created_count");
            entity.Property(e => e.UpdatedCount).HasColumnName("updated_count");
            entity.Property(e => e.NoOpCount).HasColumnName("no_op_count");
            entity.Property(e => e.ErrorCount).HasColumnName("error_count");
            entity.Property(e => e.ErrorDetails).HasColumnName("error_details");
            entity.HasIndex(e => new { e.SourceId, e.RunId }).IsUnique();
        });
    }
}
