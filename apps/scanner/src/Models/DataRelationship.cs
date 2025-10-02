using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents inferred connections between types based on query patterns and $lookup operations.
/// </summary>
public class DataRelationship
{
    /// <summary>
    /// Unique identifier for the data relationship.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Source type reference.
    /// </summary>
    [Required]
    [BsonElement("sourceTypeId")]
    public string SourceTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Target type reference.
    /// </summary>
    [Required]
    [BsonElement("targetTypeId")]
    public string TargetTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Type of relationship (REFERS_TO, LOOKUP, EMBEDDED).
    /// </summary>
    [Required]
    [BsonElement("relationshipType")]
    public RelationshipType RelationshipType { get; set; }

    /// <summary>
    /// Relationship confidence score (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Supporting evidence for the relationship.
    /// </summary>
    [BsonElement("evidence")]
    public List<RelationshipEvidence> Evidence { get; set; } = new();

    /// <summary>
    /// Field path that establishes the relationship.
    /// </summary>
    [BsonElement("fieldPath")]
    public string? FieldPath { get; set; }

    /// <summary>
    /// Whether this relationship is bidirectional.
    /// </summary>
    [BsonElement("isBidirectional")]
    public bool IsBidirectional { get; set; }

    /// <summary>
    /// Cardinality of the relationship (one-to-one, one-to-many, many-to-many).
    /// </summary>
    [BsonElement("cardinality")]
    public RelationshipCardinality Cardinality { get; set; }

    /// <summary>
    /// Whether this relationship is required.
    /// </summary>
    [BsonElement("isRequired")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Source information (repository, file, symbol, line span, commit SHA, timestamp).
    /// </summary>
    [Required]
    [BsonElement("provenance")]
    public ProvenanceRecord Provenance { get; set; } = new();

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
/// Represents the type of relationship between entities.
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// One entity refers to another (foreign key relationship).
    /// </summary>
    REFERS_TO,

    /// <summary>
    /// Relationship established through $lookup operation.
    /// </summary>
    LOOKUP,

    /// <summary>
    /// One entity is embedded within another.
    /// </summary>
    EMBEDDED,

    /// <summary>
    /// Entities are related through inheritance.
    /// </summary>
    INHERITANCE,

    /// <summary>
    /// Entities are related through composition.
    /// </summary>
    COMPOSITION,

    /// <summary>
    /// Entities are related through aggregation.
    /// </summary>
    AGGREGATION
}

/// <summary>
/// Represents the cardinality of a relationship.
/// </summary>
public enum RelationshipCardinality
{
    /// <summary>
    /// One-to-one relationship.
    /// </summary>
    OneToOne,

    /// <summary>
    /// One-to-many relationship.
    /// </summary>
    OneToMany,

    /// <summary>
    /// Many-to-one relationship.
    /// </summary>
    ManyToOne,

    /// <summary>
    /// Many-to-many relationship.
    /// </summary>
    ManyToMany,

    /// <summary>
    /// Cardinality is unknown.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents supporting evidence for a relationship.
/// </summary>
public class RelationshipEvidence
{
    /// <summary>
    /// Type of evidence (filter, lookup, naming_convention, etc.).
    /// </summary>
    [Required]
    [BsonElement("evidenceType")]
    public EvidenceType EvidenceType { get; set; }

    /// <summary>
    /// Description of the evidence.
    /// </summary>
    [Required]
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score for this piece of evidence (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Source location of the evidence.
    /// </summary>
    [BsonElement("sourceLocation")]
    public SourceLocation? SourceLocation { get; set; }

    /// <summary>
    /// Additional evidence data.
    /// </summary>
    [BsonElement("data")]
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Represents the type of evidence supporting a relationship.
/// </summary>
public enum EvidenceType
{
    /// <summary>
    /// Evidence from filter expressions.
    /// </summary>
    Filter,

    /// <summary>
    /// Evidence from $lookup operations.
    /// </summary>
    Lookup,

    /// <summary>
    /// Evidence from naming conventions.
    /// </summary>
    NamingConvention,

    /// <summary>
    /// Evidence from field types.
    /// </summary>
    FieldType,

    /// <summary>
    /// Evidence from BSON attributes.
    /// </summary>
    BSONAttribute,

    /// <summary>
    /// Evidence from aggregation pipelines.
    /// </summary>
    Aggregation,

    /// <summary>
    /// Evidence from code comments or documentation.
    /// </summary>
    Documentation,

    /// <summary>
    /// Evidence from observed data patterns.
    /// </summary>
    DataPattern
}

/// <summary>
/// Represents the source location of evidence.
/// </summary>
public class SourceLocation
{
    /// <summary>
    /// File path where the evidence was found.
    /// </summary>
    [Required]
    [BsonElement("filePath")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the evidence was found.
    /// </summary>
    [BsonElement("lineNumber")]
    public int LineNumber { get; set; }

    /// <summary>
    /// Column number where the evidence was found.
    /// </summary>
    [BsonElement("columnNumber")]
    public int ColumnNumber { get; set; }

    /// <summary>
    /// Symbol name where the evidence was found.
    /// </summary>
    [BsonElement("symbolName")]
    public string? SymbolName { get; set; }

    /// <summary>
    /// Method name where the evidence was found.
    /// </summary>
    [BsonElement("methodName")]
    public string? MethodName { get; set; }
}
