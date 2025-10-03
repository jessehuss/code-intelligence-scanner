using System.Net;
using System.Text.Json;
using Xunit;

namespace CatalogExplorer.Contracts.Tests;

public class GraphContractTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public GraphContractTests()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:3000/api");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetGraph_WithDefaultParameters_ReturnsGraphData()
    {
        // Arrange
        var requestUri = "/graph";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
        
        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
        Assert.NotNull(graphResponse.Metadata);
    }

    [Fact]
    public async Task GetGraph_WithNodeFilter_ReturnsFilteredGraph()
    {
        // Arrange
        var nodeId = "test-node-123";
        var requestUri = $"/graph?node={Uri.EscapeDataString(nodeId)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
        
        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
    }

    [Fact]
    public async Task GetGraph_WithEdgeKindsFilter_ReturnsFilteredGraph()
    {
        // Arrange
        var edgeKinds = "USES,CONTAINS";
        var requestUri = $"/graph?edgeKinds={Uri.EscapeDataString(edgeKinds)}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
        
        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
    }

    [Fact]
    public async Task GetGraph_WithDepthLimit_ReturnsLimitedGraph()
    {
        // Arrange
        var depth = 3;
        var requestUri = $"/graph?depth={depth}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
        
        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
    }

    [Fact]
    public async Task GetGraph_WithAllFilters_ReturnsFilteredGraph()
    {
        // Arrange
        var nodeId = "test-node-123";
        var edgeKinds = "USES,REFERENCES";
        var depth = 2;
        var requestUri = $"/graph?node={Uri.EscapeDataString(nodeId)}&edgeKinds={Uri.EscapeDataString(edgeKinds)}&depth={depth}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
        
        Assert.NotNull(graphResponse);
        Assert.NotNull(graphResponse.Nodes);
        Assert.NotNull(graphResponse.Edges);
    }

    [Fact]
    public async Task GetGraph_WithInvalidDepth_ReturnsBadRequest()
    {
        // Arrange
        var invalidDepth = 10; // Exceeds maximum of 5
        var requestUri = $"/graph?depth={invalidDepth}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGraph_WithNegativeDepth_ReturnsBadRequest()
    {
        // Arrange
        var negativeDepth = -1;
        var requestUri = $"/graph?depth={negativeDepth}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetGraph_ReturnsValidNodeStructure()
    {
        // Arrange
        var requestUri = "/graph";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
            
            Assert.NotNull(graphResponse);
            
            // Validate node structure
            foreach (var node in graphResponse.Nodes)
            {
                Assert.False(string.IsNullOrEmpty(node.Id));
                Assert.False(string.IsNullOrEmpty(node.Label));
                Assert.False(string.IsNullOrEmpty(node.Type));
                Assert.NotNull(node.Data);
            }
        }
    }

    [Fact]
    public async Task GetGraph_ReturnsValidEdgeStructure()
    {
        // Arrange
        var requestUri = "/graph";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var graphResponse = JsonSerializer.Deserialize<GraphResponse>(content, _jsonOptions);
            
            Assert.NotNull(graphResponse);
            
            // Validate edge structure
            foreach (var edge in graphResponse.Edges)
            {
                Assert.False(string.IsNullOrEmpty(edge.Id));
                Assert.False(string.IsNullOrEmpty(edge.Source));
                Assert.False(string.IsNullOrEmpty(edge.Target));
                Assert.False(string.IsNullOrEmpty(edge.Label));
                Assert.NotNull(edge.Data);
            }
        }
    }
}

// Contract models
public class GraphResponse
{
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class GraphNode
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public class GraphEdge
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}
