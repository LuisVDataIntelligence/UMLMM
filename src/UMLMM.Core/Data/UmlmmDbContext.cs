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

        // Configure FetchRun (Core models mapping)
        modelBuilder.Entity<FetchRun>(entity =>
        {
            entity.ToTable("fetch_runs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            // Store source id as foreign reference
            entity.Property(e => e.Source).HasColumnName("source_id");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(e => e.StartTime).HasColumnName("started_at");
            entity.Property(e => e.EndTime).HasColumnName("completed_at");
            entity.Property(e => e.RecordsFetched).HasColumnName("records_fetched");
            entity.Property(e => e.RecordsProcessed).HasColumnName("records_processed");
            entity.Property(e => e.RecordsFailed).HasColumnName("records_failed");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ErrorDetails).HasColumnName("error_details");
            entity.HasIndex(e => e.StartTime);
        });
    }
}
