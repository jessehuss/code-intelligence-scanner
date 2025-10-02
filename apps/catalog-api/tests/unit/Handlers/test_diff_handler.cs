using CatalogApi.Handlers;
using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for DiffHandler
/// </summary>
public class TestDiffHandler
{
    private readonly Mock<IKnowledgeBaseService> _mockKnowledgeBaseService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IObservabilityService> _mockObservabilityService;
    private readonly DiffHandler _handler;

    public TestDiffHandler()
    {
        _mockKnowledgeBaseService = new Mock<IKnowledgeBaseService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockObservabilityService = new Mock<IObservabilityService>();
        
        _handler = new DiffHandler(
            _mockKnowledgeBaseService.Object,
            _mockCacheService.Object,
            _mockObservabilityService.Object);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        
        var expectedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>
            {
                new FieldChange
                {
                    FieldName = "Email",
                    FieldType = "string",
                    ChangeType = "Added",
                    OldValue = null,
                    NewValue = "string",
                    Description = "Added Email field"
                }
            },
            RemovedFields = new List<FieldChange>
            {
                new FieldChange
                {
                    FieldName = "OldField",
                    FieldType = "string",
                    ChangeType = "Removed",
                    OldValue = "string",
                    NewValue = null,
                    Description = "Removed OldField"
                }
            },
            ModifiedFields = new List<FieldChange>
            {
                new FieldChange
                {
                    FieldName = "Name",
                    FieldType = "string",
                    ChangeType = "Modified",
                    OldValue = "string",
                    NewValue = "string?",
                    Description = "Made Name field nullable"
                }
            },
            AttributeChanges = new List<AttributeChange>
            {
                new AttributeChange
                {
                    AttributeName = "BsonId",
                    ChangeType = "Added",
                    OldValue = null,
                    NewValue = "BsonId",
                    Description = "Added BsonId attribute"
                }
            },
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        var okResult = (Ok<TypeDiff>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetTypeDiffAsync(It.Is<DiffRequest>(r => 
            r.FullyQualifiedName == fqcn && r.FromCommitSha == fromSha && r.ToCommitSha == toSha)), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithCachedResult_ReturnsCachedResult()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        
        var cachedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>(),
            RemovedFields = new List<FieldChange>(),
            ModifiedFields = new List<FieldChange>(),
            AttributeChanges = new List<AttributeChange>(),
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        var okResult = (Ok<TypeDiff>)result;
        Assert.Equal(cachedResponse, okResult.Value);
        
        _mockKnowledgeBaseService.Verify(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithEmptyFqcn_ReturnsBadRequest()
    {
        // Arrange
        var fqcn = "";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Fully qualified class name is required", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithEmptyFromSha_ReturnsBadRequest()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "";
        var toSha = "def456ghi789";

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Both 'fromSha' and 'toSha' parameters are required", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithEmptyToSha_ReturnsBadRequest()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "";

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
        Assert.Equal("Bad Request", badRequestResult.Value?.Title);
        Assert.Equal("Both 'fromSha' and 'toSha' parameters are required", badRequestResult.Value?.Detail);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithWhitespaceParameters_ReturnsBadRequest()
    {
        // Arrange
        var fqcn = "   ";
        var fromSha = "   ";
        var toSha = "   ";

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<BadRequest<ErrorResponse>>(result);
        var badRequestResult = (BadRequest<ErrorResponse>)result;
        Assert.Equal(400, badRequestResult.Value?.Status);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithNonExistentType_ReturnsNotFound()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync((TypeDiff?)null);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<NotFound<ErrorResponse>>(result);
        var notFoundResult = (NotFound<ErrorResponse>)result;
        Assert.Equal(404, notFoundResult.Value?.Status);
        Assert.Equal("Not Found", notFoundResult.Value?.Title);
        Assert.Contains("not found", notFoundResult.Value?.Detail);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        
        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<StatusCodeHttpResult>(result);
        var statusResult = (StatusCodeHttpResult)result;
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetTypeDiffAsync_GeneratesCorrectCacheKey()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        
        var expectedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>(),
            RemovedFields = new List<FieldChange>(),
            ModifiedFields = new List<FieldChange>(),
            AttributeChanges = new List<AttributeChange>(),
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        
        // Verify cache key is generated correctly (lowercase)
        _mockCacheService.Verify(x => x.GetAsync<TypeDiff>("diff:myapp.models.user:abc123def456:def456ghi789"), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync("diff:myapp.models.user:abc123def456:def456ghi789", expectedResponse, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetTypeDiffAsync_SetsCorrectCacheTtl()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        
        var expectedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>(),
            RemovedFields = new List<FieldChange>(),
            ModifiedFields = new List<FieldChange>(),
            AttributeChanges = new List<AttributeChange>(),
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        
        // Verify cache TTL is 24 hours (1440 minutes)
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            expectedResponse, 
            TimeSpan.FromHours(24)), Times.Once);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithComplexNamespace_HandlesCorrectly()
    {
        // Arrange
        var fqcn = "MyApp.Data.Entities.Complex.UserProfile";
        var fromSha = "abc123def456";
        var toSha = "def456ghi789";
        
        var expectedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>
            {
                new FieldChange
                {
                    FieldName = "ProfileImage",
                    FieldType = "byte[]",
                    ChangeType = "Added",
                    OldValue = null,
                    NewValue = "byte[]",
                    Description = "Added ProfileImage field"
                }
            },
            RemovedFields = new List<FieldChange>(),
            ModifiedFields = new List<FieldChange>(),
            AttributeChanges = new List<AttributeChange>(),
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/UserProfile.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        var okResult = (Ok<TypeDiff>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Single(okResult.Value.AddedFields);
        Assert.Equal("ProfileImage", okResult.Value.AddedFields.First().FieldName);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithIdenticalShas_ReturnsEmptyDiff()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456";
        var toSha = "abc123def456"; // Same SHA
        
        var expectedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>(),
            RemovedFields = new List<FieldChange>(),
            ModifiedFields = new List<FieldChange>(),
            AttributeChanges = new List<AttributeChange>(),
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        var okResult = (Ok<TypeDiff>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Empty(okResult.Value.AddedFields);
        Assert.Empty(okResult.Value.RemovedFields);
        Assert.Empty(okResult.Value.ModifiedFields);
        Assert.Empty(okResult.Value.AttributeChanges);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithSpecialCharactersInShas_HandlesCorrectly()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var fromSha = "abc123def456-ghi789";
        var toSha = "def456_ghi789-jkl012";
        
        var expectedResponse = new TypeDiff
        {
            FullyQualifiedName = fqcn,
            FromCommitSha = fromSha,
            ToCommitSha = toSha,
            AddedFields = new List<FieldChange>(),
            RemovedFields = new List<FieldChange>(),
            ModifiedFields = new List<FieldChange>(),
            AttributeChanges = new List<AttributeChange>(),
            DiffGeneratedAt = DateTime.UtcNow,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs"
        };

        _mockCacheService.Setup(x => x.GetAsync<TypeDiff>(It.IsAny<string>()))
            .ReturnsAsync((TypeDiff?)null);

        _mockKnowledgeBaseService.Setup(x => x.GetTypeDiffAsync(It.IsAny<DiffRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.GetTypeDiffAsync(fqcn, fromSha, toSha);

        // Assert
        Assert.IsType<Ok<TypeDiff>>(result);
        var okResult = (Ok<TypeDiff>)result;
        Assert.Equal(expectedResponse, okResult.Value);
        
        // Verify cache key handles special characters correctly
        _mockCacheService.Verify(x => x.GetAsync<TypeDiff>("diff:myapp.models.user:abc123def456-ghi789:def456_ghi789-jkl012"), Times.Once);
    }
}
