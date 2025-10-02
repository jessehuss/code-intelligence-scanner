using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Cataloger.Scanner.Samplers;

/// <summary>
/// Service for sampling MongoDB data with PII redaction and JSON Schema inference.
/// </summary>
public class MongoSampler
{
    private readonly ILogger<MongoSampler> _logger;
    private readonly IPIIDetector _piiDetector;

    public MongoSampler(ILogger<MongoSampler> logger, IPIIDetector piiDetector)
    {
        _logger = logger;
        _piiDetector = piiDetector;
    }

    /// <summary>
    /// Samples MongoDB collections and generates observed schemas.
    /// </summary>
    /// <param name="connectionString">MongoDB connection string.</param>
    /// <param name="collectionNames">List of collection names to sample.</param>
    /// <param name="samplingConfig">Sampling configuration.</param>
    /// <param name="provenance">Provenance information for the sampling.</param>
    /// <returns>List of observed schemas.</returns>
    public async Task<List<ObservedSchema>> SampleCollectionsAsync(
        string connectionString,
        List<string> collectionNames,
        SamplingConfiguration samplingConfig,
        ProvenanceRecord provenance)
    {
        var observedSchemas = new List<ObservedSchema>();

        _logger.LogInformation("Starting MongoDB sampling for {CollectionCount} collections", collectionNames.Count);

        try
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(GetDatabaseName(connectionString));

            foreach (var collectionName in collectionNames)
            {
                try
                {
                    var observedSchema = await SampleCollectionAsync(
                        database, collectionName, samplingConfig, provenance);
                    if (observedSchema != null)
                    {
                        observedSchemas.Add(observedSchema);
                        _logger.LogDebug("Sampled collection {CollectionName}: {SampleSize} documents", 
                            collectionName, observedSchema.SampleSize);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sample collection {CollectionName}", collectionName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MongoDB for sampling");
            throw;
        }

        _logger.LogInformation("Completed MongoDB sampling: {SchemaCount} schemas generated", observedSchemas.Count);
        return observedSchemas;
    }

    private async Task<ObservedSchema?> SampleCollectionAsync(
        IMongoDatabase database,
        string collectionName,
        SamplingConfiguration samplingConfig,
        ProvenanceRecord provenance)
    {
        var collection = database.GetCollection<BsonDocument>(collectionName);

        try
        {
            // Get collection statistics
            var stats = await GetCollectionStatsAsync(collection);
            
            // Sample documents
            var documents = await SampleDocumentsAsync(collection, samplingConfig.MaxDocumentsPerCollection);
            
            if (documents.Count == 0)
            {
                _logger.LogWarning("No documents found in collection {CollectionName}", collectionName);
                return null;
            }

            // Generate observed schema
            var observedSchema = await GenerateObservedSchemaAsync(
                collectionName, documents, samplingConfig, provenance);

            return observedSchema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sample collection {CollectionName}", collectionName);
            return null;
        }
    }

    private async Task<List<BsonDocument>> SampleDocumentsAsync(
        IMongoCollection<BsonDocument> collection,
        int maxDocuments)
    {
        var documents = new List<BsonDocument>();

        try
        {
            var cursor = await collection.Find(FilterDefinition<BsonDocument>.Empty)
                .Limit(maxDocuments)
                .ToCursorAsync();

            while (await cursor.MoveNextAsync())
            {
                documents.AddRange(cursor.Current);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sample documents from collection");
            throw;
        }

        return documents;
    }

    private async Task<ObservedSchema> GenerateObservedSchemaAsync(
        string collectionName,
        List<BsonDocument> documents,
        SamplingConfiguration samplingConfig,
        ProvenanceRecord provenance)
    {
        var observedSchema = new ObservedSchema
        {
            CollectionId = collectionName, // This would be resolved to actual collection mapping ID
            SampleSize = documents.Count,
            SampledAt = DateTime.UtcNow,
            SamplingConfig = samplingConfig,
            Provenance = provenance
        };

        // Analyze documents to generate schema
        var fieldAnalysis = AnalyzeFields(documents);
        
        // Generate JSON Schema
        observedSchema.Schema = GenerateJSONSchema(fieldAnalysis);
        
        // Calculate type frequencies
        observedSchema.TypeFrequencies = CalculateTypeFrequencies(fieldAnalysis);
        
        // Identify required fields
        observedSchema.RequiredFields = IdentifyRequiredFields(fieldAnalysis);
        
        // Detect string formats
        observedSchema.StringFormats = await DetectStringFormatsAsync(fieldAnalysis, samplingConfig);
        
        // Detect enum candidates
        observedSchema.EnumCandidates = DetectEnumCandidates(fieldAnalysis);
        
        // Detect and redact PII
        if (samplingConfig.PIIDetectionEnabled)
        {
            observedSchema.PIIDetections = await DetectPIIAsync(fieldAnalysis, samplingConfig);
            observedSchema.PIIRedacted = observedSchema.PIIDetections.Any(d => d.IsRedacted);
        }

        return observedSchema;
    }

    private Dictionary<string, FieldAnalysis> AnalyzeFields(List<BsonDocument> documents)
    {
        var fieldAnalysis = new Dictionary<string, FieldAnalysis>();

        foreach (var document in documents)
        {
            AnalyzeDocumentFields(document, fieldAnalysis, "");
        }

        return fieldAnalysis;
    }

    private void AnalyzeDocumentFields(
        BsonDocument document,
        Dictionary<string, FieldAnalysis> fieldAnalysis,
        string prefix)
    {
        foreach (var element in document.Elements)
        {
            var fieldPath = string.IsNullOrEmpty(prefix) ? element.Name : $"{prefix}.{element.Name}";
            
            if (!fieldAnalysis.ContainsKey(fieldPath))
            {
                fieldAnalysis[fieldPath] = new FieldAnalysis
                {
                    FieldPath = fieldPath,
                    ValueCounts = new Dictionary<string, int>(),
                    TypeCounts = new Dictionary<string, int>(),
                    IsRequired = true,
                    SampleValues = new List<object>()
                };
            }

            var analysis = fieldAnalysis[fieldPath];
            analysis.TotalCount++;

            // Analyze value type
            var valueType = GetValueType(element.Value);
            if (!analysis.TypeCounts.ContainsKey(valueType))
            {
                analysis.TypeCounts[valueType] = 0;
            }
            analysis.TypeCounts[valueType]++;

            // Store sample values (limited)
            if (analysis.SampleValues.Count < 10)
            {
                analysis.SampleValues.Add(ConvertBsonValue(element.Value));
            }

            // Analyze nested documents
            if (element.Value is BsonDocument nestedDoc)
            {
                AnalyzeDocumentFields(nestedDoc, fieldAnalysis, fieldPath);
            }
        }
    }

    private string GetValueType(BsonValue value)
    {
        return value.BsonType switch
        {
            BsonType.String => "string",
            BsonType.Int32 => "integer",
            BsonType.Int64 => "integer",
            BsonType.Double => "number",
            BsonType.Decimal128 => "number",
            BsonType.Boolean => "boolean",
            BsonType.DateTime => "datetime",
            BsonType.ObjectId => "objectid",
            BsonType.Array => "array",
            BsonType.Document => "object",
            BsonType.Null => "null",
            _ => "unknown"
        };
    }

    private object ConvertBsonValue(BsonValue value)
    {
        return value.BsonType switch
        {
            BsonType.String => value.AsString,
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            BsonType.Double => value.AsDouble,
            BsonType.Decimal128 => value.AsDecimal128,
            BsonType.Boolean => value.AsBoolean,
            BsonType.DateTime => value.AsDateTime,
            BsonType.ObjectId => value.AsObjectId.ToString(),
            BsonType.Array => value.AsBsonArray.ToList(),
            BsonType.Document => value.AsBsonDocument.ToDictionary(),
            BsonType.Null => null,
            _ => value.ToString()
        };
    }

    private Dictionary<string, object> GenerateJSONSchema(Dictionary<string, FieldAnalysis> fieldAnalysis)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>(),
            ["required"] = new List<string>()
        };

        var properties = (Dictionary<string, object>)schema["properties"];
        var required = (List<string>)schema["required"];

        foreach (var kvp in fieldAnalysis)
        {
            var fieldPath = kvp.Key;
            var analysis = kvp.Value;

            // Skip nested fields for now (simplified implementation)
            if (fieldPath.Contains('.'))
            {
                continue;
            }

            var fieldSchema = GenerateFieldSchema(analysis);
            properties[fieldPath] = fieldSchema;

            if (analysis.IsRequired)
            {
                required.Add(fieldPath);
            }
        }

        return schema;
    }

