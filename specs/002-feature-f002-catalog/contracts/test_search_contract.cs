using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cataloger.CatalogApi.Tests.Contract;

public class SearchContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SearchContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Search_WithValidQuery_Returns200AndValidSchema()
    {
        // Arrange
        var query = "user";
        var kinds = new[] { "type", "collection" };
        var limit = 50;
        var offset = 0;

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={string.Join(",", kinds)}&limit={limit}&offset={offset}");

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
        Assert.Equal(offset, searchResponse.Offset);
        Assert.NotNull(searchResponse.ResultCountsByType);
        Assert.NotNull(searchResponse.QueryTime);
    }

    [Fact]
    public async Task Search_WithInvalidQuery_Returns400AndValidErrorSchema()
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
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithInvalidKinds_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = "user";
        var kinds = new[] { "invalid", "type" }; // Invalid kind should cause error

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={string.Join(",", kinds)}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithExcessiveLimit_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = "user";
        var limit = 1001; // Exceeds maximum limit

        // Act
        var response = await _client.GetAsync($"/search?q={query}&limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithNegativeOffset_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = "user";
        var offset = -1; // Negative offset should be invalid

        // Act
        var response = await _client.GetAsync($"/search?q={query}&offset={offset}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithInvalidSortBy_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = "user";
        var sortBy = "invalid"; // Invalid sort field

        // Act
        var response = await _client.GetAsync($"/search?q={query}&sortBy={sortBy}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithInvalidSortOrder_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = "user";
        var sortOrder = "invalid"; // Invalid sort order

        // Act
        var response = await _client.GetAsync($"/search?q={query}&sortOrder={sortOrder}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithValidFilters_Returns200AndValidSchema()
    {
        // Arrange
        var query = "user";
        var filters = JsonSerializer.Serialize(new { repository = "myapp" });

        // Act
        var response = await _client.GetAsync($"/search?q={query}&filters={Uri.EscapeDataString(filters)}");

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
        Assert.NotNull(searchResponse.ResultCountsByType);
        Assert.NotNull(searchResponse.QueryTime);
    }

    [Fact]
    public async Task Search_WithInvalidFilters_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = "user";
        var filters = "invalid json"; // Invalid JSON should cause error

        // Act
        var response = await _client.GetAsync($"/search?q={query}&filters={Uri.EscapeDataString(filters)}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithLongQuery_Returns400AndValidErrorSchema()
    {
        // Arrange
        var query = new string('a', 501); // Exceeds maximum length

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
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(400, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task Search_WithAllValidParameters_Returns200AndValidSchema()
    {
        // Arrange
        var query = "user";
        var kinds = new[] { "type", "collection", "field" };
        var limit = 25;
        var offset = 10;
        var sortBy = "name";
        var sortOrder = "asc";
        var filters = JsonSerializer.Serialize(new { repository = "myapp", lastModified = "2024-01-01" });

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={string.Join(",", kinds)}&limit={limit}&offset={offset}&sortBy={sortBy}&sortOrder={sortOrder}&filters={Uri.EscapeDataString(filters)}");

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
        Assert.Equal(offset, searchResponse.Offset);
        Assert.NotNull(searchResponse.ResultCountsByType);
        Assert.NotNull(searchResponse.QueryTime);
    }
}

// DTOs for contract testing
public class SearchResponse
{
    public List<SearchResult> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public bool HasMore { get; set; }
    public Dictionary<string, int> ResultCountsByType { get; set; } = new();
    public string QueryTime { get; set; } = string.Empty;
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
