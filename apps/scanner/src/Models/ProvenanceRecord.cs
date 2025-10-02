using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents the source information for any extracted fact.
/// </summary>
public class ProvenanceRecord
{
    /// <summary>
    /// Repository name or URL.
    /// </summary>
    [Required]
    [BsonElement("repository")]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path within repository.
    /// </summary>
    [Required]
    [BsonElement("filePath")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Symbol name.
    /// </summary>
    [Required]
    [BsonElement("symbol")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Line number range (start, end).
    /// </summary>
    [Required]
    [BsonElement("lineSpan")]
    public LineSpan LineSpan { get; set; } = new();

    /// <summary>
    /// Git commit SHA.
    /// </summary>
    [Required]
    [BsonElement("commitSHA")]
    public string CommitSHA { get; set; } = string.Empty;

    /// <summary>
    /// Extraction timestamp.
    /// </summary>
    [Required]
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version of the extraction tool.
    /// </summary>
    [BsonElement("extractorVersion")]
    public string ExtractorVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Branch name where the extraction was performed.
    /// </summary>
    [BsonElement("branchName")]
    public string? BranchName { get; set; }

    /// <summary>
    /// Tag name if extraction was performed on a specific tag.
    /// </summary>
    [BsonElement("tagName")]
    public string? TagName { get; set; }

    /// <summary>
    /// Whether the repository was in a clean state during extraction.
    /// </summary>
    [BsonElement("isCleanState")]
    public bool IsCleanState { get; set; } = true;

    /// <summary>
    /// Additional metadata about the extraction context.
    /// </summary>
    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// When this record was created.
    /// </summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated.
    /// </summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a line number range.
/// </summary>
public class LineSpan
{
    /// <summary>
    /// Start line number.
    /// </summary>
    [Required]
    [BsonElement("start")]
    public int Start { get; set; }

    /// <summary>
    /// End line number.
    /// </summary>
    [Required]
    [BsonElement("end")]
    public int End { get; set; }

    /// <summary>
    /// Whether the line span is valid.
    /// </summary>
    [BsonIgnore]
    public bool IsValid => Start > 0 && End >= Start;

    /// <summary>
    /// Number of lines in the span.
    /// </summary>
    [BsonIgnore]
    public int LineCount => IsValid ? End - Start + 1 : 0;
}
