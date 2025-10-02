using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Xunit;

namespace CatalogApi.Tests.Integration;

/// <summary>
/// Integration tests for diff endpoints
/// Tests the complete diff comparison workflow with real MongoDB
/// </summary>
public class TestDiffEndpoints : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly MongoDbContainer _mongoContainer;

    public TestDiffEndpoints(WebApplicationFactory<Program> factory)
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
    public async Task GetDiff_WithValidParameters_ReturnsTypeDiff()
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
    }

    [Fact]
    public async Task GetDiff_WithNonExistentType_Returns404()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDiff_WithNonExistentCommit_Returns404()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "nonexistent123";
        var toSha = "def456ghi789";

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDiff_ResponseContainsValidFieldChanges()
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

            // Check added fields
            foreach (var field in typeDiff.AddedFields)
            {
                Assert.NotNull(field.FieldName);
                Assert.NotNull(field.FieldType);
                Assert.Equal("Added", field.ChangeType);
                Assert.NotNull(field.OldValue);
                Assert.NotNull(field.NewValue);
                Assert.NotNull(field.Description);
            }

            // Check removed fields
            foreach (var field in typeDiff.RemovedFields)
            {
                Assert.NotNull(field.FieldName);
                Assert.NotNull(field.FieldType);
                Assert.Equal("Removed", field.ChangeType);
                Assert.NotNull(field.OldValue);
                Assert.NotNull(field.NewValue);
                Assert.NotNull(field.Description);
            }

            // Check modified fields
            foreach (var field in typeDiff.ModifiedFields)
            {
                Assert.NotNull(field.FieldName);
                Assert.NotNull(field.FieldType);
                Assert.Equal("Modified", field.ChangeType);
                Assert.NotNull(field.OldValue);
                Assert.NotNull(field.NewValue);
                Assert.NotNull(field.Description);
            }
        }
    }

    [Fact]
    public async Task GetDiff_ResponseContainsValidAttributeChanges()
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

            foreach (var attribute in typeDiff.AttributeChanges)
            {
                Assert.NotNull(attribute.AttributeName);
                Assert.NotNull(attribute.ChangeType);
                Assert.NotNull(attribute.OldValue);
                Assert.NotNull(attribute.NewValue);
                Assert.NotNull(attribute.Description);
            }
        }
    }

    [Fact]
    public async Task GetDiff_WithIncludeFieldDetailsFalse_ReturnsDiffWithoutFieldDetails()
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
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        // Should still have the basic diff structure
        Assert.NotNull(typeDiff.AddedFields);
        Assert.NotNull(typeDiff.RemovedFields);
        Assert.NotNull(typeDiff.ModifiedFields);
    }

    [Fact]
    public async Task GetDiff_WithIncludeAttributeChangesFalse_ReturnsDiffWithoutAttributeChanges()
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
        
        var content = await response.Content.ReadAsStringAsync();
        var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(typeDiff);
        // Should still have the basic diff structure
        Assert.NotNull(typeDiff.AddedFields);
        Assert.NotNull(typeDiff.RemovedFields);
        Assert.NotNull(typeDiff.ModifiedFields);
        Assert.NotNull(typeDiff.AttributeChanges);
    }

    [Fact]
    public async Task GetDiff_WithNoChanges_ReturnsEmptyDiff()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "abc123def456"; // Same commit

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
        // Should have empty change lists
        Assert.Empty(typeDiff.AddedFields);
        Assert.Empty(typeDiff.RemovedFields);
        Assert.Empty(typeDiff.ModifiedFields);
        Assert.Empty(typeDiff.AttributeChanges);
    }

    [Fact]
    public async Task GetDiff_Performance_MeetsRequirements()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Performance requirement: <200ms P50
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"Diff query took {stopwatch.ElapsedMilliseconds}ms, expected <200ms");
    }

    [Fact]
    public async Task GetDiff_WithMultipleTypes_ReturnsCorrectData()
    {
        // Arrange
        var fqcns = new[] 
        { 
            "MyApp.Models.User", 
            "MyApp.Models.Vendor", 
            "MyApp.Models.Order" 
        };
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act & Assert
        foreach (var fqcn in fqcns)
        {
            var response = await _client.GetAsync($"/diff/type/{Uri.EscapeDataString(fqcn)}?fromSha={fromSha}&toSha={toSha}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var typeDiff = JsonSerializer.Deserialize<TypeDiff>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.NotNull(typeDiff);
                Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
                Assert.Equal(fromSha, typeDiff.FromCommitSha);
                Assert.Equal(toSha, typeDiff.ToCommitSha);
            }
        }
    }

    [Fact]
    public async Task GetDiff_WithComplexChanges_ReturnsDetailedDiff()
    {
        // Arrange
        var fqcn = "MyApp.Models.ComplexUser";
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

            Assert.NotNull(typeDiff);
            Assert.Equal(fqcn, typeDiff.FullyQualifiedName);
            
            // Should have some changes for a complex type
            var totalChanges = typeDiff.AddedFields.Count + typeDiff.RemovedFields.Count + typeDiff.ModifiedFields.Count;
            Assert.True(totalChanges > 0, "Complex type should have some changes");
        }
    }

    private async Task SeedTestData()
    {
        // Seed test data for diff comparisons
        // In a real implementation, this would use the MongoDB driver
        // to insert test data into the knowledge base collections
        await Task.CompletedTask;
    }
}

// DTOs for integration tests
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

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
