using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.Requests;

/// <summary>
/// Request model for graph traversal operations
/// </summary>
public class GraphRequest
{
    /// <summary>
    /// Node to start traversal from (format: "collection:vendors" or "type:MyApp.Models.User")
    /// </summary>
    [Required]
    [RegularExpression("^(type|collection):[a-zA-Z0-9._-]+$")]
    public string Node { get; set; } = string.Empty;

    /// <summary>
    /// Maximum depth for traversal
    /// </summary>
    [Range(1, 5)]
    public int Depth { get; set; } = 2;

    /// <summary>
    /// Edge types to include: "READS", "WRITES", "REFERS_TO"
    /// </summary>
    public List<string> EdgeKinds { get; set; } = new();

    /// <summary>
    /// Maximum number of nodes to return
    /// </summary>
    [Range(10, 1000)]
    public int MaxNodes { get; set; } = 100;

    /// <summary>
    /// Whether to include node properties
    /// </summary>
    public bool IncludeProperties { get; set; } = false;
}
