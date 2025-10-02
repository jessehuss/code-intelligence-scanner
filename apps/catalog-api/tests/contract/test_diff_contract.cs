using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogApi.Tests.Contract;

/// <summary>
/// Contract tests for GET /diff/type/{fqcn} endpoint
/// These tests MUST fail initially and validate the API contract
/// </summary>
public class TestDiffContract : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TestDiffContract(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GET_Diff_WithValidParameters_Returns200WithCorrectSchema()
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
        Assert.NotNull(typeDiff.AttributeChanges);
        Assert.True(typeDiff.DiffGeneratedAt > DateTime.MinValue);
        Assert.NotNull(typeDiff.Repository);
        Assert.NotNull(typeDiff.FilePath);
    }

    [Fact]
    public async Task GET_Diff_WithNonExistentType_Returns404()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";
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
        Assert.Equal("Not Found", errorResponse.Title);
        Assert.Equal(404, errorResponse.Status);
        Assert.NotNull(errorResponse.Detail);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task GET_Diff_WithInvalidFqcn_Returns400()
    {
        // Arrange
        var fqcn = ""; // Empty FQCN should be invalid
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Diff_WithInvalidFromSha_Returns400()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = ""; // Empty SHA should be invalid
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Diff_WithInvalidToSha_Returns400()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = ""; // Empty SHA should be invalid

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Diff_WithInvalidShaFormat_Returns400()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "invalid-sha"; // Invalid SHA format
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Diff_WithSameFromAndToSha_Returns400()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "abc123def456"; // Same SHA

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Diff_ResponseContainsValidFieldChange()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (typeDiff.AddedFields.Any())
            {
                var firstField = typeDiff.AddedFields.First();
                
                // Validate FieldChange schema
                Assert.NotNull(firstField.FieldName);
                Assert.NotNull(firstField.FieldType);
                Assert.NotNull(firstField.ChangeType);
                Assert.NotNull(firstField.OldValue);
                Assert.NotNull(firstField.NewValue);
                Assert.NotNull(firstField.Description);
            }

            if (typeDiff.RemovedFields.Any())
            {
                var firstField = typeDiff.RemovedFields.First();
                
                // Validate FieldChange schema
                Assert.NotNull(firstField.FieldName);
                Assert.NotNull(firstField.FieldType);
                Assert.NotNull(firstField.ChangeType);
                Assert.NotNull(firstField.OldValue);
                Assert.NotNull(firstField.NewValue);
                Assert.NotNull(firstField.Description);
            }

            if (typeDiff.ModifiedFields.Any())
            {
                var firstField = typeDiff.ModifiedFields.First();
                
                // Validate FieldChange schema
                Assert.NotNull(firstField.FieldName);
                Assert.NotNull(firstField.FieldType);
                Assert.NotNull(firstField.ChangeType);
                Assert.NotNull(firstField.OldValue);
                Assert.NotNull(firstField.NewValue);
                Assert.NotNull(firstField.Description);
            }
        }
    }

    [Fact]
    public async Task GET_Diff_ResponseContainsValidAttributeChange()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (typeDiff.AttributeChanges.Any())
            {
                var firstAttribute = typeDiff.AttributeChanges.First();
                
                // Validate AttributeChange schema
                Assert.NotNull(firstAttribute.AttributeName);
                Assert.NotNull(firstAttribute.ChangeType);
                Assert.NotNull(firstAttribute.OldValue);
                Assert.NotNull(firstAttribute.NewValue);
                Assert.NotNull(firstAttribute.Description);
            }
        }
    }

    [Fact]
    public async Task GET_Diff_WithIncludeFieldDetailsFalse_Returns200()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        var includeFieldDetails = false;

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}&includeFieldDetails={includeFieldDetails}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_Diff_WithIncludeAttributeChangesFalse_Returns200()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        var includeAttributeChanges = false;

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}&includeAttributeChanges={includeAttributeChanges}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

// DTOs for contract validation
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
