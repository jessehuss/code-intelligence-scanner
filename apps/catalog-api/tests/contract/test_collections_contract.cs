using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogApi.Tests.Contract;

/// <summary>
/// Contract tests for GET /collections/{name} endpoint
/// These tests MUST fail initially and validate the API contract
/// </summary>
public class TestCollectionsContract : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TestCollectionsContract(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GET_Collections_WithValidName_Returns200WithCorrectSchema()
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
        Assert.True(collectionDetail.DocumentCount >= 0);
        Assert.NotNull(collectionDetail.Repository);
        Assert.NotNull(collectionDetail.FilePath);
        Assert.True(collectionDetail.LineNumber > 0);
        Assert.NotNull(collectionDetail.CommitSha);
    }

    [Fact]
    public async Task GET_Collections_WithNonExistentName_Returns404()
    {
        // Arrange
        var collectionName = "nonexistent_collection";

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
        Assert.Equal("Not Found", errorResponse.Title);
        Assert.Equal(404, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task GET_Collections_WithInvalidName_Returns400()
    {
        // Arrange
        var collectionName = ""; // Empty name should be invalid

        // Act
        var response = await _client.GetAsync($"/collections/{collectionName}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Collections_ResponseContainsValidSchemaInfo()
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
            Assert.True(collectionDetail.DeclaredSchema.LastUpdated > DateTime.MinValue);

            Assert.NotNull(collectionDetail.ObservedSchema);
            Assert.NotNull(collectionDetail.ObservedSchema.Fields);
            Assert.NotNull(collectionDetail.ObservedSchema.RequiredFields);
            Assert.NotNull(collectionDetail.ObservedSchema.Constraints);
            Assert.True(collectionDetail.ObservedSchema.LastUpdated > DateTime.MinValue);
        }
    }

    [Fact]
    public async Task GET_Collections_ResponseContainsValidFieldInfo()
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

            if (collectionDetail.DeclaredSchema.Fields.Any())
            {
                var firstField = collectionDetail.DeclaredSchema.Fields.First();
                
                // Validate FieldInfo schema
                Assert.NotNull(firstField.Name);
                Assert.NotNull(firstField.Type);
                Assert.NotNull(firstField.Attributes);
                Assert.NotNull(firstField.Description);
            }
        }
    }

    [Fact]
    public async Task GET_Collections_ResponseContainsValidQueryInfo()
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

            if (collectionDetail.RelatedQueries.Any())
            {
                var firstQuery = collectionDetail.RelatedQueries.First();
                
                // Validate QueryInfo schema
                Assert.NotNull(firstQuery.Operation);
                Assert.NotNull(firstQuery.Filter);
                Assert.NotNull(firstQuery.Projection);
                Assert.NotNull(firstQuery.Repository);
                Assert.NotNull(firstQuery.FilePath);
                Assert.True(firstQuery.LineNumber > 0);
            }
        }
    }

    [Fact]
    public async Task GET_Collections_ResponseContainsValidRelationshipInfo()
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

            if (collectionDetail.Relationships.Any())
            {
                var firstRelationship = collectionDetail.Relationships.First();
                
                // Validate RelationshipInfo schema
                Assert.NotNull(firstRelationship.Type);
                Assert.NotNull(firstRelationship.TargetEntity);
                Assert.NotNull(firstRelationship.Description);
                Assert.NotNull(firstRelationship.Repository);
                Assert.NotNull(firstRelationship.FilePath);
                Assert.True(firstRelationship.LineNumber > 0);
            }
        }
    }
}

// DTOs for contract validation
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