    private Dictionary<string, object> GenerateFieldSchema(FieldAnalysis analysis)
    {
        var schema = new Dictionary<string, object>();

        // Determine primary type
        var primaryType = analysis.TypeCounts
            .OrderByDescending(kvp => kvp.Value)
            .First().Key;

        schema["type"] = primaryType;

        // Add format information for strings
        if (primaryType == "string")
        {
            var format = DetectStringFormat(analysis.SampleValues);
            if (!string.IsNullOrEmpty(format))
            {
                schema["format"] = format;
            }
        }

        // Add enum information
        var distinctValues = analysis.SampleValues
            .Where(v => v != null)
            .Select(v => v.ToString())
            .Distinct()
            .ToList();

        if (distinctValues.Count <= 10 && distinctValues.Count > 1)
        {
            schema["enum"] = distinctValues;
        }

        return schema;
    }

    private Dictionary<string, double> CalculateTypeFrequencies(Dictionary<string, FieldAnalysis> fieldAnalysis)
    {
        var totalFields = fieldAnalysis.Count;
        var typeFrequencies = new Dictionary<string, double>();

        foreach (var analysis in fieldAnalysis.Values)
        {
            foreach (var kvp in analysis.TypeCounts)
            {
                var type = kvp.Key;
                var count = kvp.Value;

                if (!typeFrequencies.ContainsKey(type))
                {
                    typeFrequencies[type] = 0;
                }

                typeFrequencies[type] += count;
            }
        }

        // Normalize frequencies
        var totalCount = typeFrequencies.Values.Sum();
        if (totalCount > 0)
        {
            foreach (var key in typeFrequencies.Keys.ToList())
            {
                typeFrequencies[key] = typeFrequencies[key] / totalCount;
            }
        }

        return typeFrequencies;
    }

