using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Configuration;

/// <summary>
/// Configuration settings for the Code Intelligence Scanner.
/// </summary>
public class ScannerConfiguration
{
    /// <summary>
    /// Knowledge base connection settings.
    /// </summary>
    [Required]
    public KnowledgeBaseConfiguration KnowledgeBase { get; set; } = new();

    /// <summary>
    /// MongoDB sampling settings.
    /// </summary>
    public MongoSamplingConfiguration MongoSampling { get; set; } = new();

    /// <summary>
    /// Scanning settings.
    /// </summary>
    public ScanningConfiguration Scanning { get; set; } = new();

    /// <summary>
    /// PII redaction settings.
    /// </summary>
    public PIIRedactionConfiguration PIIRedaction { get; set; } = new();

    /// <summary>
    /// Performance and monitoring settings.
    /// </summary>
    public PerformanceConfiguration Performance { get; set; } = new();

    /// <summary>
    /// Logging settings.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new();
}

/// <summary>
/// Knowledge base connection configuration.
/// </summary>
public class KnowledgeBaseConfiguration
{
    /// <summary>
    /// MongoDB connection string for the knowledge base.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = "mongodb://localhost:27017/catalog_kb";

    /// <summary>
    /// Knowledge base database name.
    /// </summary>
    [Required]
    public string DatabaseName { get; set; } = "catalog_kb";

    /// <summary>
    /// Atlas Search configuration.
    /// </summary>
    public AtlasSearchConfiguration AtlasSearch { get; set; } = new();

    /// <summary>
    /// Optional Neo4j configuration for graph queries.
    /// </summary>
    public Neo4jConfiguration? Neo4j { get; set; }
}

/// <summary>
/// Atlas Search configuration.
/// </summary>
public class AtlasSearchConfiguration
{
    /// <summary>
    /// Whether to enable Atlas Search.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Atlas Search index name prefix.
    /// </summary>
    public string IndexPrefix { get; set; } = "kb_search";

    /// <summary>
    /// Maximum number of search results to return.
    /// </summary>
    [Range(1, 1000)]
    public int MaxResults { get; set; } = 100;

    /// <summary>
    /// Default search result limit.
    /// </summary>
    [Range(1, 100)]
    public int DefaultLimit { get; set; } = 50;
}

/// <summary>
/// Neo4j configuration for graph queries.
/// </summary>
public class Neo4jConfiguration
{
    /// <summary>
    /// Neo4j connection URI.
    /// </summary>
    [Required]
    public string Uri { get; set; } = "bolt://localhost:7687";

    /// <summary>
    /// Neo4j username.
    /// </summary>
    [Required]
    public string Username { get; set; } = "neo4j";

    /// <summary>
    /// Neo4j password.
    /// </summary>
    [Required]
    public string Password { get; set; } = "password";

    /// <summary>
    /// Neo4j database name.
    /// </summary>
    public string Database { get; set; } = "neo4j";
}

/// <summary>
/// MongoDB sampling configuration.
/// </summary>
public class MongoSamplingConfiguration
{
    /// <summary>
    /// Whether to enable MongoDB sampling.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of documents to sample per collection.
    /// </summary>
    [Range(1, 10000)]
    public int MaxDocumentsPerCollection { get; set; } = 1000;

    /// <summary>
    /// Maximum number of collections to sample.
    /// </summary>
    [Range(1, 1000)]
    public int MaxCollections { get; set; } = 100;

    /// <summary>
    /// Sampling timeout in seconds.
    /// </summary>
    [Range(1, 3600)]
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Whether to use read-only credentials for sampling.
    /// </summary>
    public bool UseReadOnlyCredentials { get; set; } = true;

    /// <summary>
    /// Connection pool size for sampling.
    /// </summary>
    [Range(1, 100)]
    public int ConnectionPoolSize { get; set; } = 10;
}

/// <summary>
/// Scanning configuration.
/// </summary>
public class ScanningConfiguration
{
    /// <summary>
    /// Maximum number of repositories to scan concurrently.
    /// </summary>
    [Range(1, 20)]
    public int MaxConcurrentRepositories { get; set; } = 5;

    /// <summary>
    /// Maximum number of files to process concurrently per repository.
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrentFiles { get; set; } = 20;

    /// <summary>
    /// File extensions to include in scanning.
    /// </summary>
    public string[] IncludedExtensions { get; set; } = { ".cs", ".csproj", ".sln" };

