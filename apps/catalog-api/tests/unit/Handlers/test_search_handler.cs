using CatalogApi.Handlers;
using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for SearchHandler
/// </summary>
public class TestSearchHandler
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IObservabilityService> _mockObservabilityService;
    private readonly SearchHandler _handler;

    public TestSearchHandler()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockObservabilityService = new Mock<IObservabilityService>();
        
        _handler = new SearchHandler(
            _mockKnowledgeBaseService.Object,
            _mockCacheService.Object,
            _mockObservabilityService.Object);
    }

    [Fact]
    public async Task SearchAsync_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var query = "test query";
        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResult>
            {
                new() { Id = "1", Name = "Test Result", EntityType = "type" }
            },
            TotalCount = 1,
            Limit = 50,
            Offset = 0,
            HasMore = false,
            ResultCountsByType = new Dictionary<string, int> { ["type"] = 1 },
            QueryTime = TimeSpan.FromMilliseconds(100)
        };

        _mockCacheService.Setup(x => x.GetAsync<SearchResponse>(It.IsAny<string>()))
            .ReturnsAsync((SearchResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.SearchAsync(It.IsAny<SearchRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.SearchAsync(query);

        // Assert
        Assert.IsType<Ok<SearchResponse>>(result);
        var okResult = (Ok<SearchResponse>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.SearchAsync(It.IsAny<SearchRequest>()), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithCachedResult_ReturnsCachedResult()
    {
        // Arrange
        var query = "test query";
        var cachedResponse = new SearchResponse
        {
            Results = new List<SearchResult>(),
            TotalCount = 0,
            Limit = 50,
            Offset = 0,
            HasMore = false,
            ResultCountsByType = new Dictionary<string, int>(),
            QueryTime = TimeSpan.Zero
        };

        _mockCacheService.Setup(x => x.GetAsync<SearchResponse>(It.IsAny<string>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.SearchAsync(query);

        // Assert
        Assert.IsType<Ok<SearchResponse>>(result);
        var okResult = (Ok<SearchResponse>)result;
        Assert.Equal(cachedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.SearchAsync(It.IsAny<SearchRequest>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidQuery_ReturnsBadRequest()
    {
        // Arrange
        var query = ""; // Empty query should be invalid

        // Act
        var result = await _handler.SearchAsync(query);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Validation Error", badRequestResult.Value?.Title);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidLimit_ReturnsBadRequest()
    {
        // Arrange
        var query = "test";
        var limit = 1001; // Exceeds maximum limit

        // Act
        var result = await _handler.SearchAsync(query, limit: limit);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidOffset_ReturnsBadRequest()
    {
        // Arrange
        var query = "test";
        var offset = -1; // Negative offset

        // Act
        var result = await _handler.SearchAsync(query, offset: offset);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidSortBy_ReturnsBadRequest()
    {
        // Arrange
        var query = "test";
        var sortBy = "invalid"; // Invalid sort field

        // Act
        var result = await _handler.SearchAsync(query, sortBy: sortBy);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidSortOrder_ReturnsBadRequest()
    {
        // Arrange
        var query = "test";
        var sortOrder = "invalid"; // Invalid sort order

        // Act
        var result = await _handler.SearchAsync(query, sortOrder: sortOrder);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidFilters_ReturnsBadRequest()
    {
        // Arrange
        var query = "test";
        var filters = "invalid json"; // Invalid JSON

        // Act
        var result = await _handler.SearchAsync(query, filters: filters);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Invalid filters parameter format", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task SearchAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var query = "test";
        
        _mockCacheService.Setup(x => x.GetAsync<SearchResponse>(It.IsAny<string>()))
            .ReturnsAsync((SearchResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.SearchAsync(It.IsAny<SearchRequest>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _handler.SearchAsync(query);

        // Assert
        Assert.IsType<StatusCodeHttpResult>(result);
        var statusResult = (StatusCodeHttpResult)result;
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task SearchAsync_WithValidFilters_ProcessesFiltersCorrectly()
    {
        // Arrange
        var query = "test";
        var filters = "{\"repository\": \"test-repo\"}";
        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResult>(),
            TotalCount = 0,
            Limit = 50,
            Offset = 0,
            HasMore = false,
            ResultCountsByType = new Dictionary<string, int>(),
            QueryTime = TimeSpan.Zero
        };

        _mockCacheService.Setup(x => x.GetAsync<SearchResponse>(It.IsAny<string>()))
            .ReturnsAsync((SearchResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.SearchAsync(It.IsAny<SearchRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.SearchAsync(query, filters: filters);

        // Assert
        Assert.IsType<Ok<SearchResponse>>(result);
        _mockKnowledgeBaseService.Verify(x => x.SearchAsync(It.Is<SearchRequest>(r => 
            r.Filters.ContainsKey("repository") && 
            r.Filters["repository"].ToString() == "test-repo")), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithKindsParameter_ParsesKindsCorrectly()
    {
        // Arrange
        var query = "test";
        var kinds = "type,collection,field";
        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResult>(),
            TotalCount = 0,
            Limit = 50,
            Offset = 0,
            HasMore = false,
            ResultCountsByType = new Dictionary<string, int>(),
            QueryTime = TimeSpan.Zero
        };

        _mockCacheService.Setup(x => x.GetAsync<SearchResponse>(It.IsAny<string>()))
            .ReturnsAsync((SearchResponse?)null);

        _mockKnowledgeBaseService.Setup(x => x.SearchAsync(It.IsAny<SearchRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.SearchAsync(query, kinds: kinds);

        // Assert
        Assert.IsType<Ok<SearchResponse>>(result);
        _mockKnowledgeBaseService.Verify(x => x.SearchAsync(It.Is<SearchRequest>(r => 
            r.Kinds.Count == 3 && 
            r.Kinds.Contains("type") && 
            r.Kinds.Contains("collection") && 
            r.Kinds.Contains("field")), Times.Once);
    }
}
