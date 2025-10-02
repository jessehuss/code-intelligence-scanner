using CatalogApi.Models.DTOs;
using Xunit;

namespace CatalogApi.Tests.Unit.Models;

/// <summary>
/// Unit tests for SearchResult model
/// </summary>
public class TestSearchResult
{
    [Fact]
    public void SearchResult_DefaultConstructor_InitializesProperties()
    {
        // Act
        var result = new SearchResult();

        // Assert
        Assert.Null(result.Id);
        Assert.Null(result.EntityType);
        Assert.Null(result.Name);
        Assert.Null(result.Description);
        Assert.Equal(0.0, result.RelevanceScore);
        Assert.Null(result.Metadata);
        Assert.Null(result.Repository);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineNumber);
        Assert.Null(result.CommitSha);
        Assert.Equal(DateTime.MinValue, result.LastModified);
    }

    [Fact]
    public void SearchResult_WithAllProperties_ReturnsCorrectValues()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true }
        };

        var lastModified = DateTime.UtcNow;

        // Act
        var result = new SearchResult
        {
            Id = "type:User",
            EntityType = "type",
            Name = "User",
            Description = "User entity",
            RelevanceScore = 0.95,
            Metadata = metadata,
            Repository = "test-repo",
            FilePath = "src/Models/User.cs",
            LineNumber = 15,
            CommitSha = "abc123def456",
            LastModified = lastModified
        };

        // Assert
        Assert.Equal("type:User", result.Id);
        Assert.Equal("type", result.EntityType);
        Assert.Equal("User", result.Name);
        Assert.Equal("User entity", result.Description);
        Assert.Equal(0.95, result.RelevanceScore);
        Assert.Equal(metadata, result.Metadata);
        Assert.Equal("test-repo", result.Repository);
        Assert.Equal("src/Models/User.cs", result.FilePath);
        Assert.Equal(15, result.LineNumber);
        Assert.Equal("abc123def456", result.CommitSha);
        Assert.Equal(lastModified, result.LastModified);
    }

    [Fact]
    public void SearchResult_WithTypeEntity_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "type:MyApp.Models.User",
            EntityType = "type",
            Name = "User",
            Description = "User entity for authentication",
            RelevanceScore = 0.85,
            Metadata = new Dictionary<string, object>
            {
                { "Namespace", "MyApp.Models" },
                { "FieldCount", 5 }
            },
            Repository = "auth-service",
            FilePath = "src/Models/User.cs",
            LineNumber = 20,
            CommitSha = "def456ghi789",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("type:MyApp.Models.User", result.Id);
        Assert.Equal("type", result.EntityType);
        Assert.Equal("User", result.Name);
        Assert.Equal("User entity for authentication", result.Description);
        Assert.Equal(0.85, result.RelevanceScore);
        Assert.Equal("auth-service", result.Repository);
        Assert.Equal("src/Models/User.cs", result.FilePath);
        Assert.Equal(20, result.LineNumber);
        Assert.Equal("def456ghi789", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithCollectionEntity_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "collection:users",
            EntityType = "collection",
            Name = "users",
            Description = "Users collection",
            RelevanceScore = 0.90,
            Metadata = new Dictionary<string, object>
            {
                { "DocumentCount", 1000 },
                { "IndexCount", 3 }
            },
            Repository = "user-service",
            FilePath = "src/Collections/UserCollection.cs",
            LineNumber = 10,
            CommitSha = "ghi789jkl012",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("collection:users", result.Id);
        Assert.Equal("collection", result.EntityType);
        Assert.Equal("users", result.Name);
        Assert.Equal("Users collection", result.Description);
        Assert.Equal(0.90, result.RelevanceScore);
        Assert.Equal("user-service", result.Repository);
        Assert.Equal("src/Collections/UserCollection.cs", result.FilePath);
        Assert.Equal(10, result.LineNumber);
        Assert.Equal("ghi789jkl012", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithFieldEntity_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "field:User.Email",
            EntityType = "field",
            Name = "Email",
            Description = "User email address",
            RelevanceScore = 0.75,
            Metadata = new Dictionary<string, object>
            {
                { "Type", "string" },
                { "IsRequired", true },
                { "IsNullable", false }
            },
            Repository = "user-service",
            FilePath = "src/Models/User.cs",
            LineNumber = 25,
            CommitSha = "jkl012mno345",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("field:User.Email", result.Id);
        Assert.Equal("field", result.EntityType);
        Assert.Equal("Email", result.Name);
        Assert.Equal("User email address", result.Description);
        Assert.Equal(0.75, result.RelevanceScore);
        Assert.Equal("user-service", result.Repository);
        Assert.Equal("src/Models/User.cs", result.FilePath);
        Assert.Equal(25, result.LineNumber);
        Assert.Equal("jkl012mno345", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithQueryEntity_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "query:FindUserByEmail",
            EntityType = "query",
            Name = "FindUserByEmail",
            Description = "Find user by email address",
            RelevanceScore = 0.80,
            Metadata = new Dictionary<string, object>
            {
                { "Operation", "Find" },
                { "Collection", "users" },
                { "Filter", "{ \"Email\": \"{email}\" }" }
            },
            Repository = "user-service",
            FilePath = "src/Repositories/UserRepository.cs",
            LineNumber = 30,
            CommitSha = "mno345pqr678",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("query:FindUserByEmail", result.Id);
        Assert.Equal("query", result.EntityType);
        Assert.Equal("FindUserByEmail", result.Name);
        Assert.Equal("Find user by email address", result.Description);
        Assert.Equal(0.80, result.RelevanceScore);
        Assert.Equal("user-service", result.Repository);
        Assert.Equal("src/Repositories/UserRepository.cs", result.FilePath);
        Assert.Equal(30, result.LineNumber);
        Assert.Equal("mno345pqr678", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithServiceEntity_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "service:UserService",
            EntityType = "service",
            Name = "UserService",
            Description = "Service for user operations",
            RelevanceScore = 0.88,
            Metadata = new Dictionary<string, object>
            {
                { "Methods", 8 },
                { "Dependencies", 3 }
            },
            Repository = "user-service",
            FilePath = "src/Services/UserService.cs",
            LineNumber = 5,
            CommitSha = "pqr678stu901",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("service:UserService", result.Id);
        Assert.Equal("service", result.EntityType);
        Assert.Equal("UserService", result.Name);
        Assert.Equal("Service for user operations", result.Description);
        Assert.Equal(0.88, result.RelevanceScore);
        Assert.Equal("user-service", result.Repository);
        Assert.Equal("src/Services/UserService.cs", result.FilePath);
        Assert.Equal(5, result.LineNumber);
        Assert.Equal("pqr678stu901", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithEndpointEntity_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "endpoint:GET /api/users/{id}",
            EntityType = "endpoint",
            Name = "GET /api/users/{id}",
            Description = "Get user by ID",
            RelevanceScore = 0.92,
            Metadata = new Dictionary<string, object>
            {
                { "Method", "GET" },
                { "Path", "/api/users/{id}" },
                { "ResponseType", "User" }
            },
            Repository = "user-api",
            FilePath = "src/Controllers/UserController.cs",
            LineNumber = 40,
            CommitSha = "stu901vwx234",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("endpoint:GET /api/users/{id}", result.Id);
        Assert.Equal("endpoint", result.EntityType);
        Assert.Equal("GET /api/users/{id}", result.Name);
        Assert.Equal("Get user by ID", result.Description);
        Assert.Equal(0.92, result.RelevanceScore);
        Assert.Equal("user-api", result.Repository);
        Assert.Equal("src/Controllers/UserController.cs", result.FilePath);
        Assert.Equal(40, result.LineNumber);
        Assert.Equal("stu901vwx234", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithEmptyMetadata_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "type:SimpleType",
            EntityType = "type",
            Name = "SimpleType",
            Description = "Simple type without metadata",
            RelevanceScore = 0.60,
            Metadata = new Dictionary<string, object>(),
            Repository = "simple-service",
            FilePath = "src/Models/SimpleType.cs",
            LineNumber = 1,
            CommitSha = "vwx234yza567",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("type:SimpleType", result.Id);
        Assert.Equal("type", result.EntityType);
        Assert.Equal("SimpleType", result.Name);
        Assert.Equal("Simple type without metadata", result.Description);
        Assert.Equal(0.60, result.RelevanceScore);
        Assert.NotNull(result.Metadata);
        Assert.Empty(result.Metadata);
        Assert.Equal("simple-service", result.Repository);
        Assert.Equal("src/Models/SimpleType.cs", result.FilePath);
        Assert.Equal(1, result.LineNumber);
        Assert.Equal("vwx234yza567", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithNullMetadata_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "type:NullMetadataType",
            EntityType = "type",
            Name = "NullMetadataType",
            Description = "Type with null metadata",
            RelevanceScore = 0.50,
            Metadata = null,
            Repository = "null-service",
            FilePath = "src/Models/NullMetadataType.cs",
            LineNumber = 50,
            CommitSha = "yza567bcd890",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("type:NullMetadataType", result.Id);
        Assert.Equal("type", result.EntityType);
        Assert.Equal("NullMetadataType", result.Name);
        Assert.Equal("Type with null metadata", result.Description);
        Assert.Equal(0.50, result.RelevanceScore);
        Assert.Null(result.Metadata);
        Assert.Equal("null-service", result.Repository);
        Assert.Equal("src/Models/NullMetadataType.cs", result.FilePath);
        Assert.Equal(50, result.LineNumber);
        Assert.Equal("yza567bcd890", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithZeroRelevanceScore_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "type:ZeroRelevanceType",
            EntityType = "type",
            Name = "ZeroRelevanceType",
            Description = "Type with zero relevance score",
            RelevanceScore = 0.0,
            Metadata = new Dictionary<string, object>
            {
                { "Score", 0.0 }
            },
            Repository = "zero-service",
            FilePath = "src/Models/ZeroRelevanceType.cs",
            LineNumber = 100,
            CommitSha = "bcd890efg123",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("type:ZeroRelevanceType", result.Id);
        Assert.Equal("type", result.EntityType);
        Assert.Equal("ZeroRelevanceType", result.Name);
        Assert.Equal("Type with zero relevance score", result.Description);
        Assert.Equal(0.0, result.RelevanceScore);
        Assert.Equal("zero-service", result.Repository);
        Assert.Equal("src/Models/ZeroRelevanceType.cs", result.FilePath);
        Assert.Equal(100, result.LineNumber);
        Assert.Equal("bcd890efg123", result.CommitSha);
    }

    [Fact]
    public void SearchResult_WithNegativeLineNumber_ReturnsCorrectValues()
    {
        // Act
        var result = new SearchResult
        {
            Id = "type:NegativeLineType",
            EntityType = "type",
            Name = "NegativeLineType",
            Description = "Type with negative line number",
            RelevanceScore = 0.70,
            Metadata = new Dictionary<string, object>(),
            Repository = "negative-service",
            FilePath = "src/Models/NegativeLineType.cs",
            LineNumber = -1,
            CommitSha = "efg123hij456",
            LastModified = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("type:NegativeLineType", result.Id);
        Assert.Equal("type", result.EntityType);
        Assert.Equal("NegativeLineType", result.Name);
        Assert.Equal("Type with negative line number", result.Description);
        Assert.Equal(0.70, result.RelevanceScore);
        Assert.Equal("negative-service", result.Repository);
        Assert.Equal("src/Models/NegativeLineType.cs", result.FilePath);
        Assert.Equal(-1, result.LineNumber);
        Assert.Equal("efg123hij456", result.CommitSha);
    }
}
