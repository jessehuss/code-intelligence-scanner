using System.Net;
using System.Text.Json;
using Xunit;

namespace CatalogExplorer.Contracts.Tests;

public class SearchContractTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public SearchContractTests()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:3000/api");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Search_WithValidQuery_ReturnsSearchResponse()
    {
        // Arrange
        var query = "user";
        var requestUri = $"/search?q={Uri.EscapeDataString(query)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, _jsonOptions);
        
        Assert.NotNull(searchResponse);
        Assert.NotNull(searchResponse.Results);
        Assert.NotNull(searchResponse.Pagination);
        Assert.NotNull(searchResponse.Facets);
    }

    [Fact]
    public async Task Search_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var query = "test";
        var page = 2;
        var limit = 10;
        var requestUri = $"/search?q={Uri.EscapeDataString(query)}&page={page}&limit={limit}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, _jsonOptions);
        
        Assert.NotNull(searchResponse);
        Assert.Equal(page, searchResponse.Pagination.Page);
        Assert.Equal(limit, searchResponse.Pagination.Limit);
    }

    [Fact]
    public async Task Search_WithFacets_ReturnsFilteredResults()
    {
        // Arrange
        var query = "collection";
        var repo = "test-repo";
        var service = "test-service";
        var requestUri = $"/search?q={Uri.EscapeDataString(query)}&repo={Uri.EscapeDataString(repo)}&service={Uri.EscapeDataString(service)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, _jsonOptions);
        
        Assert.NotNull(searchResponse);
        Assert.NotNull(searchResponse.Facets);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var requestUri = "/search?q=";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithInvalidPagination_ReturnsBadRequest()
    {
        // Arrange
        var query = "test";
        var requestUri = $"/search?q={Uri.EscapeDataString(query)}&page=0&limit=0";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithChangedSinceFilter_ReturnsFilteredResults()
    {
        // Arrange
        var query = "recent";
        var changedSince = DateTime.UtcNow.AddDays(-7).ToString("O");
        var requestUri = $"/search?q={Uri.EscapeDataString(query)}&changed_since={Uri.EscapeDataString(changedSince)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, _jsonOptions);
        
        Assert.NotNull(searchResponse);
    }
}

// Contract models
public class SearchResponse
{
    public SearchResults Results { get; set; } = new();
    public Pagination Pagination { get; set; } = new();
    public Facets Facets { get; set; } = new();
}

public class SearchResults
{
    public List<CollectionSummary> Collections { get; set; } = new();
    public List<TypeSummary> Types { get; set; } = new();
    public List<FieldSummary> Fields { get; set; } = new();
    public List<QuerySummary> Queries { get; set; } = new();
    public List<ServiceSummary> Services { get; set; } = new();
}

public class CollectionSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DriftCount { get; set; }
    public int TypeCount { get; set; }
    public ProvenanceRecord Provenance { get; set; } = new();
}

public class TypeSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public int FieldCount { get; set; }
    public int UsageCount { get; set; }
    public ProvenanceRecord Provenance { get; set; } = new();
}

public class FieldSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public ProvenanceRecord Provenance { get; set; } = new();
}

public class QuerySummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public ProvenanceRecord Provenance { get; set; } = new();
}

public class ServiceSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int EndpointCount { get; set; }
    public ProvenanceRecord Provenance { get; set; } = new();
}

public class Pagination
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

public class Facets
{
    public List<string> Repositories { get; set; } = new();
    public List<string> Services { get; set; } = new();
    public List<string> Operations { get; set; } = new();
}

public class ProvenanceRecord
{
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string CommitSha { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Extractor { get; set; } = string.Empty;
}
