using System.Net;
using System.Net.Http.Json;
using Contracts.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GatewayApi.Tests;

public class TagEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TagEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTags_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/tags");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetTags_WithSearchFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/tags?search=Stable");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagDto>>();
        
        Assert.NotNull(result);
        Assert.All(result.Items, item => 
            Assert.Contains("Stable", item.Name, StringComparison.OrdinalIgnoreCase));
    }
}
