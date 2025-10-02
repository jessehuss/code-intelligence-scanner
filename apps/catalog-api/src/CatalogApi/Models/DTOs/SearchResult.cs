using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.DTOs;

/// <summary>
/// Represents a search result with entity type, relevance score, and summary data
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Unique identifier for the search result
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity: "type", "collection", "field", "query", "service", "endpoint"
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
    /// Relevance score between 0.0 and 1.0
    /// </summary>
    [Range(0.0, 1.0)]
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Additional metadata about the entity
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

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

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    [Required]
    public DateTime LastModified { get; set; }
}
