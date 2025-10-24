using Bunit;
using BlazorFrontend.Pages;
using BlazorFrontend.Services;
using Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace BlazorFrontend.Tests;

/// <summary>
/// Tests for the Runs Dashboard Page component
/// </summary>
public class RunsDashboardPageTests : TestContext
{
    private readonly IApiClient _mockApiClient;

    public RunsDashboardPageTests()
    {
        _mockApiClient = Substitute.For<IApiClient>();
        Services.AddSingleton(_mockApiClient);
    }

    [Fact]
    public void RunsDashboardPage_Renders_LoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<PagedResultDto<RunDto>>();
        _mockApiClient.GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<RunsDashboardPage>();

        // Assert
        Assert.Contains("Loading runs", cut.Markup);
        Assert.Contains("spinner-border", cut.Markup);
    }

    [Fact]
    public async Task RunsDashboardPage_Renders_RunsStatistics()
    {
        // Arrange
        var runs = new List<RunDto>
        {
            new() { Id = 1, Source = "CivitAI", Status = RunStatus.Completed, StartedAt = DateTime.UtcNow.AddHours(-1), CompletedAt = DateTime.UtcNow, RecordsProcessed = 100 },
            new() { Id = 2, Source = "Danbooru", Status = RunStatus.Running, StartedAt = DateTime.UtcNow.AddMinutes(-30), RecordsProcessed = 50 },
            new() { Id = 3, Source = "e621", Status = RunStatus.Failed, StartedAt = DateTime.UtcNow.AddHours(-2), CompletedAt = DateTime.UtcNow.AddHours(-1), RecordsProcessed = 10, ErrorCount = 5 }
        };

        var runsResult = new PagedResultDto<RunDto>
        {
            Items = runs,
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(runsResult));

        // Act
        var cut = RenderComponent<RunsDashboardPage>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("Total Runs", cut.Markup);
        Assert.Contains("3", cut.Markup);
        Assert.Contains("Running", cut.Markup);
        Assert.Contains("Completed", cut.Markup);
        Assert.Contains("Failed", cut.Markup);
    }

    [Fact]
    public async Task RunsDashboardPage_DisplaysRunsTable()
    {
        // Arrange
        var runs = new List<RunDto>
        {
            new() 
            { 
                Id = 1, 
                Source = "CivitAI", 
                Status = RunStatus.Completed, 
                StartedAt = DateTime.UtcNow.AddHours(-1), 
                CompletedAt = DateTime.UtcNow, 
                RecordsProcessed = 100,
                RecordsCreated = 40,
                RecordsUpdated = 60,
                ErrorCount = 0
            }
        };

        var runsResult = new PagedResultDto<RunDto>
        {
            Items = runs,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(runsResult));

        // Act
        var cut = RenderComponent<RunsDashboardPage>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("CivitAI", cut.Markup);
        Assert.Contains("100", cut.Markup); // Records processed
        Assert.Contains("40", cut.Markup);  // Records created
        Assert.Contains("60", cut.Markup);  // Records updated
    }

    [Fact]
    public async Task RunsDashboardPage_ShowsRunningStatus()
    {
        // Arrange
        var runs = new List<RunDto>
        {
            new() 
            { 
                Id = 1, 
                Source = "CivitAI", 
                Status = RunStatus.Running, 
                StartedAt = DateTime.UtcNow.AddMinutes(-30), 
                RecordsProcessed = 50
            }
        };

        var runsResult = new PagedResultDto<RunDto>
        {
            Items = runs,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(runsResult));

        // Act
        var cut = RenderComponent<RunsDashboardPage>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("Running", cut.Markup);
        Assert.Contains("badge bg-info", cut.Markup);
    }

    [Fact]
    public async Task RunsDashboardPage_CallsRefresh_WhenRefreshClicked()
    {
        // Arrange
        var runsResult = new PagedResultDto<RunDto>
        {
            Items = new List<RunDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(runsResult));

        // Act
        var cut = RenderComponent<RunsDashboardPage>();
        await Task.Delay(100);
        cut.Render();

        // Clear received calls
        _mockApiClient.ClearReceivedCalls();

        var refreshButton = cut.Find("button:contains('Refresh')");
        refreshButton.Click();

        await Task.Delay(100);

        // Assert
        await _mockApiClient.Received(1).GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunsDashboardPage_ShowsErrorMessage_ForFailedRuns()
    {
        // Arrange
        var runs = new List<RunDto>
        {
            new() 
            { 
                Id = 1, 
                Source = "CivitAI", 
                Status = RunStatus.Failed, 
                StartedAt = DateTime.UtcNow.AddHours(-1), 
                CompletedAt = DateTime.UtcNow, 
                RecordsProcessed = 10,
                ErrorCount = 5,
                ErrorMessage = "API rate limit exceeded"
            }
        };

        var runsResult = new PagedResultDto<RunDto>
        {
            Items = runs,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.GetRunsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(runsResult));

        // Act
        var cut = RenderComponent<RunsDashboardPage>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        Assert.Contains("Failed", cut.Markup);
        Assert.Contains("API rate limit exceeded", cut.Markup);
        Assert.Contains("5", cut.Markup); // Error count
    }
}
