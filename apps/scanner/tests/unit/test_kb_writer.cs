using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.KnowledgeBase;
using Cataloger.Scanner.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Cataloger.Scanner.Tests.Unit;

public class KnowledgeBaseWriterTests
{
    private readonly Mock<ILogger<KnowledgeBaseWriter>> _mockLogger;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly KnowledgeBaseWriter _writer;

    public KnowledgeBaseWriterTests()
    {
        _mockLogger = new Mock<ILogger<KnowledgeBaseWriter>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _writer = new KnowledgeBaseWriter(_mockLogger.Object, _mockDatabase.Object);
    }

    [Fact]
    public async Task WriteCodeType_WithValidCodeType_ShouldWriteToDatabase()
    {
        // Arrange
        var codeType = new CodeType
        {
            Id = "test-type-id",
            Name = "User",
            Namespace = "MyApp.Models",
            Fields = new List<FieldDefinition>
            {
                new FieldDefinition { Name = "Id", Type = "string" },
                new FieldDefinition { Name = "Name", Type = "string" }
            },
            Provenance = new ProvenanceRecord
            {
                Repository = "test-repo",
                FilePath = "Models/User.cs",
                Symbol = "User",
                LineSpan = new LineSpan { Start = 1, End = 10 }
            }
        };

        var mockCollection = new Mock<IMongoCollection<CodeType>>();
        _mockDatabase.Setup(x => x.GetCollection<CodeType>("code_types", null))
            .Returns(mockCollection.Object);

        // Act
        await _writer.WriteCodeType(codeType);

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<CodeType>>(),
            codeType,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteCollectionMapping_WithValidMapping_ShouldWriteToDatabase()
    {
        // Arrange
        var mapping = new CollectionMapping
        {
            Id = "test-mapping-id",
            TypeId = "test-type-id",
            CollectionName = "users",
            ResolutionMethod = "literal",
            Confidence = 1.0,
            Provenance = new ProvenanceRecord
            {
                Repository = "test-repo",
                FilePath = "Services/UserService.cs",
                Symbol = "GetCollection",
                LineSpan = new LineSpan { Start = 5, End = 5 }
            }
        };

        var mockCollection = new Mock<IMongoCollection<CollectionMapping>>();
        _mockDatabase.Setup(x => x.GetCollection<CollectionMapping>("collection_mappings", null))
            .Returns(mockCollection.Object);

        // Act
        await _writer.WriteCollectionMapping(mapping);

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<CollectionMapping>>(),
            mapping,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteQueryOperation_WithValidOperation_ShouldWriteToDatabase()
    {
        // Arrange
        var operation = new QueryOperation
        {
            Id = "test-operation-id",
            OperationType = "Find",
            CollectionId = "test-mapping-id",
            Filters = new { UserId = "user-id-value" },
            Provenance = new ProvenanceRecord
            {
                Repository = "test-repo",
                FilePath = "Services/UserService.cs",
                Symbol = "GetUserById",
                LineSpan = new LineSpan { Start = 10, End = 15 }
            }
        };

        var mockCollection = new Mock<IMongoCollection<QueryOperation>>();
        _mockDatabase.Setup(x => x.GetCollection<QueryOperation>("query_operations", null))
            .Returns(mockCollection.Object);

        // Act
        await _writer.WriteQueryOperation(operation);

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<QueryOperation>>(),
            operation,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteDataRelationship_WithValidRelationship_ShouldWriteToDatabase()
    {
        // Arrange
        var relationship = new DataRelationship
        {
            Id = "test-relationship-id",
            SourceTypeId = "order-type-id",
            TargetTypeId = "user-type-id",
            RelationshipType = "REFERS_TO",
            Confidence = 0.8,
            Evidence = "UserId field in Order class",
            Provenance = new ProvenanceRecord
            {
                Repository = "test-repo",
                FilePath = "Models/Order.cs",
                Symbol = "Order",
                LineSpan = new LineSpan { Start = 1, End = 20 }
            }
        };

        var mockCollection = new Mock<IMongoCollection<DataRelationship>>();
        _mockDatabase.Setup(x => x.GetCollection<DataRelationship>("data_relationships", null))
            .Returns(mockCollection.Object);

        // Act
        await _writer.WriteDataRelationship(relationship);

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<DataRelationship>>(),
            relationship,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteObservedSchema_WithValidSchema_ShouldWriteToDatabase()
    {
        // Arrange
        var schema = new ObservedSchema
        {
            Id = "test-schema-id",
            CollectionId = "test-mapping-id",
            Schema = new BsonDocument
            {
                { "type", "object" },
                { "properties", new BsonDocument
                    {
                        { "name", new BsonDocument { { "type", "string" } } },
                        { "age", new BsonDocument { { "type", "number" } } }
                    }
                }
            },
            SampleSize = 100,
            PIIRedacted = true,
            Provenance = new ProvenanceRecord
            {
                Repository = "test-repo",
                FilePath = "Sampling/UserSampler.cs",
                Symbol = "SampleUsers",
                LineSpan = new LineSpan { Start = 1, End = 50 }
            }
        };

        var mockCollection = new Mock<IMongoCollection<ObservedSchema>>();
        _mockDatabase.Setup(x => x.GetCollection<ObservedSchema>("observed_schemas", null))
            .Returns(mockCollection.Object);

        // Act
        await _writer.WriteObservedSchema(schema);

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<ObservedSchema>>(),
            schema,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteKnowledgeBaseEntry_WithValidEntry_ShouldWriteToDatabase()
    {
        // Arrange
        var entry = new KnowledgeBaseEntry
        {
            Id = "test-entry-id",
            EntityType = "CodeType",
            EntityId = "test-type-id",
            SearchableText = "User class in MyApp.Models namespace",
            Tags = new List<string> { "POCO", "User", "Model" },
            Relationships = new List<string> { "order-type-id", "product-type-id" },
            Provenance = new ProvenanceRecord
            {
                Repository = "test-repo",
                FilePath = "Models/User.cs",
                Symbol = "User",
                LineSpan = new LineSpan { Start = 1, End = 10 }
            }
        };

        var mockCollection = new Mock<IMongoCollection<KnowledgeBaseEntry>>();
        _mockDatabase.Setup(x => x.GetCollection<KnowledgeBaseEntry>("knowledge_base_entries", null))
            .Returns(mockCollection.Object);

        // Act
        await _writer.WriteKnowledgeBaseEntry(entry);

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<KnowledgeBaseEntry>>(),
            entry,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteBatch_WithValidEntities_ShouldWriteAllToDatabase()
    {
        // Arrange
        var codeType = new CodeType
        {
            Id = "test-type-id",
            Name = "User",
            Namespace = "MyApp.Models"
        };

        var mapping = new CollectionMapping
        {
            Id = "test-mapping-id",
            TypeId = "test-type-id",
            CollectionName = "users",
            ResolutionMethod = "literal",
            Confidence = 1.0
        };

        var entities = new List<object> { codeType, mapping };

        var mockCodeTypeCollection = new Mock<IMongoCollection<CodeType>>();
        var mockMappingCollection = new Mock<IMongoCollection<CollectionMapping>>();

        _mockDatabase.Setup(x => x.GetCollection<CodeType>("code_types", null))
            .Returns(mockCodeTypeCollection.Object);
        _mockDatabase.Setup(x => x.GetCollection<CollectionMapping>("collection_mappings", null))
            .Returns(mockMappingCollection.Object);

        // Act
        await _writer.WriteBatch(entities);

        // Assert
        mockCodeTypeCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<CodeType>>(),
            codeType,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        mockMappingCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<CollectionMapping>>(),
            mapping,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteBatch_WithEmptyList_ShouldNotWriteAnything()
    {
        // Arrange
        var entities = new List<object>();

        // Act
        await _writer.WriteBatch(entities);

        // Assert
        _mockDatabase.Verify(x => x.GetCollection<It.IsAnyType>(It.IsAny<string>(), null), Times.Never);
    }

    [Fact]
    public async Task WriteBatch_WithNullList_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteBatch(null));
    }

    [Fact]
    public async Task WriteCodeType_WithNullCodeType_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteCodeType(null));
    }

    [Fact]
    public async Task WriteCollectionMapping_WithNullMapping_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteCollectionMapping(null));
    }

    [Fact]
    public async Task WriteQueryOperation_WithNullOperation_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteQueryOperation(null));
    }

    [Fact]
    public async Task WriteDataRelationship_WithNullRelationship_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteDataRelationship(null));
    }

    [Fact]
    public async Task WriteObservedSchema_WithNullSchema_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteObservedSchema(null));
    }

    [Fact]
    public async Task WriteKnowledgeBaseEntry_WithNullEntry_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _writer.WriteKnowledgeBaseEntry(null));
    }

    [Fact]
    public async Task WriteCodeType_WithDatabaseError_ShouldLogError()
    {
        // Arrange
        var codeType = new CodeType
        {
            Id = "test-type-id",
            Name = "User",
            Namespace = "MyApp.Models"
        };

        var mockCollection = new Mock<IMongoCollection<CodeType>>();
        mockCollection.Setup(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<CodeType>>(),
            codeType,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database error"));

        _mockDatabase.Setup(x => x.GetCollection<CodeType>("code_types", null))
            .Returns(mockCollection.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MongoException>(() => _writer.WriteCodeType(codeType));
    }

    [Fact]
    public async Task WriteBatch_WithMixedEntityTypes_ShouldWriteToCorrectCollections()
    {
        // Arrange
        var codeType = new CodeType
        {
            Id = "test-type-id",
            Name = "User",
            Namespace = "MyApp.Models"
        };

        var operation = new QueryOperation
        {
            Id = "test-operation-id",
            OperationType = "Find",
            CollectionId = "test-mapping-id"
        };

        var entry = new KnowledgeBaseEntry
        {
            Id = "test-entry-id",
            EntityType = "CodeType",
            EntityId = "test-type-id",
            SearchableText = "User class"
        };

        var entities = new List<object> { codeType, operation, entry };

        var mockCodeTypeCollection = new Mock<IMongoCollection<CodeType>>();
        var mockOperationCollection = new Mock<IMongoCollection<QueryOperation>>();
        var mockEntryCollection = new Mock<IMongoCollection<KnowledgeBaseEntry>>();

        _mockDatabase.Setup(x => x.GetCollection<CodeType>("code_types", null))
            .Returns(mockCodeTypeCollection.Object);
        _mockDatabase.Setup(x => x.GetCollection<QueryOperation>("query_operations", null))
            .Returns(mockOperationCollection.Object);
        _mockDatabase.Setup(x => x.GetCollection<KnowledgeBaseEntry>("knowledge_base_entries", null))
            .Returns(mockEntryCollection.Object);

        // Act
        await _writer.WriteBatch(entities);

        // Assert
        mockCodeTypeCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<CodeType>>(),
            codeType,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        mockOperationCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<QueryOperation>>(),
            operation,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        mockEntryCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<FilterDefinition<KnowledgeBaseEntry>>(),
            entry,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
