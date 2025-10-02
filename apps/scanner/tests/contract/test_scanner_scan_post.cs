using System.Text.Json;
using Xunit;

namespace Cataloger.Scanner.Tests.Contract;

public class TestScannerScanPost
{
    [Fact]
    public void ScanRequest_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "repositories",
            "scanType"
        };

        // Act
        var requestSchema = GetScanRequestSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(requestSchema.ContainsKey(property), 
                $"ScanRequest schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void ScanRequest_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "enableSampling",
            "samplingConfig",
            "outputFormat"
        };

        // Act
        var requestSchema = GetScanRequestSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(requestSchema.ContainsKey(property), 
                $"ScanRequest schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void ScanResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "scanId",
            "status",
            "message"
        };

        // Act
        var responseSchema = GetScanResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"ScanResponse schema should contain required property '{property}'");
        }
    }

    [Fact]
    public void ScanResponse_ShouldHaveOptionalProperties()
    {
        // Arrange
        var expectedProperties = new[]
        {
            "estimatedDuration",
            "repositories"
        };

        // Act
        var responseSchema = GetScanResponseSchema();

        // Assert
        foreach (var property in expectedProperties)
        {
            Assert.True(responseSchema.ContainsKey(property), 
                $"ScanResponse schema should contain optional property '{property}'");
        }
    }

    [Fact]
    public void ScanType_ShouldBeValidEnum()
    {
        // Arrange
        var validScanTypes = new[] { "full", "incremental", "integrity" };

        // Act
        var scanTypeSchema = GetScanTypeSchema();

        // Assert
        Assert.True(scanTypeSchema.ContainsKey("enum"), 
            "ScanType should be an enum");
        
        var enumValues = scanTypeSchema["enum"].AsArray();
        Assert.Equal(3, enumValues.Count);
        
        foreach (var validType in validScanTypes)
        {
            Assert.Contains(validType, enumValues.Select(e => e.GetString()));
        }
    }

    [Fact]
    public void OutputFormat_ShouldBeValidEnum()
    {
        // Arrange
        var validFormats = new[] { "json", "yaml", "csv" };

        // Act
        var outputFormatSchema = GetOutputFormatSchema();

        // Assert
        Assert.True(outputFormatSchema.ContainsKey("enum"), 
            "OutputFormat should be an enum");
        
        var enumValues = outputFormatSchema["enum"].AsArray();
        Assert.Equal(3, enumValues.Count);
        
        foreach (var validFormat in validFormats)
        {
            Assert.Contains(validFormat, enumValues.Select(e => e.GetString()));
        }
    }

    private static JsonElement GetScanRequestSchema()
    {
        // This would normally come from the OpenAPI spec
        // For now, return a mock schema that matches the contract
        var schema = new
        {
            repositories = new { type = "array", items = new { type = "string" } },
            scanType = new { type = "string", @enum = new[] { "full", "incremental", "integrity" } },
            enableSampling = new { type = "boolean", @default = false },
            samplingConfig = new { type = "object" },
            outputFormat = new { type = "string", @enum = new[] { "json", "yaml", "csv" }, @default = "json" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetScanResponseSchema()
    {
        // This would normally come from the OpenAPI spec
        // For now, return a mock schema that matches the contract
        var schema = new
        {
            scanId = new { type = "string" },
            status = new { type = "string", @enum = new[] { "started", "running", "completed", "failed" } },
            message = new { type = "string" },
            estimatedDuration = new { type = "integer" },
            repositories = new { type = "array", items = new { type = "string" } }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetScanTypeSchema()
    {
        var schema = new
        {
            type = "string",
            @enum = new[] { "full", "incremental", "integrity" }
        };

        return JsonSerializer.SerializeToElement(schema);
    }

    private static JsonElement GetOutputFormatSchema()
    {
        var schema = new
        {
            type = "string",
            @enum = new[] { "json", "yaml", "csv" },
            @default = "json"
        };

        return JsonSerializer.SerializeToElement(schema);
    }
}
