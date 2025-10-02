using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;
using MongoDB.Driver;
using System.Diagnostics;

namespace CatalogApi.Services;

/// <summary>
/// Service implementation for knowledge base operations using MongoDB
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly IMongoDatabase _database;
    private readonly IObservabilityService _observability;

    public KnowledgeBaseService(IMongoDatabase database, IObservabilityService observability)
    {
        _database = database;
        _observability = observability;
    }

    public async Task<SearchResponse> SearchAsync(SearchRequest request)
    {
        using var activity = _observability.StartActivity("KnowledgeBaseService.Search");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _observability.LogInformation("Starting search", new Dictionary<string, object>
            {
                ["query"] = request.Query,
                ["kinds"] = request.Kinds,
                ["limit"] = request.Limit,
                ["offset"] = request.Offset
            });

            // Get the search collection
            var searchCollection = _database.GetCollection<SearchResult>("kb_search");

            // Build the filter
            var filterBuilder = Builders<SearchResult>.Filter;
            var filters = new List<FilterDefinition<SearchResult>>();

            // Text search
            if (!string.IsNullOrEmpty(request.Query))
            {
                filters.Add(filterBuilder.Text(request.Query));
            }

            // Entity type filter
            if (request.Kinds.Any())
            {
                filters.Add(filterBuilder.In(x => x.EntityType, request.Kinds));
            }

            // Additional filters
            if (request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    switch (filter.Key.ToLower())
                    {
                        case "repository":
                            if (filter.Value is string repo)
                                filters.Add(filterBuilder.Eq(x => x.Repository, repo));
                            break;
                        case "filepath":
                            if (filter.Value is string filePath)
                                filters.Add(filterBuilder.Regex(x => x.FilePath, $"^{filePath}"));
                            break;
                    }
                }
            }

            var finalFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

            // Build sort
            SortDefinition<SearchResult> sort;
            switch (request.SortBy.ToLower())
            {
                case "name":
                    sort = request.SortOrder.ToLower() == "asc" 
                        ? Builders<SearchResult>.Sort.Ascending(x => x.Name)
                        : Builders<SearchResult>.Sort.Descending(x => x.Name);
                    break;
                case "lastmodified":
                    sort = request.SortOrder.ToLower() == "asc"
                        ? Builders<SearchResult>.Sort.Ascending(x => x.LastModified)
                        : Builders<SearchResult>.Sort.Descending(x => x.LastModified);
                    break;
                default: // relevance
                    sort = Builders<SearchResult>.Sort.Descending(x => x.RelevanceScore);
                    break;
            }

            // Execute search
            var results = await searchCollection
                .Find(finalFilter)
                .Sort(sort)
                .Skip(request.Offset)
                .Limit(request.Limit)
                .ToListAsync();

            // Get total count
            var totalCount = await searchCollection.CountDocumentsAsync(finalFilter);

            // Group results by type
            var resultCountsByType = results
                .GroupBy(r => r.EntityType)
                .ToDictionary(g => g.Key, g => g.Count());

            stopwatch.Stop();

            var response = new SearchResponse
            {
                Results = results,
                TotalCount = (int)totalCount,
                Limit = request.Limit,
                Offset = request.Offset,
                HasMore = request.Offset + results.Count < totalCount,
                ResultCountsByType = resultCountsByType,
                QueryTime = stopwatch.Elapsed
            };

            _observability.LogInformation("Search completed", new Dictionary<string, object>
            {
                ["resultCount"] = results.Count,
                ["totalCount"] = totalCount,
                ["queryTime"] = stopwatch.ElapsedMilliseconds
            });

            return response;
        }
        catch (Exception ex)
        {
            _observability.LogError("Search failed", ex);
            throw;
        }
    }

    public async Task<CollectionDetail?> GetCollectionAsync(string collectionName)
    {
        using var activity = _observability.StartActivity("KnowledgeBaseService.GetCollection");
        
        try
        {
            _observability.LogInformation("Getting collection details", new Dictionary<string, object>
            {
                ["collectionName"] = collectionName
            });

            var collectionsCollection = _database.GetCollection<CollectionDetail>("collections");
            var collection = await collectionsCollection
                .Find(c => c.Name == collectionName)
                .FirstOrDefaultAsync();

            if (collection == null)
            {
                _observability.LogWarning("Collection not found", properties: new Dictionary<string, object>
                {
                    ["collectionName"] = collectionName
                });
                return null;
            }

            _observability.LogInformation("Collection details retrieved");
            return collection;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get collection details", ex);
            throw;
        }
    }

    public async Task<TypeDetail?> GetTypeAsync(string fqcn)
    {
        using var activity = _observability.StartActivity("KnowledgeBaseService.GetType");
        
        try
        {
            _observability.LogInformation("Getting type details", new Dictionary<string, object>
            {
                ["fqcn"] = fqcn
            });

            var typesCollection = _database.GetCollection<TypeDetail>("types");
            var type = await typesCollection
                .Find(t => t.FullyQualifiedName == fqcn)
                .FirstOrDefaultAsync();

            if (type == null)
            {
                _observability.LogWarning("Type not found", properties: new Dictionary<string, object>
                {
                    ["fqcn"] = fqcn
                });
                return null;
            }

            _observability.LogInformation("Type details retrieved");
            return type;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get type details", ex);
            throw;
        }
    }

    public async Task<GraphResponse> GetGraphAsync(GraphRequest request)
    {
        using var activity = _observability.StartActivity("KnowledgeBaseService.GetGraph");
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _observability.LogInformation("Getting graph data", new Dictionary<string, object>
            {
                ["node"] = request.Node,
                ["depth"] = request.Depth,
                ["maxNodes"] = request.MaxNodes
            });

            // Parse node type and name
            var nodeParts = request.Node.Split(':', 2);
            if (nodeParts.Length != 2)
            {
                throw new ArgumentException("Invalid node format. Expected 'type:name' or 'collection:name'");
            }

            var nodeType = nodeParts[0];
            var nodeName = nodeParts[1];

            // Get nodes collection
            var nodesCollection = _database.GetCollection<GraphNode>("graph_nodes");
            var edgesCollection = _database.GetCollection<GraphEdge>("graph_edges");

            // Find center node
            var centerNode = await nodesCollection
                .Find(n => n.EntityType == nodeType && n.Name == nodeName)
                .FirstOrDefaultAsync();

            if (centerNode == null)
            {
                _observability.LogWarning("Center node not found", properties: new Dictionary<string, object>
                {
                    ["node"] = request.Node
                });
                return new GraphResponse
                {
                    CenterNode = new GraphNode(),
                    Nodes = new List<GraphNode>(),
                    Edges = new List<GraphEdge>(),
                    TotalNodes = 0,
                    TotalEdges = 0,
                    QueryTime = stopwatch.Elapsed
                };
            }

            // Get connected nodes and edges using aggregation
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument("_id", centerNode.Id)),
                new BsonDocument("$graphLookup", new BsonDocument
                {
                    { "from", "graph_edges" },
                    { "startWith", "$_id" },
                    { "connectFromField", "_id" },
                    { "connectToField", "targetNodeId" },
                    { "as", "edges" },
                    { "maxDepth", request.Depth }
                })
            };

            var result = await nodesCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            
            var allNodes = new List<GraphNode> { centerNode };
            var allEdges = new List<GraphEdge>();

            if (result != null && result.Contains("edges"))
            {
                var edges = result["edges"].AsBsonArray;
                foreach (var edgeDoc in edges)
                {
                    var edge = BsonSerializer.Deserialize<GraphEdge>(edgeDoc.AsBsonDocument);
                    allEdges.Add(edge);

                    // Get target node
                    var targetNode = await nodesCollection
                        .Find(n => n.Id == edge.TargetNodeId)
                        .FirstOrDefaultAsync();
                    
                    if (targetNode != null && !allNodes.Any(n => n.Id == targetNode.Id))
                    {
                        allNodes.Add(targetNode);
                    }
                }
            }

            // Apply edge kind filter
            if (request.EdgeKinds.Any())
            {
                allEdges = allEdges.Where(e => request.EdgeKinds.Contains(e.EdgeType)).ToList();
            }

            // Apply max nodes limit
            if (allNodes.Count > request.MaxNodes)
            {
                allNodes = allNodes.Take(request.MaxNodes).ToList();
            }

            stopwatch.Stop();

            var response = new GraphResponse
            {
                CenterNode = centerNode,
                Nodes = allNodes,
                Edges = allEdges,
                TotalNodes = allNodes.Count,
                TotalEdges = allEdges.Count,
                QueryTime = stopwatch.Elapsed
            };

            _observability.LogInformation("Graph data retrieved", new Dictionary<string, object>
            {
                ["nodeCount"] = allNodes.Count,
                ["edgeCount"] = allEdges.Count,
                ["queryTime"] = stopwatch.ElapsedMilliseconds
            });

            return response;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get graph data", ex);
            throw;
        }
    }

    public async Task<TypeDiff?> GetDiffAsync(DiffRequest request)
    {
        using var activity = _observability.StartActivity("KnowledgeBaseService.GetDiff");
        
        try
        {
            _observability.LogInformation("Getting type diff", new Dictionary<string, object>
            {
                ["fqcn"] = request.FullyQualifiedName,
                ["fromSha"] = request.FromCommitSha,
                ["toSha"] = request.ToCommitSha
            });

            var diffsCollection = _database.GetCollection<TypeDiff>("type_diffs");
            var diff = await diffsCollection
                .Find(d => d.FullyQualifiedName == request.FullyQualifiedName &&
                          d.FromCommitSha == request.FromCommitSha &&
                          d.ToCommitSha == request.ToCommitSha)
                .FirstOrDefaultAsync();

            if (diff == null)
            {
                _observability.LogWarning("Type diff not found", properties: new Dictionary<string, object>
                {
                    ["fqcn"] = request.FullyQualifiedName,
                    ["fromSha"] = request.FromCommitSha,
                    ["toSha"] = request.ToCommitSha
                });
                return null;
            }

            // Filter results based on request options
            if (!request.IncludeFieldDetails)
            {
                diff.AddedFields = new List<FieldChange>();
                diff.RemovedFields = new List<FieldChange>();
                diff.ModifiedFields = new List<FieldChange>();
            }

            if (!request.IncludeAttributeChanges)
            {
                diff.AttributeChanges = new List<AttributeChange>();
            }

            _observability.LogInformation("Type diff retrieved");
            return diff;
        }
        catch (Exception ex)
        {
            _observability.LogError("Failed to get type diff", ex);
            throw;
        }
    }
}
