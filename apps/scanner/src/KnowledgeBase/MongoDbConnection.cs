using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Cataloger.Scanner.KnowledgeBase;

/// <summary>
/// Service for managing MongoDB connections and configuration.
/// </summary>
public class MongoDbConnection
{
    private readonly ILogger<MongoDbConnection> _logger;
    private readonly MongoDbOptions _options;

    public MongoDbConnection(ILogger<MongoDbConnection> logger, IOptions<MongoDbOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Creates a MongoDB client with the configured connection string.
    /// </summary>
    /// <returns>Configured MongoDB client.</returns>
    public IMongoClient CreateClient()
    {
        try
        {
            _logger.LogDebug("Creating MongoDB client with connection string: {ConnectionString}", 
                MaskConnectionString(_options.ConnectionString));

            var clientSettings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
            
            // Configure client settings
            ConfigureClientSettings(clientSettings);

            var client = new MongoClient(clientSettings);
            
            _logger.LogInformation("MongoDB client created successfully");
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MongoDB client");
            throw;
        }
    }

    /// <summary>
    /// Creates a MongoDB database instance.
    /// </summary>
    /// <param name="client">MongoDB client.</param>
    /// <returns>Configured MongoDB database.</returns>
    public IMongoDatabase CreateDatabase(IMongoClient client)
    {
        try
        {
            _logger.LogDebug("Creating MongoDB database: {DatabaseName}", _options.DatabaseName);

            var database = client.GetDatabase(_options.DatabaseName);
            
            _logger.LogInformation("MongoDB database created successfully: {DatabaseName}", _options.DatabaseName);
            return database;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MongoDB database: {DatabaseName}", _options.DatabaseName);
            throw;
        }
    }

    /// <summary>
    /// Tests the MongoDB connection.
    /// </summary>
    /// <param name="client">MongoDB client to test.</param>
    /// <returns>True if connection is successful, false otherwise.</returns>
    public async Task<bool> TestConnectionAsync(IMongoClient client)
    {
        try
        {
            _logger.LogDebug("Testing MongoDB connection");

            // Ping the database
            await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            
            _logger.LogInformation("MongoDB connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Gets database statistics.
    /// </summary>
    /// <param name="database">MongoDB database.</param>
    /// <returns>Database statistics.</returns>
    public async Task<DatabaseStats> GetDatabaseStatsAsync(IMongoDatabase database)
    {
        try
        {
            _logger.LogDebug("Getting database statistics for: {DatabaseName}", database.DatabaseNamespace.DatabaseName);

            var stats = await database.RunCommandAsync<BsonDocument>(new BsonDocument("dbStats", 1));
            
            var databaseStats = new DatabaseStats
            {
                DatabaseName = database.DatabaseNamespace.DatabaseName,
                Collections = stats.GetValue("collections", 0).AsInt32,
                DataSize = stats.GetValue("dataSize", 0).AsInt64,
                StorageSize = stats.GetValue("storageSize", 0).AsInt64,
                Indexes = stats.GetValue("indexes", 0).AsInt32,
                IndexSize = stats.GetValue("indexSize", 0).AsInt64,
                Objects = stats.GetValue("objects", 0).AsInt64
            };

            _logger.LogInformation("Database statistics retrieved: {Collections} collections, {DataSize} bytes data", 
                databaseStats.Collections, databaseStats.DataSize);

            return databaseStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database statistics");
            throw;
        }
    }

    /// <summary>
    /// Gets collection statistics.
    /// </summary>
    /// <param name="database">MongoDB database.</param>
    /// <param name="collectionName">Collection name.</param>
    /// <returns>Collection statistics.</returns>
    public async Task<CollectionStats> GetCollectionStatsAsync(IMongoDatabase database, string collectionName)
    {
        try
        {
            _logger.LogDebug("Getting collection statistics for: {CollectionName}", collectionName);

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var stats = await collection.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument("collStats", collectionName));

            var collectionStats = new CollectionStats
            {
                CollectionName = collectionName,
                Count = stats.GetValue("count", 0).AsInt64,
                Size = stats.GetValue("size", 0).AsInt64,
                StorageSize = stats.GetValue("storageSize", 0).AsInt64,
                Indexes = stats.GetValue("nindexes", 0).AsInt32,
                IndexSize = stats.GetValue("totalIndexSize", 0).AsInt64,
                AverageObjectSize = stats.GetValue("avgObjSize", 0).AsDouble
            };

            _logger.LogInformation("Collection statistics retrieved: {CollectionName} - {Count} documents, {Size} bytes", 
                collectionName, collectionStats.Count, collectionStats.Size);

            return collectionStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection statistics for: {CollectionName}", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Lists all collections in the database.
    /// </summary>
    /// <param name="database">MongoDB database.</param>
    /// <returns>List of collection names.</returns>
    public async Task<List<string>> ListCollectionsAsync(IMongoDatabase database)
    {
        try
        {
            _logger.LogDebug("Listing collections in database: {DatabaseName}", database.DatabaseNamespace.DatabaseName);

            var collections = new List<string>();
            var cursor = await database.ListCollectionNamesAsync();
            
            await cursor.ForEachAsync(collectionName => collections.Add(collectionName));

            _logger.LogInformation("Found {CollectionCount} collections in database", collections.Count);
            return collections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list collections");
            throw;
        }
    }

    /// <summary>
    /// Creates a collection if it doesn't exist.
    /// </summary>
    /// <param name="database">MongoDB database.</param>
    /// <param name="collectionName">Collection name.</param>
    /// <param name="options">Collection creation options.</param>
    /// <returns>True if collection was created, false if it already existed.</returns>
    public async Task<bool> CreateCollectionIfNotExistsAsync(
        IMongoDatabase database, 
        string collectionName, 
        CreateCollectionOptions? options = null)
    {
        try
        {
            _logger.LogDebug("Creating collection if not exists: {CollectionName}", collectionName);

            var collections = await ListCollectionsAsync(database);
            if (collections.Contains(collectionName))
            {
                _logger.LogDebug("Collection already exists: {CollectionName}", collectionName);
                return false;
            }

            await database.CreateCollectionAsync(collectionName, options);
            
            _logger.LogInformation("Collection created successfully: {CollectionName}", collectionName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create collection: {CollectionName}", collectionName);
            throw;
        }
    }

    private void ConfigureClientSettings(MongoClientSettings settings)
    {
        // Configure connection timeout
        if (_options.ConnectionTimeout > 0)
        {
            settings.ConnectTimeout = TimeSpan.FromMilliseconds(_options.ConnectionTimeout);
        }

        // Configure server selection timeout
        if (_options.ServerSelectionTimeout > 0)
        {
            settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(_options.ServerSelectionTimeout);
        }

        // Configure socket timeout
        if (_options.SocketTimeout > 0)
        {
            settings.SocketTimeout = TimeSpan.FromMilliseconds(_options.SocketTimeout);
        }

        // Configure max connection pool size
        if (_options.MaxConnectionPoolSize > 0)
        {
            settings.MaxConnectionPoolSize = _options.MaxConnectionPoolSize;
        }

        // Configure min connection pool size
        if (_options.MinConnectionPoolSize > 0)
        {
            settings.MinConnectionPoolSize = _options.MinConnectionPoolSize;
        }

        // Configure read preference
        if (!string.IsNullOrEmpty(_options.ReadPreference))
        {
            settings.ReadPreference = ParseReadPreference(_options.ReadPreference);
        }

        // Configure write concern
        if (!string.IsNullOrEmpty(_options.WriteConcern))
        {
            settings.WriteConcern = ParseWriteConcern(_options.WriteConcern);
        }

        // Configure retry reads
        settings.RetryReads = _options.RetryReads;

        // Configure retry writes
        settings.RetryWrites = _options.RetryWrites;
    }

    private ReadPreference ParseReadPreference(string readPreference)
    {
        return readPreference.ToLowerInvariant() switch
        {
            "primary" => ReadPreference.Primary,
            "primarypreferred" => ReadPreference.PrimaryPreferred,
            "secondary" => ReadPreference.Secondary,
            "secondarypreferred" => ReadPreference.SecondaryPreferred,
            "nearest" => ReadPreference.Nearest,
            _ => ReadPreference.Primary
        };
    }

    private WriteConcern ParseWriteConcern(string writeConcern)
    {
        return writeConcern.ToLowerInvariant() switch
        {
            "unacknowledged" => WriteConcern.Unacknowledged,
            "acknowledged" => WriteConcern.Acknowledged,
            "majority" => WriteConcern.Majority,
            _ => WriteConcern.Acknowledged
        };
    }

    private string MaskConnectionString(string connectionString)
    {
        // Mask sensitive information in connection string for logging
        if (string.IsNullOrEmpty(connectionString))
        {
            return "***";
        }

        var uri = new Uri(connectionString);
        var maskedUri = $"{uri.Scheme}://{uri.Host}:{uri.Port}{uri.AbsolutePath}";
        return maskedUri;
    }
}

/// <summary>
/// MongoDB connection options.
/// </summary>
public class MongoDbOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "catalog_kb";
    public int ConnectionTimeout { get; set; } = 30000;
    public int ServerSelectionTimeout { get; set; } = 30000;
    public int SocketTimeout { get; set; } = 30000;
    public int MaxConnectionPoolSize { get; set; } = 100;
    public int MinConnectionPoolSize { get; set; } = 0;
    public string ReadPreference { get; set; } = "primary";
    public string WriteConcern { get; set; } = "acknowledged";
    public bool RetryReads { get; set; } = true;
    public bool RetryWrites { get; set; } = true;
}

/// <summary>
/// Database statistics.
/// </summary>
public class DatabaseStats
{
    public string DatabaseName { get; set; } = string.Empty;
    public int Collections { get; set; }
    public long DataSize { get; set; }
    public long StorageSize { get; set; }
    public int Indexes { get; set; }
    public long IndexSize { get; set; }
    public long Objects { get; set; }
}

/// <summary>
/// Collection statistics.
/// </summary>
public class CollectionStats
{
    public string CollectionName { get; set; } = string.Empty;
    public long Count { get; set; }
    public long Size { get; set; }
    public long StorageSize { get; set; }
    public int Indexes { get; set; }
    public long IndexSize { get; set; }
    public double AverageObjectSize { get; set; }
}