    private List<string> IdentifyRequiredFields(Dictionary<string, FieldAnalysis> fieldAnalysis)
    {
        var requiredFields = new List<string>();

        foreach (var kvp in fieldAnalysis)
        {
            var fieldPath = kvp.Key;
            var analysis = kvp.Value;

            // Skip nested fields for now
            if (fieldPath.Contains('.'))
            {
                continue;
            }

            if (analysis.IsRequired)
            {
                requiredFields.Add(fieldPath);
            }
        }

        return requiredFields;
    }

    private async Task<List<StringFormat>> DetectStringFormatsAsync(
        Dictionary<string, FieldAnalysis> fieldAnalysis,
        SamplingConfiguration samplingConfig)
    {
        var stringFormats = new List<StringFormat>();

        foreach (var kvp in fieldAnalysis)
        {
            var fieldPath = kvp.Key;
            var analysis = kvp.Value;

            // Skip nested fields for now
            if (fieldPath.Contains('.'))
            {
                continue;
            }

            var stringValues = analysis.SampleValues
                .Where(v => v is string)
                .Cast<string>()
                .ToList();

            if (stringValues.Count > 0)
            {
                var format = DetectStringFormat(stringValues);
                if (!string.IsNullOrEmpty(format))
                {
                    stringFormats.Add(new StringFormat
                    {
                        FieldName = fieldPath,
                        Pattern = format,
                        Frequency = (double)stringValues.Count(v => MatchesFormat(v, format)) / stringValues.Count,
                        Confidence = 0.8 // Base confidence
                    });
                }
            }
        }

        return stringFormats;
    }

