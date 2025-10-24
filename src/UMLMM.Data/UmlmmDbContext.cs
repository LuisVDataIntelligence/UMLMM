using Microsoft.EntityFrameworkCore;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data;

public class UmlmmDbContext : DbContext
{
    public UmlmmDbContext(DbContextOptions<UmlmmDbContext> options) : base(options)
    {
    }

    public DbSet<Source> Sources { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<FetchRun> FetchRuns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UmlmmDbContext).Assembly);
    }
}
