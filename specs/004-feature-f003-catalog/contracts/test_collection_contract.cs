using System.Net;
using System.Text.Json;
using Xunit;

namespace CatalogExplorer.Contracts.Tests;

public class CollectionContractTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CollectionContractTests()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:3000/api");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetCollection_WithValidName_ReturnsCollection()
    {
        // Arrange
        var collectionName = "users";
        var requestUri = $"/collections/{Uri.EscapeDataString(collectionName)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var collection = JsonSerializer.Deserialize<Collection>(content, _jsonOptions);
        
        Assert.NotNull(collection);
        Assert.Equal(collectionName, collection.Name);
        Assert.NotNull(collection.DeclaredSchema);
        Assert.NotNull(collection.ObservedSchema);
        Assert.NotNull(collection.PresenceMetrics);
        Assert.NotNull(collection.DriftIndicators);
        Assert.NotNull(collection.Types);
        Assert.NotNull(collection.Queries);
        Assert.NotNull(collection.Relationships);
        Assert.NotNull(collection.Provenance);
    }

    [Fact]
    public async Task GetCollection_WithNonExistentName_ReturnsNotFound()
    {
        // Arrange
        var collectionName = "non-existent-collection";
        var requestUri = $"/collections/{Uri.EscapeDataString(collectionName)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
        
        Assert.NotNull(errorResponse);
        Assert.Equal("Collection not found", errorResponse.Message);
    }

    [Fact]
    public async Task GetCollection_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var collectionName = "test-collection_123";
        var requestUri = $"/collections/{Uri.EscapeDataString(collectionName)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        // Should either return the collection or 404, not 400
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCollection_ReturnsValidSchemaData()
    {
        // Arrange
        var collectionName = "users";
        var requestUri = $"/collections/{Uri.EscapeDataString(collectionName)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collection = JsonSerializer.Deserialize<Collection>(content, _jsonOptions);
            
            Assert.NotNull(collection);
            
            // Validate schema fields
            foreach (var field in collection.DeclaredSchema)
            {
                Assert.False(string.IsNullOrEmpty(field.Name));
                Assert.False(string.IsNullOrEmpty(field.Type));
            }
            
            foreach (var field in collection.ObservedSchema)
            {
                Assert.False(string.IsNullOrEmpty(field.Name));
                Assert.False(string.IsNullOrEmpty(field.Type));
            }
            
            // Validate presence metrics
            Assert.True(collection.PresenceMetrics.PresencePercentage >= 0);
            Assert.True(collection.PresenceMetrics.PresencePercentage <= 100);
            Assert.True(collection.PresenceMetrics.TotalSamples >= 0);
            Assert.True(collection.PresenceMetrics.PresentSamples >= 0);
        }
    }

    [Fact]
    public async Task GetCollection_ReturnsValidDriftIndicators()
    {
        // Arrange
        var collectionName = "users";
        var requestUri = $"/collections/{Uri.EscapeDataString(collectionName)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var collection = JsonSerializer.Deserialize<Collection>(content, _jsonOptions);
            
            Assert.NotNull(collection);
            
            // Validate drift indicators
            foreach (var drift in collection.DriftIndicators)
            {
                Assert.False(string.IsNullOrEmpty(drift.FieldName));
                Assert.False(string.IsNullOrEmpty(drift.DriftType));
                Assert.False(string.IsNullOrEmpty(drift.Severity));
                Assert.False(string.IsNullOrEmpty(drift.Description));
                Assert.False(string.IsNullOrEmpty(drift.SuggestedAction));
            }
        }
    }
}

// Contract models
public class Collection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<SchemaField> DeclaredSchema { get; set; } = new();
    public List<SchemaField> ObservedSchema { get; set; } = new();
    public PresenceMetrics PresenceMetrics { get; set; } = new();
    public List<DriftIndicator> DriftIndicators { get; set; } = new();
    public List<TypeReference> Types { get; set; } = new();
    public List<QueryReference> Queries { get; set; } = new();
    public List<Relationship> Relationships { get; set; } = new();
    public ProvenanceRecord Provenance { get; set; } = new();
}

public class SchemaField
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsArray { get; set; }
    public List<SchemaField> NestedFields { get; set; } = new();
}

public class PresenceMetrics
{
    public int TotalSamples { get; set; }
    public int PresentSamples { get; set; }
    public double PresencePercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class DriftIndicator
{
    public string FieldName { get; set; } = string.Empty;
    public string DriftType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
}

public class TypeReference
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class QueryReference
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class Relationship
{
    public string Id { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string EdgeKind { get; set; } = string.Empty;
    public double Weight { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public ProvenanceRecord Provenance { get; set; } = new();
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

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}
