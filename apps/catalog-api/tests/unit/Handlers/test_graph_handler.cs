using CatalogApi.Handlers;
using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for GraphHandler
/// </summary>
public class TestGraphHandler
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IObservabilityService> _mockObservabilityService;
    private readonly GraphHandler _handler;

    public TestGraphHandler()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockObservabilityService = new Mock<IObservabilityService>();
        
        _handler = new GraphHandler(
            _mockKnowledgeBaseService.Object,
            _mockCacheService.Object,
            _mockObservabilityService.Object);
    }

    [Fact]
    public async Task GetGraphAsync_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS", "WRITES" };
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>
            {
                new GraphNode
                {
                    Id = "collection:vendors",
                    EntityType = "collection",
                    Name = "vendors",
                    Description = "Vendors collection",
                    Properties = new Dictionary<string, object>(),
                    IncomingEdges = new List<GraphEdge>(),
                    OutgoingEdges = new List<GraphEdge>(),
                    Repository = "test-repo",
                    FilePath = "src/Collections.cs",
                    LineNumber = 15,
                    CommitSha = "abc123def456"
                },
                new GraphNode
                {
                    Id = "type:Vendor",
                    EntityType = "type",
                    Name = "Vendor",
                    Description = "Vendor entity",
                    Properties = new Dictionary<string, object>(),
                    IncomingEdges = new List<GraphEdge>(),
                    OutgoingEdges = new List<GraphEdge>(),
                    Repository = "test-repo",
                    FilePath = "src/Models/Vendor.cs",
                    LineNumber = 20,
                    CommitSha = "abc123def456"
                }
            },
            Edges = new List<GraphEdge>
            {
                new GraphEdge
                {
                    Id = "edge1",
                    SourceNodeId = "type:Vendor",
                    TargetNodeId = "collection:vendors",
                    EdgeType = "READS",
                    Description = "Vendor reads from vendors collection",
                    Properties = new Dictionary<string, object>(),
                    Repository = "test-repo",
                    FilePath = "src/Repositories/VendorRepository.cs",
                    LineNumber = 25,
                    CommitSha = "abc123def456",
                    CreatedAt = DateTime.UtcNow
                }
            },
            QueryTime = TimeSpan.FromMilliseconds(150),
            TotalNodes = 2,
            TotalEdges = 1
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        var okResult = (Ok<GraphResponse>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetGraphAsync(It.Is<GraphRequest>(r => 
            r.Node == node && r.Depth == depth && r.EdgeKinds!.SequenceEqual(edgeKinds))), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetGraphAsync_WithCachedResult_ReturnsCachedResult()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS" };
        
        var cachedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>(),
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(50),
            TotalNodes = 0,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        var okResult = (Ok<GraphResponse>)result;
        Assert.Equal(cachedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetGraphAsync(It.IsAny<GraphRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetGraphAsync_WithEmptyNode_ReturnsBadRequest()
    {
        // Arrange
        var node = "";
        var depth = 2;
        var edgeKinds = new[] { "READS" };

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Node parameter is required and must be in format 'kind:name'", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetGraphAsync_WithInvalidNodeFormat_ReturnsBadRequest()
    {
        // Arrange
        var node = "invalidformat"; // Missing colon
        var depth = 2;
        var edgeKinds = new[] { "READS" };

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task GetGraphAsync_WithNegativeDepth_ReturnsBadRequest()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = -1;
        var edgeKinds = new[] { "READS" };

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Depth must be between 1 and 5", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetGraphAsync_WithExcessiveDepth_ReturnsBadRequest()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 10; // Max allowed is 5
        var edgeKinds = new[] { "READS" };

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task GetGraphAsync_WithDefaultDepth_ReturnsOkResult()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 0; // Should default to 2
        var edgeKinds = new[] { "READS" };
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>(),
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(100),
            TotalNodes = 0,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        
        // Verify depth was set to default value of 2
        _mockKnowledgeBaseService.Verify(x => x.GetGraphAsync(It.Is<GraphRequest>(r => r.Depth == 2)), Times.Once);
    }

    [Fact]
    public async Task GetGraphAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS" };
        
        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<StatusCodeHttpResult>(result);
        var statusResult = (StatusCodeHttpResult)result;
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetGraphAsync_GeneratesCorrectCacheKey()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS", "WRITES" };
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>(),
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(100),
            TotalNodes = 0,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        
        // Verify cache key is generated correctly
        _mockCacheService.Verify(x => x.GetAsync<GraphResponse>("graph:collection:vendors:2:READS,WRITES"), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync("graph:collection:vendors:2:READS,WRITES", expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetGraphAsync_SetsCorrectCacheTtl()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new[] { "READS" };
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>(),
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(100),
            TotalNodes = 0,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        
        // Verify cache TTL is 15 minutes
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            expectedResponse, 
            TimeSpan.FromMinutes(15)), Times.Once);
    }

    [Fact]
    public async Task GetGraphAsync_WithEmptyEdgeKinds_HandlesCorrectly()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        var edgeKinds = new string[0]; // Empty array
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>(),
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(100),
            TotalNodes = 0,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        
        // Verify empty edge kinds are handled
        _mockKnowledgeBaseService.Verify(x => x.GetGraphAsync(It.Is<GraphRequest>(r => 
            r.EdgeKinds != null && r.EdgeKinds.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task GetGraphAsync_WithNullEdgeKinds_HandlesCorrectly()
    {
        // Arrange
        var node = "collection:vendors";
        var depth = 2;
        string[]? edgeKinds = null;
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>(),
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(100),
            TotalNodes = 0,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        
        // Verify null edge kinds are handled
        _mockKnowledgeBaseService.Verify(x => x.GetGraphAsync(It.Is<GraphRequest>(r => 
            r.EdgeKinds == null)), Times.Once);
    }

    [Fact]
    public async Task GetGraphAsync_WithComplexNodeId_HandlesCorrectly()
    {
        // Arrange
        var node = "type:MyApp.Data.Entities.Complex.UserProfile";
        var depth = 3;
        var edgeKinds = new[] { "REFERS_TO", "READS" };
        
        var expectedResponse = new GraphResponse
        {
            Nodes = new List<GraphNode>
            {
                new GraphNode
                {
                    Id = node,
                    EntityType = "type",
                    Name = "UserProfile",
                    Description = "Complex user profile",
                    Properties = new Dictionary<string, object>(),
                    IncomingEdges = new List<GraphEdge>(),
                    OutgoingEdges = new List<GraphEdge>(),
                    Repository = "test-repo",
                    FilePath = "src/Models/UserProfile.cs",
                    LineNumber = 20,
                    CommitSha = "abc123def456"
                }
            },
            Edges = new List<GraphEdge>(),
            QueryTime = TimeSpan.FromMilliseconds(200),
            TotalNodes = 1,
            TotalEdges = 0
        };

        _mockCacheService.Setup(x => x.GetAsync<GraphResponse>(It.IsAny<string>()))
            .ReturnsAsync((GraphResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetGraphAsync(It.IsAny<GraphRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetGraphAsync(node, depth, edgeKinds);

        // Assert
        Assert.IsType<Ok<GraphResponse>>(result);
        var okResult = (Ok<GraphResponse>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Single(okResult.Value.Nodes);
        Assert.Equal("UserProfile", okResult.Value.Nodes.First().Name);
    }
}
