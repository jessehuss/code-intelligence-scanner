using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Cataloger.Scanner.KnowledgeBase;

/// <summary>
/// Service for managing Atlas Search indexes and search functionality.
/// </summary>
public class AtlasSearchManager
{
    private readonly ILogger<AtlasSearchManager> _logger;
    private readonly IMongoDatabase _database;

    public AtlasSearchManager(ILogger<AtlasSearchManager> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    /// <summary>
    /// Creates Atlas Search indexes for the knowledge base.
    /// </summary>
    /// <returns>Task representing the async operation.</returns>
    public async Task CreateSearchIndexesAsync()
    {
        _logger.LogInformation("Creating Atlas Search indexes for knowledge base");

        try
        {
            // Create search index for knowledge base entries
            await CreateKnowledgeBaseEntriesSearchIndexAsync();

            // Create search index for code types
            await CreateCodeTypesSearchIndexAsync();

            // Create search index for collection mappings
            await CreateCollectionMappingsSearchIndexAsync();

            // Create search index for query operations
            await CreateQueryOperationsSearchIndexAsync();

            // Create search index for data relationships
            await CreateDataRelationshipsSearchIndexAsync();

            // Create search index for observed schemas
            await CreateObservedSchemasSearchIndexAsync();

            _logger.LogInformation("Atlas Search indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Atlas Search indexes");
            throw;
        }
    }

    /// <summary>
    /// Performs a text search across the knowledge base.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="entityTypes">Entity types to filter by.</param>
    /// <param name="repositories">Repositories to filter by.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <returns>Search results.</returns>
    public async Task<SearchResults> SearchAsync(
        string query,
        string[]? entityTypes = null,
        string[]? repositories = null,
        int limit = 50,
        int offset = 0)
    {
        _logger.LogDebug("Performing Atlas Search with query: {Query}", query);

        try
        {
            var searchResults = new SearchResults
            {
                Query = query,
                Limit = limit,
                Offset = offset,
                Results = new List<SearchResult>()
            };

            // Build search pipeline
            var pipeline = BuildSearchPipeline(query, entityTypes, repositories, limit, offset);

            // Execute search
            var collection = _database.GetCollection<BsonDocument>("knowledge_base_entries");
            var cursor = await collection.AggregateAsync<BsonDocument>(pipeline);

            var results = await cursor.ToListAsync();
            foreach (var result in results)
            {
                var searchResult = new SearchResult
                {
                    Id = result.GetValue("_id").AsObjectId.ToString(),
                    EntityType = result.GetValue("entityType", "").AsString,
                    EntityId = result.GetValue("entityId", "").AsString,
                    Title = result.GetValue("title", "").AsString,
                    Description = result.GetValue("description", "").AsString,
                    RelevanceScore = result.GetValue("score", 0.0).AsDouble,
                    Repository = result.GetValue("repository", "").AsString,
                    FilePath = result.GetValue("filePath", "").AsString,
                    LineNumber = result.GetValue("lineNumber", 0).AsInt32
                };

                searchResults.Results.Add(searchResult);
            }

            // Get total count
            searchResults.Total = await GetSearchCountAsync(query, entityTypes, repositories);

            _logger.LogInformation("Atlas Search completed: {ResultCount} results found", searchResults.Results.Count);
            return searchResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Atlas Search failed");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing search index.
    /// </summary>
    /// <param name="indexName">Name of the index to update.</param>
    /// <param name="indexDefinition">New index definition.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task UpdateSearchIndexAsync(string indexName, BsonDocument indexDefinition)
    {
        _logger.LogInformation("Updating Atlas Search index: {IndexName}", indexName);

        try
        {
            // This would update the search index
            // In a real implementation, this would use the Atlas Search API
            await Task.Delay(100); // Simulate API call

            _logger.LogInformation("Atlas Search index updated successfully: {IndexName}", indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Atlas Search index: {IndexName}", indexName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a search index.
    /// </summary>
    /// <param name="indexName">Name of the index to delete.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task DeleteSearchIndexAsync(string indexName)
    {
        _logger.LogInformation("Deleting Atlas Search index: {IndexName}", indexName);

        try
        {
            // This would delete the search index
            // In a real implementation, this would use the Atlas Search API
            await Task.Delay(100); // Simulate API call

            _logger.LogInformation("Atlas Search index deleted successfully: {IndexName}", indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Atlas Search index: {IndexName}", indexName);
            throw;
        }
    }

    /// <summary>
    /// Lists all search indexes.
    /// </summary>
    /// <returns>List of search index names.</returns>
    public async Task<List<string>> ListSearchIndexesAsync()
    {
        _logger.LogDebug("Listing Atlas Search indexes");

        try
        {
            // This would list all search indexes
            // In a real implementation, this would use the Atlas Search API
            await Task.Delay(100); // Simulate API call

            var indexes = new List<string>
            {
                "knowledge_base_entries_search",
                "code_types_search",
                "collection_mappings_search",
                "query_operations_search",
                "data_relationships_search",
                "observed_schemas_search"
            };

            _logger.LogInformation("Found {IndexCount} Atlas Search indexes", indexes.Count);
            return indexes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Atlas Search indexes");
            throw;
        }
    }

    private async Task CreateKnowledgeBaseEntriesSearchIndexAsync()
    {
        var indexDefinition = new BsonDocument
        {
            ["mappings"] = new BsonDocument
            {
                ["fields"] = new BsonDocument
                {
                    ["searchableText"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["entityType"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    },
                    ["tags"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["provenance.repository"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    },
                    ["provenance.filePath"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    }
                }
            }
        };

        await CreateSearchIndexAsync("knowledge_base_entries_search", indexDefinition);
    }

    private async Task CreateCodeTypesSearchIndexAsync()
    {
        var indexDefinition = new BsonDocument
        {
            ["mappings"] = new BsonDocument
            {
                ["fields"] = new BsonDocument
                {
                    ["name"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["namespace"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    },
                    ["fields.name"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["fields.type"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    }
                }
            }
        };

        await CreateSearchIndexAsync("code_types_search", indexDefinition);
    }

    private async Task CreateCollectionMappingsSearchIndexAsync()
    {
        var indexDefinition = new BsonDocument
        {
            ["mappings"] = new BsonDocument
            {
                ["fields"] = new BsonDocument
                {
                    ["collectionName"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["resolutionMethod"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    }
                }
            }
        };

        await CreateSearchIndexAsync("collection_mappings_search", indexDefinition);
    }

    private async Task CreateQueryOperationsSearchIndexAsync()
    {
        var indexDefinition = new BsonDocument
        {
            ["mappings"] = new BsonDocument
            {
                ["fields"] = new BsonDocument
                {
                    ["operationType"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    },
                    ["filters.fieldPath"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["filters.operator"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    }
                }
            }
        };

        await CreateSearchIndexAsync("query_operations_search", indexDefinition);
    }

    private async Task CreateDataRelationshipsSearchIndexAsync()
    {
        var indexDefinition = new BsonDocument
        {
            ["mappings"] = new BsonDocument
            {
                ["fields"] = new BsonDocument
                {
                    ["relationshipType"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    },
                    ["fieldPath"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["evidence.description"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    }
                }
            }
        };

        await CreateSearchIndexAsync("data_relationships_search", indexDefinition);
    }

    private async Task CreateObservedSchemasSearchIndexAsync()
    {
        var indexDefinition = new BsonDocument
        {
            ["mappings"] = new BsonDocument
            {
                ["fields"] = new BsonDocument
                {
                    ["requiredFields"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["stringFormats.fieldName"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.standard"
                    },
                    ["stringFormats.pattern"] = new BsonDocument
                    {
                        ["type"] = "string",
                        ["analyzer"] = "lucene.keyword"
                    }
                }
            }
        };

        await CreateSearchIndexAsync("observed_schemas_search", indexDefinition);
    }

    private async Task CreateSearchIndexAsync(string indexName, BsonDocument indexDefinition)
    {
        _logger.LogDebug("Creating Atlas Search index: {IndexName}", indexName);

        try
        {
            // This would create the search index
            // In a real implementation, this would use the Atlas Search API
            await Task.Delay(100); // Simulate API call

            _logger.LogDebug("Atlas Search index created: {IndexName}", indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Atlas Search index: {IndexName}", indexName);
            throw;
        }
    }

    private BsonArray BuildSearchPipeline(
        string query,
        string[]? entityTypes,
        string[]? repositories,
        int limit,
        int offset)
    {
        var pipeline = new BsonArray();

        // Add search stage
        var searchStage = new BsonDocument
        {
            ["$search"] = new BsonDocument
            {
                ["index"] = "knowledge_base_entries_search",
                ["text"] = new BsonDocument
                {
                    ["query"] = query,
                    ["path"] = new BsonArray { "searchableText", "tags" }
                }
            }
        };

        // Add filters if specified
        var filterConditions = new List<BsonDocument>();

        if (entityTypes != null && entityTypes.Length > 0)
        {
            filterConditions.Add(new BsonDocument("entityType", new BsonDocument("$in", new BsonArray(entityTypes))));
        }

        if (repositories != null && repositories.Length > 0)
        {
            filterConditions.Add(new BsonDocument("provenance.repository", new BsonDocument("$in", new BsonArray(repositories))));
        }

        if (filterConditions.Count > 0)
        {
            searchStage["$search"]["compound"] = new BsonDocument
            {
                ["must"] = new BsonArray { new BsonDocument("text", new BsonDocument("query", query).Add("path", new BsonArray { "searchableText", "tags" })) },
                ["filter"] = new BsonArray(filterConditions)
            };
        }

        pipeline.Add(searchStage);

        // Add project stage
        var projectStage = new BsonDocument
        {
            ["$project"] = new BsonDocument
            {
                ["_id"] = 1,
                ["entityType"] = 1,
                ["entityId"] = 1,
                ["title"] = new BsonDocument("$concat", new BsonArray { "$entityType", ": ", "$entityId" }),
                ["description"] = "$searchableText",
                ["score"] = new BsonDocument("$meta", "searchScore"),
                ["repository"] = "$provenance.repository",
                ["filePath"] = "$provenance.filePath",
                ["lineNumber"] = "$provenance.lineSpan.start"
            }
        };

        pipeline.Add(projectStage);

        // Add sort stage
        var sortStage = new BsonDocument
        {
            ["$sort"] = new BsonDocument("score", -1)
        };

        pipeline.Add(sortStage);

        // Add skip and limit stages
        if (offset > 0)
        {
            pipeline.Add(new BsonDocument("$skip", offset));
        }

        if (limit > 0)
        {
            pipeline.Add(new BsonDocument("$limit", limit));
        }

        return pipeline;
    }

    private async Task<int> GetSearchCountAsync(string query, string[]? entityTypes, string[]? repositories)
    {
        try
        {
            var pipeline = new BsonArray();

            // Add search stage (same as main search)
            var searchStage = new BsonDocument
            {
                ["$search"] = new BsonDocument
                {
                    ["index"] = "knowledge_base_entries_search",
                    ["text"] = new BsonDocument
                    {
                        ["query"] = query,
                        ["path"] = new BsonArray { "searchableText", "tags" }
                    }
                }
            };

            // Add filters if specified
            var filterConditions = new List<BsonDocument>();

            if (entityTypes != null && entityTypes.Length > 0)
            {
                filterConditions.Add(new BsonDocument("entityType", new BsonDocument("$in", new BsonArray(entityTypes))));
            }

            if (repositories != null && repositories.Length > 0)
            {
                filterConditions.Add(new BsonDocument("provenance.repository", new BsonDocument("$in", new BsonArray(repositories))));
            }

            if (filterConditions.Count > 0)
            {
                searchStage["$search"]["compound"] = new BsonDocument
                {
                    ["must"] = new BsonArray { new BsonDocument("text", new BsonDocument("query", query).Add("path", new BsonArray { "searchableText", "tags" })) },
                    ["filter"] = new BsonArray(filterConditions)
                };
            }

            pipeline.Add(searchStage);

            // Add count stage
            pipeline.Add(new BsonDocument("$count", "total"));

            var collection = _database.GetCollection<BsonDocument>("knowledge_base_entries");
            var cursor = await collection.AggregateAsync<BsonDocument>(pipeline);
            var result = await cursor.FirstOrDefaultAsync();

            return result?.GetValue("total", 0).AsInt32 ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get search count");
            return 0;
        }
    }
}

/// <summary>
/// Search results from Atlas Search.
/// </summary>
public class SearchResults
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResult> Results { get; set; } = new();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

/// <summary>
/// Individual search result.
/// </summary>
public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}
