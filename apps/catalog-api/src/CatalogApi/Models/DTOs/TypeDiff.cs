using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.DTOs;

/// <summary>
/// Contains comparison data showing changes between two versions of a type definition
/// </summary>
public class TypeDiff
{
    /// <summary>
    /// Fully qualified name of the type
    /// </summary>
    [Required]
    public string FullyQualifiedName { get; set; } = string.Empty;

    /// <summary>
    /// Source commit SHA for comparison
    /// </summary>
    [Required]
    public string FromCommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Target commit SHA for comparison
    /// </summary>
    [Required]
    public string ToCommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Fields that were added
    /// </summary>
    [Required]
    public List<FieldChange> AddedFields { get; set; } = new();

    /// <summary>
    /// Fields that were removed
    /// </summary>
    [Required]
    public List<FieldChange> RemovedFields { get; set; } = new();

    /// <summary>
    /// Fields that were modified
    /// </summary>
    [Required]
    public List<FieldChange> ModifiedFields { get; set; } = new();

    /// <summary>
    /// Attribute changes
    /// </summary>
    [Required]
    public List<AttributeChange> AttributeChanges { get; set; } = new();

    /// <summary>
    /// Timestamp when the diff was generated
    /// </summary>
    [Required]
    public DateTime DiffGeneratedAt { get; set; }

    /// <summary>
    /// Repository where the type is defined
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path where the type is defined
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Represents a change to a field
/// </summary>
public class FieldChange
{
    /// <summary>
    /// Name of the field
    /// </summary>
    [Required]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Type of the field
    /// </summary>
    [Required]
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// Type of change: "Added", "Removed", "Modified"
    /// </summary>
    [Required]
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Old value of the field
    /// </summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>
    /// New value of the field
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Description of the change
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Represents a change to an attribute
/// </summary>
public class AttributeChange
{
    /// <summary>
    /// Name of the attribute
    /// </summary>
    [Required]
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Type of change: "Added", "Removed", "Modified"
    /// </summary>
    [Required]
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Old value of the attribute
    /// </summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>
    /// New value of the attribute
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Description of the change
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
