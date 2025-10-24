using Microsoft.EntityFrameworkCore;
using UMLMM.Domain.Entities;
using UMLMM.Domain.Enums;
using UMLMM.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace UMLMM.Infrastructure.Tests.Integration;

public class DatabaseMigrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private AppDbContext _context = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("umlmm_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Migrations_CanBeApplied_Successfully()
    {
        // Act
        await _context.Database.MigrateAsync();

        // Assert
        var canConnect = await _context.Database.CanConnectAsync();
        Assert.True(canConnect);
    }

    [Fact]
    public async Task Migrations_CreateAllTables()
    {
        // Arrange
        await _context.Database.MigrateAsync();

        // Act
        var tables = await _context.Database
            .SqlQueryRaw<string>("SELECT tablename FROM pg_tables WHERE schemaname = 'public'")
            .ToListAsync();

        // Assert
        Assert.Contains("sources", tables);
        Assert.Contains("models", tables);
        Assert.Contains("model_versions", tables);
        Assert.Contains("tags", tables);
        Assert.Contains("model_tags", tables);
        Assert.Contains("artifacts", tables);
        Assert.Contains("images", tables);
        Assert.Contains("workflows", tables);
        Assert.Contains("prompts", tables);
        Assert.Contains("fetch_runs", tables);
    }

    [Fact]
    public async Task Migrations_CreateIndexes()
    {
        // Arrange
        await _context.Database.MigrateAsync();

        // Act
        var indexes = await _context.Database
            .SqlQueryRaw<string>(
                "SELECT indexname FROM pg_indexes WHERE schemaname = 'public' AND indexname NOT LIKE 'PK_%'")
            .ToListAsync();

        // Assert - check for some key indexes
        Assert.NotEmpty(indexes);
        // Should have indexes on foreign keys, unique constraints, etc.
    }
}
