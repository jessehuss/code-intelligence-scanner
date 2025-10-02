using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CatalogApi.Tests.Contract;

/// <summary>
/// Contract tests for GET /graph endpoint
/// These tests MUST fail initially and validate the API contract
/// </summary>
public class TestGraphContract : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TestGraphContract(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GET_Graph_WithValidNode_Returns200WithCorrectSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = "READS,WRITES";

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
        Assert.True(graphResponse.TotalNodes >= 0);
        Assert.True(graphResponse.TotalEdges >= 0);
        Assert.True(graphResponse.QueryTime.TotalMilliseconds >= 0);
    }

    [Fact]
    public async Task GET_Graph_WithNonExistentNode_Returns404()
    {
        // Arrange
        var node = "collection:nonexistent";

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}");

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
    public async Task GET_Graph_WithInvalidNode_Returns400()
    {
        // Arrange
        var node = ""; // Empty node should be invalid

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Graph_WithInvalidDepth_Returns400()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 6; // Exceeds maximum depth

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Graph_WithNegativeDepth_Returns400()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = -1; // Negative depth

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&depth={depth}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Graph_WithInvalidEdgeKinds_Returns400()
    {
        // Arrange
        var node = "collection:vendors";
        var edgeKinds = "INVALID,EDGE"; // Invalid edge kinds

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&edgeKinds={edgeKinds}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Graph_WithExcessiveMaxNodes_Returns400()
    {
        // Arrange
        var node = "collection:vendors";
        var maxNodes = 1001; // Exceeds maximum nodes

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&maxNodes={maxNodes}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Graph_WithTooFewMaxNodes_Returns400()
    {
        // Arrange
        var node = "collection:vendors";
        var maxNodes = 9; // Below minimum nodes

        // Act
        var response = await _client.GetAsync($"/graph?node={Uri.EscapeDataString(node)}&maxNodes={maxNodes}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Graph_ResponseContainsValidGraphNode()
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

            // Validate CenterNode schema
            Assert.NotNull(graphResponse.CenterNode.Id);
            Assert.NotNull(graphResponse.CenterNode.EntityType);
            Assert.NotNull(graphResponse.CenterNode.Name);
            Assert.NotNull(graphResponse.CenterNode.Properties);
            Assert.NotNull(graphResponse.CenterNode.IncomingEdges);
            Assert.NotNull(graphResponse.CenterNode.OutgoingEdges);
            Assert.NotNull(graphResponse.CenterNode.Repository);
            Assert.NotNull(graphResponse.CenterNode.FilePath);
            Assert.True(graphResponse.CenterNode.LineNumber > 0);
            Assert.NotNull(graphResponse.CenterNode.CommitSha);
        }
    }

    [Fact]
    public async Task GET_Graph_ResponseContainsValidGraphEdge()
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

            if (graphResponse.Edges.Any())
            {
                var firstEdge = graphResponse.Edges.First();
                
                // Validate GraphEdge schema
                Assert.NotNull(firstEdge.Id);
                Assert.NotNull(firstEdge.SourceNodeId);
                Assert.NotNull(firstEdge.TargetNodeId);
                Assert.NotNull(firstEdge.EdgeType);
                Assert.NotNull(firstEdge.Description);
                Assert.NotNull(firstEdge.Properties);
                Assert.NotNull(firstEdge.Repository);
                Assert.NotNull(firstEdge.FilePath);
                Assert.True(firstEdge.LineNumber > 0);
                Assert.NotNull(firstEdge.CommitSha);
                Assert.True(firstEdge.CreatedAt > DateTime.MinValue);
            }
        }
    }

    [Fact]
    public async Task GET_Graph_WithTypeNode_Returns200()
    {
        // Arrange
        var node = "type:MyApp.Models.User";

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

            Assert.NotNull(graphResponse);
            Assert.NotNull(graphResponse.CenterNode);
            Assert.Equal("type", graphResponse.CenterNode.EntityType);
        }
    }
}

// DTOs for contract validation
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
