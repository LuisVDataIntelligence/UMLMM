using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Testcontainers.PostgreSql;
using UMLMM.Infrastructure.Data;
using UMLMM.Infrastructure.Repositories;

namespace UMLMM.Tests.Integration;

public class ModelRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private IServiceProvider? _serviceProvider;

    public async Task InitializeAsync()
    {
        // Create and start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("umlmm_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();

        // Setup services
        var services = new ServiceCollection();
        
        services.AddDbContext<UmlmmDbContext>(options =>
            options.UseNpgsql(_postgresContainer.GetConnectionString()));
        
        services.AddScoped<ModelRepository>();

        _serviceProvider = services.BuildServiceProvider();

        // Run migrations
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpsertSource_ShouldCreateNewSource()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        // Act
        var source = await repository.UpsertSourceAsync("Ollama", "Test Ollama source");

        // Assert
        Assert.NotEqual(0, source.Id);
        Assert.Equal("Ollama", source.Name);
        Assert.Equal("Test Ollama source", source.Description);
    }

    [Fact]
    public async Task UpsertSource_ShouldUpdateExistingSource()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        // Create initial source
        var source1 = await repository.UpsertSourceAsync("Ollama", "Initial description");
        var firstId = source1.Id;

        // Act - Update with same name
        var source2 = await repository.UpsertSourceAsync("Ollama", "Updated description");

        // Assert
        Assert.Equal(firstId, source2.Id);
        Assert.Equal("Updated description", source2.Description);
    }

    [Fact]
    public async Task UpsertModel_ShouldBeIdempotent()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        var source = await repository.UpsertSourceAsync("Ollama", "Test");

        // Act - Insert twice with same external ID
        var model1 = await repository.UpsertModelAsync(
            source.Id,
            "llama2:7b",
            "Llama 2 7B",
            "A 7B parameter model");

        var model2 = await repository.UpsertModelAsync(
            source.Id,
            "llama2:7b",
            "Llama 2 7B Updated",
            "An updated 7B parameter model");

        // Assert
        Assert.Equal(model1.Id, model2.Id);
        Assert.Equal("Llama 2 7B Updated", model2.Name);
        Assert.Equal("An updated 7B parameter model", model2.Description);
    }

    [Fact]
    public async Task UpsertModelVersion_WithMetadata_ShouldStoreJsonb()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        var source = await repository.UpsertSourceAsync("Ollama", "Test");
        var model = await repository.UpsertModelAsync(source.Id, "llama2", "Llama 2");

        var parameters = JsonSerializer.SerializeToDocument(new Dictionary<string, object>
        {
            ["temperature"] = 0.8,
            ["top_p"] = 0.9
        });

        // Act
        var version = await repository.UpsertModelVersionAsync(
            model.Id,
            "7b",
            "llama2:7b",
            parentModel: "llama2:base",
            parameters: parameters);

        // Assert
        Assert.NotEqual(0, version.Id);
        Assert.Equal("7b", version.Tag);
        Assert.Equal("llama2:base", version.ParentModel);
        Assert.NotNull(version.Parameters);
    }

    [Fact]
    public async Task UpsertModelArtifact_ShouldCreateArtifact()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        var source = await repository.UpsertSourceAsync("Ollama", "Test");
        var model = await repository.UpsertModelAsync(source.Id, "llama2", "Llama 2");
        var version = await repository.UpsertModelVersionAsync(
            model.Id,
            "7b",
            "llama2:7b");

        // Act
        var artifact = await repository.UpsertModelArtifactAsync(
            version.Id,
            "modelfile",
            digest: "sha256:abc123",
            size: 1024,
            mediaType: "text/plain");

        // Assert
        Assert.NotEqual(0, artifact.Id);
        Assert.Equal("modelfile", artifact.Type);
        Assert.Equal("sha256:abc123", artifact.Digest);
        Assert.Equal(1024, artifact.Size);
    }

    [Fact]
    public async Task CreateFetchRun_ShouldTrackIngestion()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        var source = await repository.UpsertSourceAsync("Ollama", "Test");
        var runId = Guid.NewGuid();

        // Act
        var fetchRun = await repository.CreateFetchRunAsync(source.Id, runId);

        // Assert
        Assert.NotEqual(0, fetchRun.Id);
        Assert.Equal(runId, fetchRun.RunId);
        Assert.Equal("running", fetchRun.Status);
        Assert.Null(fetchRun.CompletedAt);
    }

    [Fact]
    public async Task UpdateFetchRun_ShouldUpdateStatus()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ModelRepository>();

        var source = await repository.UpsertSourceAsync("Ollama", "Test");
        var runId = Guid.NewGuid();
        var fetchRun = await repository.CreateFetchRunAsync(source.Id, runId);

        // Act
        await repository.UpdateFetchRunAsync(
            fetchRun.Id,
            "completed",
            modelsProcessed: 5,
            versionsProcessed: 10,
            artifactsProcessed: 15);

        // Verify
        var dbContext = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
        var updated = await dbContext.FetchRuns.FindAsync(fetchRun.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("completed", updated.Status);
        Assert.Equal(5, updated.ModelsProcessed);
        Assert.Equal(10, updated.VersionsProcessed);
        Assert.Equal(15, updated.ArtifactsProcessed);
        Assert.NotNull(updated.CompletedAt);
    }
}
