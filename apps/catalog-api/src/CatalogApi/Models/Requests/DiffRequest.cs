using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.Requests;

/// <summary>
/// Request model for diff comparison operations
/// </summary>
public class DiffRequest
{
    /// <summary>
    /// Fully qualified name of the type to compare
    /// </summary>
    [Required]
    public string FullyQualifiedName { get; set; } = string.Empty;

    /// <summary>
    /// Source commit SHA for comparison
    /// </summary>
    [Required]
    [RegularExpression("^[a-f0-9]{40}$")]
    public string FromCommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Target commit SHA for comparison
    /// </summary>
    [Required]
    [RegularExpression("^[a-f0-9]{40}$")]
    public string ToCommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include detailed field information
    /// </summary>
    public bool IncludeFieldDetails { get; set; } = true;

    /// <summary>
    /// Whether to include attribute changes
    /// </summary>
    public bool IncludeAttributeChanges { get; set; } = true;
}
