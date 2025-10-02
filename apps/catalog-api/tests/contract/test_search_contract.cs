using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogApi.Tests.Contract;

/// <summary>
/// Contract tests for GET /search endpoint
/// These tests MUST fail initially and validate the API contract
/// </summary>
public class TestSearchContract : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TestSearchContract(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GET_Search_WithValidQuery_Returns200WithCorrectSchema()
    {
        // Arrange
        var query = "user";
        var kinds = "type,collection";
        var limit = 50;

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={kinds}&limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        Assert.NotNull(searchResponse.Results);
        Assert.True(searchResponse.TotalCount >= 0);
        Assert.Equal(limit, searchResponse.Limit);
        Assert.True(searchResponse.Offset >= 0);
        Assert.NotNull(searchResponse.ResultCountsByType);
        Assert.True(searchResponse.QueryTime.TotalMilliseconds >= 0);
    }

    [Fact]
    public async Task GET_Search_WithInvalidQuery_Returns400()
    {
        // Arrange
        var query = ""; // Empty query should be invalid

        // Act
        var response = await _client.GetAsync($"/search?q={query}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Validation Error", errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task GET_Search_WithInvalidKinds_Returns400()
    {
        // Arrange
        var query = "user";
        var kinds = "invalid,kind"; // Invalid entity kinds

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={kinds}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Validation Error", errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
    }

    [Fact]
    public async Task GET_Search_WithExcessiveLimit_Returns400()
    {
        // Arrange
        var query = "user";
        var limit = 1001; // Exceeds maximum limit

        // Act
        var response = await _client.GetAsync($"/search?q={query}&limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Search_WithNegativeOffset_Returns400()
    {
        // Arrange
        var query = "user";
        var offset = -1; // Negative offset

        // Act
        var response = await _client.GetAsync($"/search?q={query}&offset={offset}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Search_WithInvalidSortBy_Returns400()
    {
        // Arrange
        var query = "user";
        var sortBy = "invalid"; // Invalid sort field

        // Act
        var response = await _client.GetAsync($"/search?q={query}&sortBy={sortBy}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Search_WithInvalidSortOrder_Returns400()
    {
        // Arrange
        var query = "user";
        var sortOrder = "invalid"; // Invalid sort order

        // Act
        var response = await _client.GetAsync($"/search?q={query}&sortOrder={sortOrder}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Search_ResponseContainsValidSearchResults()
    {
        // Arrange
        var query = "user";

        // Act
        var response = await _client.GetAsync($"/search?q={query}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResponse.Results.Any())
            {
                var firstResult = searchResponse.Results.First();
                
                // Validate SearchResult schema
                Assert.NotNull(firstResult.Id);
                Assert.NotNull(firstResult.EntityType);
                Assert.NotNull(firstResult.Name);
                Assert.True(firstResult.RelevanceScore >= 0.0 && firstResult.RelevanceScore <= 1.0);
                Assert.NotNull(firstResult.Metadata);
                Assert.NotNull(firstResult.Repository);
                Assert.NotNull(firstResult.FilePath);
                Assert.True(firstResult.LineNumber > 0);
                Assert.NotNull(firstResult.CommitSha);
                Assert.True(firstResult.LastModified > DateTime.MinValue);
            }
        }
    }
}

// DTOs for contract validation
public class SearchResponse
{
    public List<SearchResult> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public bool HasMore { get; set; }
    public Dictionary<string, int> ResultCountsByType { get; set; } = new();
    public TimeSpan QueryTime { get; set; }
}

public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string CommitSha { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public Dictionary<string, object> Extensions { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
