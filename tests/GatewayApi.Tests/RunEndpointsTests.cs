using System.Net;
using System.Net.Http.Json;
using Contracts.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GatewayApi.Tests;

public class RunEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RunEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRuns_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/runs");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RunDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetRuns_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/runs?status=Completed");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<RunDto>>();
        
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Equal("Completed", item.Status));
    }

    [Fact]
    public async Task GetRunById_WithValidId_ReturnsRun()
    {
        // First get the list to find a valid ID
        var listResponse = await _client.GetAsync("/api/runs");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<RunDto>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list.Items);
        
        var runId = list.Items.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/runs/{runId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RunDto>();
        
        Assert.NotNull(result);
        Assert.Equal(runId, result.Id);
    }

    [Fact]
    public async Task GetRunById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/runs/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
