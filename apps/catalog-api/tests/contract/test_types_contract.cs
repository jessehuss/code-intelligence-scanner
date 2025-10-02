using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogApi.Tests.Contract;

/// <summary>
/// Contract tests for GET /types/{fqcn} endpoint
/// These tests MUST fail initially and validate the API contract
/// </summary>
public class TestTypesContract : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TestTypesContract(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GET_Types_WithValidFqcn_Returns200WithCorrectSchema()
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
        Assert.NotNull(typeDetail.Fields);
        Assert.NotNull(typeDetail.BsonAttributes);
        Assert.NotNull(typeDetail.CollectionMappings);
        Assert.NotNull(typeDetail.UsageStats);
        Assert.NotNull(typeDetail.ChangeSummary);
        Assert.NotNull(typeDetail.Repository);
        Assert.NotNull(typeDetail.FilePath);
        Assert.True(typeDetail.LineNumber > 0);
        Assert.NotNull(typeDetail.CommitSha);
        Assert.True(typeDetail.LastModified > DateTime.MinValue);
    }

    [Fact]
    public async Task GET_Types_WithNonExistentFqcn_Returns404()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";

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
        Assert.Equal("Not Found", errorResponse.Title);
        Assert.Equal(404, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task GET_Types_WithInvalidFqcn_Returns400()
    {
        // Arrange
        var fqcn = ""; // Empty FQCN should be invalid

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Types_ResponseContainsValidFieldDetail()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (typeDetail.Fields.Any())
            {
                var firstField = typeDetail.Fields.First();
                
                // Validate FieldDetail schema
                Assert.NotNull(firstField.Name);
                Assert.NotNull(firstField.Type);
                Assert.NotNull(firstField.Attributes);
                Assert.NotNull(firstField.Description);
                Assert.NotNull(firstField.DefaultValue);
                Assert.NotNull(firstField.ValidationRules);
            }
        }
    }

    [Fact]
    public async Task GET_Types_ResponseContainsValidCollectionMapping()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (typeDetail.CollectionMappings.Any())
            {
                var firstMapping = typeDetail.CollectionMappings.First();
                
                // Validate CollectionMapping schema
                Assert.NotNull(firstMapping.CollectionName);
                Assert.NotNull(firstMapping.MappingType);
                Assert.NotNull(firstMapping.Repository);
                Assert.NotNull(firstMapping.FilePath);
                Assert.True(firstMapping.LineNumber > 0);
            }
        }
    }

    [Fact]
    public async Task GET_Types_ResponseContainsValidUsageStatistics()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate UsageStatistics schema
            Assert.True(typeDetail.UsageStats.QueryCount >= 0);
            Assert.True(typeDetail.UsageStats.RepositoryCount >= 0);
            Assert.NotNull(typeDetail.UsageStats.UsedInRepositories);
            Assert.True(typeDetail.UsageStats.LastUsed > DateTime.MinValue);
            Assert.NotNull(typeDetail.UsageStats.CommonOperations);
        }
    }

    [Fact]
    public async Task GET_Types_ResponseContainsValidChangeSummary()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate ChangeSummary schema
            Assert.True(typeDetail.ChangeSummary.TotalChanges >= 0);
            Assert.True(typeDetail.ChangeSummary.AddedFields >= 0);
            Assert.True(typeDetail.ChangeSummary.RemovedFields >= 0);
            Assert.True(typeDetail.ChangeSummary.ModifiedFields >= 0);
            Assert.True(typeDetail.ChangeSummary.LastChange > DateTime.MinValue);
            Assert.NotNull(typeDetail.ChangeSummary.RecentCommits);
        }
    }
}

// DTOs for contract validation
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
