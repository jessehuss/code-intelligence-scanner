using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MongoDb;
using Xunit;

namespace CatalogApi.Tests.Integration;

/// <summary>
/// Integration tests for search endpoints
/// Tests the complete search workflow with real MongoDB
/// </summary>
public class TestSearchEndpoints : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly MongoDbContainer _mongoContainer;

    public TestSearchEndpoints(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27017, true)
            .Build();
        
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override MongoDB connection string for test container
                services.Configure<MongoDbSettings>(options =>
                {
                    options.ConnectionString = _mongoContainer.GetConnectionString();
                });
            });
        }).CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        
        // Seed test data
        await SeedTestData();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        _client.Dispose();
    }

    [Fact]
    public async Task Search_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var query = "user";

        // Act
        var response = await _client.GetAsync($"/search?q={query}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        Assert.NotNull(searchResponse.Results);
        Assert.True(searchResponse.Results.Count > 0);
        Assert.True(searchResponse.TotalCount > 0);
    }

    [Fact]
    public async Task Search_WithTypeFilter_ReturnsOnlyTypes()
    {
        // Arrange
        var query = "user";
        var kinds = "type";

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={kinds}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.Results.All(r => r.EntityType == "type"));
    }

    [Fact]
    public async Task Search_WithCollectionFilter_ReturnsOnlyCollections()
    {
        // Arrange
        var query = "vendor";
        var kinds = "collection";

        // Act
        var response = await _client.GetAsync($"/search?q={query}&kinds={kinds}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.Results.All(r => r.EntityType == "collection"));
    }

    [Fact]
    public async Task Search_WithLimit_RespectsLimit()
    {
        // Arrange
        var query = "user";
        var limit = 5;

        // Act
        var response = await _client.GetAsync($"/search?q={query}&limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.Results.Count <= limit);
        Assert.Equal(limit, searchResponse.Limit);
    }

    [Fact]
    public async Task Search_WithOffset_RespectsOffset()
    {
        // Arrange
        var query = "user";
        var limit = 10;
        var offset = 5;

        // Act
        var response = await _client.GetAsync($"/search?q={query}&limit={limit}&offset={offset}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        Assert.Equal(offset, searchResponse.Offset);
    }

    [Fact]
    public async Task Search_WithSortByName_ReturnsSortedResults()
    {
        // Arrange
        var query = "user";
        var sortBy = "name";
        var sortOrder = "asc";

        // Act
        var response = await _client.GetAsync($"/search?q={query}&sortBy={sortBy}&sortOrder={sortOrder}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        if (searchResponse.Results.Count > 1)
        {
            var names = searchResponse.Results.Select(r => r.Name).ToList();
            var sortedNames = names.OrderBy(n => n).ToList();
            Assert.Equal(sortedNames, names);
        }
    }

    [Fact]
    public async Task Search_WithSortByRelevance_ReturnsRelevanceSortedResults()
    {
        // Arrange
        var query = "user";
        var sortBy = "relevance";
        var sortOrder = "desc";

        // Act
        var response = await _client.GetAsync($"/search?q={query}&sortBy={sortBy}&sortOrder={sortOrder}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(searchResponse);
        if (searchResponse.Results.Count > 1)
        {
            var scores = searchResponse.Results.Select(r => r.RelevanceScore).ToList();
            var sortedScores = scores.OrderByDescending(s => s).ToList();
            Assert.Equal(sortedScores, scores);
        }
    }

    [Fact]
    public async Task Search_WithRepositoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var query = "user";
        var filters = JsonSerializer.Serialize(new { repository = "test-repo" });

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
        Assert.True(searchResponse.Results.All(r => r.Repository == "test-repo"));
    }

    [Fact]
    public async Task Search_Performance_MeetsRequirements()
    {
        // Arrange
        var query = "user";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/search?q={query}");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Performance requirement: <300ms P50
        Assert.True(stopwatch.ElapsedMilliseconds < 300, 
            $"Search took {stopwatch.ElapsedMilliseconds}ms, expected <300ms");
    }

    private async Task SeedTestData()
    {
        // This would seed the MongoDB container with test data
        // For now, we'll create a simple test document
        var testData = new
        {
            _id = "test-user-type",
            entityType = "type",
            name = "User",
            description = "User entity",
            relevanceScore = 0.95,
            metadata = new { fieldCount = 5 },
            repository = "test-repo",
            filePath = "src/Models/User.cs",
            lineNumber = 15,
            commitSha = "abc123def456",
            lastModified = DateTime.UtcNow
        };

        // In a real implementation, this would use the MongoDB driver
        // to insert test data into the knowledge base collections
        await Task.CompletedTask;
    }
}

// DTOs for integration tests
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

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
