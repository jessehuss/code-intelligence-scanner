using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents a normalized fact stored in the searchable knowledge base with its provenance.
/// </summary>
public class KnowledgeBaseEntry
{
    /// <summary>
    /// Unique identifier for the knowledge base entry.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity (CodeType, CollectionMapping, QueryOperation, etc.).
    /// </summary>
    [Required]
    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the specific entity.
    /// </summary>
    [Required]
    [BsonElement("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Text content for search indexing.
    /// </summary>
    [Required]
    [BsonElement("searchableText")]
    public string SearchableText { get; set; } = string.Empty;

    /// <summary>
    /// List of search tags.
    /// </summary>
    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// List of related entity references.
    /// </summary>
    [BsonElement("relationships")]
    public List<EntityReference> Relationships { get; set; } = new();

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    [Required]
    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source information (repository, file, symbol, line span, commit SHA, timestamp).
    /// </summary>
    [Required]
    [BsonElement("provenance")]
    public ProvenanceRecord Provenance { get; set; } = new();

    /// <summary>
    /// Search relevance score (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("relevanceScore")]
    public double RelevanceScore { get; set; } = 1.0;

    /// <summary>
    /// Whether this entry is active.
    /// </summary>
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this entry has been indexed for search.
    /// </summary>
    [BsonElement("isIndexed")]
    public bool IsIndexed { get; set; } = false;

    /// <summary>
    /// Additional metadata for this entry.
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
/// Represents a reference to another entity.
/// </summary>
public class EntityReference
{
    /// <summary>
    /// Type of the referenced entity.
    /// </summary>
    [Required]
    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the referenced entity.
    /// </summary>
    [Required]
    [BsonElement("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Type of relationship to the referenced entity.
    /// </summary>
    [BsonElement("relationshipType")]
    public string? RelationshipType { get; set; }

    /// <summary>
    /// Confidence score for this relationship (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; } = 1.0;

    /// <summary>
    /// Whether this reference is bidirectional.
    /// </summary>
    [BsonElement("isBidirectional")]
    public bool IsBidirectional { get; set; } = false;

    /// <summary>
    /// Additional reference metadata.
    /// </summary>
    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}
