using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cataloger.CatalogApi.Tests.Contract;

public class GraphContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GraphContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetGraph_WithValidNode_Returns200AndValidSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS", "WRITES" };

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&depth={depth}&edgeKinds={string.Join(",", edgeKinds)}");

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
        Assert.NotNull(graphResponse.QueryTime);
    }

    [Fact]
    public async Task GetGraph_WithInvalidNode_Returns400AndValidErrorSchema()
    {
        // Arrange
        var node = "invalid:node"; // Invalid format

        // Act
        var response = await _client.GetAsync($"/graph?node={node}");

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
    public async Task GetGraph_WithNonExistentNode_Returns404AndValidErrorSchema()
    {
        // Arrange
        var node = "collection:nonexistent";

        // Act
        var response = await _client.GetAsync($"/graph?node={node}");

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
    public async Task GetGraph_WithMissingNode_Returns400AndValidErrorSchema()
    {
        // Arrange
        // Missing node parameter

        // Act
        var response = await _client.GetAsync("/graph");

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
    public async Task GetGraph_WithInvalidDepth_Returns400AndValidErrorSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 10; // Exceeds maximum depth

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&depth={depth}");

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
    public async Task GetGraph_WithNegativeDepth_Returns400AndValidErrorSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = -1; // Negative depth

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&depth={depth}");

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
    public async Task GetGraph_WithInvalidEdgeKinds_Returns400AndValidErrorSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var edgeKinds = new[] { "INVALID", "READS" }; // Invalid edge kind

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&edgeKinds={string.Join(",", edgeKinds)}");

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
    public async Task GetGraph_WithExcessiveMaxNodes_Returns400AndValidErrorSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var maxNodes = 1001; // Exceeds maximum

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&maxNodes={maxNodes}");

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
    public async Task GetGraph_WithInsufficientMaxNodes_Returns400AndValidErrorSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var maxNodes = 5; // Below minimum

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&maxNodes={maxNodes}");

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
    public async Task GetGraph_WithValidParameters_ReturnsCompleteSchema()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS", "WRITES", "REFERS_TO" };
        var maxNodes = 100;
        var includeProperties = true;

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&depth={depth}&edgeKinds={string.Join(",", edgeKinds)}&maxNodes={maxNodes}&includeProperties={includeProperties}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(graphResponse);
        
        // Validate center node
        Assert.NotNull(graphResponse.CenterNode);
        Assert.NotNull(graphResponse.CenterNode.Id);
        Assert.NotNull(graphResponse.CenterNode.EntityType);
        Assert.NotNull(graphResponse.CenterNode.Name);
        Assert.NotNull(graphResponse.CenterNode.IncomingEdges);
        Assert.NotNull(graphResponse.CenterNode.OutgoingEdges);
        
        // Validate nodes
        Assert.NotNull(graphResponse.Nodes);
        foreach (var graphNode in graphResponse.Nodes)
        {
            Assert.NotNull(graphNode.Id);
            Assert.NotNull(graphNode.EntityType);
            Assert.NotNull(graphNode.Name);
            Assert.NotNull(graphNode.IncomingEdges);
            Assert.NotNull(graphNode.OutgoingEdges);
        }
        
        // Validate edges
        Assert.NotNull(graphResponse.Edges);
        foreach (var edge in graphResponse.Edges)
        {
            Assert.NotNull(edge.Id);
            Assert.NotNull(edge.SourceNodeId);
            Assert.NotNull(edge.TargetNodeId);
            Assert.NotNull(edge.EdgeType);
            Assert.NotNull(edge.CreatedAt);
        }
        
        // Validate counts
        Assert.True(graphResponse.TotalNodes >= 0);
        Assert.True(graphResponse.TotalEdges >= 0);
        Assert.NotNull(graphResponse.QueryTime);
    }

    [Fact]
    public async Task GetGraph_WithTypeNode_Returns200AndValidSchema()
    {
        // Arrange
        var node = "type:MyApp.Models.User";
        var depth = 1;

        // Act
        var response = await _client.GetAsync($"/graph?node={node}&depth={depth}");

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
        Assert.NotNull(graphResponse.QueryTime);
    }

    [Fact]
    public async Task GetGraph_WithUnicodeNode_Returns200AndValidSchema()
    {
        // Arrange
        var node = "collection:用户"; // Unicode characters
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
        Assert.NotNull(graphResponse.CenterNode);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
        Assert.True(graphResponse.TotalNodes >= 0);
        Assert.True(graphResponse.TotalEdges >= 0);
        Assert.NotNull(graphResponse.QueryTime);
    }
}

// DTOs for contract testing
public class GraphResponse
{
    public GraphNode CenterNode { get; set; } = new();
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
    public int TotalNodes { get; set; }
    public int TotalEdges { get; set; }
    public string QueryTime { get; set; } = string.Empty;
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
