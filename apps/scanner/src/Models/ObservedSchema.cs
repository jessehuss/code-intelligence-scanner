using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Cataloger.Scanner.Models;

/// <summary>
/// Represents the inferred JSON Schema from sampled MongoDB data with type frequencies and patterns.
/// </summary>
public class ObservedSchema
{
    /// <summary>
    /// Unique identifier for the observed schema.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Reference to Collection Mapping.
    /// </summary>
    [Required]
    [BsonElement("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    /// <summary>
    /// JSON Schema definition.
    /// </summary>
    [Required]
    [BsonElement("schema")]
    public Dictionary<string, object> Schema { get; set; } = new();

    /// <summary>
    /// Frequency of each data type.
    /// </summary>
    [Required]
    [BsonElement("typeFrequencies")]
    public Dictionary<string, double> TypeFrequencies { get; set; } = new();

    /// <summary>
    /// List of required field names.
    /// </summary>
    [BsonElement("requiredFields")]
    public List<string> RequiredFields { get; set; } = new();

    /// <summary>
    /// Detected string format patterns.
    /// </summary>
    [BsonElement("stringFormats")]
    public List<StringFormat> StringFormats { get; set; } = new();

    /// <summary>
    /// Potential enum values.
    /// </summary>
    [BsonElement("enumCandidates")]
    public List<EnumCandidate> EnumCandidates { get; set; } = new();

    /// <summary>
    /// Number of documents sampled.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    [BsonElement("sampleSize")]
    public int SampleSize { get; set; }

    /// <summary>
    /// Whether PII was detected and redacted.
    /// </summary>
    [BsonElement("piiRedacted")]
    public bool PIIRedacted { get; set; }

    /// <summary>
    /// List of PII detections found during sampling.
    /// </summary>
    [BsonElement("piiDetections")]
    public List<PIIDetection> PIIDetections { get; set; } = new();

    /// <summary>
    /// Sampling configuration used.
    /// </summary>
    [BsonElement("samplingConfig")]
    public SamplingConfiguration? SamplingConfig { get; set; }

    /// <summary>
    /// When the sampling was performed.
    /// </summary>
    [BsonElement("sampledAt")]
    public DateTime SampledAt { get; set; } = DateTime.UtcNow;

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
/// Represents a detected string format pattern.
/// </summary>
public class StringFormat
{
    /// <summary>
    /// Field name where the format was detected.
    /// </summary>
    [Required]
    [BsonElement("fieldName")]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Detected format pattern (e.g., "email", "phone", "uuid").
    /// </summary>
    [Required]
    [BsonElement("pattern")]
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Frequency of this format in the sampled data (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    /// Regular expression used to detect this format.
    /// </summary>
    [BsonElement("regex")]
    public string? Regex { get; set; }

    /// <summary>
    /// Examples of values matching this format (redacted).
    /// </summary>
    [BsonElement("examples")]
    public List<string> Examples { get; set; } = new();

    /// <summary>
    /// Confidence score for this format detection (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; }
}

/// <summary>
/// Represents a potential enum value.
/// </summary>
public class EnumCandidate
{
    /// <summary>
    /// Field name where the enum was detected.
    /// </summary>
    [Required]
    [BsonElement("fieldName")]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Potential enum values.
    /// </summary>
    [Required]
    [BsonElement("values")]
    public List<string> Values { get; set; } = new();

    /// <summary>
    /// Frequency of each enum value in the sampled data.
    /// </summary>
    [BsonElement("valueFrequencies")]
    public Dictionary<string, double> ValueFrequencies { get; set; } = new();

    /// <summary>
    /// Confidence score for this enum detection (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Whether this field is a good candidate for an enum.
    /// </summary>
    [BsonElement("isGoodCandidate")]
    public bool IsGoodCandidate { get; set; }

    /// <summary>
    /// Number of distinct values found.
    /// </summary>
    [BsonElement("distinctValueCount")]
    public int DistinctValueCount { get; set; }
}

/// <summary>
/// Represents a PII detection found during sampling.
/// </summary>
public class PIIDetection
{
    /// <summary>
    /// Field name where PII was detected.
    /// </summary>
    [Required]
    [BsonElement("fieldName")]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Type of PII detected (e.g., "email", "phone", "ssn").
    /// </summary>
    [Required]
    [BsonElement("piiType")]
    public string PIIType { get; set; } = string.Empty;

    /// <summary>
    /// Detection method used (e.g., "field_name", "value_pattern").
    /// </summary>
    [Required]
    [BsonElement("detectionMethod")]
    public string DetectionMethod { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score for this PII detection (0.0-1.0).
    /// </summary>
    [Range(0.0, 1.0)]
    [BsonElement("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Whether this PII was successfully redacted.
    /// </summary>
    [BsonElement("isRedacted")]
    public bool IsRedacted { get; set; }

    /// <summary>
    /// Number of instances of this PII found.
    /// </summary>
    [BsonElement("instanceCount")]
    public int InstanceCount { get; set; }

    /// <summary>
    /// Whether this detection requires manual review.
    /// </summary>
    [BsonElement("requiresManualReview")]
    public bool RequiresManualReview { get; set; }

    /// <summary>
    /// Additional detection metadata.
    /// </summary>
    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the sampling configuration used.
/// </summary>
public class SamplingConfiguration
{
    /// <summary>
    /// Maximum number of documents to sample per collection.
    /// </summary>
    [Required]
    [Range(1, 10000)]
    [BsonElement("maxDocumentsPerCollection")]
    public int MaxDocumentsPerCollection { get; set; }

    /// <summary>
    /// Whether PII detection was enabled.
    /// </summary>
    [BsonElement("piiDetectionEnabled")]
    public bool PIIDetectionEnabled { get; set; }

    /// <summary>
    /// Connection timeout in milliseconds.
    /// </summary>
    [Range(1000, 300000)]
    [BsonElement("connectionTimeout")]
    public int ConnectionTimeout { get; set; }

    /// <summary>
    /// Whether sampling was performed in read-only mode.
    /// </summary>
    [BsonElement("readOnlyMode")]
    public bool ReadOnlyMode { get; set; } = true;

    /// <summary>
    /// Additional sampling options.
    /// </summary>
    [BsonElement("options")]
    public Dictionary<string, object> Options { get; set; } = new();
}
