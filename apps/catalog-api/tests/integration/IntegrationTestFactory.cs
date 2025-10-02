using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace CatalogApi.Tests.Integration;

/// <summary>
/// Integration test factory for setting up test environment with MongoDB container
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer;
    private IMongoClient? _mongoClient;

    public IntegrationTestFactory()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27017, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();
    }

    public IMongoClient MongoClient => _mongoClient ?? throw new InvalidOperationException("MongoDB client not initialized");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing MongoDB service registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register the test MongoDB client
            services.AddSingleton<IMongoClient>(_ => _mongoClient!);
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for integration tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:KnowledgeBase"] = _mongoDbContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = "localhost:6379", // Use local Redis for tests
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning"
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        
        var connectionString = _mongoDbContainer.GetConnectionString();
        _mongoClient = new MongoClient(connectionString);
        
        // Initialize the test database with required collections
        await InitializeTestDatabase();
    }

    public async Task DisposeAsync()
    {
        _mongoClient?.Dispose();
        await _mongoDbContainer.DisposeAsync();
    }

    private async Task InitializeTestDatabase()
    {
        if (_mongoClient == null) return;

        var database = _mongoClient.GetDatabase("catalog_kb");

        // Create collections if they don't exist
        var collections = new[]
        {
            "nodes",
            "collections", 
            "types",
            "edges",
            "schemas_declared",
            "schemas_observed"
        };

        foreach (var collectionName in collections)
        {
            await database.CreateCollectionAsync(collectionName);
        }

        // Create indexes for better performance
        await CreateIndexes(database);
    }

    private async Task CreateIndexes(IMongoDatabase database)
    {
        // Indexes for nodes collection
        var nodesCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("nodes");
        await nodesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Text("Name").Text("Description")));

        await nodesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("EntityType")));

        await nodesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("Repository")));

        // Indexes for collections collection
        var collectionsCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("collections");
        await collectionsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("Name")));

        // Indexes for types collection
        var typesCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("types");
        await typesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("FullyQualifiedName")));

        await typesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("CommitSha")));

        // Indexes for edges collection
        var edgesCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("edges");
        await edgesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("SourceNodeId")));

        await edgesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("TargetNodeId")));

        await edgesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("EdgeType")));
    }

    /// <summary>
    /// Seeds the test database with sample data
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        if (_mongoClient == null) return;

        var database = _mongoClient.GetDatabase("catalog_kb");

        // Seed nodes collection
        var nodesCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("nodes");
        var nodes = new[]
        {
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "type:User" },
                { "EntityType", "type" },
                { "Name", "User" },
                { "Description", "User entity" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Models/User.cs" },
                { "LineNumber", 15 },
                { "CommitSha", "abc123" },
                { "LastModified", DateTime.UtcNow }
            },
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "collection:users" },
                { "EntityType", "collection" },
                { "Name", "users" },
                { "Description", "Users collection" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Collections/Users.cs" },
                { "LineNumber", 20 },
                { "CommitSha", "abc123" },
                { "LastModified", DateTime.UtcNow }
            },
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "field:User.Email" },
                { "EntityType", "field" },
                { "Name", "Email" },
                { "Description", "User email address" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Models/User.cs" },
                { "LineNumber", 25 },
                { "CommitSha", "abc123" },
                { "LastModified", DateTime.UtcNow }
            }
        };

        await nodesCollection.InsertManyAsync(nodes);

        // Seed collections collection
        var collectionsCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("collections");
        var collections = new[]
        {
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "users" },
                { "Name", "users" },
                { "Description", "Users collection" },
                { "DeclaredSchemaId", "schema:declared:users" },
                { "ObservedSchemaId", "schema:observed:users" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Collections/Users.cs" },
                { "LineNumber", 20 },
                { "CommitSha", "abc123" },
                { "LastModified", DateTime.UtcNow }
            }
        };

        await collectionsCollection.InsertManyAsync(collections);

        // Seed types collection
        var typesCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("types");
        var types = new[]
        {
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "User" },
                { "FullyQualifiedName", "User" },
                { "Name", "User" },
                { "Namespace", "" },
                { "Description", "User entity" },
                { "Fields", new MongoDB.Bson.BsonArray
                    {
                        new MongoDB.Bson.BsonDocument
                        {
                            { "Name", "Id" },
                            { "Type", "string" },
                            { "IsRequired", true },
                            { "IsNullable", false },
                            { "Attributes", new MongoDB.Bson.BsonArray { "BsonId" } },
                            { "Description", "User identifier" }
                        },
                        new MongoDB.Bson.BsonDocument
                        {
                            { "Name", "Email" },
                            { "Type", "string" },
                            { "IsRequired", true },
                            { "IsNullable", false },
                            { "Attributes", new MongoDB.Bson.BsonArray() },
                            { "Description", "User email address" }
                        }
                    }
                },
                { "BsonAttributes", new MongoDB.Bson.BsonArray { "BsonId", "BsonElement" } },
                { "CollectionMappings", new MongoDB.Bson.BsonArray
                    {
                        new MongoDB.Bson.BsonDocument
                        {
                            { "CollectionName", "users" },
                            { "MappingType", "Primary" },
                            { "Repository", "test-repo" },
                            { "FilePath", "src/Models/User.cs" },
                            { "LineNumber", 15 }
                        }
                    }
                },
                { "UsageStats", new MongoDB.Bson.BsonDocument
                    {
                        { "QueryCount", 5 },
                        { "RepositoryCount", 1 },
                        { "UsedInRepositories", new MongoDB.Bson.BsonArray { "test-repo" } },
                        { "LastUsed", DateTime.UtcNow },
                        { "CommonOperations", new MongoDB.Bson.BsonArray { "Find", "Insert" } }
                    }
                },
                { "ChangeSummary", new MongoDB.Bson.BsonDocument
                    {
                        { "TotalChanges", 3 },
                        { "AddedFields", 1 },
                        { "RemovedFields", 0 },
                        { "ModifiedFields", 2 },
                        { "LastChange", DateTime.UtcNow },
                        { "RecentCommits", new MongoDB.Bson.BsonArray { "abc123", "def456" } }
                    }
                },
                { "Repository", "test-repo" },
                { "FilePath", "src/Models/User.cs" },
                { "LineNumber", 15 },
                { "CommitSha", "abc123" },
                { "LastModified", DateTime.UtcNow }
            }
        };

        await typesCollection.InsertManyAsync(types);

        // Seed edges collection
        var edgesCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("edges");
        var edges = new[]
        {
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "edge1" },
                { "SourceNodeId", "type:User" },
                { "TargetNodeId", "collection:users" },
                { "EdgeType", "READS" },
                { "Description", "User reads from users collection" },
                { "Repository", "test-repo" },
                { "FilePath", "src/Repositories/UserRepository.cs" },
                { "LineNumber", 25 },
                { "CommitSha", "abc123" },
                { "CreatedAt", DateTime.UtcNow }
            }
        };

        await edgesCollection.InsertManyAsync(edges);

        // Seed schemas collections
        var schemasDeclaredCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("schemas_declared");
        var declaredSchemas = new[]
        {
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "schema:declared:users" },
                { "CollectionName", "users" },
                { "Fields", new MongoDB.Bson.BsonArray
                    {
                        new MongoDB.Bson.BsonDocument { { "Name", "Id" }, { "Type", "string" }, { "IsRequired", true } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Email" }, { "Type", "string" }, { "IsRequired", true } }
                    }
                },
                { "LastUpdated", DateTime.UtcNow }
            }
        };

        await schemasDeclaredCollection.InsertManyAsync(declaredSchemas);

        var schemasObservedCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("schemas_observed");
        var observedSchemas = new[]
        {
            new MongoDB.Bson.BsonDocument
            {
                { "_id", "schema:observed:users" },
                { "CollectionName", "users" },
                { "Fields", new MongoDB.Bson.BsonArray
                    {
                        new MongoDB.Bson.BsonDocument { { "Name", "Id" }, { "Type", "string" }, { "IsRequired", true } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Email" }, { "Type", "string" }, { "IsRequired", true } },
                        new MongoDB.Bson.BsonDocument { { "Name", "Name" }, { "Type", "string" }, { "IsRequired", false } }
                    }
                },
                { "LastUpdated", DateTime.UtcNow }
            }
        };

        await schemasObservedCollection.InsertManyAsync(observedSchemas);
    }

    /// <summary>
    /// Clears all test data from the database
    /// </summary>
    public async Task ClearTestDataAsync()
    {
        if (_mongoClient == null) return;

        var database = _mongoClient.GetDatabase("catalog_kb");
        var collections = new[]
        {
            "nodes",
            "collections", 
            "types",
            "edges",
            "schemas_declared",
            "schemas_observed"
        };

        foreach (var collectionName in collections)
        {
            var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
            await collection.DeleteManyAsync(MongoDB.Bson.BsonDocument.Parse("{}"));
        }
    }
}
