using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models;

/// <summary>
/// Structured error response following RFC 7807 (Problem Details)
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// URI reference that identifies the problem type
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Short, human-readable summary of the problem type
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code
    /// </summary>
    [Required]
    public int Status { get; set; }

    /// <summary>
    /// Human-readable explanation specific to this occurrence of the problem
    /// </summary>
    [Required]
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// URI reference that identifies the specific occurrence of the problem
    /// </summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// Application-specific error code
    /// </summary>
    [Required]
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Additional properties for the error
    /// </summary>
    public Dictionary<string, object> Extensions { get; set; } = new();

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Trace identifier for correlation
    /// </summary>
    [Required]
    public string TraceId { get; set; } = string.Empty;
}
