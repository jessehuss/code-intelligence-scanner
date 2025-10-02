using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cataloger.CatalogApi.Tests.Contract;

public class CollectionsContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CollectionsContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCollection_WithValidName_Returns200AndValidSchema()
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
        Assert.NotNull(collectionDetail.AssociatedTypes);
        Assert.NotNull(collectionDetail.RelatedQueries);
        Assert.NotNull(collectionDetail.Relationships);
    }

    [Fact]
    public async Task GetCollection_WithNonExistentName_Returns404AndValidErrorSchema()
    {
        // Arrange
        var collectionName = "nonexistent";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.Type);
        Assert.NotNull(errorResponse.Title);
        Assert.Equal(404, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.Timestamp);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task GetCollection_WithEmptyName_Returns400AndValidErrorSchema()
    {
        // Arrange
        var collectionName = "";

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

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
    public async Task GetCollection_WithSpecialCharacters_Returns200AndValidSchema()
    {
        // Arrange
        var collectionName = "user-profiles"; // Collection name with special characters

        // Act
        var response = await _client.GetAsync($"/collections/{Uri.EscapeDataString(collectionName)}");

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
        Assert.NotNull(collectionDetail.AssociatedTypes);
        Assert.NotNull(collectionDetail.RelatedQueries);
        Assert.NotNull(collectionDetail.Relationships);
    }

    [Fact]
    public async Task GetCollection_WithUnicodeCharacters_Returns200AndValidSchema()
    {
        // Arrange
        var collectionName = "用户"; // Unicode characters

        // Act
        var response = await _client.GetAsync($"/collections/{Uri.EscapeDataString(collectionName)}");

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
        Assert.NotNull(collectionDetail.AssociatedTypes);
        Assert.NotNull(collectionDetail.RelatedQueries);
        Assert.NotNull(collectionDetail.Relationships);
    }

    [Fact]
    public async Task GetCollection_WithVeryLongName_Returns400AndValidErrorSchema()
    {
        // Arrange
        var collectionName = new string('a', 1000); // Very long name

        // Act
        var response = await _client.GetAsync($"/collections/{Uri.EscapeDataString(collectionName)}");

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
    public async Task GetCollection_WithInvalidCharacters_Returns400AndValidErrorSchema()
    {
        // Arrange
        var collectionName = "invalid/name"; // Invalid characters

        // Act
        var response = await _client.GetAsync($"/collections/{Uri.EscapeDataString(collectionName)}");

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
    public async Task GetCollection_WithValidName_ReturnsCompleteSchema()
    {
        // Arrange
        var collectionName = "users";

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
        
        // Validate declared schema
        Assert.NotNull(collectionDetail.DeclaredSchema);
        Assert.NotNull(collectionDetail.DeclaredSchema.Fields);
        Assert.NotNull(collectionDetail.DeclaredSchema.RequiredFields);
        Assert.NotNull(collectionDetail.DeclaredSchema.Constraints);
        
        // Validate observed schema
        Assert.NotNull(collectionDetail.ObservedSchema);
        Assert.NotNull(collectionDetail.ObservedSchema.Fields);
        Assert.NotNull(collectionDetail.ObservedSchema.RequiredFields);
        Assert.NotNull(collectionDetail.ObservedSchema.Constraints);
        
        // Validate related queries
        Assert.NotNull(collectionDetail.RelatedQueries);
        foreach (var query in collectionDetail.RelatedQueries)
        {
            Assert.NotNull(query.Operation);
            Assert.NotNull(query.Repository);
            Assert.NotNull(query.FilePath);
            Assert.True(query.LineNumber > 0);
        }
        
        // Validate relationships
        Assert.NotNull(collectionDetail.Relationships);
        foreach (var relationship in collectionDetail.Relationships)
        {
            Assert.NotNull(relationship.Type);
            Assert.NotNull(relationship.TargetEntity);
            Assert.NotNull(relationship.Repository);
            Assert.NotNull(relationship.FilePath);
            Assert.True(relationship.LineNumber > 0);
        }
        
        // Validate drift information
        Assert.NotNull(collectionDetail.DriftFlags);
        Assert.True(collectionDetail.DocumentCount >= 0);
        Assert.NotNull(collectionDetail.LastSampled);
        Assert.NotNull(collectionDetail.Repository);
        Assert.NotNull(collectionDetail.FilePath);
        Assert.True(collectionDetail.LineNumber > 0);
        Assert.NotNull(collectionDetail.CommitSha);
    }
}

// DTOs for contract testing
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
