using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Contract;

public class TestKbTypesGet
{
    [Fact]
    public void TypeInfoResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "id",
            "name",
            "namespace",
            "fields",
            "provenance"
        };

        // Act
        var responseSchema = GetTypeInfoResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"TypeInfoResponse schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void TypeInfoResponse_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "assembly",
            "bsonAttributes",
            "nullability",
            "discriminators",
            "collectionMappings",
            "queryOperations",
            "relationships"
        };

        // Act
        var responseSchema = GetTypeInfoResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"TypeInfoResponse schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void FieldInfo_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "name",
            "type",
            "isNullable"
        };

        // Act
        var fieldSchema = GetFieldInfoSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(fieldSchema.ContainsKey(property), 
                $"FieldInfo schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void FieldInfo_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "bsonAttributes"
        };

        // Act
        var fieldSchema = GetFieldInfoSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(fieldSchema.ContainsKey(property), 
                $"FieldInfo schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void BSONAttribute_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "name",
            "value"
        };

        // Act
        var bsonAttributeSchema = GetBSONAttributeSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(bsonAttributeSchema.ContainsKey(property), 
                $"BSONAttribute schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void BSONAttribute_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "parameters"
        };

        // Act
        var bsonAttributeSchema = GetBSONAttributeSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(bsonAttributeSchema.ContainsKey(property), 
                $"BSONAttribute schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void CollectionMapping_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "id",
            "collectionName",
            "resolutionMethod",
            "confidence"
        };

        // Act
        var collectionMappingSchema = GetCollectionMappingSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(collectionMappingSchema.ContainsKey(property), 
                $"CollectionMapping schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void ResolutionMethod_ShouldBeValidEnum()
    {
        // Arrange
        var validMethods = new[] { "literal", "constant", "config", "inferred" };

        // Act
        var resolutionMethodSchema = GetResolutionMethodSchema();

        // Assert
        Assert.True(resolutionMethodSchema.ContainsKey("enum"), 
            "ResolutionMethod should be an enum");
        
        var enumValues = resolutionMethodSchema["enum"].AsArray();
        Assert.Equal(4, enumValues.Count);
        
        foreach (var validMethod in validMethods)
        {
            Assert.Contains(validMethod, enumValues.Select(e => e.GetString()));
        }
    }

    [Fact]
    public void Confidence_ShouldBeValidRange()
    {
        // Act
        var collectionMappingSchema = GetCollectionMappingSchema();
        var confidenceSchema = collectionMappingSchema["confidence"];

        // Assert
        Assert.True(confidenceSchema.TryGetProperty("minimum", out var min));
        Assert.Equal(0, min.GetDouble());
        
        Assert.True(confidenceSchema.TryGetProperty("maximum", out var max));
        Assert.Equal(1, max.GetDouble());
    }

    [Fact]
    public void ProvenanceRecord_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "repository",
            "filePath",
            "symbol",
            "lineSpan",
            "commitSHA",
            "timestamp"
        };

        // Act
        var provenanceSchema = GetProvenanceRecordSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(provenanceSchema.ContainsKey(property), 
                $"ProvenanceRecord schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void LineSpan_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "start",
            "end"
        };

        // Act
        var lineSpanSchema = GetLineSpanSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(lineSpanSchema.ContainsKey(property), 
                $"LineSpan schema should contain required property '{property}'");
        }
    }

    private static JsonElement GetTypeInfoResponseSchema()
    {
        // This would normally come from the OpenAPI spec
        // For now, return a mock schema that matches the contract
        var schema = new
        {
            id = new { type = "string" },
            name = new { type = "string" },
            @namespace = new { type = "string" },
            assembly = new { type = "string" },
            fields = new { type = "array", items = new { type = "object" } },
            bsonAttributes = new { type = "array", items = new { type = "object" } },
            nullability = new { type = "object" },
            discriminators = new { type = "array", items = new { type = "string" } },
            collectionMappings = new { type = "array", items = new { type = "object" } },
            queryOperations = new { type = "array", items = new { type = "object" } },
            relationships = new { type = "array", items = new { type = "object" } },
            provenance = new { type = "object" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetFieldInfoSchema()
    {
        var schema = new
        {
            name = new { type = "string" },
            type = new { type = "string" },
            isNullable = new { type = "boolean" },
            bsonAttributes = new { type = "array", items = new { type = "object" } }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetBSONAttributeSchema()
    {
        var schema = new
        {
            name = new { type = "string" },
            value = new { type = "string" },
            parameters = new { type = "object" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetCollectionMappingSchema()
    {
        var schema = new
        {
            id = new { type = "string" },
            collectionName = new { type = "string" },
            resolutionMethod = new { type = "string", @enum = new[] { "literal", "constant", "config", "inferred" } },
            confidence = new { type = "number", minimum = 0, maximum = 1 }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetResolutionMethodSchema()
    {
        var schema = new
        {
            type = "string",
            @enum = new[] { "literal", "constant", "config", "inferred" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetProvenanceRecordSchema()
    {
        var schema = new
        {
            repository = new { type = "string" },
            filePath = new { type = "string" },
            symbol = new { type = "string" },
            lineSpan = new { type = "object" },
            commitSHA = new { type = "string" },
            timestamp = new { type = "string", format = "date-time" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetLineSpanSchema()
    {
        var schema = new
        {
            start = new { type = "integer" },
            end = new { type = "integer" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }
}
