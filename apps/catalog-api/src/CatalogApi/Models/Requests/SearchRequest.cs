using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.Requests;

/// <summary>
/// Request model for search operations
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// Search query string
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Entity types to search for: "type", "collection", "field", "query", "service", "endpoint"
    /// </summary>
    public List<string> Kinds { get; set; } = new();

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    [Range(1, 1000)]
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Number of results to skip
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    /// <summary>
    /// Additional filters to apply
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Field to sort by: "relevance", "name", "lastModified"
    /// </summary>
    [RegularExpression("^(relevance|name|lastModified)$")]
    public string SortBy { get; set; } = "relevance";

    /// <summary>
    /// Sort order: "asc" or "desc"
    /// </summary>
    [RegularExpression("^(asc|desc)$")]
    public string SortOrder { get; set; } = "desc";
}
