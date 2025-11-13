using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using UMLMM.Domain.Entities;
using UMLMM.Infrastructure.Data;

namespace UMLMM.Infrastructure.IntegrationTests.Data;

public class UmlmmDbContextTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    private UmlmmDbContext? _dbContext;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<UmlmmDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new UmlmmDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }
    }

    [Fact]
    public async Task InsertImage_ShouldSucceed()
    {
        // Arrange
        var image = new Image
        {
            Id = Guid.NewGuid(),
            SourceId = "danbooru",
            ExternalId = "12345",
            Sha256 = "abc123",
            PreviewUrl = "https://example.com/preview.jpg",
            OriginalUrl = "https://example.com/original.jpg",
            Rating = "safe",
            Metadata = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext!.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedImage = await _dbContext.Images.FirstOrDefaultAsync(i => i.ExternalId == "12345");
        savedImage.Should().NotBeNull();
        savedImage!.SourceId.Should().Be("danbooru");
    }

    [Fact]
    public async Task UpsertImage_ShouldNotCreateDuplicate()
    {
        // Arrange
        var image1 = new Image
        {
            Id = Guid.NewGuid(),
            SourceId = "danbooru",
            ExternalId = "12345",
            PreviewUrl = "https://example.com/preview1.jpg",
            OriginalUrl = "https://example.com/original1.jpg",
            Rating = "safe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext!.Images.Add(image1);
        await _dbContext.SaveChangesAsync();

        // Act - Try to insert with same source_id and external_id
        var existing = await _dbContext.Images
            .FirstOrDefaultAsync(i => i.SourceId == "danbooru" && i.ExternalId == "12345");

        existing.Should().NotBeNull();
        existing!.PreviewUrl = "https://example.com/preview2.jpg";
        await _dbContext.SaveChangesAsync();

        // Assert
        var images = await _dbContext.Images
            .Where(i => i.SourceId == "danbooru" && i.ExternalId == "12345")
            .ToListAsync();

        images.Should().HaveCount(1);
        images[0].PreviewUrl.Should().Be("https://example.com/preview2.jpg");
    }

    [Fact]
    public async Task InsertTag_ShouldSucceed()
    {
        // Arrange
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "test_tag",
            Category = "general",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext!.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedTag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == "test_tag");
        savedTag.Should().NotBeNull();
        savedTag!.Category.Should().Be("general");
    }

    [Fact]
    public async Task CreateImageTagRelationship_ShouldSucceed()
    {
        // Arrange
        var image = new Image
        {
            Id = Guid.NewGuid(),
            SourceId = "danbooru",
            ExternalId = "12345",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "test_tag",
            Category = "general",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext!.Images.Add(image);
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        var imageTag = new ImageTag
        {
            ImageId = image.Id,
            TagId = tag.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.ImageTags.Add(imageTag);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedImageTag = await _dbContext.ImageTags
            .Include(it => it.Image)
            .Include(it => it.Tag)
            .FirstOrDefaultAsync(it => it.ImageId == image.Id && it.TagId == tag.Id);

        savedImageTag.Should().NotBeNull();
        savedImageTag!.Image.ExternalId.Should().Be("12345");
        savedImageTag.Tag.Name.Should().Be("test_tag");
    }

    [Fact]
    public async Task CreateFetchRun_ShouldSucceed()
    {
        // Arrange
        var fetchRun = new FetchRun
        {
            Id = Guid.NewGuid(),
            RunId = "test-run-001",
            SourceId = "danbooru",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow.AddMinutes(5),
            CreatedCount = 10,
            UpdatedCount = 5,
            NoOpCount = 2,
            ErrorCount = 0,
            Parameters = "{\"tags\":\"rating:safe\",\"pageSize\":100}"
        };

        // Act
        _dbContext!.FetchRuns.Add(fetchRun);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedRun = await _dbContext.FetchRuns.FirstOrDefaultAsync(f => f.RunId == "test-run-001");
        savedRun.Should().NotBeNull();
        savedRun!.CreatedCount.Should().Be(10);
        savedRun.UpdatedCount.Should().Be(5);
    }

    [Fact]
    public async Task UniqueConstraint_OnSourceIdAndExternalId_ShouldPreventDuplicates()
    {
        // Arrange
        var image1 = new Image
        {
            Id = Guid.NewGuid(),
            SourceId = "danbooru",
            ExternalId = "12345",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext!.Images.Add(image1);
        await _dbContext.SaveChangesAsync();

        var image2 = new Image
        {
            Id = Guid.NewGuid(),
            SourceId = "danbooru",
            ExternalId = "12345",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act & Assert
        _dbContext.Images.Add(image2);
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task UniqueConstraint_OnTagName_ShouldPreventDuplicates()
    {
        // Arrange
        var tag1 = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "duplicate_tag",
            Category = "general",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext!.Tags.Add(tag1);
        await _dbContext.SaveChangesAsync();

        var tag2 = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "duplicate_tag",
            Category = "character",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act & Assert
        _dbContext.Tags.Add(tag2);
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
