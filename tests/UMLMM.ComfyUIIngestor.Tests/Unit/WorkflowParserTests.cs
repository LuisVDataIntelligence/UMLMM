using FluentAssertions;
using UMLMM.ComfyUIIngestor.Services;

namespace UMLMM.ComfyUIIngestor.Tests.Unit;

public class WorkflowParserTests
{
    private readonly IWorkflowParser _parser;

    public WorkflowParserTests()
    {
        _parser = new WorkflowParser();
    }

    [Fact]
    public void ParseWorkflow_ShouldParseSimpleWorkflow()
    {
        // Arrange
        var filePath = Path.Combine("Resources", "sample_workflow.json");
        var sourceId = "comfyui-test";

        // Act
        var workflow = _parser.ParseWorkflow(filePath, sourceId);

        // Assert
        workflow.Should().NotBeNull();
        workflow.SourceId.Should().Be(sourceId);
        workflow.ExternalId.Should().Be("sample_workflow.json");
        workflow.Name.Should().Be("Simple Text to Image");
        workflow.NodesCount.Should().Be(5);
        workflow.GraphJsonb.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ParseWorkflow_ShouldParseNodesArrayFormat()
    {
        // Arrange
        var filePath = Path.Combine("Resources", "upscale_workflow.json");
        var sourceId = "comfyui-test";

        // Act
        var workflow = _parser.ParseWorkflow(filePath, sourceId);

        // Assert
        workflow.Should().NotBeNull();
        workflow.SourceId.Should().Be(sourceId);
        workflow.ExternalId.Should().Be("upscale_workflow.json");
        workflow.Name.Should().Be("upscale_workflow");
        workflow.NodesCount.Should().Be(3);
        workflow.GraphJsonb.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ParseWorkflow_ShouldSetCreatedAndUpdatedDates()
    {
        // Arrange
        var filePath = Path.Combine("Resources", "sample_workflow.json");
        var sourceId = "comfyui-test";
        var beforeParse = DateTime.UtcNow;

        // Act
        var workflow = _parser.ParseWorkflow(filePath, sourceId);
        var afterParse = DateTime.UtcNow;

        // Assert
        workflow.CreatedAt.Should().BeOnOrAfter(beforeParse).And.BeOnOrBefore(afterParse);
        workflow.UpdatedAt.Should().BeOnOrAfter(beforeParse).And.BeOnOrBefore(afterParse);
    }
}
