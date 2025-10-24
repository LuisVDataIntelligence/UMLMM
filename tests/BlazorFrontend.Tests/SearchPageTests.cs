using Bunit;
using BlazorFrontend.Pages;
using BlazorFrontend.Services;
using Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace BlazorFrontend.Tests;

/// <summary>
/// Tests for the Search Page component
/// </summary>
public class SearchPageTests : TestContext
{
    private readonly IApiClient _mockApiClient;

    public SearchPageTests()
    {
        _mockApiClient = Substitute.For<IApiClient>();
        Services.AddSingleton(_mockApiClient);
    }

    [Fact]
    public void SearchPage_Renders_InitialLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<PagedResultDto<ModelDto>>();
        _mockApiClient.SearchModelsAsync(Arg.Any<SearchRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<SearchPage>();

        // Assert
        Assert.Contains("Searching models", cut.Markup);
        Assert.Contains("spinner-border", cut.Markup);
    }

    [Fact]
    public async Task SearchPage_Renders_SearchResults()
    {
        // Arrange
        var models = new List<ModelDto>
        {
            new() { Id = 1, Name = "Test Model 1", Source = "CivitAI", Rating = 4.5, Tags = new List<string> { "test" }, Images = new List<ModelImageDto>() },
            new() { Id = 2, Name = "Test Model 2", Source = "Danbooru", Rating = 4.0, Tags = new List<string> { "test" }, Images = new List<ModelImageDto>() }
        };

        var searchResult = new PagedResultDto<ModelDto>
        {
            Items = models,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.SearchModelsAsync(Arg.Any<SearchRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(searchResult));

        // Act
        var cut = RenderComponent<SearchPage>();
        await Task.Delay(100); // Wait for async initialization
        cut.Render();

        // Assert
        Assert.Contains("Test Model 1", cut.Markup);
        Assert.Contains("Test Model 2", cut.Markup);
        Assert.Contains("Found 2 model(s)", cut.Markup);
    }

    [Fact]
    public async Task SearchPage_AppliesFilters_WhenSearchClicked()
    {
        // Arrange
        var searchResult = new PagedResultDto<ModelDto>
        {
            Items = new List<ModelDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        SearchRequestDto? capturedRequest = null;
        _mockApiClient.SearchModelsAsync(Arg.Any<SearchRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedRequest = callInfo.Arg<SearchRequestDto>();
                return Task.FromResult(searchResult);
            });

        // Act
        var cut = RenderComponent<SearchPage>();
        await Task.Delay(100);
        cut.Render();

        var searchInput = cut.Find("input[type='text']");
        searchInput.Change("test query");

        var searchButton = cut.Find("button:contains('Search')");
        searchButton.Click();

        await Task.Delay(100);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("test query", capturedRequest.Query);
    }

    [Fact]
    public async Task SearchPage_ShowsPagination_WhenMultiplePages()
    {
        // Arrange
        var models = new List<ModelDto>
        {
            new() { Id = 1, Name = "Test Model 1", Source = "CivitAI", Images = new List<ModelImageDto>(), Tags = new List<string>() }
        };

        var searchResult = new PagedResultDto<ModelDto>
        {
            Items = models,
            TotalCount = 50, // More than one page
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.SearchModelsAsync(Arg.Any<SearchRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(searchResult));

        // Act
        var cut = RenderComponent<SearchPage>();
        await Task.Delay(100);
        cut.Render();

        // Assert
        var pagination = cut.FindAll(".pagination");
        Assert.NotEmpty(pagination);
    }

    [Fact]
    public async Task SearchPage_ClearsFilters_WhenClearClicked()
    {
        // Arrange
        var searchResult = new PagedResultDto<ModelDto>
        {
            Items = new List<ModelDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _mockApiClient.SearchModelsAsync(Arg.Any<SearchRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(searchResult));

        // Act
        var cut = RenderComponent<SearchPage>();
        await Task.Delay(100);
        cut.Render();

        var searchInput = cut.Find("input[type='text']");
        searchInput.Change("test query");

        var clearButton = cut.Find("button:contains('Clear')");
        clearButton.Click();

        await Task.Delay(100);
        cut.Render();

        // Assert
        var inputValue = cut.Find("input[type='text']").GetAttribute("value");
        Assert.True(string.IsNullOrEmpty(inputValue));
    }
}
