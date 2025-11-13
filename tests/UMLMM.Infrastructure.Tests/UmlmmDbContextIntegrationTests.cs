using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using UMLMM.Domain.Entities;
using UMLMM.Infrastructure.Data;

namespace UMLMM.Infrastructure.Tests;

public class UmlmmDbContextIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private UmlmmDbContext? _dbContext;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<UmlmmDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _dbContext = new UmlmmDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task CanCreateAndRetrieveSource()
    {
        // Arrange
        var source = new Source
        {
            Name = "TestSource",
            Type = "test-type",
            BaseUrl = "https://test.com"
        };

        // Act
        _dbContext!.Sources.Add(source);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _dbContext.Sources
            .FirstOrDefaultAsync(s => s.Name == "TestSource");

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("TestSource");
        retrieved.Type.Should().Be("test-type");
        retrieved.BaseUrl.Should().Be("https://test.com");
    }

    [Fact]
    public async Task CanCreateModelWithVersionsAndArtifacts()
    {
        // Arrange
        var source = new Source
        {
            Name = "CivitAI",
            Type = "model-repo",
            BaseUrl = "https://civitai.com"
        };
        _dbContext!.Sources.Add(source);
        await _dbContext.SaveChangesAsync();

        var model = new Model
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Name = "Test Model",
            Type = "Checkpoint",
            Description = "A test model",
            Nsfw = false
        };

        var version = new ModelVersion
        {
            Model = model,
            ExternalId = "v1",
            VersionLabel = "1.0",
            PublishedAt = DateTime.UtcNow
        };

        var artifact = new Artifact
        {
            Version = version,
            ExternalId = "file1",
            FileKind = "Model",
            FileSizeBytes = 1024 * 1024 * 1024,
            Sha256 = "abc123",
            DownloadUrl = "https://example.com/model.safetensors"
        };

        model.Versions.Add(version);
        version.Artifacts.Add(artifact);

        // Act
        _dbContext.Models.Add(model);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _dbContext.Models
            .Include(m => m.Versions)
                .ThenInclude(v => v.Artifacts)
            .FirstOrDefaultAsync(m => m.ExternalId == "12345");

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Model");
        retrieved.Versions.Should().HaveCount(1);
        retrieved.Versions.First().Artifacts.Should().HaveCount(1);
        retrieved.Versions.First().Artifacts.First().Sha256.Should().Be("abc123");
    }

    [Fact]
    public async Task IdempotentUpsert_ShouldNotCreateDuplicates()
    {
        // Arrange
        var source = new Source
        {
            Name = "CivitAI",
            Type = "model-repo",
            BaseUrl = "https://civitai.com"
        };
        _dbContext!.Sources.Add(source);
        await _dbContext.SaveChangesAsync();

        var externalId = "12345";

        // Act - First insert
        var model1 = new Model
        {
            SourceId = source.Id,
            ExternalId = externalId,
            Name = "Test Model",
            Type = "Checkpoint"
        };
        _dbContext.Models.Add(model1);
        await _dbContext.SaveChangesAsync();

        // Try to insert duplicate with same source_id and external_id
        var model2 = new Model
        {
            SourceId = source.Id,
            ExternalId = externalId,
            Name = "Duplicate Model",
            Type = "Checkpoint"
        };
        _dbContext.Models.Add(model2);

        // Assert - Should throw due to unique constraint
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await _dbContext.SaveChangesAsync());

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task CanCreateModelWithTags()
    {
        // Arrange
        var source = new Source
        {
            Name = "CivitAI",
            Type = "model-repo"
        };
        _dbContext!.Sources.Add(source);
        await _dbContext.SaveChangesAsync();

        var tag1 = new Tag { Name = "anime" };
        var tag2 = new Tag { Name = "realistic" };
        _dbContext.Tags.AddRange(tag1, tag2);
        await _dbContext.SaveChangesAsync();

        var model = new Model
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Name = "Test Model",
            Type = "Checkpoint"
        };

        model.ModelTags.Add(new ModelTag { Model = model, Tag = tag1 });
        model.ModelTags.Add(new ModelTag { Model = model, Tag = tag2 });

        // Act
        _dbContext.Models.Add(model);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _dbContext.Models
            .Include(m => m.ModelTags)
                .ThenInclude(mt => mt.Tag)
            .FirstOrDefaultAsync(m => m.ExternalId == "12345");

        retrieved.Should().NotBeNull();
        retrieved!.ModelTags.Should().HaveCount(2);
        retrieved.ModelTags.Select(mt => mt.Tag.Name).Should().Contain(new[] { "anime", "realistic" });
    }

    [Fact]
    public async Task CanCreateFetchRun()
    {
        // Arrange
        var source = new Source
        {
            Name = "CivitAI",
            Type = "model-repo"
        };
        _dbContext!.Sources.Add(source);
        await _dbContext.SaveChangesAsync();

        var fetchRun = new FetchRun
        {
            SourceId = source.Id,
            Status = "running",
            RecordsCreated = 10,
            RecordsUpdated = 5,
            RecordsNoOp = 2,
            RecordsError = 1,
            Cursor = "page-5"
        };

        // Act
        _dbContext.FetchRuns.Add(fetchRun);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _dbContext.FetchRuns
            .FirstOrDefaultAsync(f => f.SourceId == source.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be("running");
        retrieved.RecordsCreated.Should().Be(10);
        retrieved.Cursor.Should().Be("page-5");
    }

    [Fact]
    public async Task ArtifactSha256Index_ShouldAllowQueries()
    {
        // Arrange
        var source = new Source
        {
            Name = "CivitAI",
            Type = "model-repo"
        };
        _dbContext!.Sources.Add(source);
        await _dbContext.SaveChangesAsync();

        var model = new Model
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Name = "Test Model",
            Type = "Checkpoint"
        };

        var version = new ModelVersion
        {
            Model = model,
            ExternalId = "v1",
            VersionLabel = "1.0"
        };

        var artifact = new Artifact
        {
            Version = version,
            ExternalId = "file1",
            Sha256 = "abc123def456"
        };

        model.Versions.Add(version);
        version.Artifacts.Add(artifact);
        _dbContext.Models.Add(model);
        await _dbContext.SaveChangesAsync();

        // Act
        var retrieved = await _dbContext.Artifacts
            .FirstOrDefaultAsync(a => a.Sha256 == "abc123def456");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Sha256.Should().Be("abc123def456");
    }
}
