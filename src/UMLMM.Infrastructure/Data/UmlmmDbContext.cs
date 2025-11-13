using Microsoft.EntityFrameworkCore;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Data;

public class UmlmmDbContext : DbContext
{
    public UmlmmDbContext(DbContextOptions<UmlmmDbContext> options) : base(options)
    {
    }

    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<ModelVersion> ModelVersions => Set<ModelVersion>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ModelTag> ModelTags => Set<ModelTag>();
    public DbSet<FetchRun> FetchRuns => Set<FetchRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Source
        modelBuilder.Entity<Source>(entity =>
        {
            entity.ToTable("sources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).HasColumnName("type").IsRequired().HasMaxLength(100);
            entity.Property(e => e.BaseUrl).HasColumnName("base_url").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Model
        modelBuilder.Entity<Model>(entity =>
        {
            entity.ToTable("models");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Nsfw).HasColumnName("nsfw");
            entity.Property(e => e.Raw).HasColumnName("raw").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Source)
                .WithMany(s => s.Models)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SourceId, e.ExternalId }).IsUnique();
        });

        // Configure ModelVersion
        modelBuilder.Entity<ModelVersion>(entity =>
        {
            entity.ToTable("model_versions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(200);
            entity.Property(e => e.VersionLabel).HasColumnName("version_label").HasMaxLength(200);
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.Raw).HasColumnName("raw").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Model)
                .WithMany(m => m.Versions)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ModelId, e.ExternalId }).IsUnique();
        });

        // Configure Artifact
        modelBuilder.Entity<Artifact>(entity =>
        {
            entity.ToTable("artifacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VersionId).HasColumnName("version_id");
            entity.Property(e => e.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(200);
            entity.Property(e => e.FileKind).HasColumnName("file_kind").HasMaxLength(100);
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.Sha256).HasColumnName("sha256").HasMaxLength(64);
            entity.Property(e => e.DownloadUrl).HasColumnName("download_url").HasMaxLength(2000);
            entity.Property(e => e.Raw).HasColumnName("raw").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Version)
                .WithMany(v => v.Artifacts)
                .HasForeignKey(e => e.VersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Sha256).HasDatabaseName("idx_artifacts_sha256");
            entity.HasIndex(e => new { e.VersionId, e.ExternalId }).IsUnique();
        });

        // Configure Image
        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VersionId).HasColumnName("version_id");
            entity.Property(e => e.ExternalId).HasColumnName("external_id").IsRequired().HasMaxLength(200);
            entity.Property(e => e.PreviewUrl).HasColumnName("preview_url").HasMaxLength(2000);
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Rating).HasColumnName("rating").HasMaxLength(50);
            entity.Property(e => e.Sha256).HasColumnName("sha256").HasMaxLength(64);
            entity.Property(e => e.Raw).HasColumnName("raw").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Version)
                .WithMany(v => v.Images)
                .HasForeignKey(e => e.VersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Sha256).HasDatabaseName("idx_images_sha256");
            entity.HasIndex(e => new { e.VersionId, e.ExternalId }).IsUnique();
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure ModelTag (join table)
        modelBuilder.Entity<ModelTag>(entity =>
        {
            entity.ToTable("model_tags");
            entity.HasKey(e => new { e.ModelId, e.TagId });
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");

            entity.HasOne(e => e.Model)
                .WithMany(m => m.ModelTags)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ModelTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure FetchRun
        modelBuilder.Entity<FetchRun>(entity =>
        {
            entity.ToTable("fetch_runs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.RecordsCreated).HasColumnName("records_created");
            entity.Property(e => e.RecordsUpdated).HasColumnName("records_updated");
            entity.Property(e => e.RecordsNoOp).HasColumnName("records_no_op");
            entity.Property(e => e.RecordsError).HasColumnName("records_error");
            entity.Property(e => e.Cursor).HasColumnName("cursor");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");

            entity.HasOne(e => e.Source)
                .WithMany(s => s.FetchRuns)
                .HasForeignKey(e => e.SourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SourceId, e.StartedAt });
        });
    }
}
