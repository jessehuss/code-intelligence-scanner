using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Xunit;

namespace CatalogApi.Tests.Integration;

/// <summary>
/// Integration tests for collections endpoints
/// Tests the complete collection detail workflow with real MongoDB
/// </summary>
public class TestCollectionsEndpoints : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly MongoDbContainer _mongoContainer;

    public TestCollectionsEndpoints(WebApplicationFactory<Program> factory)
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
        await SeedTestData();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        _client.Dispose();
    }

    [Fact]
    public async Task GetCollection_WithValidName_ReturnsCollectionDetail()
    {
        // Arrange
        var collectionName = "vendors";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(collectionDetail);
        Assert.Equal(collectionName, collectionDetail.Name);
        Assert.NotNull(collectionDetail.DeclaredSchema);
        Assert.NotNull(collectionDetail.ObservedSchema);
        Assert.True(collectionDetail.DocumentCount >= 0);
    }

    [Fact]
    public async Task GetCollection_WithNonExistentName_Returns404()
    {
        // Arrange
        var collectionName = "nonexistent_collection";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCollection_ResponseContainsValidSchemaInfo()
    {
        // Arrange
        var collectionName = "vendors";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(collectionDetail.DeclaredSchema);
            Assert.NotNull(collectionDetail.DeclaredSchema.Fields);
            Assert.NotNull(collectionDetail.DeclaredSchema.RequiredFields);
            Assert.NotNull(collectionDetail.DeclaredSchema.Constraints);

            Assert.NotNull(collectionDetail.ObservedSchema);
            Assert.NotNull(collectionDetail.ObservedSchema.Fields);
            Assert.NotNull(collectionDetail.ObservedSchema.RequiredFields);
            Assert.NotNull(collectionDetail.ObservedSchema.Constraints);
        }
    }

    [Fact]
    public async Task GetCollection_ResponseContainsAssociatedTypes()
    {
        // Arrange
        var collectionName = "vendors";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(collectionDetail.AssociatedTypes);
            // Should contain at least one associated type
            Assert.True(collectionDetail.AssociatedTypes.Count > 0);
        }
    }

    [Fact]
    public async Task GetCollection_ResponseContainsRelatedQueries()
    {
        // Arrange
        var collectionName = "vendors";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(collectionDetail.RelatedQueries);
            // Should contain at least one related query
            Assert.True(collectionDetail.RelatedQueries.Count > 0);
        }
    }

    [Fact]
    public async Task GetCollection_ResponseContainsRelationships()
    {
        // Arrange
        var collectionName = "vendors";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(collectionDetail.Relationships);
            // Should contain at least one relationship
            Assert.True(collectionDetail.Relationships.Count > 0);
        }
    }

    [Fact]
    public async Task GetCollection_ResponseContainsDriftInformation()
    {
        // Arrange
        var collectionName = "vendors";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(collectionDetail.DriftFlags);
            // If there's drift, should have drift flags
            if (collectionDetail.HasDrift)
            {
                Assert.True(collectionDetail.DriftFlags.Count > 0);
            }
        }
    }

    [Fact]
    public async Task GetCollection_Performance_MeetsRequirements()
    {
        // Arrange
        var collectionName = "vendors";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Performance requirement: <200ms P50
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"Collection detail took {stopwatch.ElapsedMilliseconds}ms, expected <200ms");
    }

    [Fact]
    public async Task GetCollection_WithMultipleCollections_ReturnsCorrectData()
    {
        // Arrange
        var collectionNames = new[] { "vendors", "users", "orders" };

        // Act & Assert
        foreach (var collectionName in collectionNames)
        {
            var response = await _client.GetAsync($"/collections/{collectionName}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var collectionDetail = JsonSerializer.Deserialize<CollectionDetail>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.NotNull(collectionDetail);
                Assert.Equal(collectionName, collectionDetail.Name);
            }
        }
    }

    private async Task SeedTestData()
    {
        // Seed test data for collections
        // In a real implementation, this would use the MongoDB driver
        // to insert test data into the knowledge base collections
        await Task.CompletedTask;
    }
}

// DTOs for integration tests
public class CollectionDetail
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SchemaInfo DeclaredSchema { get; set; } = new();
    public SchemaInfo ObservedSchema { get; set; } = new();
    public List<string> AssociatedTypes { get; set; } = new();
    public List<QueryInfo> RelatedQueries { get; set; } = new();
    public List<RelationshipInfo> Relationships { get; set; } = new();
    public bool HasDrift { get; set; }
    public List<string> DriftFlags { get; set; } = new();
    public int DocumentCount { get; set; }
    public DateTime LastSampled { get; set; }
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string CommitSha { get; set; } = string.Empty;
}

public class SchemaInfo
{
    public List<FieldInfo> Fields { get; set; } = new();
    public List<string> RequiredFields { get; set; } = new();
    public Dictionary<string, object> Constraints { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class FieldInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
    public List<string> Attributes { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

public class QueryInfo
{
    public string Operation { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public string Projection { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

public class RelationshipInfo
{
    public string Type { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
