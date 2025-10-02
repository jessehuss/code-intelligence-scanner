using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cataloger.CatalogApi.Tests.Contract;

public class TypesContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TypesContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetType_WithValidFqcn_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDetail);
        Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
        Assert.NotNull(typeDetail.Fields);
        Assert.NotNull(typeDetail.CollectionMappings);
        Assert.NotNull(typeDetail.UsageStats);
        Assert.NotNull(typeDetail.ChangeSummary);
    }

    [Fact]
    public async Task GetType_WithNonExistentFqcn_Returns404AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "NonExistent.Type";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

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
    public async Task GetType_WithEmptyFqcn_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "";

        // Act
        var response = await _client.GetAsync($"/types/{fqcn}");

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
    public async Task GetType_WithInvalidFqcn_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "Invalid..Type"; // Invalid format

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

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
    public async Task GetType_WithUnicodeFqcn_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.用户.User"; // Unicode characters

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDetail);
        Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
        Assert.NotNull(typeDetail.Fields);
        Assert.NotNull(typeDetail.CollectionMappings);
        Assert.NotNull(typeDetail.UsageStats);
        Assert.NotNull(typeDetail.ChangeSummary);
    }

    [Fact]
    public async Task GetType_WithVeryLongFqcn_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "Very.Long.Namespace." + new string('A', 1000) + ".Type"; // Very long FQCN

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

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
    public async Task GetType_WithValidFqcn_ReturnsCompleteSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDetail);
        Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
        Assert.NotNull(typeDetail.Name);
        Assert.NotNull(typeDetail.Namespace);
        
        // Validate fields
        Assert.NotNull(typeDetail.Fields);
        foreach (var field in typeDetail.Fields)
        {
            Assert.NotNull(field.Name);
            Assert.NotNull(field.Type);
            Assert.NotNull(field.Attributes);
            Assert.NotNull(field.ValidationRules);
        }
        
        // Validate BSON attributes
        Assert.NotNull(typeDetail.BsonAttributes);
        
        // Validate collection mappings
        Assert.NotNull(typeDetail.CollectionMappings);
        foreach (var mapping in typeDetail.CollectionMappings)
        {
            Assert.NotNull(mapping.CollectionName);
            Assert.NotNull(mapping.MappingType);
            Assert.NotNull(mapping.Repository);
            Assert.NotNull(mapping.FilePath);
            Assert.True(mapping.LineNumber > 0);
        }
        
        // Validate usage statistics
        Assert.NotNull(typeDetail.UsageStats);
        Assert.True(typeDetail.UsageStats.QueryCount >= 0);
        Assert.True(typeDetail.UsageStats.RepositoryCount >= 0);
        Assert.NotNull(typeDetail.UsageStats.UsedInRepositories);
        Assert.NotNull(typeDetail.UsageStats.CommonOperations);
        
        // Validate change summary
        Assert.NotNull(typeDetail.ChangeSummary);
        Assert.True(typeDetail.ChangeSummary.TotalChanges >= 0);
        Assert.True(typeDetail.ChangeSummary.AddedFields >= 0);
        Assert.True(typeDetail.ChangeSummary.RemovedFields >= 0);
        Assert.True(typeDetail.ChangeSummary.ModifiedFields >= 0);
        Assert.NotNull(typeDetail.ChangeSummary.RecentCommits);
        
        // Validate provenance
        Assert.NotNull(typeDetail.Repository);
        Assert.NotNull(typeDetail.FilePath);
        Assert.True(typeDetail.LineNumber > 0);
        Assert.NotNull(typeDetail.CommitSha);
        Assert.NotNull(typeDetail.LastModified);
    }

    [Fact]
    public async Task GetType_WithGenericType_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.List`1[MyApp.Models.User]"; // Generic type

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDetail);
        Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
        Assert.NotNull(typeDetail.Fields);
        Assert.NotNull(typeDetail.CollectionMappings);
        Assert.NotNull(typeDetail.UsageStats);
        Assert.NotNull(typeDetail.ChangeSummary);
    }

    [Fact]
    public async Task GetType_WithNestedType_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User+Address"; // Nested type

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDetail);
        Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
        Assert.NotNull(typeDetail.Fields);
        Assert.NotNull(typeDetail.CollectionMappings);
        Assert.NotNull(typeDetail.UsageStats);
        Assert.NotNull(typeDetail.ChangeSummary);
    }
}

// DTOs for contract testing
public class TypeDetail
{
    public string FullyQualifiedName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FieldDetail> Fields { get; set; } = new();
    public List<string> BsonAttributes { get; set; } = new();
    public List<CollectionMapping> CollectionMappings { get; set; } = new();
    public UsageStatistics UsageStats { get; set; } = new();
    public ChangeSummary ChangeSummary { get; set; } = new();
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string CommitSha { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

public class FieldDetail
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
    public List<string> Attributes { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public List<string> ValidationRules { get; set; } = new();
}

public class CollectionMapping
{
    public string CollectionName { get; set; } = string.Empty;
    public string MappingType { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

public class UsageStatistics
{
    public int QueryCount { get; set; }
    public int RepositoryCount { get; set; }
    public List<string> UsedInRepositories { get; set; } = new();
    public DateTime LastUsed { get; set; }
    public List<string> CommonOperations { get; set; } = new();
}

public class ChangeSummary
{
    public int TotalChanges { get; set; }
    public int AddedFields { get; set; }
    public int RemovedFields { get; set; }
    public int ModifiedFields { get; set; }
    public DateTime LastChange { get; set; }
    public List<string> RecentCommits { get; set; } = new();
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
