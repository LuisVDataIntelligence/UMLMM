using UMLMM.Domain.Entities;
using Xunit;

namespace UMLMM.Domain.Tests.Entities;

public class TagTests
{
    [Fact]
    public void Tag_CanBeCreated_WithNormalizedName()
    {
        // Arrange & Act
        var tag = new Tag
        {
            Name = "Character",
            NormalizedName = "character",
            SourceId = 1
        };

        // Assert
        Assert.Equal("Character", tag.Name);
        Assert.Equal("character", tag.NormalizedName);
        Assert.Equal(1, tag.SourceId);
    }

    [Fact]
    public void Tag_CanBeCreated_WithoutSource()
    {
        // Arrange & Act
        var tag = new Tag
        {
            Name = "Global Tag",
            NormalizedName = "global_tag",
            SourceId = null
        };

        // Assert
        Assert.Equal("Global Tag", tag.Name);
        Assert.Null(tag.SourceId);
    }
}
