using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Analyzers;
using Cataloger.Scanner.Models;

namespace Cataloger.Scanner.Tests.Unit;

public class RelationshipInferencerTests
{
    private readonly Mock<ILogger<RelationshipInferencer>> _mockLogger;
    private readonly RelationshipInferencer _inferencer;

    public RelationshipInferencerTests()
    {
        _mockLogger = new Mock<ILogger<RelationshipInferencer>>();
        _inferencer = new RelationshipInferencer(_mockLogger.Object);
    }

    [Fact]
    public void InferRelationships_WithForeignKeyReference_ShouldReturnDataRelationship()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "UserId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Find",
                CollectionId = "orders",
                Filters = new { UserId = "user-id-value" }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.Equal("order-type-id", relationship.SourceTypeId);
        Assert.Equal("user-type-id", relationship.TargetTypeId);
        Assert.Equal("REFERS_TO", relationship.RelationshipType);
        Assert.True(relationship.Confidence > 0.7);
        Assert.Contains("UserId", relationship.Evidence);
    }

    [Fact]
    public void InferRelationships_WithLookupOperation_ShouldReturnDataRelationship()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "UserId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Aggregate",
                CollectionId = "orders",
                AggregationPipeline = new List<object>
                {
                    new { $lookup = new { from = "users", localField = "UserId", foreignField = "_id", as = "user" } }
                }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.Equal("order-type-id", relationship.SourceTypeId);
        Assert.Equal("user-type-id", relationship.TargetTypeId);
        Assert.Equal("LOOKUP", relationship.RelationshipType);
        Assert.True(relationship.Confidence > 0.8);
        Assert.Contains("$lookup", relationship.Evidence);
    }

    [Fact]
    public void InferRelationships_WithEmbeddedDocument_ShouldReturnDataRelationship()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Customer", Type = "User" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>();

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.Equal("order-type-id", relationship.SourceTypeId);
        Assert.Equal("user-type-id", relationship.TargetTypeId);
        Assert.Equal("EMBEDDED", relationship.RelationshipType);
        Assert.True(relationship.Confidence > 0.9);
        Assert.Contains("Customer", relationship.Evidence);
    }

    [Fact]
    public void InferRelationships_WithCollectionReference_ShouldReturnDataRelationship()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "UserId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Find",
                CollectionId = "orders",
                Filters = new { UserId = "user-id-value" }
            },
            new QueryOperation
            {
                Id = "query-op-id-2",
                OperationType = "Find",
                CollectionId = "users",
                Filters = new { Id = "user-id-value" }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.Equal("order-type-id", relationship.SourceTypeId);
        Assert.Equal("user-type-id", relationship.TargetTypeId);
        Assert.Equal("REFERS_TO", relationship.RelationshipType);
        Assert.True(relationship.Confidence > 0.7);
        Assert.Contains("UserId", relationship.Evidence);
    }

    [Fact]
    public void InferRelationships_WithMultipleRelationships_ShouldReturnAllRelationships()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "UserId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            },
            new CodeType
            {
                Id = "product-type-id",
                Name = "Product",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" },
                    new FieldDefinition { Name = "Price", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Find",
                CollectionId = "orders",
                Filters = new { UserId = "user-id-value" }
            },
            new QueryOperation
            {
                Id = "query-op-id-2",
                OperationType = "Find",
                CollectionId = "orders",
                Filters = new { ProductId = "product-id-value" }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var userRelationship = result.First(r => r.TargetTypeId == "user-type-id");
        Assert.Equal("order-type-id", userRelationship.SourceTypeId);
        Assert.Equal("user-type-id", userRelationship.TargetTypeId);
        Assert.Equal("REFERS_TO", userRelationship.RelationshipType);
        
        var productRelationship = result.First(r => r.TargetTypeId == "product-type-id");
        Assert.Equal("order-type-id", productRelationship.SourceTypeId);
        Assert.Equal("product-type-id", productRelationship.TargetTypeId);
        Assert.Equal("REFERS_TO", productRelationship.RelationshipType);
    }

    [Fact]
    public void InferRelationships_WithNoRelationships_ShouldReturnEmptyList()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>();

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void InferRelationships_WithNullInputs_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _inferencer.InferRelationships(null, new List<QueryOperation>()));
        Assert.Throws<ArgumentNullException>(() => _inferencer.InferRelationships(new List<CodeType>(), null));
    }

    [Fact]
    public void InferRelationships_WithEmptyInputs_ShouldReturnEmptyList()
    {
        // Act
        var result = _inferencer.InferRelationships(new List<CodeType>(), new List<QueryOperation>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void InferRelationships_WithHighConfidenceRelationship_ShouldReturnHighConfidence()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "UserId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Find",
                CollectionId = "orders",
                Filters = new { UserId = "user-id-value" }
            },
            new QueryOperation
            {
                Id = "query-op-id-2",
                OperationType = "UpdateOne",
                CollectionId = "orders",
                Filters = new { UserId = "user-id-value" }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.True(relationship.Confidence > 0.8);
    }

    [Fact]
    public void InferRelationships_WithLowConfidenceRelationship_ShouldReturnLowConfidence()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "CustomerId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Find",
                CollectionId = "orders",
                Filters = new { CustomerId = "user-id-value" }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.True(relationship.Confidence < 0.7);
    }

    [Fact]
    public void InferRelationships_WithComplexEvidence_ShouldReturnRelationshipWithComplexEvidence()
    {
        // Arrange
        var codeTypes = new List<CodeType>
        {
            new CodeType
            {
                Id = "user-type-id",
                Name = "User",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "Name", Type = "string" }
                }
            },
            new CodeType
            {
                Id = "order-type-id",
                Name = "Order",
                Namespace = "MyApp.Models",
                Fields = new List<FieldDefinition>
                {
                    new FieldDefinition { Name = "Id", Type = "string" },
                    new FieldDefinition { Name = "UserId", Type = "string" },
                    new FieldDefinition { Name = "Total", Type = "decimal" }
                }
            }
        };

        var queryOperations = new List<QueryOperation>
        {
            new QueryOperation
            {
                Id = "query-op-id",
                OperationType = "Aggregate",
                CollectionId = "orders",
                AggregationPipeline = new List<object>
                {
                    new { $match = new { UserId = "user-id-value" } },
                    new { $lookup = new { from = "users", localField = "UserId", foreignField = "_id", as = "user" } }
                }
            }
        };

        // Act
        var result = _inferencer.InferRelationships(codeTypes, queryOperations);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var relationship = result.First();
        Assert.Contains("UserId", relationship.Evidence);
        Assert.Contains("$lookup", relationship.Evidence);
        Assert.True(relationship.Confidence > 0.8);
    }
}
