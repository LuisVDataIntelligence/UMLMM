using FluentAssertions;
using UMLMM.Core.Domain.Enums;
using UMLMM.E621Ingestor.Client.DTOs;
using UMLMM.E621Ingestor.Mapping;
using Xunit;

namespace UMLMM.E621Ingestor.Tests.Unit;

public class E621MapperTests
{
    private readonly E621Mapper _mapper;

    public E621MapperTests()
    {
        _mapper = new E621Mapper();
    }

    [Fact]
    public void MapToPost_ShouldMapAllProperties()
    {
        // Arrange
        var apiPost = new E621Post
        {
            Id = 12345,
            Description = "Test description",
            CreatedAt = DateTime.Parse("2024-01-01T10:00:00Z"),
            Rating = "s"
        };
        var sourceId = 1;

        // Act
        var result = _mapper.MapToPost(apiPost, sourceId);

        // Assert
        result.SourceId.Should().Be(sourceId);
        result.ExternalId.Should().Be("12345");
        result.Description.Should().Be("Test description");
        result.Rating.Should().Be(Rating.Safe);
        result.ExternalCreatedAt.Should().Be(DateTime.Parse("2024-01-01T10:00:00Z"));
    }

    [Theory]
    [InlineData("s", Rating.Safe)]
    [InlineData("safe", Rating.Safe)]
    [InlineData("q", Rating.Questionable)]
    [InlineData("questionable", Rating.Questionable)]
    [InlineData("e", Rating.Explicit)]
    [InlineData("explicit", Rating.Explicit)]
    [InlineData("unknown", Rating.Safe)]
    public void MapRating_ShouldMapCorrectly(string input, Rating expected)
    {
        // Act
        var result = _mapper.MapRating(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ExtractTags_ShouldIncludeAllCategories()
    {
        // Arrange
        var apiPost = new E621Post
        {
            Id = 1,
            Tags = new E621Tags
            {
                General = new List<string> { "tag1", "tag2" },
                Species = new List<string> { "species1" },
                Character = new List<string> { "character1" },
                Copyright = new List<string> { "copyright1" },
                Artist = new List<string> { "artist1" },
                Lore = new List<string> { "lore1" },
                Meta = new List<string> { "meta1" }
            }
        };

        // Act
        var result = _mapper.ExtractTags(apiPost);

        // Assert
        result.Should().HaveCount(8);
        result.Should().Contain(t => t.Name == "tag1" && t.Category == "general");
        result.Should().Contain(t => t.Name == "species1" && t.Category == "species");
        result.Should().Contain(t => t.Name == "character1" && t.Category == "character");
        result.Should().Contain(t => t.Name == "artist1" && t.Category == "artist");
    }

    [Fact]
    public void MapToImage_ShouldMapAllProperties()
    {
        // Arrange
        var apiPost = new E621Post
        {
            Id = 1,
            File = new E621File
            {
                Width = 1920,
                Height = 1080,
                Ext = "jpg",
                Size = 123456,
                Md5 = "abc123",
                Url = "https://example.com/image.jpg"
            },
            Sample = new E621Sample
            {
                Url = "https://example.com/sample.jpg",
                Width = 800,
                Height = 600
            }
        };

        // Act
        var result = _mapper.MapToImage(apiPost);

        // Assert
        result.Url.Should().Be("https://example.com/image.jpg");
        result.SampleUrl.Should().Be("https://example.com/sample.jpg");
        result.Width.Should().Be(1920);
        result.Height.Should().Be(1080);
        result.FileSize.Should().Be(123456);
        result.FileExtension.Should().Be("jpg");
        result.Sha256.Should().Be("abc123");
    }
}
