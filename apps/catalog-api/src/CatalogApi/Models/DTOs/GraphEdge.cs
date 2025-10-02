using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.DTOs;

/// <summary>
/// Represents relationships between entities with types like READS, WRITES, REFERS_TO
/// </summary>
public class GraphEdge
{
    /// <summary>
    /// Unique identifier for the edge
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID of the source node
    /// </summary>
    [Required]
    public string SourceNodeId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target node
    /// </summary>
    [Required]
    public string TargetNodeId { get; set; } = string.Empty;

    /// <summary>
    /// Type of edge: "READS", "WRITES", "REFERS_TO"
    /// </summary>
    [Required]
    public string EdgeType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the relationship
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional properties of the edge
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Repository where the relationship is defined
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path where the relationship is defined
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the relationship is defined
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }

    /// <summary>
    /// Commit SHA where the relationship was last modified
    /// </summary>
    [Required]
    public string CommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the edge was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }
}
