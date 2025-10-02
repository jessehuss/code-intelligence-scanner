using System.CommandLine;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using MongoDB.Driver;
using System.Text.Json;

namespace Cataloger.Scanner.Commands;

/// <summary>
/// CLI command for getting detailed information about a specific type.
/// </summary>
public class GetTypeCommand
{
    private readonly ILogger<GetTypeCommand> _logger;
    private readonly IMongoDatabase _database;

    public GetTypeCommand(ILogger<GetTypeCommand> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    /// <summary>
    /// Creates the get-type command with all options and arguments.
    /// </summary>
    /// <returns>The configured get-type command.</returns>
    public static Command CreateCommand()
    {
        var typeIdArgument = new Argument<string>(
            "type-id",
            "Unique identifier for the code type");

        var includeRelationshipsOption = new Option<bool>(
            "--include-relationships",
            () => false,
            "Include relationship information")
        {
            IsRequired = false
        };

        var includeQueriesOption = new Option<bool>(
            "--include-queries",
            () => false,
            "Include query operations using this type")
        {
            IsRequired = false
        };

        var includeSchemasOption = new Option<bool>(
            "--include-schemas",
            () => false,
            "Include observed schemas for this type")
        {
            IsRequired = false
        };

        var outputFormatOption = new Option<string>(
            "--output-format",
            () => "json",
            "Output format for type information (json, yaml, csv)")
        {
            IsRequired = false
        };

        var outputFileOption = new Option<string>(
            "--output-file",
            "Output file path for type information")
        {
            IsRequired = false
        };

        var verboseOption = new Option<bool>(
            "--verbose",
            () => false,
            "Enable verbose logging")
        {
            IsRequired = false
        };

        var command = new Command("get-type", "Get detailed information about a specific code type")
        {
            typeIdArgument,
            includeRelationshipsOption,
            includeQueriesOption,
            includeSchemasOption,
            outputFormatOption,
            outputFileOption,
            verboseOption
        };

        command.SetHandler(async (typeId, includeRelationships, includeQueries, includeSchemas, outputFormat, outputFile, verbose) =>
        {
            var getTypeCommand = new GetTypeCommand(
                null!, // Logger would be injected
                null!); // Database would be injected

            await getTypeCommand.ExecuteAsync(
                typeId,
                includeRelationships,
                includeQueries,
                includeSchemas,
                outputFormat,
                outputFile,
                verbose);
        }, typeIdArgument, includeRelationshipsOption, includeQueriesOption, includeSchemasOption, outputFormatOption, outputFileOption, verboseOption);

        return command;
    }

    /// <summary>
    /// Executes the get-type command.
    /// </summary>
    /// <param name="typeId">Type ID to retrieve.</param>
    /// <param name="includeRelationships">Whether to include relationships.</param>
    /// <param name="includeQueries">Whether to include queries.</param>
    /// <param name="includeSchemas">Whether to include schemas.</param>
    /// <param name="outputFormat">Output format.</param>
    /// <param name="outputFile">Output file path.</param>
    /// <param name="verbose">Whether verbose logging is enabled.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task ExecuteAsync(
        string typeId,
        bool includeRelationships,
        bool includeQueries,
        bool includeSchemas,
        string outputFormat,
        string? outputFile,
        bool verbose)
    {
        _logger?.LogInformation("Getting type information for: {TypeId}", typeId);

        try
        {
            // Get type information
            var typeInfo = await GetTypeInfoAsync(typeId, includeRelationships, includeQueries, includeSchemas);

            if (typeInfo == null)
            {
                throw new ArgumentException($"Type with ID '{typeId}' not found");
            }

            // Output results
            await OutputResultsAsync(typeInfo, outputFormat, outputFile);

            _logger?.LogInformation("Type information retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get type information");
            throw;
        }
    }

