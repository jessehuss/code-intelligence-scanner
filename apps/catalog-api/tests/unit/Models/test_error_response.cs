using CatalogApi.Models;
using Xunit;

namespace CatalogApi.Tests.Unit.Models;

/// <summary>
/// Unit tests for ErrorResponse model
/// </summary>
public class TestErrorResponse
{
    [Fact]
    public void ErrorResponse_DefaultConstructor_InitializesProperties()
    {
        // Act
        var response = new ErrorResponse();

        // Assert
        Assert.Null(response.Type);
        Assert.Null(response.Title);
        Assert.Equal(0, response.Status);
        Assert.Null(response.Detail);
        Assert.Null(response.Instance);
        Assert.Null(response.ErrorCode);
        Assert.Null(response.Extensions);
        Assert.Equal(DateTime.MinValue, response.Timestamp);
        Assert.Null(response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithAllProperties_ReturnsCorrectValues()
    {
        // Arrange
        var extensions = new Dictionary<string, object>
        {
            { "field", "email" },
            { "value", "invalid@email" },
            { "constraint", "email_format" }
        };

        var timestamp = DateTime.UtcNow;

        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "The email field must be a valid email address",
            Instance = "/api/users",
            ErrorCode = "VALIDATION_001",
            Extensions = extensions,
            Timestamp = timestamp,
            TraceId = "abc123def456"
        };

        // Assert
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.1", response.Type);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal(400, response.Status);
        Assert.Equal("The email field must be a valid email address", response.Detail);
        Assert.Equal("/api/users", response.Instance);
        Assert.Equal("VALIDATION_001", response.ErrorCode);
        Assert.Equal(extensions, response.Extensions);
        Assert.Equal(timestamp, response.Timestamp);
        Assert.Equal("abc123def456", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithBadRequest_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "The request is invalid",
            Instance = "/api/search",
            ErrorCode = "BAD_REQUEST_001",
            Extensions = new Dictionary<string, object>
            {
                { "parameter", "q" },
                { "reason", "required" }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "bad123request456"
        };

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal("The request is invalid", response.Detail);
        Assert.Equal("BAD_REQUEST_001", response.ErrorCode);
        Assert.Equal("/api/search", response.Instance);
        Assert.Equal("bad123request456", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithUnauthorized_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = 401,
            Detail = "Authentication required",
            Instance = "/api/collections",
            ErrorCode = "AUTH_001",
            Extensions = new Dictionary<string, object>
            {
                { "scheme", "Bearer" },
                { "realm", "CatalogAPI" }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "unauth123456"
        };

        // Assert
        Assert.Equal(401, response.Status);
        Assert.Equal("Unauthorized", response.Title);
        Assert.Equal("Authentication required", response.Detail);
        Assert.Equal("AUTH_001", response.ErrorCode);
        Assert.Equal("/api/collections", response.Instance);
        Assert.Equal("unauth123456", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithForbidden_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden",
            Status = 403,
            Detail = "Access denied",
            Instance = "/api/admin",
            ErrorCode = "FORBIDDEN_001",
            Extensions = new Dictionary<string, object>
            {
                { "permission", "admin" },
                { "resource", "users" }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "forbidden123"
        };

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Equal("Forbidden", response.Title);
        Assert.Equal("Access denied", response.Detail);
        Assert.Equal("FORBIDDEN_001", response.ErrorCode);
        Assert.Equal("/api/admin", response.Instance);
        Assert.Equal("forbidden123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithNotFound_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = 404,
            Detail = "The requested resource was not found",
            Instance = "/api/types/NonExistentType",
            ErrorCode = "NOT_FOUND_001",
            Extensions = new Dictionary<string, object>
            {
                { "resource", "type" },
                { "identifier", "NonExistentType" }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "notfound123"
        };

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Equal("Not Found", response.Title);
        Assert.Equal("The requested resource was not found", response.Detail);
        Assert.Equal("NOT_FOUND_001", response.ErrorCode);
        Assert.Equal("/api/types/NonExistentType", response.Instance);
        Assert.Equal("notfound123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithInternalServerError_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = 500,
            Detail = "An unexpected error occurred",
            Instance = "/api/search",
            ErrorCode = "INTERNAL_001",
            Extensions = new Dictionary<string, object>
            {
                { "component", "database" },
                { "operation", "search" }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "internal123"
        };

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Equal("Internal Server Error", response.Title);
        Assert.Equal("An unexpected error occurred", response.Detail);
        Assert.Equal("INTERNAL_001", response.ErrorCode);
        Assert.Equal("/api/search", response.Instance);
        Assert.Equal("internal123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithServiceUnavailable_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.4",
            Title = "Service Unavailable",
            Status = 503,
            Detail = "Service temporarily unavailable",
            Instance = "/api/graph",
            ErrorCode = "SERVICE_UNAVAILABLE_001",
            Extensions = new Dictionary<string, object>
            {
                { "service", "graph" },
                { "retry_after", 30 }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "unavailable123"
        };

        // Assert
        Assert.Equal(503, response.Status);
        Assert.Equal("Service Unavailable", response.Title);
        Assert.Equal("Service temporarily unavailable", response.Detail);
        Assert.Equal("SERVICE_UNAVAILABLE_001", response.ErrorCode);
        Assert.Equal("/api/graph", response.Instance);
        Assert.Equal("unavailable123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithEmptyExtensions_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "Invalid request",
            Instance = "/api/test",
            ErrorCode = "TEST_001",
            Extensions = new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow,
            TraceId = "empty123"
        };

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal("Invalid request", response.Detail);
        Assert.Equal("TEST_001", response.ErrorCode);
        Assert.Equal("/api/test", response.Instance);
        Assert.NotNull(response.Extensions);
        Assert.Empty(response.Extensions);
        Assert.Equal("empty123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithNullExtensions_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "Invalid request",
            Instance = "/api/test",
            ErrorCode = "TEST_002",
            Extensions = null,
            Timestamp = DateTime.UtcNow,
            TraceId = "null123"
        };

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal("Invalid request", response.Detail);
        Assert.Equal("TEST_002", response.ErrorCode);
        Assert.Equal("/api/test", response.Instance);
        Assert.Null(response.Extensions);
        Assert.Equal("null123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithComplexExtensions_ReturnsCorrectValues()
    {
        // Arrange
        var extensions = new Dictionary<string, object>
        {
            { "validation_errors", new List<string> { "Email is required", "Name is too short" } },
            { "nested_object", new Dictionary<string, object>
                {
                    { "field", "user" },
                    { "value", "invalid" }
                }
            },
            { "numbers", new List<int> { 1, 2, 3 } },
            { "boolean_flag", true }
        };

        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "Multiple validation errors",
            Instance = "/api/validation",
            ErrorCode = "VALIDATION_002",
            Extensions = extensions,
            Timestamp = DateTime.UtcNow,
            TraceId = "complex123"
        };

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal("Multiple validation errors", response.Detail);
        Assert.Equal("VALIDATION_002", response.ErrorCode);
        Assert.Equal("/api/validation", response.Instance);
        Assert.Equal(extensions, response.Extensions);
        Assert.Equal("complex123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithSpecialCharacters_ReturnsCorrectValues()
    {
        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = "Special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?",
            Instance = "/api/special",
            ErrorCode = "SPECIAL_001",
            Extensions = new Dictionary<string, object>
            {
                { "special_chars", "!@#$%^&*()_+-=[]{}|;':\",./<>?" },
                { "unicode", "测试" },
                { "emoji", "🚀" }
            },
            Timestamp = DateTime.UtcNow,
            TraceId = "special123"
        };

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal("Special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?", response.Detail);
        Assert.Equal("SPECIAL_001", response.ErrorCode);
        Assert.Equal("/api/special", response.Instance);
        Assert.Equal("special123", response.TraceId);
    }

    [Fact]
    public void ErrorResponse_WithLongDetail_ReturnsCorrectValues()
    {
        // Arrange
        var longDetail = new string('A', 1000); // 1000 character string

        // Act
        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = 400,
            Detail = longDetail,
            Instance = "/api/long",
            ErrorCode = "LONG_001",
            Extensions = new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow,
            TraceId = "long123"
        };

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Equal("Bad Request", response.Title);
        Assert.Equal(longDetail, response.Detail);
        Assert.Equal("LONG_001", response.ErrorCode);
        Assert.Equal("/api/long", response.Instance);
        Assert.Equal("long123", response.TraceId);
    }
}
