using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents a MongoDB operation (Find, Update, etc.) with its filters, projections, and target collection.
/// </summary>
public class QueryOperation
{
    /// <summary>
    /// Unique identifier for the query operation.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of operation (Find, Update, Aggregate, Replace, Delete).
    /// </summary>
    [Required]
    [BsonElement("operationType")]
    public OperationType OperationType { get; set; }

    /// <summary>
    /// Reference to Collection Mapping.
    /// </summary>
    [Required]
    [BsonElement("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    /// <summary>
    /// Query filter expressions.
    /// </summary>
    [BsonElement("filters")]
    public List<FilterExpression> Filters { get; set; } = new();

    /// <summary>
    /// Field projection specifications.
    /// </summary>
    [BsonElement("projections")]
    public List<ProjectionExpression> Projections { get; set; } = new();

    /// <summary>
    /// Sort specifications.
    /// </summary>
    [BsonElement("sort")]
    public List<SortExpression> Sort { get; set; } = new();

    /// <summary>
    /// Result limit.
    /// </summary>
    [BsonElement("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Result skip count.
    /// </summary>
    [BsonElement("skip")]
    public int? Skip { get; set; }

    /// <summary>
    /// Aggregation pipeline for aggregate operations.
    /// </summary>
    [BsonElement("aggregationPipeline")]
    public List<AggregationStage>? AggregationPipeline { get; set; }

    /// <summary>
    /// Whether this operation is part of a transaction.
    /// </summary>
    [BsonElement("isTransactional")]
    public bool IsTransactional { get; set; }

    /// <summary>
    /// Whether this operation uses read preferences.
    /// </summary>
    [BsonElement("hasReadPreference")]
    public bool HasReadPreference { get; set; }

    /// <summary>
    /// Whether this operation uses write concerns.
    /// </summary>
    [BsonElement("hasWriteConcern")]
    public bool HasWriteConcern { get; set; }

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
/// Represents the type of MongoDB operation.
/// </summary>
public enum OperationType
{
    /// <summary>
    /// Find operation.
    /// </summary>
    Find,

    /// <summary>
    /// Insert operation.
    /// </summary>
    Insert,

    /// <summary>
    /// Update operation.
    /// </summary>
    Update,

    /// <summary>
    /// Replace operation.
    /// </summary>
    Replace,

    /// <summary>
    /// Delete operation.
    /// </summary>
    Delete,

    /// <summary>
    /// Aggregate operation.
    /// </summary>
    Aggregate,

    /// <summary>
    /// Count operation.
    /// </summary>
    Count,

    /// <summary>
    /// Distinct operation.
    /// </summary>
    Distinct,

    /// <summary>
    /// FindOneAndUpdate operation.
    /// </summary>
    FindOneAndUpdate,

    /// <summary>
    /// FindOneAndReplace operation.
    /// </summary>
    FindOneAndReplace,

    /// <summary>
    /// FindOneAndDelete operation.
    /// </summary>
    FindOneAndDelete
}

/// <summary>
/// Represents a filter expression in a MongoDB query.
/// </summary>
public class FilterExpression
{
    /// <summary>
    /// Field path (e.g., "user.email", "address.city").
    /// </summary>
    [Required]
    [BsonElement("fieldPath")]
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Filter operator (e.g., "eq", "gt", "in", "regex").
    /// </summary>
    [Required]
    [BsonElement("operator")]
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// Filter value.
    /// </summary>
    [BsonElement("value")]
    public object? Value { get; set; }

    /// <summary>
    /// Whether this filter is negated.
    /// </summary>
    [BsonElement("isNegated")]
    public bool IsNegated { get; set; }

    /// <summary>
    /// Additional filter options.
    /// </summary>
    [BsonElement("options")]
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Represents a projection expression in a MongoDB query.
/// </summary>
public class ProjectionExpression
{
    /// <summary>
    /// Field path (e.g., "user.email", "address.city").
    /// </summary>
    [Required]
    [BsonElement("fieldPath")]
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether this field is included (true) or excluded (false).
    /// </summary>
    [BsonElement("isIncluded")]
    public bool IsIncluded { get; set; }

    /// <summary>
    /// Projection expression (for computed fields).
    /// </summary>
    [BsonElement("expression")]
    public string? Expression { get; set; }
}

/// <summary>
/// Represents a sort expression in a MongoDB query.
/// </summary>
public class SortExpression
{
    /// <summary>
    /// Field path (e.g., "user.email", "address.city").
    /// </summary>
    [Required]
    [BsonElement("fieldPath")]
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Sort direction (1 for ascending, -1 for descending).
    /// </summary>
    [BsonElement("direction")]
    public int Direction { get; set; } = 1;

    /// <summary>
    /// Sort order priority (for multiple sort fields).
    /// </summary>
    [BsonElement("priority")]
    public int Priority { get; set; }
}

/// <summary>
/// Represents an aggregation stage in a MongoDB aggregation pipeline.
/// </summary>
public class AggregationStage
{
    /// <summary>
    /// Stage name (e.g., "$match", "$lookup", "$group").
    /// </summary>
    [Required]
    [BsonElement("stageName")]
    public string StageName { get; set; } = string.Empty;

    /// <summary>
    /// Stage expression.
    /// </summary>
    [BsonElement("expression")]
    public Dictionary<string, object> Expression { get; set; } = new();

    /// <summary>
    /// Stage order in the pipeline.
    /// </summary>
    [BsonElement("order")]
    public int Order { get; set; }
}
