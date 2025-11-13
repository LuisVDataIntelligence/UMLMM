using Bunit;
using BlazorFrontend.Pages;
using BlazorFrontend.Services;
using Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace BlazorFrontend.Tests;

/// <summary>
/// Tests for the Model Detail Page component
/// </summary>
public class ModelDetailPageTests : TestContext
{
    private readonly IApiClient _mockApiClient;

    public ModelDetailPageTests()
    {
        _mockApiClient = Substitute.For<IApiClient>();
        Services.AddSingleton(_mockApiClient);
    }

    [Fact]
    public void ModelDetailPage_Renders_LoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ModelDto?>();
        _mockApiClient.GetModelAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<ModelDetailPage>(parameters => parameters
            .Add(p => p.Id, 1));

        // Assert
        Assert.Contains("Loading model details", cut.Markup);
        Assert.Contains("spinner-border", cut.Markup);
    }

    [Fact]
    public async Task ModelDetailPage_Renders_ModelDetails()
    {
        // Arrange
        var model = new ModelDto
        {
            Id = 1,
            Name = "Test Model",
            Description = "Test Description",
            Source = "CivitAI",
            Rating = 4.5,
            Tags = new List<string> { "tag1", "tag2" },
            Versions = new List<ModelVersionDto>
            {
                new() { Id = 1, Name = "v1.0", Description = "Initial release" }
            },
            Images = new List<ModelImageDto>
            {
                new() { Id = 1, Url = "https://example.com/image.jpg", IsPrimary = true }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow
        };

        _mockApiClient.GetModelAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ModelDto?>(model));

        // Act
        var cut = RenderComponent<ModelDetailPage>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("Test Model", cut.Markup);
        Assert.Contains("Test Description", cut.Markup);
        Assert.Contains("CivitAI", cut.Markup);
        Assert.Contains("4.5", cut.Markup);
        Assert.Contains("tag1", cut.Markup);
        Assert.Contains("tag2", cut.Markup);
    }

    [Fact]
    public async Task ModelDetailPage_ShowsEditForm_WhenEditClicked()
    {
        // Arrange
        var model = new ModelDto
        {
            Id = 1,
            Name = "Test Model",
            Source = "CivitAI",
            Tags = new List<string>(),
            Versions = new List<ModelVersionDto>(),
            Images = new List<ModelImageDto>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockApiClient.GetModelAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ModelDto?>(model));

        // Act
        var cut = RenderComponent<ModelDetailPage>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);
        cut.Render();

        var editButton = cut.Find("button:contains('Edit')");
        editButton.Click();
        cut.Render();

        // Assert
        Assert.Contains("Edit Model", cut.Markup);
        Assert.Contains("Save", cut.Markup);
        Assert.Contains("Cancel", cut.Markup);
    }

    [Fact]
    public async Task ModelDetailPage_CallsUpdateApi_WhenSaveClicked()
    {
        // Arrange
        var model = new ModelDto
        {
            Id = 1,
            Name = "Test Model",
            Description = "Original Description",
            Source = "CivitAI",
            Tags = new List<string> { "tag1" },
            Versions = new List<ModelVersionDto>(),
            Images = new List<ModelImageDto>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockApiClient.GetModelAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ModelDto?>(model));

        var updatedModel = new ModelDto
        {
            Id = 1,
            Name = "Updated Model",
            Description = "Updated Description",
            Source = "CivitAI",
            Tags = new List<string> { "tag1", "tag2" },
            Versions = new List<ModelVersionDto>(),
            Images = new List<ModelImageDto>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockApiClient.UpdateModelAsync(1, Arg.Any<ModelDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(updatedModel));

        // Act
        var cut = RenderComponent<ModelDetailPage>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);
        cut.Render();

        var editButton = cut.Find("button:contains('Edit')");
        editButton.Click();
        cut.Render();

        var nameInput = cut.Find("input[type='text']");
        nameInput.Change("Updated Model");

        var saveButton = cut.Find("button:contains('Save')");
        saveButton.Click();

        await Task.Delay(100);

        // Assert
        await _mockApiClient.Received(1).UpdateModelAsync(1, Arg.Any<ModelDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModelDetailPage_ShowsVersions_WhenAvailable()
    {
        // Arrange
        var model = new ModelDto
        {
            Id = 1,
            Name = "Test Model",
            Source = "CivitAI",
            Tags = new List<string>(),
            Versions = new List<ModelVersionDto>
            {
                new() { Id = 1, Name = "v1.0", Description = "First version" },
                new() { Id = 2, Name = "v2.0", Description = "Second version" }
            },
            Images = new List<ModelImageDto>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockApiClient.GetModelAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ModelDto?>(model));

        // Act
        var cut = RenderComponent<ModelDetailPage>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("Versions (2)", cut.Markup);
        Assert.Contains("v1.0", cut.Markup);
        Assert.Contains("v2.0", cut.Markup);
    }
}
