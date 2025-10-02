using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.RegularExpressions;

namespace Cataloger.Scanner.Analyzers;

/// <summary>
/// Service for inferring relationships between types based on query patterns and $lookup operations.
/// </summary>
public class RelationshipInferencer
{
    private readonly ILogger<RelationshipInferencer> _logger;

    public RelationshipInferencer(ILogger<RelationshipInferencer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Infers relationships between code types based on query patterns and operations.
    /// </summary>
    /// <param name="codeTypes">List of code types to analyze.</param>
    /// <param name="queryOperations">List of query operations to analyze.</param>
    /// <param name="collectionMappings">List of collection mappings.</param>
    /// <param name="provenance">Provenance information for the inference.</param>
    /// <returns>List of inferred data relationships.</returns>
    public async Task<List<DataRelationship>> InferRelationshipsAsync(
        List<CodeType> codeTypes,
        List<QueryOperation> queryOperations,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        var relationships = new List<DataRelationship>();

        _logger.LogDebug("Inferring relationships between {TypeCount} types using {OperationCount} operations", 
            codeTypes.Count, queryOperations.Count);

        // Infer relationships from filter expressions
        var filterRelationships = await InferRelationshipsFromFiltersAsync(
            codeTypes, queryOperations, collectionMappings, provenance);
        relationships.AddRange(filterRelationships);

        // Infer relationships from $lookup operations
        var lookupRelationships = await InferRelationshipsFromLookupsAsync(
            codeTypes, queryOperations, collectionMappings, provenance);
        relationships.AddRange(lookupRelationships);

        // Infer relationships from naming conventions
        var namingRelationships = await InferRelationshipsFromNamingConventionsAsync(
            codeTypes, provenance);
        relationships.AddRange(namingRelationships);

        // Infer relationships from field types
        var fieldTypeRelationships = await InferRelationshipsFromFieldTypesAsync(
            codeTypes, provenance);
        relationships.AddRange(fieldTypeRelationships);

        _logger.LogInformation("Inferred {Count} relationships", relationships.Count);

        return relationships;
    }

    private async Task<List<DataRelationship>> InferRelationshipsFromFiltersAsync(
        List<CodeType> codeTypes,
        List<QueryOperation> queryOperations,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        var relationships = new List<DataRelationship>();

        foreach (var operation in queryOperations)
        {
            foreach (var filter in operation.Filters)
            {
                var relationship = await InferRelationshipFromFilterAsync(
                    filter, operation, codeTypes, collectionMappings, provenance);
                if (relationship != null)
                {
                    relationships.Add(relationship);
                }
            }
        }

        return relationships;
    }

    private async Task<DataRelationship?> InferRelationshipFromFilterAsync(
        FilterExpression filter,
        QueryOperation operation,
        List<CodeType> codeTypes,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        // Look for foreign key patterns in filter expressions
        if (IsForeignKeyPattern(filter.FieldPath, filter.Value))
        {
            var sourceType = FindTypeByCollection(operation.CollectionId, codeTypes, collectionMappings);
            var targetType = FindTypeByFieldPattern(filter.FieldPath, codeTypes);

            if (sourceType != null && targetType != null && sourceType.Id != targetType.Id)
            {
                return new DataRelationship
                {
                    SourceTypeId = sourceType.Id,
                    TargetTypeId = targetType.Id,
                    RelationshipType = RelationshipType.REFERS_TO,
                    Confidence = CalculateFilterConfidence(filter),
                    FieldPath = filter.FieldPath,
                    IsBidirectional = false,
                    Cardinality = RelationshipCardinality.ManyToOne,
                    IsRequired = false,
                    Evidence = new List<RelationshipEvidence>
                    {
                        new()
                        {
                            EvidenceType = EvidenceType.Filter,
                            Description = $"Filter expression '{filter.FieldPath} {filter.Operator} {filter.Value}'",
                            Confidence = CalculateFilterConfidence(filter),
                            SourceLocation = new SourceLocation
                            {
                                FilePath = provenance.FilePath,
                                LineNumber = provenance.LineSpan.Start,
                                SymbolName = operation.OperationType.ToString()
                            }
                        }
                    },
                    Provenance = provenance
                };
            }
        }

        return null;
    }

    private async Task<List<DataRelationship>> InferRelationshipsFromLookupsAsync(
        List<CodeType> codeTypes,
        List<QueryOperation> queryOperations,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        var relationships = new List<DataRelationship>();

        foreach (var operation in queryOperations)
        {
            if (operation.AggregationPipeline != null)
            {
                foreach (var stage in operation.AggregationPipeline)
                {
                    if (stage.StageName == "$lookup")
                    {
                        var relationship = await InferRelationshipFromLookupAsync(
                            stage, operation, codeTypes, collectionMappings, provenance);
                        if (relationship != null)
                        {
                            relationships.Add(relationship);
                        }
                    }
                }
            }
        }

        return relationships;
    }

    private async Task<DataRelationship?> InferRelationshipFromLookupAsync(
        AggregationStage stage,
        QueryOperation operation,
        List<CodeType> codeTypes,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        // Extract lookup information from the stage expression
        var fromCollection = ExtractLookupFromCollection(stage.Expression);
        var localField = ExtractLookupLocalField(stage.Expression);
        var foreignField = ExtractLookupForeignField(stage.Expression);

        if (string.IsNullOrEmpty(fromCollection) || string.IsNullOrEmpty(localField) || string.IsNullOrEmpty(foreignField))
        {
            return null;
        }

        var sourceType = FindTypeByCollection(operation.CollectionId, codeTypes, collectionMappings);
        var targetType = FindTypeByCollectionName(fromCollection, codeTypes, collectionMappings);

        if (sourceType != null && targetType != null && sourceType.Id != targetType.Id)
        {
            return new DataRelationship
            {
                SourceTypeId = sourceType.Id,
                TargetTypeId = targetType.Id,
                RelationshipType = RelationshipType.LOOKUP,
                Confidence = 0.9, // High confidence for explicit $lookup
                FieldPath = $"{localField} -> {foreignField}",
                IsBidirectional = false,
                Cardinality = RelationshipCardinality.OneToMany,
                IsRequired = false,
                Evidence = new List<RelationshipEvidence>
                {
                    new()
                    {
                        EvidenceType = EvidenceType.Lookup,
                        Description = $"$lookup from '{fromCollection}' on '{localField}' = '{foreignField}'",
                        Confidence = 0.9,
                        SourceLocation = new SourceLocation
                        {
                            FilePath = provenance.FilePath,
                            LineNumber = provenance.LineSpan.Start,
                            SymbolName = operation.OperationType.ToString()
                        }
                    }
                },
                Provenance = provenance
            };
        }

        return null;
    }

    private async Task<List<DataRelationship>> InferRelationshipsFromNamingConventionsAsync(
        List<CodeType> codeTypes,
        ProvenanceRecord provenance)
    {
        var relationships = new List<DataRelationship>();

        for (int i = 0; i < codeTypes.Count; i++)
        {
            for (int j = i + 1; j < codeTypes.Count; j++)
            {
                var relationship = await InferRelationshipFromNamingConventionAsync(
                    codeTypes[i], codeTypes[j], provenance);
                if (relationship != null)
                {
                    relationships.Add(relationship);
                }
            }
        }

        return relationships;
    }

    private async Task<DataRelationship?> InferRelationshipFromNamingConventionAsync(
        CodeType sourceType,
        CodeType targetType,
        ProvenanceRecord provenance)
    {
        // Look for naming conventions that suggest relationships
        var sourceFields = sourceType.Fields.Select(f => f.Name).ToList();
        var targetFields = targetType.Fields.Select(f => f.Name).ToList();

        // Check for foreign key patterns (e.g., UserId -> User.Id)
        foreach (var sourceField in sourceFields)
        {
            if (IsForeignKeyField(sourceField, targetType.Name))
            {
                return new DataRelationship
                {
                    SourceTypeId = sourceType.Id,
                    TargetTypeId = targetType.Id,
                    RelationshipType = RelationshipType.REFERS_TO,
                    Confidence = 0.6, // Medium confidence for naming convention
                    FieldPath = sourceField,
                    IsBidirectional = false,
                    Cardinality = RelationshipCardinality.ManyToOne,
                    IsRequired = false,
                    Evidence = new List<RelationshipEvidence>
                    {
                        new()
                        {
                            EvidenceType = EvidenceType.NamingConvention,
                            Description = $"Field '{sourceField}' follows foreign key naming convention for '{targetType.Name}'",
                            Confidence = 0.6,
                            SourceLocation = new SourceLocation
                            {
                                FilePath = provenance.FilePath,
                                LineNumber = provenance.LineSpan.Start,
                                SymbolName = sourceType.Name
                            }
                        }
                    },
                    Provenance = provenance
                };
            }
        }

        return null;
    }

    private async Task<List<DataRelationship>> InferRelationshipsFromFieldTypesAsync(
        List<CodeType> codeTypes,
        ProvenanceRecord provenance)
    {
        var relationships = new List<DataRelationship>();

        for (int i = 0; i < codeTypes.Count; i++)
        {
            for (int j = i + 1; j < codeTypes.Count; j++)
            {
                var relationship = await InferRelationshipFromFieldTypesAsync(
                    codeTypes[i], codeTypes[j], provenance);
                if (relationship != null)
                {
                    relationships.Add(relationship);
                }
            }
        }

        return relationships;
    }

    private async Task<DataRelationship?> InferRelationshipFromFieldTypesAsync(
        CodeType sourceType,
        CodeType targetType,
        ProvenanceRecord provenance)
    {
        // Look for fields that reference other types
        foreach (var sourceField in sourceType.Fields)
        {
            if (IsTypeReference(sourceField.Type, targetType.Name))
            {
                return new DataRelationship
                {
                    SourceTypeId = sourceType.Id,
                    TargetTypeId = targetType.Id,
                    RelationshipType = RelationshipType.REFERS_TO,
                    Confidence = 0.7, // Medium-high confidence for type reference
                    FieldPath = sourceField.Name,
                    IsBidirectional = false,
                    Cardinality = RelationshipCardinality.OneToOne,
                    IsRequired = !sourceField.IsNullable,
                    Evidence = new List<RelationshipEvidence>
                    {
                        new()
                        {
                            EvidenceType = EvidenceType.FieldType,
                            Description = $"Field '{sourceField.Name}' of type '{sourceField.Type}' references '{targetType.Name}'",
                            Confidence = 0.7,
                            SourceLocation = new SourceLocation
                            {
                                FilePath = provenance.FilePath,
                                LineNumber = provenance.LineSpan.Start,
                                SymbolName = sourceType.Name
                            }
                        }
                    },
                    Provenance = provenance
                };
            }
        }

        return null;
    }

    private bool IsForeignKeyPattern(string fieldPath, object? value)
    {
        // Simple heuristic for foreign key patterns
        return fieldPath.EndsWith("Id") || fieldPath.EndsWith("_id") || fieldPath.EndsWith("ID");
    }

    private bool IsForeignKeyField(string fieldName, string targetTypeName)
    {
        // Check if field name follows foreign key convention
        var expectedFieldName = targetTypeName + "Id";
        return fieldName.Equals(expectedFieldName, StringComparison.OrdinalIgnoreCase) ||
               fieldName.Equals(targetTypeName + "_id", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsTypeReference(string fieldType, string targetTypeName)
    {
        // Check if field type references the target type
        return fieldType.Contains(targetTypeName) && 
               !fieldType.Contains("Collection") && 
               !fieldType.Contains("List") && 
               !fieldType.Contains("Array");
    }

    private CodeType? FindTypeByCollection(
        string collectionId,
        List<CodeType> codeTypes,
        List<CollectionMapping> collectionMappings)
    {
        var mapping = collectionMappings.FirstOrDefault(m => m.Id == collectionId);
        if (mapping == null) return null;

        return codeTypes.FirstOrDefault(t => t.Id == mapping.TypeId);
    }

    private CodeType? FindTypeByCollectionName(
        string collectionName,
        List<CodeType> codeTypes,
        List<CollectionMapping> collectionMappings)
    {
        var mapping = collectionMappings.FirstOrDefault(m => m.CollectionName == collectionName);
        if (mapping == null) return null;

        return codeTypes.FirstOrDefault(t => t.Id == mapping.TypeId);
    }

    private CodeType? FindTypeByFieldPattern(string fieldPath, List<CodeType> codeTypes)
    {
        // Simple heuristic to find type by field pattern
        // In practice, this would be more sophisticated
        var fieldName = fieldPath.Split('.').Last();
        if (fieldName.EndsWith("Id"))
        {
            var typeName = fieldName.Substring(0, fieldName.Length - 2);
            return codeTypes.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    private double CalculateFilterConfidence(FilterExpression filter)
    {
        // Calculate confidence based on filter characteristics
        var confidence = 0.5; // Base confidence

        if (filter.Operator == "eq")
        {
            confidence += 0.2;
        }

        if (filter.FieldPath.EndsWith("Id"))
        {
            confidence += 0.2;
        }

        if (filter.Value is string stringValue && IsObjectIdPattern(stringValue))
        {
            confidence += 0.1;
        }

        return Math.Min(confidence, 1.0);
    }

    private bool IsObjectIdPattern(object? value)
    {
        if (value is string stringValue)
        {
            // Simple ObjectId pattern check
            return stringValue.Length == 24 && Regex.IsMatch(stringValue, @"^[0-9a-fA-F]+$");
        }

        return false;
    }

    private string? ExtractLookupFromCollection(Dictionary<string, object> expression)
    {
        if (expression.TryGetValue("from", out var fromValue))
        {
            return fromValue.ToString();
        }

        return null;
    }

    private string? ExtractLookupLocalField(Dictionary<string, object> expression)
    {
        if (expression.TryGetValue("localField", out var localFieldValue))
        {
            return localFieldValue.ToString();
        }

        return null;
    }

    private string? ExtractLookupForeignField(Dictionary<string, object> expression)
    {
        if (expression.TryGetValue("foreignField", out var foreignFieldValue))
        {
            return foreignFieldValue.ToString();
        }

        return null;
    }
}
