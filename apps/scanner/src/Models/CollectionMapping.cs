using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents the relationship between a C# type and its MongoDB collection name.
/// </summary>
public class CollectionMapping
{
    /// <summary>
    /// Unique identifier for the collection mapping.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to Code Type.
    /// </summary>
    [Required]
    [BsonElement("typeId")]
    public string TypeId { get; set; } = string.Empty;

    /// <summary>
    /// MongoDB collection name.
    /// </summary>
    [Required]
    [BsonElement("collectionName")]
    public string CollectionName { get; set; } = string.Empty;

    /// <summary>
    /// How the collection name was resolved (literal, constant, config, etc.).
    /// </summary>
    [Required]
    [BsonElement("resolutionMethod")]
    public ResolutionMethod ResolutionMethod { get; set; }

    /// <summary>
    /// Resolution confidence score (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Additional resolution context or evidence.
    /// </summary>
    [BsonElement("resolutionContext")]
    public string? ResolutionContext { get; set; }

    /// <summary>
    /// Whether this mapping is the primary mapping for the type.
    /// </summary>
    [BsonElement("isPrimary")]
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Alternative collection names that were considered.
    /// </summary>
    [BsonElement("alternatives")]
    public List<string> Alternatives { get; set; } = new();

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
/// Represents how a collection name was resolved.
/// </summary>
public enum ResolutionMethod
{
    /// <summary>
    /// Resolved from a string literal.
    /// </summary>
    Literal,

    /// <summary>
    /// Resolved from a constant or readonly field.
    /// </summary>
    Constant,

    /// <summary>
    /// Resolved from configuration.
    /// </summary>
    Config,

    /// <summary>
    /// Inferred from naming conventions.
    /// </summary>
    Inferred,

    /// <summary>
    /// Resolved from environment variables.
    /// </summary>
    Environment,

    /// <summary>
    /// Resolved from dependency injection.
    /// </summary>
    DependencyInjection,

    /// <summary>
    /// Resolution method is unknown.
    /// </summary>
    Unknown
}