    private async Task<TypeInfoResponse?> GetTypeInfoAsync(
        string typeId,
        bool includeRelationships,
        bool includeQueries,
        bool includeSchemas)
    {
        try
        {
            // Get the code type
            var codeType = await GetCodeTypeAsync(typeId);
            if (codeType == null)
            {
                return null;
            }

            var typeInfo = new TypeInfoResponse
            {
                Id = codeType.Id,
                Name = codeType.Name,
                Namespace = codeType.Namespace,
                Assembly = codeType.Assembly,
                Fields = codeType.Fields.Select(f => new FieldInfo
                {
                    Name = f.Name,
                    Type = f.Type,
                    IsNullable = f.IsNullable,
                    BSONAttributes = f.BSONAttributes.Select(a => new BSONAttributeInfo
                    {
                        Name = a.Name,
                        Value = a.Value,
                        Parameters = a.Parameters
                    }).ToList()
                }).ToList(),
                BSONAttributes = codeType.BSONAttributes.Select(a => new BSONAttributeInfo
                {
                    Name = a.Name,
                    Value = a.Value,
                    Parameters = a.Parameters
                }).ToList(),
                Nullability = codeType.Nullability,
                Discriminators = codeType.Discriminators,
                Provenance = codeType.Provenance
            };

            // Get collection mappings
            typeInfo.CollectionMappings = await GetCollectionMappingsAsync(typeId);

            // Get relationships if requested
            if (includeRelationships)
            {
                typeInfo.Relationships = await GetRelationshipsAsync(typeId);
            }

            // Get query operations if requested
            if (includeQueries)
            {
                typeInfo.QueryOperations = await GetQueryOperationsAsync(typeId);
            }

            // Get observed schemas if requested
            if (includeSchemas)
            {
                typeInfo.ObservedSchemas = await GetObservedSchemasAsync(typeId);
            }

            return typeInfo;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get type information for {TypeId}", typeId);
            throw;
        }
    }

    private async Task<CodeType?> GetCodeTypeAsync(string typeId)
    {
        var collection = _database.GetCollection<CodeType>("code_types");
        var filter = Builders<CodeType>.Filter.Eq(ct => ct.Id, typeId);
        return await collection.Find(filter).FirstOrDefaultAsync();
    }

    private async Task<List<CollectionMappingInfo>> GetCollectionMappingsAsync(string typeId)
    {
        var collection = _database.GetCollection<CollectionMapping>("collection_mappings");
        var filter = Builders<CollectionMapping>.Filter.Eq(cm => cm.TypeId, typeId);
        var mappings = await collection.Find(filter).ToListAsync();

        return mappings.Select(m => new CollectionMappingInfo
        {
            Id = m.Id,
            CollectionName = m.CollectionName,
            ResolutionMethod = m.ResolutionMethod.ToString(),
            Confidence = m.Confidence
        }).ToList();
    }

    private async Task<List<DataRelationshipInfo>> GetRelationshipsAsync(string typeId)
    {
        var collection = _database.GetCollection<DataRelationship>("data_relationships");
        var filter = Builders<DataRelationship>.Filter.Or(
            Builders<DataRelationship>.Filter.Eq(dr => dr.SourceTypeId, typeId),
            Builders<DataRelationship>.Filter.Eq(dr => dr.TargetTypeId, typeId));
        var relationships = await collection.Find(filter).ToListAsync();

        return relationships.Select(r => new DataRelationshipInfo
        {
            Id = r.Id,
            SourceTypeId = r.SourceTypeId,
            TargetTypeId = r.TargetTypeId,
            RelationshipType = r.RelationshipType.ToString(),
            Confidence = r.Confidence,
            FieldPath = r.FieldPath,
            IsBidirectional = r.IsBidirectional,
            Cardinality = r.Cardinality.ToString()
        }).ToList();
    }

