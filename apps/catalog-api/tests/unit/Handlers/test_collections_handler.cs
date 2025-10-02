using CatalogApi.Handlers;
using CatalogApi.Models.DTOs;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for CollectionsHandler
/// </summary>
public class TestCollectionsHandler
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IObservabilityService> _mockObservabilityService;
    private readonly CollectionsHandler _handler;

    public TestCollectionsHandler()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockObservabilityService = new Mock<IObservabilityService>();
        
        _handler = new CollectionsHandler(
            _mockKnowledgeBaseService.Object,
            _mockCacheService.Object,
            _mockObservabilityService.Object);
    }

    [Fact]
    public async Task GetCollectionAsync_WithValidName_ReturnsOkResult()
    {
        // Arrange
        var collectionName = "vendors";
        var expectedResponse = new CollectionDetail
        {
            Name = collectionName,
            Description = "Vendors collection",
            DeclaredSchema = new SchemaInfo(),
            ObservedSchema = new SchemaInfo(),
            AssociatedTypes = new List<string> { "Vendor" },
            RelatedQueries = new List<QueryInfo>(),
            Relationships = new List<RelationshipInfo>(),
            HasDrift = false,
            DriftFlags = new List<string>(),
            DocumentCount = 100,
            LastSampled = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/Vendor.cs",
            LineNumber = 15,
            CommitSha = "abc123def456"
        };

        _mockCacheService.Setup(x => x.GetAsync<CollectionDetail>(It.IsAny<string>()))
            .ReturnsAsync((CollectionDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetCollectionAsync(collectionName))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<Ok<CollectionDetail>>(result);
        var okResult = (Ok<CollectionDetail>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetCollectionAsync(collectionName), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetCollectionAsync_WithCachedResult_ReturnsCachedResult()
    {
        // Arrange
        var collectionName = "vendors";
        var cachedResponse = new CollectionDetail
        {
            Name = collectionName,
            Description = "Cached vendors collection",
            DeclaredSchema = new SchemaInfo(),
            ObservedSchema = new SchemaInfo(),
            AssociatedTypes = new List<string>(),
            RelatedQueries = new List<QueryInfo>(),
            Relationships = new List<RelationshipInfo>(),
            HasDrift = false,
            DriftFlags = new List<string>(),
            DocumentCount = 0,
            LastSampled = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/Vendor.cs",
            LineNumber = 15,
            CommitSha = "abc123def456"
        };

        _mockCacheService.Setup(x => x.GetAsync<CollectionDetail>(It.IsAny<string>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<Ok<CollectionDetail>>(result);
        var okResult = (Ok<CollectionDetail>)result;
        Assert.Equal(cachedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetCollectionAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCollectionAsync_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var collectionName = "";

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Collection name is required", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetCollectionAsync_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var collectionName = "   ";

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task GetCollectionAsync_WithNonExistentCollection_ReturnsNotFound()
    {
        // Arrange
        var collectionName = "nonexistent";

        _mockCacheService.Setup(x => x.GetAsync<CollectionDetail>(It.IsAny<string>()))
            .ReturnsAsync((CollectionDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetCollectionAsync(collectionName))
            .ReturnsAsync((CollectionDetail?)null);

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<NotFound<ErrorResponse>>(result);
        var notFoundResult = (NotFound<ErrorResponse>)result;
        Assert.Equal(404, notFoundResult.Value?.Status);
        Assert.Equal("Not Found", notFoundResult.Value?.Title);
        Assert.Contains("not found", notFoundResult.Value?.Detail);
    }

    [Fact]
    public async Task GetCollectionAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var collectionName = "vendors";
        
        _mockCacheService.Setup(x => x.GetAsync<CollectionDetail>(It.IsAny<string>()))
            .ReturnsAsync((CollectionDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetCollectionAsync(collectionName))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<StatusCodeHttpResult>(result);
        var statusResult = (StatusCodeHttpResult)result;
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetCollectionAsync_GeneratesCorrectCacheKey()
    {
        // Arrange
        var collectionName = "Vendors"; // Test case sensitivity
        var expectedResponse = new CollectionDetail
        {
            Name = collectionName,
            Description = "Test collection",
            DeclaredSchema = new SchemaInfo(),
            ObservedSchema = new SchemaInfo(),
            AssociatedTypes = new List<string>(),
            RelatedQueries = new List<QueryInfo>(),
            Relationships = new List<RelationshipInfo>(),
            HasDrift = false,
            DriftFlags = new List<string>(),
            DocumentCount = 0,
            LastSampled = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/Vendor.cs",
            LineNumber = 15,
            CommitSha = "abc123def456"
        };

        _mockCacheService.Setup(x => x.GetAsync<CollectionDetail>(It.IsAny<string>()))
            .ReturnsAsync((CollectionDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetCollectionAsync(collectionName))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<Ok<CollectionDetail>>(result);
        
        // Verify cache key is generated correctly (lowercase)
        _mockCacheService.Verify(x => x.GetAsync<CollectionDetail>("collection:vendors"), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync("collection:vendors", expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetCollectionAsync_SetsCorrectCacheTtl()
    {
        // Arrange
        var collectionName = "vendors";
        var expectedResponse = new CollectionDetail
        {
            Name = collectionName,
            Description = "Test collection",
            DeclaredSchema = new SchemaInfo(),
            ObservedSchema = new SchemaInfo(),
            AssociatedTypes = new List<string>(),
            RelatedQueries = new List<QueryInfo>(),
            Relationships = new List<RelationshipInfo>(),
            HasDrift = false,
            DriftFlags = new List<string>(),
            DocumentCount = 0,
            LastSampled = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/Vendor.cs",
            LineNumber = 15,
            CommitSha = "abc123def456"
        };

        _mockCacheService.Setup(x => x.GetAsync<CollectionDetail>(It.IsAny<string>()))
            .ReturnsAsync((CollectionDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetCollectionAsync(collectionName))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetCollectionAsync(collectionName);

        // Assert
        Assert.IsType<Ok<CollectionDetail>>(result);
        
        // Verify cache TTL is 30 minutes
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            expectedResponse, 
            TimeSpan.FromMinutes(30)), Times.Once);
    }
}
