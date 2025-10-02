using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using MongoDB.Driver;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace CatalogApi.Tests.Unit.Services;

/// <summary>
/// Unit tests for KnowledgeBaseService
/// </summary>
public class TestKnowledgeBaseService
{
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<KnowledgeBaseService>> _mockLogger;
    private readonly KnowledgeBaseService _service;

    public TestKnowledgeBaseService()
    {
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockClient = new Mock<IMongoClient>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<KnowledgeBaseService>>();

        _mockClient.Setup(x => x.GetDatabase(It.IsAny<string>()))
            .Returns(_mockDatabase.Object);

        _mockConfiguration.Setup(x => x["ConnectionStrings:KnowledgeBaseDatabaseName"])
            .Returns("catalog_kb");

        _service = new KnowledgeBaseService(_mockClient.Object, _mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SearchAsync_WithValidRequest_ReturnsSearchResults()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "User",
            Kinds = new List<string> { "type", "collection" },
            Limit = 10,
            Offset = 0
        };

        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("nodes", null))
            .Returns(mockCollection.Object);

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        var documents = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", "type:User" },
                { "EntityType", "type" },
                { "Name", "User" },
                { "Description", "User entity" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Models/User.cs" },
                { "LineNumber", 15 },
                { "CommitSha", "abc123" },
                { "LastModified", DateTime.UtcNow }
            }
        };

        mockCursor.Setup(x => x.Current).Returns(documents);
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Results);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(10, result.Limit);
        Assert.Equal(0, result.Offset);
        Assert.False(result.HasMore);

        var searchResult = result.Results.First();
        Assert.Equal("type:User", searchResult.Id);
        Assert.Equal("type", searchResult.EntityType);
        Assert.Equal("User", searchResult.Name);
    }

    [Fact]
    public async Task GetCollectionDetailAsync_WithValidName_ReturnsCollectionDetail()
    {
        // Arrange
        var collectionName = "users";
        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        var mockSchemasDeclared = new Mock<IMongoCollection<BsonDocument>>();
        var mockSchemasObserved = new Mock<IMongoCollection<BsonDocument>>();

        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("collections", null))
            .Returns(mockCollection.Object);
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("schemas_declared", null))
            .Returns(mockSchemasDeclared.Object);
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("schemas_observed", null))
            .Returns(mockSchemasObserved.Object);

        var collectionDoc = new BsonDocument
        {
            { "_id", collectionName },
            { "Name", collectionName },
            { "Description", "Users collection" },
            { "DeclaredSchemaId", "schema:declared:users" },
            { "ObservedSchemaId", "schema:observed:users" },
            { "Repository", "test-repo" },
            { "FilePath", "src/Collections.cs" },
            { "LineNumber", 20 },
            { "CommitSha", "abc123" },
            { "LastModified", DateTime.UtcNow }
        };

        var declaredSchemaDoc = new BsonDocument
        {
            { "_id", "schema:declared:users" },
            { "CollectionName", collectionName },
            { "Fields", new BsonArray
                {
                    new BsonDocument { { "Name", "Id" }, { "Type", "string" }, { "IsRequired", true } },
                    new BsonDocument { { "Name", "Name" }, { "Type", "string" }, { "IsRequired", true } }
                }
            },
            { "LastUpdated", DateTime.UtcNow }
        };

        var observedSchemaDoc = new BsonDocument
        {
            { "_id", "schema:observed:users" },
            { "CollectionName", collectionName },
            { "Fields", new BsonArray
                {
                    new BsonDocument { { "Name", "Id" }, { "Type", "string" }, { "IsRequired", true } },
                    new BsonDocument { { "Name", "Name" }, { "Type", "string" }, { "IsRequired", true } },
                    new BsonDocument { { "Name", "Email" }, { "Type", "string" }, { "IsRequired", false } }
                }
            },
            { "LastUpdated", DateTime.UtcNow }
        };

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument> { collectionDoc });
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var declaredSchemaCursor = new Mock<IAsyncCursor<BsonDocument>>();
        declaredSchemaCursor.Setup(x => x.Current).Returns(new List<BsonDocument> { declaredSchemaDoc });
        declaredSchemaCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockSchemasDeclared.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(declaredSchemaCursor.Object);

        var observedSchemaCursor = new Mock<IAsyncCursor<BsonDocument>>();
        observedSchemaCursor.Setup(x => x.Current).Returns(new List<BsonDocument> { observedSchemaDoc });
        observedSchemaCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockSchemasObserved.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(observedSchemaCursor.Object);

        // Act
        var result = await _service.GetCollectionDetailAsync(collectionName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.Name);
        Assert.Equal("Users collection", result.Description);
        Assert.NotNull(result.DeclaredSchema);
        Assert.NotNull(result.ObservedSchema);
        Assert.Equal(2, result.DeclaredSchema.Fields.Count);
        Assert.Equal(3, result.ObservedSchema.Fields.Count);
        Assert.True(result.HasDrift);
    }

    [Fact]
    public async Task GetTypeDetailAsync_WithValidFqcn_ReturnsTypeDetail()
    {
        // Arrange
        var fqcn = "MyApp.Models.User";
        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("types", null))
            .Returns(mockCollection.Object);

        var typeDoc = new BsonDocument
        {
            { "_id", fqcn },
            { "FullyQualifiedName", fqcn },
            { "Name", "User" },
            { "Namespace", "MyApp.Models" },
            { "Description", "User entity" },
            { "Fields", new BsonArray
                {
                    new BsonDocument
                    {
                        { "Name", "Id" },
                        { "Type", "string" },
                        { "IsRequired", true },
                        { "IsNullable", false },
                        { "Attributes", new BsonArray { "BsonId" } },
                        { "Description", "User identifier" }
                    },
                    new BsonDocument
                    {
                        { "Name", "Name" },
                        { "Type", "string" },
                        { "IsRequired", true },
                        { "IsNullable", false },
                        { "Attributes", new BsonArray() },
                        { "Description", "User name" }
                    }
                }
            },
            { "BsonAttributes", new BsonArray { "BsonId", "BsonElement" } },
            { "CollectionMappings", new BsonArray
                {
                    new BsonDocument
                    {
                        { "CollectionName", "users" },
                        { "MappingType", "Primary" },
                        { "Repository", "test-repo" },
                        { "FilePath", "src/Models/User.cs" },
                        { "LineNumber", 15 }
                    }
                }
            },
            { "UsageStats", new BsonDocument
                {
                    { "QueryCount", 5 },
                    { "RepositoryCount", 1 },
                    { "UsedInRepositories", new BsonArray { "test-repo" } },
                    { "LastUsed", DateTime.UtcNow },
                    { "CommonOperations", new BsonArray { "Find", "Insert" } }
                }
            },
            { "ChangeSummary", new BsonDocument
                {
                    { "TotalChanges", 3 },
                    { "AddedFields", 1 },
                    { "RemovedFields", 0 },
                    { "ModifiedFields", 2 },
                    { "LastChange", DateTime.UtcNow },
                    { "RecentCommits", new BsonArray { "abc123", "def456" } }
                }
            },
            { "Repository", "test-repo" },
            { "FilePath", "src/Models/User.cs" },
            { "LineNumber", 15 },
            { "CommitSha", "abc123" },
            { "LastModified", DateTime.UtcNow }
        };

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument> { typeDoc });
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetTypeDetailAsync(fqcn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fqcn, result.FullyQualifiedName);
        Assert.Equal("User", result.Name);
        Assert.Equal("MyApp.Models", result.Namespace);
        Assert.Equal("User entity", result.Description);
        Assert.Equal(2, result.Fields.Count);
        Assert.Equal(2, result.BsonAttributes.Count);
        Assert.Single(result.CollectionMappings);
        Assert.NotNull(result.UsageStats);
        Assert.NotNull(result.ChangeSummary);
        Assert.Equal(5, result.UsageStats.QueryCount);
        Assert.Equal(3, result.ChangeSummary.TotalChanges);
    }

    [Fact]
    public async Task GetGraphAsync_WithValidRequest_ReturnsGraphResponse()
    {
        // Arrange
        var request = new GraphRequest
        {
            Node = "collection:users",
            Depth = 2,
            EdgeKinds = new List<string> { "READS", "WRITES" },
            MaxNodes = 100
        };

        var mockNodesCollection = new Mock<IMongoCollection<BsonDocument>>();
        var mockEdgesCollection = new Mock<IMongoCollection<BsonDocument>>();

        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("nodes", null))
            .Returns(mockNodesCollection.Object);
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("edges", null))
            .Returns(mockEdgesCollection.Object);

        var nodeDocs = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", "collection:users" },
                { "EntityType", "collection" },
                { "Name", "users" },
                { "Description", "Users collection" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Collections.cs" },
                { "LineNumber", 20 },
                { "CommitSha", "abc123" }
            },
            new BsonDocument
            {
                { "_id", "type:User" },
                { "EntityType", "type" },
                { "Name", "User" },
                { "Description", "User entity" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Models/User.cs" },
                { "LineNumber", 15 },
                { "CommitSha", "abc123" }
            }
        };

        var edgeDocs = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", "edge1" },
                { "SourceNodeId", "type:User" },
                { "TargetNodeId", "collection:users" },
                { "EdgeType", "READS" },
                { "Description", "User reads from users collection" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Repositories/UserRepository.cs" },
                { "LineNumber", 25 },
                { "CommitSha", "abc123" },
                { "CreatedAt", DateTime.UtcNow }
            }
        };

        var nodesCursor = new Mock<IAsyncCursor<BsonDocument>>();
        nodesCursor.Setup(x => x.Current).Returns(nodeDocs);
        nodesCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockNodesCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodesCursor.Object);

        var edgesCursor = new Mock<IAsyncCursor<BsonDocument>>();
        edgesCursor.Setup(x => x.Current).Returns(edgeDocs);
        edgesCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        mockEdgesCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(edgesCursor.Object);

        // Act
        var result = await _service.GetGraphAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Nodes.Count);
        Assert.Single(result.Edges);
        Assert.True(result.QueryTime.TotalMilliseconds >= 0);
        Assert.Equal(2, result.TotalNodes);
        Assert.Equal(1, result.TotalEdges);

        var node = result.Nodes.First(n => n.Id == "collection:users");
        Assert.Equal("collection", node.EntityType);
        Assert.Equal("users", node.Name);

        var edge = result.Edges.First();
        Assert.Equal("type:User", edge.SourceNodeId);
        Assert.Equal("collection:users", edge.TargetNodeId);
        Assert.Equal("READS", edge.EdgeType);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithValidRequest_ReturnsTypeDiff()
    {
        // Arrange
        var request = new DiffRequest
        {
            FullyQualifiedName = "MyApp.Models.User",
            FromCommitSha = "abc123def456",
            ToCommitSha = "def456ghi789",
            IncludeFieldDetails = true,
            IncludeAttributeChanges = true
        };

        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("types", null))
            .Returns(mockCollection.Object);

        var fromTypeDoc = new BsonDocument
        {
            { "_id", "MyApp.Models.User@abc123def456" },
            { "FullyQualifiedName", "MyApp.Models.User" },
            { "CommitSha", "abc123def456" },
            { "Fields", new BsonArray
                {
                    new BsonDocument
                    {
                        { "Name", "Id" },
                        { "Type", "string" },
                        { "IsRequired", true },
                        { "IsNullable", false }
                    },
                    new BsonDocument
                    {
                        { "Name", "Name" },
                        { "Type", "string" },
                        { "IsRequired", true },
                        { "IsNullable", false }
                    }
                }
            },
            { "BsonAttributes", new BsonArray { "BsonId" } },
            { "LastModified", DateTime.UtcNow.AddDays(-1) }
        };

        var toTypeDoc = new BsonDocument
        {
            { "_id", "MyApp.Models.User@def456ghi789" },
            { "FullyQualifiedName", "MyApp.Models.User" },
            { "CommitSha", "def456ghi789" },
            { "Fields", new BsonArray
                {
                    new BsonDocument
                    {
                        { "Name", "Id" },
                        { "Type", "string" },
                        { "IsRequired", true },
                        { "IsNullable", false }
                    },
                    new BsonDocument
                    {
                        { "Name", "Name" },
                        { "Type", "string?" },
                        { "IsRequired", false },
                        { "IsNullable", true }
                    },
                    new BsonDocument
                    {
                        { "Name", "Email" },
                        { "Type", "string" },
                        { "IsRequired", false },
                        { "IsNullable", false }
                    }
                }
            },
            { "BsonAttributes", new BsonArray { "BsonId", "BsonElement" } },
            { "LastModified", DateTime.UtcNow }
        };

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument> { fromTypeDoc, toTypeDoc });
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(true)
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetTypeDiffAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyApp.Models.User", result.FullyQualifiedName);
        Assert.Equal("abc123def456", result.FromCommitSha);
        Assert.Equal("def456ghi789", result.ToCommitSha);
        Assert.Single(result.AddedFields);
        Assert.Empty(result.RemovedFields);
        Assert.Single(result.ModifiedFields);
        Assert.Single(result.AttributeChanges);

        var addedField = result.AddedFields.First();
        Assert.Equal("Email", addedField.FieldName);
        Assert.Equal("Added", addedField.ChangeType);

        var modifiedField = result.ModifiedFields.First();
        Assert.Equal("Name", modifiedField.FieldName);
        Assert.Equal("Modified", modifiedField.ChangeType);
        Assert.Equal("string", modifiedField.OldValue);
        Assert.Equal("string?", modifiedField.NewValue);

        var attributeChange = result.AttributeChanges.First();
        Assert.Equal("BsonElement", attributeChange.AttributeName);
        Assert.Equal("Added", attributeChange.ChangeType);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyResults_ReturnsEmptyResponse()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "NonExistent",
            Kinds = new List<string> { "type" },
            Limit = 10,
            Offset = 0
        };

        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("nodes", null))
            .Returns(mockCollection.Object);

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument>());
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.SearchAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Results);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task GetCollectionDetailAsync_WithNonExistentCollection_ReturnsNull()
    {
        // Arrange
        var collectionName = "non-existent";

        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("collections", null))
            .Returns(mockCollection.Object);

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument>());
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetCollectionDetailAsync(collectionName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTypeDetailAsync_WithNonExistentType_ReturnsNull()
    {
        // Arrange
        var fqcn = "NonExistent.Namespace.Type";

        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("types", null))
            .Returns(mockCollection.Object);

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument>());
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetTypeDetailAsync(fqcn);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetGraphAsync_WithNonExistentNode_ReturnsEmptyGraph()
    {
        // Arrange
        var request = new GraphRequest
        {
            Node = "collection:non-existent",
            Depth = 2,
            EdgeKinds = new List<string> { "READS" },
            MaxNodes = 100
        };

        var mockNodesCollection = new Mock<IMongoCollection<BsonDocument>>();
        var mockEdgesCollection = new Mock<IMongoCollection<BsonDocument>>();

        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("nodes", null))
            .Returns(mockNodesCollection.Object);
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("edges", null))
            .Returns(mockEdgesCollection.Object);

        var nodesCursor = new Mock<IAsyncCursor<BsonDocument>>();
        nodesCursor.Setup(x => x.Current).Returns(new List<BsonDocument>());
        nodesCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        mockNodesCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodesCursor.Object);

        var edgesCursor = new Mock<IAsyncCursor<BsonDocument>>();
        edgesCursor.Setup(x => x.Current).Returns(new List<BsonDocument>());
        edgesCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        mockEdgesCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(edgesCursor.Object);

        // Act
        var result = await _service.GetGraphAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Nodes);
        Assert.Empty(result.Edges);
        Assert.Equal(0, result.TotalNodes);
        Assert.Equal(0, result.TotalEdges);
    }

    [Fact]
    public async Task GetTypeDiffAsync_WithNonExistentType_ReturnsNull()
    {
        // Arrange
        var request = new DiffRequest
        {
            FullyQualifiedName = "NonExistent.Namespace.Type",
            FromCommitSha = "abc123def456",
            ToCommitSha = "def456ghi789"
        };

        var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
        _mockDatabase.Setup(x => x.GetCollection<BsonDocument>("types", null))
            .Returns(mockCollection.Object);

        var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
        mockCursor.Setup(x => x.Current).Returns(new List<BsonDocument>());
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _service.GetTypeDiffAsync(request);

        // Assert
        Assert.Null(result);
    }
}
