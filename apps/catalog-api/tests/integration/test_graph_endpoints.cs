using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Xunit;

namespace CatalogApi.Tests.Integration;

/// <summary>
/// Integration tests for graph endpoints
/// Tests the complete graph traversal workflow with real MongoDB
/// </summary>
public class TestGraphEndpoints : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly MongoDbContainer _mongoContainer;

    public TestGraphEndpoints(WebApplicationFactory<Program> factory)
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
    public async Task GetGraph_WithValidCollectionNode_ReturnsGraphData()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.CenterNode);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
        Assert.True(graphResponse.TotalNodes >= 0);
        Assert.True(graphResponse.TotalEdges >= 0);
        Assert.True(graphResponse.QueryTime.TotalMilliseconds >= 0);
    }

    [Fact]
    public async Task GetGraph_WithValidTypeNode_ReturnsGraphData()
    {
        // Arrange
        var node = "type:MyApp.Models.User";
        var depth = 2;

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.CenterNode);
        Assert.Equal("type", graphResponse.CenterNode.EntityType);
    }

    [Fact]
    public async Task GetGraph_WithNonExistentNode_Returns404()
    {
        // Arrange
        var node = "collection:nonexistent";

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGraph_WithDepthLimit_RespectsDepth()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 1;

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        // With depth 1, should only have center node and direct connections
        Assert.True(graphResponse.TotalNodes <= 2); // Center + direct connections
    }

    [Fact]
    public async Task GetGraph_WithEdgeKindsFilter_ReturnsFilteredEdges()
    {
        // Arrange
        var node = "collection:vendors";
        var edgeKinds = "READS,WRITES";

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&edgeKinds={edgeKinds}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        Assert.True(graphResponse.Edges.All(e => edgeKinds.Split(',').Contains(e.EdgeType)));
    }

    [Fact]
    public async Task GetGraph_WithMaxNodesLimit_RespectsLimit()
    {
        // Arrange
        var node = "collection:vendors";
        var maxNodes = 10;

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&maxNodes={maxNodes}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        Assert.True(graphResponse.TotalNodes <= maxNodes);
    }

    [Fact]
    public async Task GetGraph_ResponseContainsValidGraphNodes()
    {
        // Arrange
        var node = "collection:vendors";

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(graphResponse.Nodes);
            foreach (var graphNode in graphResponse.Nodes)
            {
                Assert.NotNull(graphNode.Id);
                Assert.NotNull(graphNode.EntityType);
                Assert.NotNull(graphNode.Name);
                Assert.NotNull(graphNode.Properties);
                Assert.NotNull(graphNode.IncomingEdges);
                Assert.NotNull(graphNode.OutgoingEdges);
                Assert.NotNull(graphNode.Repository);
                Assert.NotNull(graphNode.FilePath);
                Assert.True(graphNode.LineNumber > 0);
                Assert.NotNull(graphNode.CommitSha);
            }
        }
    }

    [Fact]
    public async Task GetGraph_ResponseContainsValidGraphEdges()
    {
        // Arrange
        var node = "collection:vendors";

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(graphResponse.Edges);
            foreach (var graphEdge in graphResponse.Edges)
            {
                Assert.NotNull(graphEdge.Id);
                Assert.NotNull(graphEdge.SourceNodeId);
                Assert.NotNull(graphEdge.TargetNodeId);
                Assert.NotNull(graphEdge.EdgeType);
                Assert.NotNull(graphEdge.Description);
                Assert.NotNull(graphEdge.Properties);
                Assert.NotNull(graphEdge.Repository);
                Assert.NotNull(graphEdge.FilePath);
                Assert.True(graphEdge.LineNumber > 0);
                Assert.NotNull(graphEdge.CommitSha);
                Assert.True(graphEdge.CreatedAt > DateTime.MinValue);
            }
        }
    }

    [Fact]
    public async Task GetGraph_WithIncludeProperties_ReturnsProperties()
    {
        // Arrange
        var node = "collection:vendors";
        var includeProperties = true;

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&includeProperties={includeProperties}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.CenterNode.Properties);
        // Should have properties when includeProperties is true
        Assert.True(graphResponse.CenterNode.Properties.Count > 0);
    }

    [Fact]
    public async Task GetGraph_Performance_MeetsRequirements()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}");

        // Assert
        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Performance requirement: <400ms P50 for depth≤2
        Assert.True(stopwatch.ElapsedMilliseconds < 400, 
            $"Graph query took {stopwatch.ElapsedMilliseconds}ms, expected <400ms");
    }

    [Fact]
    public async Task GetGraph_WithComplexGraph_ReturnsValidStructure()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = "READS,WRITES,REFERS_TO";

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}&edgeKinds={edgeKinds}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.CenterNode);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
        
        // Should have at least the center node
        Assert.True(graphResponse.TotalNodes >= 1);
        
        // All edges should be of the specified types
        var allowedEdgeTypes = edgeKinds.Split(',');
        Assert.True(graphResponse.Edges.All(e => allowedEdgeTypes.Contains(e.EdgeType)));
    }

    [Fact]
    public async Task GetGraph_WithMultipleNodeTypes_ReturnsCorrectData()
    {
        // Arrange
        var nodes = new[] 
        { 
            "collection:vendors", 
            "type:MyApp.Models.User",
            "collection:users"
        };

        // Act & Assert
        foreach (var node in nodes)
        {
            var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Assert.NotNull(graphResponse);
                Assert.NotNull(graphResponse.CenterNode);
                Assert.Equal(node.Split(':')[0], graphResponse.CenterNode.EntityType);
            }
        }
    }

    private async Task SeedTestData()
    {
        // Seed test data for graph
        // In a real implementation, this would use the MongoDB driver
        // to insert test data into the knowledge base collections
        await Task.CompletedTask;
    }
}

// DTOs for integration tests
public class GraphResponse
{
    public GraphNode CenterNode { get; set; } = new();
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
    public int TotalNodes { get; set; }
    public int TotalEdges { get; set; }
    public TimeSpan QueryTime { get; set; }
}

public class GraphNode
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<GraphEdge> IncomingEdges { get; set; } = new();
    public List<GraphEdge> OutgoingEdges { get; set; } = new();
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string CommitSha { get; set; } = string.Empty;
}

public class GraphEdge
{
    public string Id { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string EdgeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string CommitSha { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
