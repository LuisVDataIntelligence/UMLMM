using System.Net;
using System.Net.Http.Json;
using Contracts.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GatewayApi.Tests;

public class ImageEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ImageEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetImages_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/images");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ImageDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetImages_WithRatingFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/images?rating=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ImageDto>>();
        
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Equal(5, item.Rating));
    }

    [Fact]
    public async Task GetImageById_WithValidId_ReturnsImage()
    {
        // First get the list to find a valid ID
        var listResponse = await _client.GetAsync("/api/images");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<ImageDto>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list.Items);
        
        var imageId = list.Items.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/images/{imageId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ImageDto>();
        
        Assert.NotNull(result);
        Assert.Equal(imageId, result.Id);
    }

    [Fact]
    public async Task GetImageById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/images/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