    private async Task<List<QueryOperationInfo>> GetQueryOperationsAsync(string typeId)
    {
        // Get collection mappings for this type
        var collectionMappings = await GetCollectionMappingsAsync(typeId);
        var collectionIds = collectionMappings.Select(cm => cm.Id).ToList();

        if (collectionIds.Count == 0)
        {
            return new List<QueryOperationInfo>();
        }

        var collection = _database.GetCollection<QueryOperation>("query_operations");
        var filter = Builders<QueryOperation>.Filter.In(qo => qo.CollectionId, collectionIds);
        var operations = await collection.Find(filter).ToListAsync();

        return operations.Select(o => new QueryOperationInfo
        {
            Id = o.Id,
            OperationType = o.OperationType.ToString(),
            CollectionId = o.CollectionId,
            Filters = o.Filters.Select(f => new FilterExpressionInfo
            {
                FieldPath = f.FieldPath,
                Operator = f.Operator,
                Value = f.Value?.ToString(),
                IsNegated = f.IsNegated
            }).ToList(),
            Projections = o.Projections.Select(p => new ProjectionExpressionInfo
            {
                FieldPath = p.FieldPath,
                IsIncluded = p.IsIncluded,
                Expression = p.Expression
            }).ToList(),
            Sort = o.Sort.Select(s => new SortExpressionInfo
            {
                FieldPath = s.FieldPath,
                Direction = s.Direction,
                Priority = s.Priority
            }).ToList(),
            Limit = o.Limit,
            Skip = o.Skip
        }).ToList();
    }

    private async Task<List<ObservedSchemaInfo>> GetObservedSchemasAsync(string typeId)
    {
        // Get collection mappings for this type
        var collectionMappings = await GetCollectionMappingsAsync(typeId);
        var collectionIds = collectionMappings.Select(cm => cm.Id).ToList();

        if (collectionIds.Count == 0)
        {
            return new List<ObservedSchemaInfo>();
        }

        var collection = _database.GetCollection<ObservedSchema>("observed_schemas");
        var filter = Builders<ObservedSchema>.Filter.In(os => os.CollectionId, collectionIds);
        var schemas = await collection.Find(filter).ToListAsync();

        return schemas.Select(s => new ObservedSchemaInfo
        {
            Id = s.Id,
            CollectionId = s.CollectionId,
            Schema = s.Schema,
            TypeFrequencies = s.TypeFrequencies,
            RequiredFields = s.RequiredFields,
            StringFormats = s.StringFormats.Select(sf => new StringFormatInfo
            {
                FieldName = sf.FieldName,
                Pattern = sf.Pattern,
                Frequency = sf.Frequency,
                Confidence = sf.Confidence
            }).ToList(),
            EnumCandidates = s.EnumCandidates.Select(ec => new EnumCandidateInfo
            {
                FieldName = ec.FieldName,
                Values = ec.Values,
                ValueFrequencies = ec.ValueFrequencies,
                Confidence = ec.Confidence,
                IsGoodCandidate = ec.IsGoodCandidate,
                DistinctValueCount = ec.DistinctValueCount
            }).ToList(),
            SampleSize = s.SampleSize,
            PIIRedacted = s.PIIRedacted,
            SampledAt = s.SampledAt
        }).ToList();
    }

    private async Task OutputResultsAsync(TypeInfoResponse typeInfo, string outputFormat, string? outputFile)
    {
        string output;

        switch (outputFormat.ToLowerInvariant())
        {
            case "json":
                output = JsonSerializer.Serialize(typeInfo, new JsonSerializerOptions { WriteIndented = true });
                break;
            case "yaml":
                // This would use a YAML serializer
                output = "YAML output not implemented";
                break;
            case "csv":
                // This would generate CSV output
                output = "CSV output not implemented";
                break;
            default:
                throw new ArgumentException($"Unsupported output format: {outputFormat}");
        }

        if (!string.IsNullOrEmpty(outputFile))
        {
            await File.WriteAllTextAsync(outputFile, output);
            _logger?.LogInformation("Type information written to {OutputFile}", outputFile);
        }
        else
        {
            Console.WriteLine(output);
        }
    }
}