    /// <summary>
    /// Directories to exclude from scanning.
    /// </summary>
    public string[] ExcludedDirectories { get; set; } = { "bin", "obj", "node_modules", ".git", ".vs" };

    /// <summary>
    /// Maximum file size to process in MB.
    /// </summary>
    [Range(1, 100)]
    public int MaxFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Whether to perform incremental scanning.
    /// </summary>
    public bool EnableIncrementalScanning { get; set; } = true;

    /// <summary>
    /// Whether to perform integrity checks.
    /// </summary>
    public bool EnableIntegrityChecks { get; set; } = true;

    /// <summary>
    /// Integrity check interval in days.
    /// </summary>
    [Range(1, 30)]
    public int IntegrityCheckIntervalDays { get; set; } = 7;
}

/// <summary>
/// PII redaction configuration.
/// </summary>
public class PIIRedactionConfiguration
{
    /// <summary>
    /// Whether to enable PII redaction.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Field names that indicate PII.
    /// </summary>
    public string[] PIIFieldNames { get; set; } = 
    {
        "email", "phone", "ssn", "token", "key", "address", "name", "ip", "jwt", "credit",
        "password", "secret", "private", "personal", "sensitive", "confidential"
    };

    /// <summary>
    /// Regex patterns for PII values.
    /// </summary>
    public string[] PIIValuePatterns { get; set; } = 
    {
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", // Email
        @"\b\d{3}[-.\s]?\d{3}[-.\s]?\d{4}\b", // Phone number
        @"\b\d{3}-\d{2}-\d{4}\b", // SSN
        @"\b[A-Za-z0-9+/]{40,}={0,2}\b", // Base64 encoded data
        @"\b[A-Fa-f0-9]{32,}\b" // Hex encoded data
    };

    /// <summary>
    /// Redaction replacement value.
    /// </summary>
    public string RedactionValue { get; set; } = "[REDACTED]";

    /// <summary>
    /// Whether to preserve data types during redaction.
    /// </summary>
    public bool PreserveDataTypes { get; set; } = true;
}

/// <summary>
/// Performance and monitoring configuration.
/// </summary>
public class PerformanceConfiguration
{
    /// <summary>
    /// Whether to enable performance monitoring.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Performance monitoring interval in seconds.
    /// </summary>
    [Range(1, 3600)]
    public int MonitoringIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum memory usage in MB before warning.
    /// </summary>
    [Range(100, 10000)]
    public int MaxMemoryUsageMB { get; set; } = 2048;

    /// <summary>
    /// Maximum CPU usage percentage before warning.
    /// </summary>
    [Range(50, 100)]
    public int MaxCpuUsagePercent { get; set; } = 80;

    /// <summary>
    /// Whether to enable detailed performance metrics.
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = false;

    /// <summary>
    /// Performance metrics retention period in days.
    /// </summary>
    [Range(1, 365)]
    public int MetricsRetentionDays { get; set; } = 30;
}

/// <summary>
/// Logging configuration.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Log level for the application.
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// Whether to enable structured logging.
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable console logging.
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable file logging.
    /// </summary>
    public bool EnableFileLogging { get; set; } = false;

    /// <summary>
    /// File logging path.
    /// </summary>
    public string FileLoggingPath { get; set; } = "logs/scanner.log";

    /// <summary>
    /// Maximum log file size in MB.
    /// </summary>
    [Range(1, 1000)]
    public int MaxLogFileSizeMB { get; set; } = 100;

    /// <summary>
    /// Number of log files to retain.
    /// </summary>
    [Range(1, 100)]
    public int MaxLogFiles { get; set; } = 10;

    /// <summary>
    /// Whether to enable external logging integration.
    /// </summary>
    public bool EnableExternalLogging { get; set; } = false;

    /// <summary>
    /// External logging configuration.
    /// </summary>
    public ExternalLoggingConfiguration? ExternalLogging { get; set; }
}

/// <summary>
/// External logging configuration.
/// </summary>
public class ExternalLoggingConfiguration
{
    /// <summary>
    /// External logging provider type.
    /// </summary>
    public string Provider { get; set; } = "ApplicationInsights";

    /// <summary>
    /// Connection string or endpoint for external logging.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Additional configuration for external logging.
    /// </summary>
    public Dictionary<string, string> AdditionalSettings { get; set; } = new();
}
