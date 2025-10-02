using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.DTOs;

/// <summary>
/// Represents a node in the knowledge graph with connections to other entities
/// </summary>
public class GraphNode
{
    /// <summary>
    /// Unique identifier for the node
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity this node represents
    /// </summary>
    [Required]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Name of the entity
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the entity
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional properties of the node
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Incoming edges to this node
    /// </summary>
    [Required]
    public List<GraphEdge> IncomingEdges { get; set; } = new();

    /// <summary>
    /// Outgoing edges from this node
    /// </summary>
    [Required]
    public List<GraphEdge> OutgoingEdges { get; set; } = new();

    /// <summary>
    /// Repository where the entity is defined
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path where the entity is defined
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the entity is defined
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }

    /// <summary>
    /// Commit SHA where the entity was last modified
    /// </summary>
    [Required]
    public string CommitSha { get; set; } = string.Empty;
}
