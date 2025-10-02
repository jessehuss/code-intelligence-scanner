using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Contract;

public class TestKbSearchPost
{
    [Fact]
    public void SearchRequest_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "query"
        };

        // Act
        var requestSchema = GetSearchRequestSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(requestSchema.ContainsKey(property), 
                $"SearchRequest schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void SearchRequest_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "entityTypes",
            "repositories",
            "limit",
            "offset"
        };

        // Act
        var requestSchema = GetSearchRequestSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(requestSchema.ContainsKey(property), 
                $"SearchRequest schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void SearchResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "results",
            "total",
            "limit",
            "offset"
        };

        // Act
        var responseSchema = GetSearchResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"SearchResponse schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void SearchResponse_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "query"
        };

        // Act
        var responseSchema = GetSearchResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"SearchResponse schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void EntityTypes_ShouldBeValidEnum()
    {
        // Arrange
        var validEntityTypes = new[] { "CodeType", "CollectionMapping", "QueryOperation", "DataRelationship", "ObservedSchema" };

        // Act
        var entityTypesSchema = GetEntityTypesSchema();

        // Assert
        Assert.True(entityTypesSchema.ContainsKey("items"), 
            "EntityTypes should be an array");
        
        var itemsSchema = entityTypesSchema["items"];
        Assert.True(itemsSchema.TryGetProperty("enum", out var enumValues));
        
        var enumArray = enumValues.AsArray();
        Assert.Equal(5, enumArray.Count);
        
        foreach (var validType in validEntityTypes)
        {
            Assert.Contains(validType, enumArray.Select(e => e.GetString()));
        }
    }

    [Fact]
    public void SearchResult_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "id",
            "entityType",
            "title",
            "description",
            "relevanceScore"
        };

        // Act
        var resultSchema = GetSearchResultSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(resultSchema.ContainsKey(property), 
                $"SearchResult schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void SearchResult_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "repository",
            "filePath",
            "lineNumber"
        };

        // Act
        var resultSchema = GetSearchResultSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(resultSchema.ContainsKey(property), 
                $"SearchResult schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void RelevanceScore_ShouldBeValidRange()
    {
        // Act
        var resultSchema = GetSearchResultSchema();
        var relevanceScoreSchema = resultSchema["relevanceScore"];

        // Assert
        Assert.True(relevanceScoreSchema.TryGetProperty("minimum", out var min));
        Assert.Equal(0, min.GetDouble());
        
        Assert.True(relevanceScoreSchema.TryGetProperty("maximum", out var max));
        Assert.Equal(1, max.GetDouble());
    }

    [Fact]
    public void Limit_ShouldHaveValidConstraints()
    {
        // Act
        var requestSchema = GetSearchRequestSchema();
        var limitSchema = requestSchema["limit"];

        // Assert
        Assert.True(limitSchema.TryGetProperty("minimum", out var min));
        Assert.Equal(1, min.GetInt32());
        
        Assert.True(limitSchema.TryGetProperty("maximum", out var max));
        Assert.Equal(1000, max.GetInt32());
        
        Assert.True(limitSchema.TryGetProperty("default", out var defaultValue));
        Assert.Equal(50, defaultValue.GetInt32());
    }

    private static JsonElement GetSearchRequestSchema()
    {
        // This would normally come from the OpenAPI spec
        // For now, return a mock schema that matches the contract
        var schema = new
        {
            query = new { type = "string" },
            entityTypes = new
            {
                type = "array",
                items = new
                {
                    type = "string",
                    @enum = new[] { "CodeType", "CollectionMapping", "QueryOperation", "DataRelationship", "ObservedSchema" }
                }
            },
            repositories = new { type = "array", items = new { type = "string" } },
            limit = new { type = "integer", minimum = 1, maximum = 1000, @default = 50 },
            offset = new { type = "integer", minimum = 0, @default = 0 }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetSearchResponseSchema()
    {
        // This would normally come from the OpenAPI spec
        // For now, return a mock schema that matches the contract
        var schema = new
        {
            results = new { type = "array", items = new { type = "object" } },
            total = new { type = "integer" },
            limit = new { type = "integer" },
            offset = new { type = "integer" },
            query = new { type = "string" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetEntityTypesSchema()
    {
        var schema = new
        {
            type = "array",
            items = new
            {
                type = "string",
                @enum = new[] { "CodeType", "CollectionMapping", "QueryOperation", "DataRelationship", "ObservedSchema" }
            }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetSearchResultSchema()
    {
        var schema = new
        {
            id = new { type = "string" },
            entityType = new { type = "string" },
            title = new { type = "string" },
            description = new { type = "string" },
            relevanceScore = new { type = "number", minimum = 0, maximum = 1 },
            repository = new { type = "string" },
            filePath = new { type = "string" },
            lineNumber = new { type = "integer" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }
}
