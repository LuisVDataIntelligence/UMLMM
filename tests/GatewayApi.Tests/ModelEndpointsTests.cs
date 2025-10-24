using System.Net;
using System.Net.Http.Json;
using Contracts.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GatewayApi.Tests;

public class ModelEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ModelEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetModels_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/models");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ModelDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.True(result.TotalCount > 0);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetModels_WithSearchFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/models?search=Anime");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ModelDto>>();
        
        Assert.NotNull(result);
        Assert.All(result.Items, item => 
            Assert.Contains("Anime", item.Name + item.Description, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetModels_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/models?page=1&pageSize=1");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ModelDto>>();
        
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task GetModelById_WithValidId_ReturnsModel()
    {
        // First get the list to find a valid ID
        var listResponse = await _client.GetAsync("/api/models");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<ModelDto>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list.Items);
        
        var modelId = list.Items.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/models/{modelId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ModelDetailDto>();
        
        Assert.NotNull(result);
        Assert.Equal(modelId, result.Id);
        Assert.NotEmpty(result.Versions);
    }

    [Fact]
    public async Task GetModelById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/models/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
