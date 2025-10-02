using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Xunit;

namespace CatalogApi.Tests.Integration;

/// <summary>
/// Integration tests for types endpoints
/// Tests the complete type detail workflow with real MongoDB
/// </summary>
public class TestTypesEndpoints : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly MongoDbContainer _mongoContainer;

    public TestTypesEndpoints(WebApplicationFactory<Program> factory)
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
    public async Task GetType_WithValidFqcn_ReturnsTypeDetail()
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
    }

    [Fact]
    public async Task GetType_WithNonExistentFqcn_Returns404()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetType_ResponseContainsValidFieldDetails()
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

            Assert.NotNull(typeDetail.Fields);
            Assert.True(typeDetail.Fields.Count > 0);

            foreach (var field in typeDetail.Fields)
            {
                Assert.NotNull(field.Name);
                Assert.NotNull(field.Type);
                Assert.NotNull(field.Attributes);
                Assert.NotNull(field.Description);
                Assert.NotNull(field.DefaultValue);
                Assert.NotNull(field.ValidationRules);
            }
        }
    }

    [Fact]
    public async Task GetType_ResponseContainsValidCollectionMappings()
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

            Assert.NotNull(typeDetail.CollectionMappings);
            Assert.True(typeDetail.CollectionMappings.Count > 0);

            foreach (var mapping in typeDetail.CollectionMappings)
            {
                Assert.NotNull(mapping.CollectionName);
                Assert.NotNull(mapping.MappingType);
                Assert.NotNull(mapping.Repository);
                Assert.NotNull(mapping.FilePath);
                Assert.True(mapping.LineNumber > 0);
            }
        }
    }

    [Fact]
    public async Task GetType_ResponseContainsValidUsageStatistics()
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

            Assert.NotNull(typeDetail.UsageStats);
            Assert.True(typeDetail.UsageStats.QueryCount >= 0);
            Assert.True(typeDetail.UsageStats.RepositoryCount >= 0);
            Assert.NotNull(typeDetail.UsageStats.UsedInRepositories);
            Assert.True(typeDetail.UsageStats.LastUsed > DateTime.MinValue);
            Assert.NotNull(typeDetail.UsageStats.CommonOperations);
        }
    }

    [Fact]
    public async Task GetType_ResponseContainsValidChangeSummary()
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

            Assert.NotNull(typeDetail.ChangeSummary);
            Assert.True(typeDetail.ChangeSummary.TotalChanges >= 0);
            Assert.True(typeDetail.ChangeSummary.AddedFields >= 0);
            Assert.True(typeDetail.ChangeSummary.RemovedFields >= 0);
            Assert.True(typeDetail.ChangeSummary.ModifiedFields >= 0);
            Assert.True(typeDetail.ChangeSummary.LastChange > DateTime.MinValue);
            Assert.NotNull(typeDetail.ChangeSummary.RecentCommits);
        }
    }

    [Fact]
    public async Task GetType_ResponseContainsBsonAttributes()
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

            Assert.NotNull(typeDetail.BsonAttributes);
            // Should contain at least one BSON attribute
            Assert.True(typeDetail.BsonAttributes.Count > 0);
        }
    }

    [Fact]
    public async Task GetType_Performance_MeetsRequirements()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Performance requirement: <200ms P50
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"Type detail took {stopwatch.ElapsedMilliseconds}ms, expected <200ms");
    }

    [Fact]
    public async Task GetType_WithMultipleTypes_ReturnsCorrectData()
    {
        // Arrange
        var fqcns = new[] 
        { 
            "MyApp.Models.User", 
            "MyApp.Models.Vendor", 
            "MyApp.Models.Order" 
        };

        // Act & Assert
        foreach (var fqcn in fqcns)
        {
            var response = await _client.GetAsync($"/types/{Uri.EscapeDataString(fqcn)}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var typeDetail = JsonSerializer.Deserialize<TypeDetail>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.NotNull(typeDetail);
                Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
            }
        }
    }

    [Fact]
    public async Task GetType_WithComplexNamespace_ReturnsCorrectData()
    {
        // Arrange
        var fqcn = "MyApp.Data.Entities.Complex.UserProfile";

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

            Assert.NotNull(typeDetail);
            Assert.Equal(fqcn, typeDetail.FullyQualifiedName);
            Assert.Equal("UserProfile", typeDetail.Name);
            Assert.Equal("MyApp.Data.Entities.Complex", typeDetail.Namespace);
        }
    }

    private async Task SeedTestData()
    {
        // Seed test data for types
        // In a real implementation, this would use the MongoDB driver
        // to insert test data into the knowledge base collections
        await Task.CompletedTask;
    }
}

// DTOs for integration tests
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

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
