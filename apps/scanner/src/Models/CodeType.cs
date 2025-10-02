using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents a C# POCO class with its fields, BSON attributes, nullability, and discriminators.
/// </summary>
public class CodeType
{
    /// <summary>
    /// Unique identifier for the code type.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Class name.
    /// </summary>
    [Required]
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full namespace.
    /// </summary>
    [Required]
    [BsonElement("namespace")]
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Assembly name.
    /// </summary>
    [BsonElement("assembly")]
    public string Assembly { get; set; } = string.Empty;

    /// <summary>
    /// List of field definitions.
    /// </summary>
    [BsonElement("fields")]
    public List<FieldDefinition> Fields { get; set; } = new();

    /// <summary>
    /// List of BSON attribute configurations.
    /// </summary>
    [BsonElement("bsonAttributes")]
    public List<BSONAttribute> BSONAttributes { get; set; } = new();

    /// <summary>
    /// Nullable reference type information.
    /// </summary>
    [BsonElement("nullability")]
    public NullabilityInfo? Nullability { get; set; }

    /// <summary>
    /// Type discrimination information.
    /// </summary>
    [BsonElement("discriminators")]
    public List<string> Discriminators { get; set; } = new();

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
/// Represents a field definition within a code type.
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Field name.
    /// </summary>
    [Required]
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Field type.
    /// </summary>
    [Required]
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the field is nullable.
    /// </summary>
    [BsonElement("isNullable")]
    public bool IsNullable { get; set; }

    /// <summary>
    /// BSON attributes applied to this field.
    /// </summary>
    [BsonElement("bsonAttributes")]
    public List<BSONAttribute> BSONAttributes { get; set; } = new();

    /// <summary>
    /// Whether this field is required.
    /// </summary>
    [BsonElement("isRequired")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Default value for this field.
    /// </summary>
    [BsonElement("defaultValue")]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Documentation comment for this field.
    /// </summary>
    [BsonElement("documentation")]
    public string? Documentation { get; set; }
}

/// <summary>
/// Represents a BSON attribute configuration.
/// </summary>
public class BSONAttribute
{
    /// <summary>
    /// Attribute name (e.g., "BsonId", "BsonElement").
    /// </summary>
    [Required]
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Attribute value.
    /// </summary>
    [Required]
    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Additional attribute parameters.
    /// </summary>
    [BsonElement("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Represents nullable reference type information.
/// </summary>
public class NullabilityInfo
{
    /// <summary>
    /// Whether nullable reference types are enabled.
    /// </summary>
    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Nullable context for this type.
    /// </summary>
    [BsonElement("context")]
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// List of nullable fields.
    /// </summary>
    [BsonElement("nullableFields")]
    public List<string> NullableFields { get; set; } = new();

    /// <summary>
    /// List of non-nullable fields.
    /// </summary>
    [BsonElement("nonNullableFields")]
    public List<string> NonNullableFields { get; set; } = new();
}
