using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cataloger.CatalogApi.Tests.Contract;

public class DiffContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DiffContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTypeDiff_WithValidParameters_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
        Assert.Equal(fromSha, typeDiff.FromCommitSha);
        Assert.Equal(toSha, typeDiff.ToCommitSha);
        Assert.NotNull(typeDiff.AddedFields);
        Assert.NotNull(typeDiff.RemovedFields);
        Assert.NotNull(typeDiff.ModifiedFields);
        Assert.NotNull(typeDiff.DiffGeneratedAt);
    }

    [Fact]
    public async Task GetTypeDiff_WithNonExistentType_Returns404AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "NonExistent.Type";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

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
    public async Task GetTypeDiff_WithNonExistentCommit_Returns404AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "nonexistent";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

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
    public async Task GetTypeDiff_WithEmptyFqcn_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{fqcn}?fromSha={fromSha}&toSha={toSha}");

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
    public async Task GetTypeDiff_WithMissingFromSha_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?toSha={toSha}");

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
    public async Task GetTypeDiff_WithMissingToSha_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}");

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
    public async Task GetTypeDiff_WithInvalidFromSha_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "invalid"; // Invalid SHA format
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

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
    public async Task GetTypeDiff_WithInvalidToSha_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "invalid"; // Invalid SHA format

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

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
    public async Task GetTypeDiff_WithSameCommitSha_Returns400AndValidErrorSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "abc123def456"; // Same as fromSha

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

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
    public async Task GetTypeDiff_WithAllValidParameters_ReturnsCompleteSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        var includeFieldDetails = true;
        var includeAttributeChanges = true;

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}&includeFieldDetails={includeFieldDetails}&includeAttributeChanges={includeAttributeChanges}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
        Assert.Equal(fromSha, typeDiff.FromCommitSha);
        Assert.Equal(toSha, typeDiff.ToCommitSha);
        
        // Validate added fields
        Assert.NotNull(typeDiff.AddedFields);
        foreach (var field in typeDiff.AddedFields)
        {
            Assert.NotNull(field.FieldName);
            Assert.NotNull(field.FieldType);
            Assert.NotNull(field.ChangeType);
            Assert.Equal("Added", field.ChangeType);
        }
        
        // Validate removed fields
        Assert.NotNull(typeDiff.RemovedFields);
        foreach (var field in typeDiff.RemovedFields)
        {
            Assert.NotNull(field.FieldName);
            Assert.NotNull(field.FieldType);
            Assert.NotNull(field.ChangeType);
            Assert.Equal("Removed", field.ChangeType);
        }
        
        // Validate modified fields
        Assert.NotNull(typeDiff.ModifiedFields);
        foreach (var field in typeDiff.ModifiedFields)
        {
            Assert.NotNull(field.FieldName);
            Assert.NotNull(field.FieldType);
            Assert.NotNull(field.ChangeType);
            Assert.Equal("Modified", field.ChangeType);
        }
        
        // Validate attribute changes
        Assert.NotNull(typeDiff.AttributeChanges);
        foreach (var attr in typeDiff.AttributeChanges)
        {
            Assert.NotNull(attr.AttributeName);
            Assert.NotNull(attr.ChangeType);
        }
        
        // Validate provenance
        Assert.NotNull(typeDiff.DiffGeneratedAt);
        Assert.NotNull(typeDiff.Repository);
        Assert.NotNull(typeDiff.FilePath);
    }

    [Fact]
    public async Task GetTypeDiff_WithUnicodeFqcn_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.用户.User"; // Unicode characters
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
        Assert.Equal(fromSha, typeDiff.FromCommitSha);
        Assert.Equal(toSha, typeDiff.ToCommitSha);
        Assert.NotNull(typeDiff.AddedFields);
        Assert.NotNull(typeDiff.RemovedFields);
        Assert.NotNull(typeDiff.ModifiedFields);
        Assert.NotNull(typeDiff.DiffGeneratedAt);
    }

    [Fact]
    public async Task GetTypeDiff_WithGenericType_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.List`1[MyApp.Models.User]"; // Generic type
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
        Assert.Equal(fromSha, typeDiff.FromCommitSha);
        Assert.Equal(toSha, typeDiff.ToCommitSha);
        Assert.NotNull(typeDiff.AddedFields);
        Assert.NotNull(typeDiff.RemovedFields);
        Assert.NotNull(typeDiff.ModifiedFields);
        Assert.NotNull(typeDiff.DiffGeneratedAt);
    }

    [Fact]
    public async Task GetTypeDiff_WithNestedType_Returns200AndValidSchema()
    {
        // Arrange
        var fqcn = "MyApp.Models.User+Address"; // Nested type
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
        Assert.Equal(fromSha, typeDiff.FromCommitSha);
        Assert.Equal(toSha, typeDiff.ToCommitSha);
        Assert.NotNull(typeDiff.AddedFields);
        Assert.NotNull(typeDiff.RemovedFields);
        Assert.NotNull(typeDiff.ModifiedFields);
        Assert.NotNull(typeDiff.DiffGeneratedAt);
    }
}

// DTOs for contract testing
public class TypeDiff
{
    public string FullyQualifiedName { get; set; } = string.Empty;
    public string FromCommitSha { get; set; } = string.Empty;
    public string ToCommitSha { get; set; } = string.Empty;
    public List<FieldChange> AddedFields { get; set; } = new();
    public List<FieldChange> RemovedFields { get; set; } = new();
    public List<FieldChange> ModifiedFields { get; set; } = new();
    public List<AttributeChange> AttributeChanges { get; set; } = new();
    public DateTime DiffGeneratedAt { get; set; }
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public class FieldChange
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AttributeChange
{
    public string AttributeName { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
