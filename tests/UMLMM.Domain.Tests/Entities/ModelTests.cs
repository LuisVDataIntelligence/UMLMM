using UMLMM.Domain.Entities;
using UMLMM.Domain.Enums;
using Xunit;

namespace UMLMM.Domain.Tests.Entities;

public class ModelTests
{
    [Fact]
    public void Model_CanBeCreated_WithValidProperties()
    {
        // Arrange & Act
        var model = new Model
        {
            SourceId = 1,
            ExternalId = "12345",
            Name = "Test Model",
            Type = ModelType.Checkpoint,
            Description = "A test model",
            NsfwRating = NsfwRating.Safe
        };

        // Assert
        Assert.NotNull(model);
        Assert.Equal(1, model.SourceId);
        Assert.Equal("12345", model.ExternalId);
        Assert.Equal("Test Model", model.Name);
        Assert.Equal(ModelType.Checkpoint, model.Type);
        Assert.Equal("A test model", model.Description);
        Assert.Equal(NsfwRating.Safe, model.NsfwRating);
    }

    [Fact]
    public void Model_DefaultNsfwRating_IsUnknown()
    {
        // Arrange & Act
        var model = new Model
        {
            SourceId = 1,
            ExternalId = "12345",
            Name = "Test Model",
            Type = ModelType.Checkpoint
        };

        // Assert
        Assert.Equal(NsfwRating.Unknown, model.NsfwRating);
    }

    [Fact]
    public void Model_SupportsRawJsonData()
    {
        // Arrange & Act
        var model = new Model
        {
            SourceId = 1,
            ExternalId = "12345",
            Name = "Test Model",
            Type = ModelType.Checkpoint,
            Raw = "{\"test\": \"data\"}"
        };

        // Assert
        Assert.NotNull(model.Raw);
        Assert.Contains("test", model.Raw);
    }
}
