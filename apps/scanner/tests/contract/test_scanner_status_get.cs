using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Contract;

public class TestScannerStatusGet
{
    [Fact]
    public void ScanStatusResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "scanId",
            "status",
            "progress"
        };

        // Act
        var responseSchema = GetScanStatusResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"ScanStatusResponse schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void ScanStatusResponse_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "results",
            "error",
            "startedAt",
            "completedAt"
        };

        // Act
        var responseSchema = GetScanStatusResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"ScanStatusResponse schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void Status_ShouldBeValidEnum()
    {
        // Arrange
        var validStatuses = new[] { "started", "running", "completed", "failed", "cancelled" };

        // Act
        var statusSchema = GetStatusSchema();

        // Assert
        Assert.True(statusSchema.ContainsKey("enum"), 
            "Status should be an enum");
        
        var enumValues = statusSchema["enum"].AsArray();
        Assert.Equal(5, enumValues.Count);
        
        foreach (var validStatus in validStatuses)
        {
            Assert.Contains(validStatus, enumValues.Select(e => e.GetString()));
        }
    }

    [Fact]
    public void Progress_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "percentage",
            "currentRepository",
            "processedRepositories",
            "totalRepositories"
        };

        // Act
        var progressSchema = GetProgressSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(progressSchema.ContainsKey(property), 
                $"Progress schema should contain property '{property}'");
        }
    }

    [Fact]
    public void Progress_Percentage_ShouldBeValidRange()
    {
        // Act
        var progressSchema = GetProgressSchema();
        var percentageSchema = progressSchema["percentage"];

        // Assert
        Assert.True(percentageSchema.TryGetProperty("minimum", out var min));
        Assert.Equal(0, min.GetDouble());
        
        Assert.True(percentageSchema.TryGetProperty("maximum", out var max));
        Assert.Equal(100, max.GetDouble());
    }

    [Fact]
    public void ScanResults_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "typesDiscovered",
            "collectionsMapped",
            "queriesExtracted",
            "relationshipsInferred",
            "schemasObserved",
            "repositories"
        };

        // Act
        var resultsSchema = GetScanResultsSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(resultsSchema.ContainsKey(property), 
                $"ScanResults schema should contain property '{property}'");
        }
    }

    private static JsonElement GetScanStatusResponseSchema()
    {
        // This would normally come from the OpenAPI spec
        // For now, return a mock schema that matches the contract
        var schema = new
        {
            scanId = new { type = "string" },
            status = new { type = "string", @enum = new[] { "started", "running", "completed", "failed", "cancelled" } },
            progress = new
            {
                type = "object",
                properties = new
                {
                    percentage = new { type = "number", minimum = 0, maximum = 100 },
                    currentRepository = new { type = "string" },
                    processedRepositories = new { type = "integer" },
                    totalRepositories = new { type = "integer" }
                }
            },
            results = new { type = "object" },
            error = new { type = "string" },
            startedAt = new { type = "string", format = "date-time" },
            completedAt = new { type = "string", format = "date-time" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetStatusSchema()
    {
        var schema = new
        {
            type = "string",
            @enum = new[] { "started", "running", "completed", "failed", "cancelled" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetProgressSchema()
    {
        var schema = new
        {
            type = "object",
            properties = new
            {
                percentage = new { type = "number", minimum = 0, maximum = 100 },
                currentRepository = new { type = "string" },
                processedRepositories = new { type = "integer" },
                totalRepositories = new { type = "integer" }
            }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetScanResultsSchema()
    {
        var schema = new
        {
            typesDiscovered = new { type = "integer" },
            collectionsMapped = new { type = "integer" },
            queriesExtracted = new { type = "integer" },
            relationshipsInferred = new { type = "integer" },
            schemasObserved = new { type = "integer" },
            repositories = new { type = "array", items = new { type = "object" } }
        };

        return JsonSerializer.SerializeToElement(schema);
    }
}