    private List<EnumCandidate> DetectEnumCandidates(Dictionary<string, FieldAnalysis> fieldAnalysis)
    {
        var enumCandidates = new List<EnumCandidate>();

        foreach (var kvp in fieldAnalysis)
        {
            var fieldPath = kvp.Key;
            var analysis = kvp.Value;

            // Skip nested fields for now
            if (fieldPath.Contains('.'))
            {
                continue;
            }

            var distinctValues = analysis.SampleValues
                .Where(v => v != null)
                .Select(v => v.ToString())
                .Distinct()
                .ToList();

            // Consider as enum candidate if we have a small number of distinct values
            if (distinctValues.Count >= 2 && distinctValues.Count <= 20)
            {
                var valueFrequencies = distinctValues.ToDictionary(
                    v => v,
                    v => (double)analysis.SampleValues.Count(sv => sv?.ToString() == v) / analysis.SampleValues.Count
                );

                enumCandidates.Add(new EnumCandidate
                {
                    FieldName = fieldPath,
                    Values = distinctValues.ToArray(),
                    ValueFrequencies = valueFrequencies,
                    DistinctValueCount = distinctValues.Count,
                    IsGoodCandidate = distinctValues.Count <= 10,
                    Confidence = CalculateEnumConfidence(distinctValues.Count, analysis.SampleValues.Count)
                });
            }
        }

        return enumCandidates;
    }

    private async Task<List<PIIDetection>> DetectPIIAsync(
        Dictionary<string, FieldAnalysis> fieldAnalysis,
        SamplingConfiguration samplingConfig)
    {
        var piiDetections = new List<PIIDetection>();

        foreach (var kvp in fieldAnalysis)
        {
            var fieldPath = kvp.Key;
            var analysis = kvp.Value;

            // Skip nested fields for now
            if (fieldPath.Contains('.'))
            {
                continue;
            }

            var detection = await _piiDetector.DetectPIIAsync(fieldPath, analysis.SampleValues);
            if (detection != null)
            {
                piiDetections.Add(detection);
            }
        }

        return piiDetections;
    }

    private string? DetectStringFormat(List<object> values)
    {
        var stringValues = values.OfType<string>().ToList();
        if (stringValues.Count == 0) return null;

        // Check for email format
        if (stringValues.All(v => IsEmailFormat(v)))
        {
            return "email";
        }

        // Check for phone format
        if (stringValues.All(v => IsPhoneFormat(v)))
        {
            return "phone";
        }

        // Check for UUID format
        if (stringValues.All(v => IsUUIDFormat(v)))
        {
            return "uuid";
        }

        // Check for date format
        if (stringValues.All(v => IsDateFormat(v)))
        {
            return "date";
        }

        return null;
    }

    private bool MatchesFormat(string value, string format)
    {
        return format switch
        {
            "email" => IsEmailFormat(value),
            "phone" => IsPhoneFormat(value),
            "uuid" => IsUUIDFormat(value),
            "date" => IsDateFormat(value),
            _ => false
        };
    }

    private bool IsEmailFormat(string value)
    {
        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsPhoneFormat(string value)
    {
        return Regex.IsMatch(value, @"^\+?[\d\s\-\(\)]+$");
    }

    private bool IsUUIDFormat(string value)
    {
        return Regex.IsMatch(value, @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
    }

    private bool IsDateFormat(string value)
    {
        return DateTime.TryParse(value, out _);
    }

    private double CalculateEnumConfidence(int distinctCount, int totalCount)
    {
        // Higher confidence for fewer distinct values relative to total
        var ratio = (double)distinctCount / totalCount;
        return Math.Max(0.1, 1.0 - ratio);
    }

    private async Task<BsonDocument> GetCollectionStatsAsync(IMongoCollection<BsonDocument> collection)
    {
        // This would get collection statistics
        // For now, return a mock document
        return new BsonDocument
        {
            ["count"] = 1000,
            ["size"] = 1024000,
            ["avgObjSize"] = 1024
        };
    }

    private string GetDatabaseName(string connectionString)
    {
        // Extract database name from connection string
        // This is a simplified implementation
        var uri = new Uri(connectionString);
        return uri.AbsolutePath.TrimStart('/');
    }

    private class FieldAnalysis
    {
        public string FieldPath { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public Dictionary<string, int> ValueCounts { get; set; } = new();
        public Dictionary<string, int> TypeCounts { get; set; } = new();
        public bool IsRequired { get; set; }
        public List<object> SampleValues { get; set; } = new();
    }
}

/// <summary>
/// Interface for PII detection services.
/// </summary>
public interface IPIIDetector
{
    Task<PIIDetection?> DetectPIIAsync(string fieldName, List<object> sampleValues);
}
