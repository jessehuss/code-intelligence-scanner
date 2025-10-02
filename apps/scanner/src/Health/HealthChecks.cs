using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;

namespace Cataloger.Scanner.Health;

/// <summary>
/// Health check service for the Code Intelligence Scanner.
/// </summary>
public class ScannerHealthChecks
{
    private readonly ILogger<ScannerHealthChecks> _logger;
    private readonly IMongoDatabase _database;
    private readonly HealthCheckService _healthCheckService;

    public ScannerHealthChecks(
        ILogger<ScannerHealthChecks> logger,
        IMongoDatabase database,
        HealthCheckService healthCheckService)
    {
        _logger = logger;
        _database = database;
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Performs a comprehensive health check of the scanner system.
    /// </summary>
    /// <returns>Health check result.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            var healthStatus = HealthStatus.Healthy;
            var healthIssues = new List<string>();

            // Check MongoDB connection
            var mongoHealth = await CheckMongoDbHealthAsync();
            healthData["MongoDB"] = mongoHealth;
            if (mongoHealth.Status != HealthStatus.Healthy)
            {
                healthStatus = HealthStatus.Unhealthy;
                healthIssues.Add($"MongoDB: {mongoHealth.Description}");
            }

            // Check system resources
            var systemHealth = await CheckSystemHealthAsync();
            healthData["System"] = systemHealth;
            if (systemHealth.Status != HealthStatus.Healthy)
            {
                healthStatus = HealthStatus.Degraded;
                healthIssues.Add($"System: {systemHealth.Description}");
            }

            // Check knowledge base integrity
            var kbHealth = await CheckKnowledgeBaseHealthAsync();
            healthData["KnowledgeBase"] = kbHealth;
            if (kbHealth.Status != HealthStatus.Healthy)
            {
                healthStatus = HealthStatus.Degraded;
                healthIssues.Add($"KnowledgeBase: {kbHealth.Description}");
            }

            // Check Atlas Search (if enabled)
            var searchHealth = await CheckAtlasSearchHealthAsync();
            healthData["AtlasSearch"] = searchHealth;
            if (searchHealth.Status != HealthStatus.Healthy)
            {
                healthStatus = HealthStatus.Degraded;
                healthIssues.Add($"AtlasSearch: {searchHealth.Description}");
            }

            var description = healthIssues.Any() 
                ? string.Join("; ", healthIssues) 
                : "All systems healthy";

            return new HealthCheckResult(healthStatus, description, data: healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            return new HealthCheckResult(HealthStatus.Unhealthy, 
                $"Health check failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks MongoDB connection and basic operations.
    /// </summary>
    /// <returns>MongoDB health check result.</returns>
    public async Task<HealthCheckResult> CheckMongoDbHealthAsync()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Test basic connectivity
            await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            
            // Test collection access
            var collections = await _database.ListCollectionNamesAsync();
            var collectionList = await collections.ToListAsync();
            
            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["ResponseTime"] = stopwatch.ElapsedMilliseconds,
                ["CollectionCount"] = collectionList.Count,
                ["DatabaseName"] = _database.DatabaseNamespace.DatabaseName
            };

            return new HealthCheckResult(HealthStatus.Healthy, 
                "MongoDB connection healthy", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed");
            return new HealthCheckResult(HealthStatus.Unhealthy, 
                $"MongoDB connection failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks system resources (memory, CPU, disk).
    /// </summary>
    /// <returns>System health check result.</returns>
    public async Task<HealthCheckResult> CheckSystemHealthAsync()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var virtualMemory = process.VirtualMemorySize64;
            var cpuTime = process.TotalProcessorTime;

            // Get system memory info
            var totalMemory = GC.GetTotalMemory(false);
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);

            // Check disk space
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:");
            var freeSpace = drive.AvailableFreeSpace;
            var totalSpace = drive.TotalSize;

            var data = new Dictionary<string, object>
            {
                ["WorkingSetMB"] = workingSet / (1024 * 1024),
                ["PrivateMemoryMB"] = privateMemory / (1024 * 1024),
                ["VirtualMemoryMB"] = virtualMemory / (1024 * 1024),
                ["CpuTime"] = cpuTime.TotalMilliseconds,
                ["GcMemoryMB"] = totalMemory / (1024 * 1024),
                ["GcGen0Collections"] = gen0Collections,
                ["GcGen1Collections"] = gen1Collections,
                ["GcGen2Collections"] = gen2Collections,
                ["FreeDiskSpaceGB"] = freeSpace / (1024 * 1024 * 1024),
                ["TotalDiskSpaceGB"] = totalSpace / (1024 * 1024 * 1024),
                ["DiskUsagePercent"] = (double)(totalSpace - freeSpace) / totalSpace * 100
            };

            // Check for resource constraints
            var issues = new List<string>();
            var status = HealthStatus.Healthy;

            if (workingSet > 2L * 1024 * 1024 * 1024) // 2GB
            {
                issues.Add("High memory usage");
                status = HealthStatus.Degraded;
            }

            if (freeSpace < 1024L * 1024 * 1024) // 1GB
            {
                issues.Add("Low disk space");
                status = HealthStatus.Degraded;
            }

            var description = issues.Any() 
                ? string.Join("; ", issues) 
                : "System resources healthy";

            return new HealthCheckResult(status, description, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System health check failed");
            return new HealthCheckResult(HealthStatus.Unhealthy, 
                $"System health check failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks knowledge base integrity and consistency.
    /// </summary>
    /// <returns>Knowledge base health check result.</returns>
    public async Task<HealthCheckResult> CheckKnowledgeBaseHealthAsync()
    {
        try
        {
            var data = new Dictionary<string, object>();
            var issues = new List<string>();
            var status = HealthStatus.Healthy;

            // Check collection counts
            var codeTypesCollection = _database.GetCollection<BsonDocument>("code_types");
            var collectionMappingsCollection = _database.GetCollection<BsonDocument>("collection_mappings");
            var queryOperationsCollection = _database.GetCollection<BsonDocument>("query_operations");
            var dataRelationshipsCollection = _database.GetCollection<BsonDocument>("data_relationships");
            var observedSchemasCollection = _database.GetCollection<BsonDocument>("observed_schemas");
            var knowledgeBaseEntriesCollection = _database.GetCollection<BsonDocument>("knowledge_base_entries");

            var codeTypesCount = await codeTypesCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            var collectionMappingsCount = await collectionMappingsCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            var queryOperationsCount = await queryOperationsCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            var dataRelationshipsCount = await dataRelationshipsCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            var observedSchemasCount = await observedSchemasCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            var knowledgeBaseEntriesCount = await knowledgeBaseEntriesCollection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);

            data["CodeTypesCount"] = codeTypesCount;
            data["CollectionMappingsCount"] = collectionMappingsCount;
            data["QueryOperationsCount"] = queryOperationsCount;
            data["DataRelationshipsCount"] = dataRelationshipsCount;
            data["ObservedSchemasCount"] = observedSchemasCount;
            data["KnowledgeBaseEntriesCount"] = knowledgeBaseEntriesCount;

            // Check for orphaned records
            var orphanedMappings = await collectionMappingsCollection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.Not(
                    Builders<BsonDocument>.Filter.In("typeId", 
                        codeTypesCollection.Find(FilterDefinition<BsonDocument>.Empty)
                            .Project(Builders<BsonDocument>.Projection.Include("_id"))
                            .ToListAsync().Result.Select(doc => doc["_id"]))));

            if (orphanedMappings > 0)
            {
                issues.Add($"{orphanedMappings} orphaned collection mappings");
                status = HealthStatus.Degraded;
            }

            // Check for missing provenance
            var missingProvenance = await codeTypesCollection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Not(Builders<BsonDocument>.Filter.Exists("provenance")),
                    Builders<BsonDocument>.Filter.Eq("provenance", BsonNull.Value)));

            if (missingProvenance > 0)
            {
                issues.Add($"{missingProvenance} records missing provenance");
                status = HealthStatus.Degraded;
            }

            var description = issues.Any() 
                ? string.Join("; ", issues) 
                : "Knowledge base integrity healthy";

            return new HealthCheckResult(status, description, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Knowledge base health check failed");
            return new HealthCheckResult(HealthStatus.Unhealthy, 
                $"Knowledge base health check failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks Atlas Search functionality (if enabled).
    /// </summary>
    /// <returns>Atlas Search health check result.</returns>
    public async Task<HealthCheckResult> CheckAtlasSearchHealthAsync()
    {
        try
        {
            // Check if Atlas Search is enabled
            var knowledgeBaseEntriesCollection = _database.GetCollection<BsonDocument>("knowledge_base_entries");
            
            // Try a simple search operation
            var searchPipeline = new BsonArray
            {
                new BsonDocument
                {
                    ["$search"] = new BsonDocument
                    {
                        ["index"] = "knowledge_base_entries_search",
                        ["text"] = new BsonDocument
                        {
                            ["query"] = "test",
                            ["path"] = new BsonArray { "searchableText" }
                        }
                    }
                },
                new BsonDocument("$limit", 1)
            };

            var cursor = await knowledgeBaseEntriesCollection.AggregateAsync<BsonDocument>(searchPipeline);
            var results = await cursor.ToListAsync();

            var data = new Dictionary<string, object>
            {
                ["SearchEnabled"] = true,
                ["TestQueryResults"] = results.Count,
                ["IndexName"] = "knowledge_base_entries_search"
            };

            return new HealthCheckResult(HealthStatus.Healthy, 
                "Atlas Search functionality healthy", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Atlas Search health check failed - may not be enabled");
            return new HealthCheckResult(HealthStatus.Degraded, 
                $"Atlas Search not available: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks specific health aspects.
    /// </summary>
    /// <param name="aspect">Health aspect to check.</param>
    /// <returns>Health check result for the specific aspect.</returns>
    public async Task<HealthCheckResult> CheckSpecificHealthAsync(string aspect)
    {
        return aspect.ToLowerInvariant() switch
        {
            "mongodb" => await CheckMongoDbHealthAsync(),
            "system" => await CheckSystemHealthAsync(),
            "knowledgebase" => await CheckKnowledgeBaseHealthAsync(),
            "atlassearch" => await CheckAtlasSearchHealthAsync(),
            _ => new HealthCheckResult(HealthStatus.Healthy, $"Unknown health aspect: {aspect}")
        };
    }

    /// <summary>
    /// Gets health status summary.
    /// </summary>
    /// <returns>Health status summary.</returns>
    public async Task<HealthStatusSummary> GetHealthStatusSummaryAsync()
    {
        var healthResult = await CheckHealthAsync();
        
        return new HealthStatusSummary
        {
            OverallStatus = healthResult.Status,
            Description = healthResult.Description,
            Timestamp = DateTime.UtcNow,
            Details = healthResult.Data ?? new Dictionary<string, object>(),
            Exception = healthResult.Exception?.Message
        };
    }
}

/// <summary>
/// Health status summary.
/// </summary>
public class HealthStatusSummary
{
    public HealthStatus OverallStatus { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public string? Exception { get; set; }
}
