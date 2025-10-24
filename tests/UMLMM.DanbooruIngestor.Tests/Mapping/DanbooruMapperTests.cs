using FluentAssertions;
using UMLMM.DanbooruIngestor.Danbooru;
using UMLMM.DanbooruIngestor.Mapping;

namespace UMLMM.DanbooruIngestor.Tests.Mapping;

public class DanbooruMapperTests
{
    private readonly DanbooruMapper _mapper = new();

    [Fact]
    public void MapToImage_ShouldMapAllFields()
    {
        // Arrange
        var post = new DanbooruPostDto
        {
            Id = 12345,
            CreatedAt = "2023-01-01T00:00:00Z",
            UploaderId = 999,
            Score = 100,
            Source = "https://example.com/source",
            Md5 = "abc123",
            Rating = "s",
            ImageWidth = 1920,
            ImageHeight = 1080,
            TagString = "tag1 tag2 tag3",
            PreviewFileUrl = "https://example.com/preview.jpg",
            FileUrl = "https://example.com/original.jpg"
        };

        // Act
        var image = _mapper.MapToImage(post);

        // Assert
        image.SourceId.Should().Be("danbooru");
        image.ExternalId.Should().Be("12345");
        image.PreviewUrl.Should().Be("https://example.com/preview.jpg");
        image.OriginalUrl.Should().Be("https://example.com/original.jpg");
        image.Rating.Should().Be("sensitive");
        image.Sha256.Should().NotBeNullOrEmpty();
        image.Metadata.Should().Contain("abc123");
        image.Metadata.Should().Contain("999");
    }

    [Fact]
    public void MapToImage_ShouldUseLargeFileUrlIfFileUrlIsNull()
    {
        // Arrange
        var post = new DanbooruPostDto
        {
            Id = 12345,
            CreatedAt = "2023-01-01T00:00:00Z",
            TagString = "tag1",
            FileUrl = null,
            LargeFileUrl = "https://example.com/large.jpg"
        };

        // Act
        var image = _mapper.MapToImage(post);

        // Assert
        image.OriginalUrl.Should().Be("https://example.com/large.jpg");
    }

    [Theory]
    [InlineData("g", "general")]
    [InlineData("s", "sensitive")]
    [InlineData("q", "questionable")]
    [InlineData("e", "explicit")]
    [InlineData(null, null)]
    public void MapToImage_ShouldMapRatingCorrectly(string? input, string? expected)
    {
        // Arrange
        var post = new DanbooruPostDto
        {
            Id = 12345,
            CreatedAt = "2023-01-01T00:00:00Z",
            TagString = "tag1",
            Rating = input
        };

        // Act
        var image = _mapper.MapToImage(post);

        // Assert
        image.Rating.Should().Be(expected);
    }

    [Fact]
    public void ExtractTags_ShouldExtractAllTagCategories()
    {
        // Arrange
        var post = new DanbooruPostDto
        {
            Id = 12345,
            CreatedAt = "2023-01-01T00:00:00Z",
            TagString = "all tags",
            TagStringGeneral = "tag1 tag2",
            TagStringCharacter = "character1",
            TagStringCopyright = "series1",
            TagStringArtist = "artist1",
            TagStringMeta = "meta1"
        };

        // Act
        var tags = _mapper.ExtractTags(post);

        // Assert
        tags.Should().HaveCount(6);
        tags.Should().Contain(t => t.Name == "tag1" && t.Category == "general");
        tags.Should().Contain(t => t.Name == "tag2" && t.Category == "general");
        tags.Should().Contain(t => t.Name == "character1" && t.Category == "character");
        tags.Should().Contain(t => t.Name == "series1" && t.Category == "copyright");
        tags.Should().Contain(t => t.Name == "artist1" && t.Category == "artist");
        tags.Should().Contain(t => t.Name == "meta1" && t.Category == "meta");
    }

    [Fact]
    public void ExtractTags_ShouldHandleEmptyTags()
    {
        // Arrange
        var post = new DanbooruPostDto
        {
            Id = 12345,
            CreatedAt = "2023-01-01T00:00:00Z",
            TagString = "",
            TagStringGeneral = null,
            TagStringCharacter = "",
            TagStringCopyright = null,
            TagStringArtist = "",
            TagStringMeta = null
        };

        // Act
        var tags = _mapper.ExtractTags(post);

        // Assert
        tags.Should().BeEmpty();
    }

    [Fact]
    public void ExtractTags_ShouldTrimWhitespace()
    {
        // Arrange
        var post = new DanbooruPostDto
        {
            Id = 12345,
            CreatedAt = "2023-01-01T00:00:00Z",
            TagString = "tags",
            TagStringGeneral = "  tag1   tag2  "
        };

        // Act
        var tags = _mapper.ExtractTags(post);

        // Assert
        tags.Should().HaveCount(2);
        tags.Should().Contain(t => t.Name == "tag1");
        tags.Should().Contain(t => t.Name == "tag2");
    }
}
