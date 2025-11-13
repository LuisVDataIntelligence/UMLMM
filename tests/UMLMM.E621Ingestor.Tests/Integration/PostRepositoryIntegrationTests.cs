using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using UMLMM.Core.Domain.Entities;
using UMLMM.Core.Domain.Enums;
using UMLMM.Data;
using UMLMM.Data.Repositories;
using Xunit;

namespace UMLMM.E621Ingestor.Tests.Integration;

public class PostRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private UmlmmDbContext? _dbContext;
    private PostRepository? _repository;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<UmlmmDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _dbContext = new UmlmmDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        var logger = Mock.Of<ILogger<PostRepository>>();
        _repository = new PostRepository(_dbContext, logger);
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
    public async Task EnsureSourceAsync_ShouldCreateNewSource()
    {
        // Act
        var source = await _repository!.EnsureSourceAsync("e621");

        // Assert
        source.Should().NotBeNull();
        source.Id.Should().BeGreaterThan(0);
        source.Name.Should().Be("e621");
    }

    [Fact]
    public async Task EnsureSourceAsync_ShouldReturnExistingSource()
    {
        // Arrange
        var firstSource = await _repository!.EnsureSourceAsync("e621");

        // Act
        var secondSource = await _repository.EnsureSourceAsync("e621");

        // Assert
        firstSource.Id.Should().Be(secondSource.Id);
    }

    [Fact]
    public async Task CreateFetchRunAsync_ShouldCreateFetchRun()
    {
        // Arrange
        var source = await _repository!.EnsureSourceAsync("e621");

        // Act
        var fetchRun = await _repository.CreateFetchRunAsync(source.Id);

        // Assert
        fetchRun.Should().NotBeNull();
        fetchRun.Id.Should().BeGreaterThan(0);
        fetchRun.SourceId.Should().Be(source.Id);
        fetchRun.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpsertPostAsync_ShouldCreateNewPost()
    {
        // Arrange
        var source = await _repository!.EnsureSourceAsync("e621");
        var post = new Post
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Description = "Test post",
            Rating = Rating.Safe,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var tags = new List<Tag>
        {
            new Tag { Name = "tag1", Category = "general", CreatedAt = DateTime.UtcNow }
        };
        var image = new Image
        {
            Url = "https://example.com/image.jpg",
            Width = 1920,
            Height = 1080,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.UpsertPostAsync(post, tags, image);

        // Assert
        var savedPost = await _repository.GetPostByExternalIdAsync(source.Id, "12345");
        savedPost.Should().NotBeNull();
        savedPost!.Description.Should().Be("Test post");
        savedPost.Images.Should().HaveCount(1);
        savedPost.PostTags.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpsertPostAsync_ShouldUpdateExistingPost()
    {
        // Arrange
        var source = await _repository!.EnsureSourceAsync("e621");
        var post = new Post
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Description = "Original description",
            Rating = Rating.Safe,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var tags = new List<Tag>
        {
            new Tag { Name = "tag1", Category = "general", CreatedAt = DateTime.UtcNow }
        };
        var image = new Image
        {
            Url = "https://example.com/image.jpg",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.UpsertPostAsync(post, tags, image);

        // Act - Update with new data
        var updatedPost = new Post
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Description = "Updated description",
            Rating = Rating.Questionable,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var updatedTags = new List<Tag>
        {
            new Tag { Name = "tag2", Category = "general", CreatedAt = DateTime.UtcNow }
        };
        var updatedImage = new Image
        {
            Url = "https://example.com/updated.jpg",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.UpsertPostAsync(updatedPost, updatedTags, updatedImage);

        // Assert
        var savedPost = await _repository.GetPostByExternalIdAsync(source.Id, "12345");
        savedPost.Should().NotBeNull();
        savedPost!.Description.Should().Be("Updated description");
        savedPost.Rating.Should().Be(Rating.Questionable);
        
        // Should have only one post (idempotent upsert)
        var allPosts = await _dbContext!.Posts.ToListAsync();
        allPosts.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpsertPostAsync_ShouldBeIdempotent()
    {
        // Arrange
        var source = await _repository!.EnsureSourceAsync("e621");
        var post = new Post
        {
            SourceId = source.Id,
            ExternalId = "12345",
            Description = "Test post",
            Rating = Rating.Safe,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var tags = new List<Tag>
        {
            new Tag { Name = "tag1", Category = "general", CreatedAt = DateTime.UtcNow }
        };
        var image = new Image
        {
            Url = "https://example.com/image.jpg",
            CreatedAt = DateTime.UtcNow
        };

        // Act - Insert same post twice
        await _repository.UpsertPostAsync(post, tags, image);
        await _repository.UpsertPostAsync(post, tags, image);

        // Assert
        var allPosts = await _dbContext!.Posts.ToListAsync();
        allPosts.Should().HaveCount(1, "upsert should be idempotent");
        
        var allImages = await _dbContext.Images.ToListAsync();
        allImages.Should().HaveCount(1, "images should not be duplicated");
    }
}
