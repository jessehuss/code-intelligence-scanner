using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cataloger.Scanner.Services;

/// <summary>
/// Centralized error handling service for the Code Intelligence Scanner.
/// </summary>
public class ErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;
    private readonly bool _enableDetailedErrors;

    public ErrorHandler(ILogger<ErrorHandler> logger, bool enableDetailedErrors = false)
    {
        _logger = logger;
        _enableDetailedErrors = enableDetailedErrors;
    }

    /// <summary>
    /// Handles and logs errors with appropriate context.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="context">Additional context information.</param>
    /// <param name="operation">Operation being performed when error occurred.</param>
    /// <returns>Error response with appropriate details.</returns>
    public ErrorResponse HandleError(Exception error, Dictionary<string, object>? context = null, string operation = "Unknown")
    {
        var errorResponse = new ErrorResponse
        {
            ErrorId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            ErrorType = error.GetType().Name,
            Message = GetUserFriendlyMessage(error),
            Details = _enableDetailedErrors ? error.ToString() : null
        };

        // Log the error with appropriate level
        LogError(error, context, operation, errorResponse.ErrorId);

        // Add context to error response if available
        if (context != null)
        {
            errorResponse.Context = context;
        }

        return errorResponse;
    }

    /// <summary>
    /// Handles validation errors.
    /// </summary>
    /// <param name="validationErrors">List of validation errors.</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for validation errors.</returns>
    public ErrorResponse HandleValidationError(List<string> validationErrors, string operation = "Validation")
    {
        var errorResponse = new ErrorResponse
        {
            ErrorId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            ErrorType = "ValidationError",
            Message = "Validation failed",
            Details = string.Join("; ", validationErrors)
        };

        _logger.LogWarning("Validation error in {Operation}: {ValidationErrors}",
            operation, string.Join("; ", validationErrors));

        return errorResponse;
    }

    /// <summary>
    /// Handles MongoDB connection errors.
    /// </summary>
    /// <param name="error">MongoDB error.</param>
    /// <param name="connectionString">Connection string (redacted).</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for MongoDB errors.</returns>
    public ErrorResponse HandleMongoError(Exception error, string connectionString, string operation = "MongoDB")
    {
        var redactedConnectionString = RedactConnectionString(connectionString);
        
        var context = new Dictionary<string, object>
        {
            ["ConnectionString"] = redactedConnectionString,
            ["Operation"] = operation
        };

        var errorResponse = HandleError(error, context, operation);
        errorResponse.ErrorType = "MongoError";
        errorResponse.Message = "Database operation failed";

        return errorResponse;
    }

    /// <summary>
    /// Handles Roslyn analysis errors.
    /// </summary>
    /// <param name="error">Roslyn error.</param>
    /// <param name="filePath">File being analyzed.</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for Roslyn errors.</returns>
    public ErrorResponse HandleRoslynError(Exception error, string filePath, string operation = "RoslynAnalysis")
    {
        var context = new Dictionary<string, object>
        {
            ["FilePath"] = filePath,
            ["Operation"] = operation
        };

        var errorResponse = HandleError(error, context, operation);
        errorResponse.ErrorType = "RoslynError";
        errorResponse.Message = "Code analysis failed";

        return errorResponse;
    }

    /// <summary>
    /// Handles PII detection errors.
    /// </summary>
    /// <param name="error">PII detection error.</param>
    /// <param name="fieldName">Field being analyzed.</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for PII errors.</returns>
    public ErrorResponse HandlePIIError(Exception error, string fieldName, string operation = "PIIDetection")
    {
        var context = new Dictionary<string, object>
        {
            ["FieldName"] = fieldName,
            ["Operation"] = operation
        };

        var errorResponse = HandleError(error, context, operation);
        errorResponse.ErrorType = "PIIError";
        errorResponse.Message = "PII detection failed";

        return errorResponse;
    }

    /// <summary>
    /// Handles configuration errors.
    /// </summary>
    /// <param name="error">Configuration error.</param>
    /// <param name="configurationKey">Configuration key that failed.</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for configuration errors.</returns>
    public ErrorResponse HandleConfigurationError(Exception error, string configurationKey, string operation = "Configuration")
    {
        var context = new Dictionary<string, object>
        {
            ["ConfigurationKey"] = configurationKey,
            ["Operation"] = operation
        };

        var errorResponse = HandleError(error, context, operation);
        errorResponse.ErrorType = "ConfigurationError";
        errorResponse.Message = "Configuration error";

        return errorResponse;
    }

    /// <summary>
    /// Handles performance-related errors.
    /// </summary>
    /// <param name="error">Performance error.</param>
    /// <param name="metrics">Performance metrics.</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for performance errors.</returns>
    public ErrorResponse HandlePerformanceError(Exception error, Dictionary<string, object>? metrics, string operation = "Performance")
    {
        var context = new Dictionary<string, object>
        {
            ["Operation"] = operation
        };

        if (metrics != null)
        {
            foreach (var kvp in metrics)
            {
                context[kvp.Key] = kvp.Value;
            }
        }

        var errorResponse = HandleError(error, context, operation);
        errorResponse.ErrorType = "PerformanceError";
        errorResponse.Message = "Performance threshold exceeded";

        return errorResponse;
    }

    /// <summary>
    /// Handles timeout errors.
    /// </summary>
    /// <param name="operation">Operation that timed out.</param>
    /// <param name="timeout">Timeout duration.</param>
    /// <param name="context">Additional context.</param>
    /// <returns>Error response for timeout errors.</returns>
    public ErrorResponse HandleTimeoutError(string operation, TimeSpan timeout, Dictionary<string, object>? context = null)
    {
        var errorResponse = new ErrorResponse
        {
            ErrorId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            ErrorType = "TimeoutError",
            Message = $"Operation timed out after {timeout.TotalSeconds} seconds",
            Context = context
        };

        _logger.LogWarning("Timeout error in {Operation} after {Timeout}",
            operation, timeout);

        return errorResponse;
    }

    /// <summary>
    /// Handles resource exhaustion errors.
    /// </summary>
    /// <param name="resourceType">Type of resource that was exhausted.</param>
    /// <param name="currentUsage">Current resource usage.</param>
    /// <param name="limit">Resource limit.</param>
    /// <param name="operation">Operation being performed.</param>
    /// <returns>Error response for resource exhaustion errors.</returns>
    public ErrorResponse HandleResourceExhaustionError(string resourceType, long currentUsage, long limit, string operation = "Resource")
    {
        var context = new Dictionary<string, object>
        {
            ["ResourceType"] = resourceType,
            ["CurrentUsage"] = currentUsage,
            ["Limit"] = limit,
            ["Operation"] = operation
        };

        var errorResponse = new ErrorResponse
        {
            ErrorId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            ErrorType = "ResourceExhaustionError",
            Message = $"{resourceType} limit exceeded ({currentUsage}/{limit})",
            Context = context
        };

        _logger.LogError("Resource exhaustion: {ResourceType} limit exceeded ({CurrentUsage}/{Limit}) in {Operation}",
            resourceType, currentUsage, limit, operation);

        return errorResponse;
    }

    private void LogError(Exception error, Dictionary<string, object>? context, string operation, string errorId)
    {
        var logLevel = GetLogLevel(error);
        var message = $"Error in {operation} (ID: {errorId}): {error.Message}";

        if (context != null)
        {
            var contextJson = JsonSerializer.Serialize(context);
            message += $" | Context: {contextJson}";
        }

        _logger.Log(logLevel, error, message);
    }

    private LogLevel GetLogLevel(Exception error)
    {
        return error switch
        {
            ArgumentException => LogLevel.Warning,
            InvalidOperationException => LogLevel.Warning,
            TimeoutException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            FileNotFoundException => LogLevel.Warning,
            DirectoryNotFoundException => LogLevel.Warning,
            OutOfMemoryException => LogLevel.Critical,
            StackOverflowException => LogLevel.Critical,
            _ => LogLevel.Error
        };
    }

    private string GetUserFriendlyMessage(Exception error)
    {
        return error switch
        {
            ArgumentException => "Invalid input provided",
            InvalidOperationException => "Operation cannot be performed in current state",
            TimeoutException => "Operation timed out",
            UnauthorizedAccessException => "Access denied",
            FileNotFoundException => "File not found",
            DirectoryNotFoundException => "Directory not found",
            OutOfMemoryException => "Insufficient memory",
            StackOverflowException => "Stack overflow occurred",
            _ => "An unexpected error occurred"
        };
    }

    private string RedactConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return string.Empty;

        try
        {
            var uri = new Uri(connectionString);
            return $"{uri.Scheme}://{uri.Host}:{uri.Port}/{uri.AbsolutePath.TrimStart('/')}";
        }
        catch
        {
            return "[REDACTED]";
        }
    }
}

/// <summary>
/// Standardized error response structure.
/// </summary>
public class ErrorResponse
{
    public string ErrorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
