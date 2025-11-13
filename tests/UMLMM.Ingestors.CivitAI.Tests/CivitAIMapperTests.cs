using FluentAssertions;
using UMLMM.Ingestors.CivitAI.CivitAI.DTOs;
using UMLMM.Ingestors.CivitAI.Mapping;

namespace UMLMM.Ingestors.CivitAI.Tests;

public class CivitAIMapperTests
{
    [Fact]
    public void MapToModel_ShouldMapBasicProperties()
    {
        // Arrange
        var dto = new CivitAIModelDto
        {
            Id = 12345,
            Name = "Test Model",
            Type = "Checkpoint",
            Description = "A test model",
            Nsfw = true,
            Tags = new List<string> { "anime", "realistic" }
        };
        var sourceId = 1;

        // Act
        var model = CivitAIMapper.MapToModel(dto, sourceId);

        // Assert
        model.SourceId.Should().Be(sourceId);
        model.ExternalId.Should().Be("12345");
        model.Name.Should().Be("Test Model");
        model.Type.Should().Be("Checkpoint");
        model.Description.Should().Be("A test model");
        model.Nsfw.Should().BeTrue();
        model.Raw.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MapToModel_ShouldMapVersions()
    {
        // Arrange
        var dto = new CivitAIModelDto
        {
            Id = 12345,
            Name = "Test Model",
            ModelVersions = new List<CivitAIVersionDto>
            {
                new CivitAIVersionDto
                {
                    Id = 1,
                    Name = "v1.0",
                    PublishedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new CivitAIVersionDto
                {
                    Id = 2,
                    Name = "v2.0",
                    PublishedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };

        // Act
        var model = CivitAIMapper.MapToModel(dto, 1);

        // Assert
        model.Versions.Should().HaveCount(2);
        model.Versions.Should().Contain(v => v.ExternalId == "1" && v.VersionLabel == "v1.0");
        model.Versions.Should().Contain(v => v.ExternalId == "2" && v.VersionLabel == "v2.0");
    }

    [Fact]
    public void MapToArtifact_ShouldMapFileProperties()
    {
        // Arrange
        var dto = new CivitAIFileDto
        {
            Id = 789,
            Type = "Model",
            SizeKB = 1024,
            DownloadUrl = "https://example.com/model.safetensors",
            Hashes = new CivitAIHashesDto
            {
                SHA256 = "abcd1234"
            }
        };

        // Act
        var artifact = CivitAIMapper.MapToArtifact(dto);

        // Assert
        artifact.ExternalId.Should().Be("789");
        artifact.FileKind.Should().Be("Model");
        artifact.FileSizeBytes.Should().Be(1024 * 1024);
        artifact.Sha256.Should().Be("abcd1234");
        artifact.DownloadUrl.Should().Be("https://example.com/model.safetensors");
    }

    [Fact]
    public void MapToImage_ShouldMapImageProperties()
    {
        // Arrange
        var dto = new CivitAIImageDto
        {
            Id = 456,
            Url = "https://example.com/preview.jpg",
            Width = 1024,
            Height = 768,
            NsfwLevel = "None"
        };

        // Act
        var image = CivitAIMapper.MapToImage(dto);

        // Assert
        image.ExternalId.Should().Be("456");
        image.PreviewUrl.Should().Be("https://example.com/preview.jpg");
        image.Width.Should().Be(1024);
        image.Height.Should().Be(768);
        image.Rating.Should().Be("None");
    }

    [Theory]
    [InlineData("My Tag", "my-tag")]
    [InlineData("Test_Tag", "test-tag")]
    [InlineData("UPPERCASE", "uppercase")]
    [InlineData("multiple   spaces", "multiple-spaces")]
    [InlineData("special!@#chars", "specialchars")]
    [InlineData("--leading-trailing--", "leading-trailing")]
    public void NormalizeTag_ShouldNormalizeCorrectly(string input, string expected)
    {
        // Act
        var result = CivitAIMapper.NormalizeTag(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeTags_ShouldHandleNullAndEmpty()
    {
        // Act & Assert
        CivitAIMapper.NormalizeTags(null).Should().BeEmpty();
        CivitAIMapper.NormalizeTags(new List<string>()).Should().BeEmpty();
    }

    [Fact]
    public void NormalizeTags_ShouldRemoveDuplicates()
    {
        // Arrange
        var tags = new List<string> { "Tag1", "tag1", "TAG1", "Tag2" };

        // Act
        var result = CivitAIMapper.NormalizeTags(tags);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
    }
}
