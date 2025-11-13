using Bunit;
using BlazorFrontend.Components.Shared;
using Xunit;

namespace BlazorFrontend.Tests;

/// <summary>
/// Tests for shared components
/// </summary>
public class SharedComponentTests : TestContext
{
    [Fact]
    public void LoadingSpinner_Renders_WithoutMessage()
    {
        // Act
        var cut = RenderComponent<LoadingSpinner>();

        // Assert
        var spinner = cut.Find(".spinner-border");
        Assert.NotNull(spinner);
        Assert.Contains("Loading...", cut.Markup);
    }

    [Fact]
    public void LoadingSpinner_Renders_WithMessage()
    {
        // Arrange
        var message = "Loading data...";

        // Act
        var cut = RenderComponent<LoadingSpinner>(parameters => parameters
            .Add(p => p.Message, message));

        // Assert
        var spinner = cut.Find(".spinner-border");
        Assert.NotNull(spinner);
        Assert.Contains(message, cut.Markup);
    }

    [Fact]
    public void ErrorDisplay_Renders_WithMessage()
    {
        // Arrange
        var errorMessage = "An error occurred";

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Message, errorMessage));

        // Assert
        Assert.Contains(errorMessage, cut.Markup);
        Assert.Contains("alert-danger", cut.Markup);
        Assert.Contains("Error", cut.Markup);
    }

    [Fact]
    public void ErrorDisplay_ShowsRetryButton_WhenEnabled()
    {
        // Arrange
        var errorMessage = "An error occurred";

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Message, errorMessage)
            .Add(p => p.ShowRetry, true)
            .Add(p => p.OnRetry, () => { })); // Need to provide a callback for button to render

        // Assert
        var retryButton = cut.Find("button");
        Assert.NotNull(retryButton);
        Assert.Contains("Retry", retryButton.TextContent);
    }

    [Fact]
    public void ErrorDisplay_HidesRetryButton_WhenDisabled()
    {
        // Arrange
        var errorMessage = "An error occurred";

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Message, errorMessage)
            .Add(p => p.ShowRetry, false));

        // Assert
        var buttons = cut.FindAll("button");
        Assert.Empty(buttons);
    }

    [Fact]
    public void ErrorDisplay_InvokesRetryCallback_WhenButtonClicked()
    {
        // Arrange
        var errorMessage = "An error occurred";
        var retryCalled = false;

        // Act
        var cut = RenderComponent<ErrorDisplay>(parameters => parameters
            .Add(p => p.Message, errorMessage)
            .Add(p => p.ShowRetry, true)
            .Add(p => p.OnRetry, () => retryCalled = true));

        var retryButton = cut.Find("button");
        retryButton.Click();

        // Assert
        Assert.True(retryCalled);
    }
}
