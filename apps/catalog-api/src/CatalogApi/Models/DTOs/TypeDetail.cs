using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.DTOs;

/// <summary>
/// Contains field definitions, BSON attributes, collection mappings, usage statistics, and change history for a code type
/// </summary>
public class TypeDetail
{
    /// <summary>
    /// Fully qualified name of the type
    /// </summary>
    [Required]
    public string FullyQualifiedName { get; set; } = string.Empty;

    /// <summary>
    /// Simple name of the type
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Namespace of the type
    /// </summary>
    [Required]
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Description of the type
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Fields in the type
    /// </summary>
    [Required]
    public List<FieldDetail> Fields { get; set; } = new();

    /// <summary>
    /// BSON attributes on the type
    /// </summary>
    [Required]
    public List<string> BsonAttributes { get; set; } = new();

    /// <summary>
    /// Collection mappings for this type
    /// </summary>
    [Required]
    public List<CollectionMapping> CollectionMappings { get; set; } = new();

    /// <summary>
    /// Usage statistics for this type
    /// </summary>
    [Required]
    public UsageStatistics UsageStats { get; set; } = new();

    /// <summary>
    /// Change summary for this type
    /// </summary>
    [Required]
    public ChangeSummary ChangeSummary { get; set; } = new();

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

    /// <summary>
    /// Line number where the type is defined
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }

    /// <summary>
    /// Commit SHA where the type was last modified
    /// </summary>
    [Required]
    public string CommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    [Required]
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Field detail information
/// </summary>
public class FieldDetail
{
    /// <summary>
    /// Name of the field
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of the field
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the field is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether the field is nullable
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Attributes on the field
    /// </summary>
    public List<string> Attributes { get; set; } = new();

    /// <summary>
    /// Description of the field
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Default value of the field
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Validation rules for the field
    /// </summary>
    public List<string> ValidationRules { get; set; } = new();
}

/// <summary>
/// Collection mapping information
/// </summary>
public class CollectionMapping
{
    /// <summary>
    /// Name of the collection
    /// </summary>
    [Required]
    public string CollectionName { get; set; } = string.Empty;

    /// <summary>
    /// Type of mapping: "Primary", "Secondary", "Reference"
    /// </summary>
    [Required]
    public string MappingType { get; set; } = string.Empty;

    /// <summary>
    /// Repository where the mapping is defined
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path where the mapping is defined
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the mapping is defined
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }
}

/// <summary>
/// Usage statistics for a type
/// </summary>
public class UsageStatistics
{
    /// <summary>
    /// Number of queries using this type
    /// </summary>
    public int QueryCount { get; set; }

    /// <summary>
    /// Number of repositories using this type
    /// </summary>
    public int RepositoryCount { get; set; }

    /// <summary>
    /// List of repositories using this type
    /// </summary>
    [Required]
    public List<string> UsedInRepositories { get; set; } = new();

    /// <summary>
    /// Last time this type was used
    /// </summary>
    [Required]
    public DateTime LastUsed { get; set; }

    /// <summary>
    /// Common operations performed on this type
    /// </summary>
    [Required]
    public List<string> CommonOperations { get; set; } = new();
}

/// <summary>
/// Change summary for a type
/// </summary>
public class ChangeSummary
{
    /// <summary>
    /// Total number of changes
    /// </summary>
    public int TotalChanges { get; set; }

    /// <summary>
    /// Number of added fields
    /// </summary>
    public int AddedFields { get; set; }

    /// <summary>
    /// Number of removed fields
    /// </summary>
    public int RemovedFields { get; set; }

    /// <summary>
    /// Number of modified fields
    /// </summary>
    public int ModifiedFields { get; set; }

    /// <summary>
    /// Last change timestamp
    /// </summary>
    [Required]
    public DateTime LastChange { get; set; }

    /// <summary>
    /// Recent commits that modified this type
    /// </summary>
    [Required]
    public List<string> RecentCommits { get; set; } = new();
}
