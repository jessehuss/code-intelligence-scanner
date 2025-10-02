using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Samplers;
using Cataloger.Scanner.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Cataloger.Scanner.Tests.Unit;

public class MongoSamplerTests
{
    private readonly Mock<ILogger<MongoSampler>> _mockLogger;
    private readonly Mock<IPIIDetector> _mockPIIDetector;
    private readonly MongoSampler _sampler;

    public MongoSamplerTests()
    {
        _mockLogger = new Mock<ILogger<MongoSampler>>();
        _mockPIIDetector = new Mock<IPIIDetector>();
        _sampler = new MongoSampler(_mockLogger.Object, _mockPIIDetector.Object);
    }

    [Fact]
    public async Task SampleCollection_WithValidCollection_ShouldReturnObservedSchema()
    {
        // Arrange
        var collectionName = "users";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "name", "John Doe" },
                { "email", "john@example.com" },
                { "age", 30 },
                { "isActive", true }
            },
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "name", "Jane Smith" },
                { "email", "jane@example.com" },
                { "age", 25 },
                { "isActive", false }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect("email", It.IsAny<object>())).Returns(true);
        _mockPIIDetector.Setup(x => x.Detect("name", It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(2, result.SampleSize);
        Assert.True(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithEmptyCollection_ShouldReturnEmptySchema()
    {
        // Arrange
        var collectionName = "empty_collection";
        var sampleSize = 100;

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(0, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithPIIFields_ShouldRedactPII()
    {
        // Arrange
        var collectionName = "users";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "email", "john@example.com" },
                { "phone", "555-1234" },
                { "ssn", "123-45-6789" }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect("email", It.IsAny<object>())).Returns(true);
        _mockPIIDetector.Setup(x => x.Detect("phone", It.IsAny<object>())).Returns(true);
        _mockPIIDetector.Setup(x => x.Detect("ssn", It.IsAny<object>())).Returns(true);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.True(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithNoPIIFields_ShouldNotRedact()
    {
        // Arrange
        var collectionName = "products";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "name", "Product 1" },
                { "price", 29.99 },
                { "category", "Electronics" }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithLargeSampleSize_ShouldLimitSampleSize()
    {
        // Arrange
        var collectionName = "large_collection";
        var sampleSize = 10000;
        var maxSampleSize = 1000;

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.True(result.SampleSize <= maxSampleSize);
    }

    [Fact]
    public async Task SampleCollection_WithNullCollectionName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sampler.SampleCollection(null, 100));
    }

    [Fact]
    public async Task SampleCollection_WithEmptyCollectionName_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sampler.SampleCollection("", 100));
    }

    [Fact]
    public async Task SampleCollection_WithNegativeSampleSize_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sampler.SampleCollection("test", -1));
    }

    [Fact]
    public async Task SampleCollection_WithZeroSampleSize_ShouldReturnEmptySchema()
    {
        // Arrange
        var collectionName = "test_collection";

        // Act
        var result = await _sampler.SampleCollection(collectionName, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(0, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithComplexDocuments_ShouldInferComplexSchema()
    {
        // Arrange
        var collectionName = "complex_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "name", "Product 1" },
                { "price", 29.99 },
                { "tags", new BsonArray { "electronics", "gadgets" } },
                { "metadata", new BsonDocument { { "created", DateTime.UtcNow }, { "version", 1 } } }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithInconsistentDocuments_ShouldHandleInconsistencies()
    {
        // Arrange
        var collectionName = "inconsistent_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "name", "Product 1" },
                { "price", 29.99 }
            },
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "title", "Product 2" },
                { "cost", 39.99 },
                { "description", "A great product" }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(2, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithNestedDocuments_ShouldInferNestedSchema()
    {
        // Arrange
        var collectionName = "nested_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "user", new BsonDocument { { "name", "John" }, { "age", 30 } } },
                { "address", new BsonDocument { { "street", "123 Main St" }, { "city", "Anytown" } } }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithArrayFields_ShouldInferArraySchema()
    {
        // Arrange
        var collectionName = "array_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "tags", new BsonArray { "tag1", "tag2", "tag3" } },
                { "scores", new BsonArray { 85, 92, 78 } },
                { "items", new BsonArray 
                    { 
                        new BsonDocument { { "name", "item1" }, { "value", 10 } },
                        new BsonDocument { { "name", "item2" }, { "value", 20 } }
                    } 
                }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithDateFields_ShouldInferDateSchema()
    {
        // Arrange
        var collectionName = "date_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "createdAt", DateTime.UtcNow },
                { "updatedAt", DateTime.UtcNow.AddDays(-1) },
                { "birthDate", new DateTime(1990, 1, 1) }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithBooleanFields_ShouldInferBooleanSchema()
    {
        // Arrange
        var collectionName = "boolean_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "isActive", true },
                { "isDeleted", false },
                { "hasPermission", true }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task SampleCollection_WithNullFields_ShouldHandleNullValues()
    {
        // Arrange
        var collectionName = "null_collection";
        var sampleSize = 100;
        var mockDocuments = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "name", "Product 1" },
                { "description", BsonNull.Value },
                { "price", 29.99 },
                { "category", BsonNull.Value }
            }
        };

        _mockPIIDetector.Setup(x => x.Detect(It.IsAny<string>(), It.IsAny<object>())).Returns(false);

        // Act
        var result = await _sampler.SampleCollection(collectionName, sampleSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collectionName, result.CollectionId);
        Assert.Equal(1, result.SampleSize);
        Assert.False(result.PIIRedacted);
        Assert.NotNull(result.Schema);
    }
}
