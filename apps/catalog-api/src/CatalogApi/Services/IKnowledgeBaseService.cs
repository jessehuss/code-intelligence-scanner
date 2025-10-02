using CatalogApi.Models.DTOs;
using CatalogApi.Models.Requests;

namespace CatalogApi.Services;

/// <summary>
/// Service interface for knowledge base operations
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Search the knowledge base for entities
    /// </summary>
    /// <param name="request">Search request parameters</param>
    /// <returns>Search results</returns>
    Task<SearchResponse> SearchAsync(SearchRequest request);

    /// <summary>
    /// Get detailed information about a collection
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>Collection details</returns>
    Task<CollectionDetail?> GetCollectionAsync(string collectionName);

    /// <summary>
    /// Get detailed information about a type
    /// </summary>
    /// <param name="fqcn">Fully qualified name of the type</param>
    /// <returns>Type details</returns>
    Task<TypeDetail?> GetTypeAsync(string fqcn);

    /// <summary>
    /// Get graph data for a node
    /// </summary>
    /// <param name="request">Graph request parameters</param>
    /// <returns>Graph response</returns>
    Task<GraphResponse> GetGraphAsync(GraphRequest request);

    /// <summary>
    /// Get diff between two versions of a type
    /// </summary>
    /// <param name="request">Diff request parameters</param>
    /// <returns>Type diff</returns>
    Task<TypeDiff?> GetDiffAsync(DiffRequest request);
}

/// <summary>
/// Search response containing results and metadata
/// </summary>
public class SearchResponse
{
    public List<SearchResult> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public bool HasMore { get; set; }
    public Dictionary<string, int> ResultCountsByType { get; set; } = new();
    public TimeSpan QueryTime { get; set; }
}

/// <summary>
/// Graph response containing nodes and edges
/// </summary>
public class GraphResponse
{
    public GraphNode CenterNode { get; set; } = new();
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
    public int TotalNodes { get; set; }
    public int TotalEdges { get; set; }
    public TimeSpan QueryTime { get; set; }
}