/// <summary>
/// Response containing detailed type information.
/// </summary>
public class TypeInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Assembly { get; set; } = string.Empty;
    public List<FieldInfo> Fields { get; set; } = new();
    public List<BSONAttributeInfo> BSONAttributes { get; set; } = new();
    public NullabilityInfo? Nullability { get; set; }
    public List<string> Discriminators { get; set; } = new();
    public List<CollectionMappingInfo> CollectionMappings { get; set; } = new();
    public List<DataRelationshipInfo> Relationships { get; set; } = new();
    public List<QueryOperationInfo> QueryOperations { get; set; } = new();
    public List<ObservedSchemaInfo> ObservedSchemas { get; set; } = new();
    public ProvenanceRecord Provenance { get; set; } = new();
}

/// <summary>
/// Information about a field.
/// </summary>
public class FieldInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public List<BSONAttributeInfo> BSONAttributes { get; set; } = new();
}

/// <summary>
/// Information about a BSON attribute.
/// </summary>
public class BSONAttributeInfo
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Information about a collection mapping.
/// </summary>
public class CollectionMappingInfo
{
    public string Id { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public string ResolutionMethod { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Information about a data relationship.
/// </summary>
public class DataRelationshipInfo
{
    public string Id { get; set; } = string.Empty;
    public string SourceTypeId { get; set; } = string.Empty;
    public string TargetTypeId { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? FieldPath { get; set; }
    public bool IsBidirectional { get; set; }
    public string Cardinality { get; set; } = string.Empty;
}

/// <summary>
/// Information about a query operation.
/// </summary>
public class QueryOperationInfo
{
    public string Id { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string CollectionId { get; set; } = string.Empty;
    public List<FilterExpressionInfo> Filters { get; set; } = new();
    public List<ProjectionExpressionInfo> Projections { get; set; } = new();
    public List<SortExpressionInfo> Sort { get; set; } = new();
    public int? Limit { get; set; }
    public int? Skip { get; set; }
}

/// <summary>
/// Information about a filter expression.
/// </summary>
public class FilterExpressionInfo
{
    public string FieldPath { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool IsNegated { get; set; }
}

/// <summary>
/// Information about a projection expression.
/// </summary>
public class ProjectionExpressionInfo
{
    public string FieldPath { get; set; } = string.Empty;
    public bool IsIncluded { get; set; }
    public string? Expression { get; set; }
}

/// <summary>
/// Information about a sort expression.
/// </summary>
public class SortExpressionInfo
{
    public string FieldPath { get; set; } = string.Empty;
    public int Direction { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Information about an observed schema.
/// </summary>
public class ObservedSchemaInfo
{
    public string Id { get; set; } = string.Empty;
    public string CollectionId { get; set; } = string.Empty;
    public Dictionary<string, object> Schema { get; set; } = new();
    public Dictionary<string, double> TypeFrequencies { get; set; } = new();
    public List<string> RequiredFields { get; set; } = new();
    public List<StringFormatInfo> StringFormats { get; set; } = new();
    public List<EnumCandidateInfo> EnumCandidates { get; set; } = new();
    public int SampleSize { get; set; }
    public bool PIIRedacted { get; set; }
    public DateTime SampledAt { get; set; }
}

/// <summary>
/// Information about a string format.
/// </summary>
public class StringFormatInfo
{
    public string FieldName { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Information about an enum candidate.
/// </summary>
public class EnumCandidateInfo
{
    public string FieldName { get; set; } = string.Empty;
    public string[] Values { get; set; } = Array.Empty<string>();
    public Dictionary<string, double> ValueFrequencies { get; set; } = new();
    public double Confidence { get; set; }
    public bool IsGoodCandidate { get; set; }
    public int DistinctValueCount { get; set; }
}
