using CatalogApi.Handlers;
using CatalogApi.Models.DTOs;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for TypesHandler
/// </summary>
public class TestTypesHandler
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IObservabilityService> _mockObservabilityService;
    private readonly TypesHandler _handler;

    public TestTypesHandler()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockObservabilityService = new Mock<IObservabilityService>();
        
        _handler = new TypesHandler(
            _mockKnowledgeBaseService.Object,
            _mockCacheService.Object,
            _mockObservabilityService.Object);
    }

    [Fact]
    public async Task GetTypeAsync_WithValidFqcn_ReturnsOkResult()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var expectedResponse = new TypeDetail
        {
            FullyQualifiedName = fqcn,
            Name = "User",
            Namespace = "MyApp.Models",
            Description = "User entity",
            Fields = new List<FieldDetail>(),
            BsonAttributes = new List<string> { "BsonId" },
            CollectionMappings = new List<CollectionMapping>(),
            UsageStats = new UsageStatistics(),
            ChangeSummary = new ChangeSummary(),
            Repository = "test-repo",
            FilePath = "src/Models/User.cs",
            LineNumber = 15,
            CommitSha = "abc123def456",
            LastModified = DateTime.UtcNow
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync((TypeDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeAsync(fqcn))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<Ok<TypeDetail>>(result);
        var okResult = (Ok<TypeDetail>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetTypeAsync(fqcn), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetTypeAsync_WithCachedResult_ReturnsCachedResult()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var cachedResponse = new TypeDetail
        {
            FullyQualifiedName = fqcn,
            Name = "User",
            Namespace = "MyApp.Models",
            Description = "Cached user entity",
            Fields = new List<FieldDetail>(),
            BsonAttributes = new List<string>(),
            CollectionMappings = new List<CollectionMapping>(),
            UsageStats = new UsageStatistics(),
            ChangeSummary = new ChangeSummary(),
            Repository = "test-repo",
            FilePath = "src/Models/User.cs",
            LineNumber = 15,
            CommitSha = "abc123def456",
            LastModified = DateTime.UtcNow
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<Ok<TypeDetail>>(result);
        var okResult = (Ok<TypeDetail>)result;
        Assert.Equal(cachedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetTypeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetTypeAsync_WithEmptyFqcn_ReturnsBadRequest()
    {
        // Arrange
        var fqcn = "";

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Fully qualified class name is required", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetTypeAsync_WithWhitespaceFqcn_ReturnsBadRequest()
    {
        // Arrange
        var fqcn = "   ";

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task GetTypeAsync_WithNonExistentType_ReturnsNotFound()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";

        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync((TypeDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeAsync(fqcn))
            .ReturnsAsync((TypeDetail?)null);

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<NotFound<ErrorResponse>>(result);
        var notFoundResult = (NotFound<ErrorResponse>)result;
        Assert.Equal(404, notFoundResult.Value?.Status);
        Assert.Equal("Not Found", notFoundResult.Value?.Title);
        Assert.Contains("not found", notFoundResult.Value?.Detail);
    }

    [Fact]
    public async Task GetTypeAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        
        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync((TypeDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeAsync(fqcn))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<StatusCodeHttpResult>(result);
        var statusResult = (StatusCodeHttpResult)result;
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetTypeAsync_GeneratesCorrectCacheKey()
    {
        // Arrange
        var fqcn = "MyApp.Models.User"; // Test case sensitivity
        var expectedResponse = new TypeDetail
        {
            FullyQualifiedName = fqcn,
            Name = "User",
            Namespace = "MyApp.Models",
            Description = "Test type",
            Fields = new List<FieldDetail>(),
            BsonAttributes = new List<string>(),
            CollectionMappings = new List<CollectionMapping>(),
            UsageStats = new UsageStatistics(),
            ChangeSummary = new ChangeSummary(),
            Repository = "test-repo",
            FilePath = "src/Models/User.cs",
            LineNumber = 15,
            CommitSha = "abc123def456",
            LastModified = DateTime.UtcNow
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync((TypeDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeAsync(fqcn))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<Ok<TypeDetail>>(result);
        
        // Verify cache key is generated correctly (lowercase)
        _mockCacheService.Verify(x => x.GetAsync<TypeDetail>("type:myapp.models.user"), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync("type:myapp.models.user", expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetTypeAsync_SetsCorrectCacheTtl()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var expectedResponse = new TypeDetail
        {
            FullyQualifiedName = fqcn,
            Name = "User",
            Namespace = "MyApp.Models",
            Description = "Test type",
            Fields = new List<FieldDetail>(),
            BsonAttributes = new List<string>(),
            CollectionMappings = new List<CollectionMapping>(),
            UsageStats = new UsageStatistics(),
            ChangeSummary = new ChangeSummary(),
            Repository = "test-repo",
            FilePath = "src/Models/User.cs",
            LineNumber = 15,
            CommitSha = "abc123def456",
            LastModified = DateTime.UtcNow
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync((TypeDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeAsync(fqcn))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<Ok<TypeDetail>>(result);
        
        // Verify cache TTL is 1 hour
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            expectedResponse, 
            TimeSpan.FromHours(1)), Times.Once);
    }

    [Fact]
    public async Task GetTypeAsync_WithComplexNamespace_HandlesCorrectly()
    {
        // Arrange
        var fqcn = "MyApp.Data.Entities.Complex.UserProfile";
        var expectedResponse = new TypeDetail
        {
            FullyQualifiedName = fqcn,
            Name = "UserProfile",
            Namespace = "MyApp.Data.Entities.Complex",
            Description = "Complex user profile",
            Fields = new List<FieldDetail>(),
            BsonAttributes = new List<string>(),
            CollectionMappings = new List<CollectionMapping>(),
            UsageStats = new UsageStatistics(),
            ChangeSummary = new ChangeSummary(),
            Repository = "test-repo",
            FilePath = "src/Models/UserProfile.cs",
            LineNumber = 20,
            CommitSha = "abc123def456",
            LastModified = DateTime.UtcNow
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDetail>(It.IsAny<string>()))
            .ReturnsAsync((TypeDetail?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeAsync(fqcn))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeAsync(fqcn);

        // Assert
        Assert.IsType<Ok<TypeDetail>>(result);
        var okResult = (Ok<TypeDetail>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Equal("UserProfile", okResult.Value.Name);
        Assert.Equal("MyApp.Data.Entities.Complex", okResult.Value.Namespace);
    }
}
