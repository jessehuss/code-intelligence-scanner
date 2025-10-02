using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cataloger.Scanner.Logging;

/// <summary>
/// Enhanced logger for the Code Intelligence Scanner with structured logging and provenance tracking.
/// </summary>
public class ScannerLogger
{
    private readonly ILogger<ScannerLogger> _logger;
    private readonly bool _enableStructuredLogging;

    public ScannerLogger(ILogger<ScannerLogger> logger, bool enableStructuredLogging = true)
    {
        _logger = logger;
        _enableStructuredLogging = enableStructuredLogging;
    }

    /// <summary>
    /// Logs the start of a scan operation with provenance information.
    /// </summary>
    /// <param name="repository">Repository being scanned.</param>
    /// <param name="scanId">Unique scan identifier.</param>
    /// <param name="scanType">Type of scan (full, incremental, integrity).</param>
    public void LogScanStarted(string repository, string scanId, string scanType)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogInformation("Scan started: {Repository} | {ScanId} | {ScanType} | {Timestamp}",
                repository, scanId, scanType, DateTime.UtcNow);
        }
        else
        {
            _logger.LogInformation("Scan started for repository {Repository} with ID {ScanId} ({ScanType})",
                repository, scanId, scanType);
        }
    }

    /// <summary>
    /// Logs the completion of a scan operation with statistics.
    /// </summary>
    /// <param name="repository">Repository that was scanned.</param>
    /// <param name="scanId">Unique scan identifier.</param>
    /// <param name="duration">Scan duration.</param>
    /// <param name="statistics">Scan statistics.</param>
    public void LogScanCompleted(string repository, string scanId, TimeSpan duration, ScanStatistics statistics)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogInformation("Scan completed: {Repository} | {ScanId} | {Duration} | {Statistics}",
                repository, scanId, duration, JsonSerializer.Serialize(statistics));
        }
        else
        {
            _logger.LogInformation("Scan completed for repository {Repository} with ID {ScanId} in {Duration}. " +
                "Found {TypeCount} types, {CollectionCount} collections, {OperationCount} operations",
                repository, scanId, duration, statistics.TypeCount, statistics.CollectionCount, statistics.OperationCount);
        }
    }

    /// <summary>
    /// Logs a scan error with detailed context.
    /// </summary>
    /// <param name="repository">Repository being scanned.</param>
    /// <param name="scanId">Unique scan identifier.</param>
    /// <param name="error">Error that occurred.</param>
    /// <param name="context">Additional context information.</param>
    public void LogScanError(string repository, string scanId, Exception error, Dictionary<string, object>? context = null)
    {
        if (_enableStructuredLogging)
        {
            var logData = new Dictionary<string, object>
            {
                ["Repository"] = repository,
                ["ScanId"] = scanId,
                ["Error"] = error.Message,
                ["ErrorType"] = error.GetType().Name,
                ["StackTrace"] = error.StackTrace ?? string.Empty,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (context != null)
            {
                foreach (var kvp in context)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogError("Scan error: {LogData}", JsonSerializer.Serialize(logData));
        }
        else
        {
            _logger.LogError(error, "Scan error for repository {Repository} with ID {ScanId}: {ErrorMessage}",
                repository, scanId, error.Message);
        }
    }

    /// <summary>
    /// Logs the extraction of a code type with provenance information.
    /// </summary>
    /// <param name="typeName">Name of the extracted type.</param>
    /// <param name="namespace">Namespace of the type.</param>
    /// <param name="filePath">File path where the type was found.</param>
    /// <param name="lineNumber">Line number where the type was found.</param>
    public void LogTypeExtracted(string typeName, string @namespace, string filePath, int lineNumber)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogDebug("Type extracted: {TypeName} | {Namespace} | {FilePath} | {LineNumber}",
                typeName, @namespace, filePath, lineNumber);
        }
        else
        {
            _logger.LogDebug("Extracted type {TypeName} from {Namespace} at {FilePath}:{LineNumber}",
                typeName, @namespace, filePath, lineNumber);
        }
    }

    /// <summary>
    /// Logs the resolution of a collection mapping.
    /// </summary>
    /// <param name="typeName">Type name.</param>
    /// <param name="collectionName">Resolved collection name.</param>
    /// <param name="resolutionMethod">Method used for resolution.</param>
    /// <param name="confidence">Confidence score (0.0-1.0).</param>
    public void LogCollectionResolved(string typeName, string collectionName, string resolutionMethod, double confidence)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogDebug("Collection resolved: {TypeName} | {CollectionName} | {ResolutionMethod} | {Confidence}",
                typeName, collectionName, resolutionMethod, confidence);
        }
        else
        {
            _logger.LogDebug("Resolved collection {CollectionName} for type {TypeName} using {ResolutionMethod} (confidence: {Confidence})",
                collectionName, typeName, resolutionMethod, confidence);
        }
    }

    /// <summary>
    /// Logs the extraction of a MongoDB operation.
    /// </summary>
    /// <param name="operationType">Type of operation (Find, Update, etc.).</param>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="filePath">File path where the operation was found.</param>
    /// <param name="lineNumber">Line number where the operation was found.</param>
    public void LogOperationExtracted(string operationType, string collectionName, string filePath, int lineNumber)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogDebug("Operation extracted: {OperationType} | {CollectionName} | {FilePath} | {LineNumber}",
                operationType, collectionName, filePath, lineNumber);
        }
        else
        {
            _logger.LogDebug("Extracted {OperationType} operation on collection {CollectionName} at {FilePath}:{LineNumber}",
                operationType, collectionName, filePath, lineNumber);
        }
    }

    /// <summary>
    /// Logs the inference of a data relationship.
    /// </summary>
    /// <param name="sourceType">Source type name.</param>
    /// <param name="targetType">Target type name.</param>
    /// <param name="relationshipType">Type of relationship.</param>
    /// <param name="confidence">Confidence score (0.0-1.0).</param>
    /// <param name="evidence">Evidence for the relationship.</param>
    public void LogRelationshipInferred(string sourceType, string targetType, string relationshipType, double confidence, string evidence)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogDebug("Relationship inferred: {SourceType} | {TargetType} | {RelationshipType} | {Confidence} | {Evidence}",
                sourceType, targetType, relationshipType, confidence, evidence);
        }
        else
        {
            _logger.LogDebug("Inferred {RelationshipType} relationship from {SourceType} to {TargetType} (confidence: {Confidence}) - {Evidence}",
                relationshipType, sourceType, targetType, confidence, evidence);
        }
    }

    /// <summary>
    /// Logs MongoDB sampling activity.
    /// </summary>
    /// <param name="collectionName">Collection being sampled.</param>
    /// <param name="sampleSize">Number of documents sampled.</param>
    /// <param name="duration">Sampling duration.</param>
    /// <param name="piiRedacted">Whether PII was redacted.</param>
    public void LogMongoSampling(string collectionName, int sampleSize, TimeSpan duration, bool piiRedacted)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogInformation("MongoDB sampling: {CollectionName} | {SampleSize} | {Duration} | {PIIRedacted}",
                collectionName, sampleSize, duration, piiRedacted);
        }
        else
        {
            _logger.LogInformation("Sampled {SampleSize} documents from collection {CollectionName} in {Duration} (PII redacted: {PIIRedacted})",
                sampleSize, collectionName, duration, piiRedacted);
        }
    }

    /// <summary>
    /// Logs knowledge base write operations.
    /// </summary>
    /// <param name="entityType">Type of entity being written.</param>
    /// <param name="entityId">ID of the entity.</param>
    /// <param name="operation">Write operation (insert, update, delete).</param>
    public void LogKnowledgeBaseWrite(string entityType, string entityId, string operation)
    {
        if (_enableStructuredLogging)
        {
            _logger.LogDebug("Knowledge base write: {EntityType} | {EntityId} | {Operation}",
                entityType, entityId, operation);
        }
        else
        {
            _logger.LogDebug("Knowledge base {Operation} operation for {EntityType} with ID {EntityId}",
                operation, entityType, entityId);
        }
    }

    /// <summary>
    /// Logs performance metrics.
    /// </summary>
    /// <param name="operation">Operation being measured.</param>
    /// <param name="duration">Operation duration.</param>
    /// <param name="metrics">Additional performance metrics.</param>
    public void LogPerformanceMetrics(string operation, TimeSpan duration, Dictionary<string, object>? metrics = null)
    {
        if (_enableStructuredLogging)
        {
            var logData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["Duration"] = duration.TotalMilliseconds,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (metrics != null)
            {
                foreach (var kvp in metrics)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation("Performance metrics: {LogData}", JsonSerializer.Serialize(logData));
        }
        else
        {
            _logger.LogInformation("Performance: {Operation} completed in {Duration}ms",
                operation, duration.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Logs configuration validation results.
    /// </summary>
    /// <param name="isValid">Whether the configuration is valid.</param>
    /// <param name="errors">Configuration errors if any.</param>
    public void LogConfigurationValidation(bool isValid, List<string>? errors = null)
    {
        if (_enableStructuredLogging)
        {
            var logData = new Dictionary<string, object>
            {
                ["IsValid"] = isValid,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (errors != null && errors.Count > 0)
            {
                logData["Errors"] = errors;
            }

            _logger.LogInformation("Configuration validation: {LogData}", JsonSerializer.Serialize(logData));
        }
        else
        {
            if (isValid)
            {
                _logger.LogInformation("Configuration validation passed");
            }
            else
            {
                _logger.LogWarning("Configuration validation failed: {Errors}", string.Join(", ", errors ?? new List<string>()));
            }
        }
    }

    /// <summary>
    /// Logs system health status.
    /// </summary>
    /// <param name="status">Health status.</param>
    /// <param name="details">Health details.</param>
    public void LogHealthStatus(string status, Dictionary<string, object>? details = null)
    {
        if (_enableStructuredLogging)
        {
            var logData = new Dictionary<string, object>
            {
                ["Status"] = status,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (details != null)
            {
                foreach (var kvp in details)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation("Health status: {LogData}", JsonSerializer.Serialize(logData));
        }
        else
        {
            _logger.LogInformation("Health status: {Status}", status);
        }
    }
}

/// <summary>
/// Statistics for a scan operation.
/// </summary>
public class ScanStatistics
{
    public int TypeCount { get; set; }
    public int CollectionCount { get; set; }
    public int OperationCount { get; set; }
    public int RelationshipCount { get; set; }
    public int FileCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public long MemoryUsageBytes { get; set; }
}
