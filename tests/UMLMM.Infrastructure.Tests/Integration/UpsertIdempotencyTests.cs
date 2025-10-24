using Microsoft.EntityFrameworkCore;
using UMLMM.Domain.Entities;
using UMLMM.Domain.Enums;
using UMLMM.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace UMLMM.Infrastructure.Tests.Integration;

public class UpsertIdempotencyTests : IAsyncLifetime
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
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Source_UniqueConstraint_PreventsMultipleSameNames()
    {
        // Arrange
        var source1 = new Source { Name = "CivitAI", BaseUrl = "https://civitai.com" };
        
        _context.Sources.Add(source1);
        await _context.SaveChangesAsync();

        // Act & Assert - Try to insert duplicate
        var source2 = new Source { Name = "CivitAI", BaseUrl = "https://civitai.com/v2" };
        _context.Sources.Add(source2);
        
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task Model_UniqueConstraint_PreventsMultipleSameExternalId()
    {
        // Arrange
        var source = new Source { Name = "TestSource", BaseUrl = "https://test.com" };
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();

        var model1 = new Model
        {
            SourceId = source.SourceId,
            ExternalId = "12345",
            Name = "Test Model",
            Type = ModelType.Checkpoint
        };
        _context.Models.Add(model1);
        await _context.SaveChangesAsync();

        // Act & Assert - Try to insert duplicate
        var model2 = new Model
        {
            SourceId = source.SourceId,
            ExternalId = "12345",
            Name = "Different Name",
            Type = ModelType.Checkpoint
        };
        _context.Models.Add(model2);
        
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task Model_Update_IsIdempotent()
    {
        // Arrange
        var source = new Source { Name = "TestSource2", BaseUrl = "https://test2.com" };
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();

        var model = new Model
        {
            SourceId = source.SourceId,
            ExternalId = "67890",
            Name = "Original Name",
            Type = ModelType.Checkpoint,
            Description = "Original description"
        };
        _context.Models.Add(model);
        await _context.SaveChangesAsync();

        var modelId = model.ModelId;

        // Act - Update the model
        model.Name = "Updated Name";
        model.Description = "Updated description";
        await _context.SaveChangesAsync();

        // Clear tracking
        _context.ChangeTracker.Clear();

        // Assert - Verify update
        var updated = await _context.Models.FindAsync(modelId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(modelId, updated.ModelId);
    }

    [Fact]
    public async Task Tag_UniqueConstraint_OnNormalizedNameAndSource()
    {
        // Arrange
        var source = new Source { Name = "TagSource", BaseUrl = "https://tags.com" };
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();

        var tag1 = new Tag
        {
            Name = "Character",
            NormalizedName = "character",
            SourceId = source.SourceId
        };
        _context.Tags.Add(tag1);
        await _context.SaveChangesAsync();

        // Act & Assert - Try to insert duplicate with same source
        var tag2 = new Tag
        {
            Name = "CHARACTER",
            NormalizedName = "character",
            SourceId = source.SourceId
        };
        _context.Tags.Add(tag2);
        
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task Tag_DifferentSources_CanHaveSameNormalizedName()
    {
        // Arrange
        var source1 = new Source { Name = "Source1", BaseUrl = "https://source1.com" };
        var source2 = new Source { Name = "Source2", BaseUrl = "https://source2.com" };
        _context.Sources.AddRange(source1, source2);
        await _context.SaveChangesAsync();

        var tag1 = new Tag
        {
            Name = "Anime",
            NormalizedName = "anime",
            SourceId = source1.SourceId
        };
        
        var tag2 = new Tag
        {
            Name = "Anime",
            NormalizedName = "anime",
            SourceId = source2.SourceId
        };

        // Act
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        // Assert
        var tags = await _context.Tags.Where(t => t.NormalizedName == "anime").ToListAsync();
        Assert.Equal(2, tags.Count);
    }

    [Fact]
    public async Task Workflow_UniqueConstraint_PreventsMultipleSameExternalId()
    {
        // Arrange
        var source = new Source { Name = "WorkflowSource", BaseUrl = "https://workflows.com" };
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();

        var workflow1 = new Workflow
        {
            SourceId = source.SourceId,
            ExternalId = "wf-123",
            Title = "Test Workflow",
            NodesCount = 5
        };
        _context.Workflows.Add(workflow1);
        await _context.SaveChangesAsync();

        // Act & Assert - Try to insert duplicate
        var workflow2 = new Workflow
        {
            SourceId = source.SourceId,
            ExternalId = "wf-123",
            Title = "Different Workflow",
            NodesCount = 10
        };
        _context.Workflows.Add(workflow2);
        
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task JsonbFields_CanStoreAndRetrieve()
    {
        // Arrange
        var source = new Source { Name = "JsonSource", BaseUrl = "https://json.com" };
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();

        var model = new Model
        {
            SourceId = source.SourceId,
            ExternalId = "json-test",
            Name = "JSON Test Model",
            Type = ModelType.Checkpoint,
            Raw = "{\"key\": \"value\", \"number\": 42}"
        };
        _context.Models.Add(model);
        await _context.SaveChangesAsync();

        var modelId = model.ModelId;
        _context.ChangeTracker.Clear();

        // Act
        var retrieved = await _context.Models.FindAsync(modelId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Raw);
        Assert.Contains("key", retrieved.Raw);
        Assert.Contains("value", retrieved.Raw);
    }

    [Fact]
    public async Task BatchInsert_PerformsWell()
    {
        // Arrange
        var source = new Source { Name = "BatchSource", BaseUrl = "https://batch.com" };
        _context.Sources.Add(source);
        await _context.SaveChangesAsync();

        var models = new List<Model>();
        for (int i = 0; i < 100; i++)
        {
            models.Add(new Model
            {
                SourceId = source.SourceId,
                ExternalId = $"batch-{i}",
                Name = $"Batch Model {i}",
                Type = ModelType.Checkpoint
            });
        }

        // Act
        var startTime = DateTime.UtcNow;
        _context.Models.AddRange(models);
        await _context.SaveChangesAsync();
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        var count = await _context.Models.CountAsync();
        Assert.True(count >= 100);
        Assert.True(elapsed.TotalSeconds < 10, $"Batch insert took {elapsed.TotalSeconds}s, expected < 10s");
    }
}
