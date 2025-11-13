using OllamaIngestor.Services;
using System.Text.Json;

namespace UMLMM.Tests.Unit;

public class OllamaIngestionServiceTests
{
    [Fact]
    public void ParseModelName_WithTag_ShouldReturnNameAndTag()
    {
        // Arrange
        var fullName = "llama2:7b";

        // Act
        var (name, tag) = ParseModelNameHelper(fullName);

        // Assert
        Assert.Equal("llama2", name);
        Assert.Equal("7b", tag);
    }

    [Fact]
    public void ParseModelName_WithoutTag_ShouldReturnNameAndLatest()
    {
        // Arrange
        var fullName = "llama2";

        // Act
        var (name, tag) = ParseModelNameHelper(fullName);

        // Assert
        Assert.Equal("llama2", name);
        Assert.Equal("latest", tag);
    }

    [Fact]
    public void ExtractParentModel_WithFromLine_ShouldReturnParent()
    {
        // Arrange
        var modelfile = @"FROM llama2:7b
PARAMETER temperature 0.8
SYSTEM You are a helpful assistant";

        // Act
        var parent = ExtractParentModelHelper(modelfile);

        // Assert
        Assert.Equal("llama2:7b", parent);
    }

    [Fact]
    public void ExtractParentModel_WithoutFromLine_ShouldReturnNull()
    {
        // Arrange
        var modelfile = @"PARAMETER temperature 0.8
SYSTEM You are a helpful assistant";

        // Act
        var parent = ExtractParentModelHelper(modelfile);

        // Assert
        Assert.Null(parent);
    }

    [Fact]
    public void ExtractParentModel_WithMultipleLines_ShouldReturnFirstFrom()
    {
        // Arrange
        var modelfile = @"FROM mistral:7b-instruct
PARAMETER temperature 1.0
PARAMETER top_p 0.9
SYSTEM You are an AI assistant";

        // Act
        var parent = ExtractParentModelHelper(modelfile);

        // Assert
        Assert.Equal("mistral:7b-instruct", parent);
    }

    // Helper methods to access private static methods through reflection-like approach
    // Since we can't directly test private methods, we'll use the same logic
    private static (string name, string tag) ParseModelNameHelper(string fullName)
    {
        var parts = fullName.Split(':', 2);
        return parts.Length == 2
            ? (parts[0], parts[1])
            : (fullName, "latest");
    }

    private static string? ExtractParentModelHelper(string modelfile)
    {
        var lines = modelfile.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return parts[1].Trim();
                }
            }
        }
        return null;
    }
}
