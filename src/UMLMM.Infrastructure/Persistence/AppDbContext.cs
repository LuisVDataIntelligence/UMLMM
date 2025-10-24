using Microsoft.EntityFrameworkCore;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Source> Sources { get; set; } = null!;
    public DbSet<Model> Models { get; set; } = null!;
    public DbSet<ModelVersion> ModelVersions { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ModelTag> ModelTags { get; set; } = null!;
    public DbSet<Artifact> Artifacts { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
    public DbSet<Workflow> Workflows { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;
    public DbSet<FetchRun> FetchRuns { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
