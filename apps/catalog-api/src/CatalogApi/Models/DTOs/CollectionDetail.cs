using System.ComponentModel.DataAnnotations;

namespace CatalogApi.Models.DTOs;

/// <summary>
/// Contains declared schema, observed schema, associated types, queries, and relationships for a collection
/// </summary>
public class CollectionDetail
{
    /// <summary>
    /// Name of the collection
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the collection
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Schema declared in code
    /// </summary>
    [Required]
    public SchemaInfo DeclaredSchema { get; set; } = new();

    /// <summary>
    /// Schema observed from MongoDB data
    /// </summary>
    [Required]
    public SchemaInfo ObservedSchema { get; set; } = new();

    /// <summary>
    /// Types associated with this collection
    /// </summary>
    [Required]
    public List<string> AssociatedTypes { get; set; } = new();

    /// <summary>
    /// Queries that operate on this collection
    /// </summary>
    [Required]
    public List<QueryInfo> RelatedQueries { get; set; } = new();

    /// <summary>
    /// Relationships to other entities
    /// </summary>
    [Required]
    public List<RelationshipInfo> Relationships { get; set; } = new();

    /// <summary>
    /// Whether there is drift between declared and observed schema
    /// </summary>
    public bool HasDrift { get; set; }

    /// <summary>
    /// Flags indicating types of drift
    /// </summary>
    public List<string> DriftFlags { get; set; } = new();

    /// <summary>
    /// Number of documents in the collection
    /// </summary>
    public int DocumentCount { get; set; }

    /// <summary>
    /// Timestamp when the collection was last sampled
    /// </summary>
    public DateTime LastSampled { get; set; }

    /// <summary>
    /// Repository where the collection is defined
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path where the collection is defined
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the collection is defined
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }

    /// <summary>
    /// Commit SHA where the collection was last modified
    /// </summary>
    [Required]
    public string CommitSha { get; set; } = string.Empty;
}

/// <summary>
/// Schema information for a collection
/// </summary>
public class SchemaInfo
{
    /// <summary>
    /// Fields in the schema
    /// </summary>
    [Required]
    public List<FieldInfo> Fields { get; set; } = new();

    /// <summary>
    /// Required fields
    /// </summary>
    [Required]
    public List<string> RequiredFields { get; set; } = new();

    /// <summary>
    /// Schema constraints
    /// </summary>
    public Dictionary<string, object> Constraints { get; set; } = new();

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [Required]
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Field information in a schema
/// </summary>
public class FieldInfo
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
    /// BSON attributes on the field
    /// </summary>
    public List<string> Attributes { get; set; } = new();

    /// <summary>
    /// Description of the field
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Query information for a collection
/// </summary>
public class QueryInfo
{
    /// <summary>
    /// Type of operation: "Find", "Update", "Insert", "Delete", "Aggregate"
    /// </summary>
    [Required]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Filter expression
    /// </summary>
    public string Filter { get; set; } = string.Empty;

    /// <summary>
    /// Projection expression
    /// </summary>
    public string Projection { get; set; } = string.Empty;

    /// <summary>
    /// Repository where the query is defined
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// File path where the query is defined
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the query is defined
    /// </summary>
    [Range(1, int.MaxValue)]
    public int LineNumber { get; set; }
}

/// <summary>
/// Relationship information for a collection
/// </summary>
public class RelationshipInfo
{
    /// <summary>
    /// Type of relationship: "READS", "WRITES", "REFERS_TO"
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Target entity of the relationship
    /// </summary>
    [Required]
    public string TargetEntity { get; set; } = string.Empty;

    /// <summary>
    /// Description of the relationship
    /// </summary>
    public string Description { get; set; } = string.Empty;

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
}
