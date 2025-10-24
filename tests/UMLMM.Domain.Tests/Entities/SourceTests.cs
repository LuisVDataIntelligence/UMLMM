using UMLMM.Domain.Entities;
using UMLMM.Domain.Enums;
using Xunit;

namespace UMLMM.Domain.Tests.Entities;

public class SourceTests
{
    [Fact]
    public void Source_CanBeCreated_WithValidProperties()
    {
        // Arrange & Act
        var source = new Source
        {
            Name = "CivitAI",
            BaseUrl = "https://civitai.com"
        };

        // Assert
        Assert.NotNull(source);
        Assert.Equal("CivitAI", source.Name);
        Assert.Equal("https://civitai.com", source.BaseUrl);
        Assert.NotEqual(default, source.CreatedAtUtc);
    }
}
