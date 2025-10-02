using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.Json;

namespace Cataloger.Scanner.KnowledgeBase;

/// <summary>
/// Service for writing extracted data to the knowledge base.
/// </summary>
public class KnowledgeBaseWriter
{
    private readonly ILogger<KnowledgeBaseWriter> _logger;
    private readonly IMongoDatabase _database;

    public KnowledgeBaseWriter(ILogger<KnowledgeBaseWriter> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    /// <summary>
    /// Writes code types to the knowledge base.
    /// </summary>
    /// <param name="codeTypes">List of code types to write.</param>
    /// <returns>Number of code types written.</returns>
    public async Task<int> WriteCodeTypesAsync(List<CodeType> codeTypes)
    {
        if (codeTypes.Count == 0)
        {
            return 0;
        }

        _logger.LogDebug("Writing {Count} code types to knowledge base", codeTypes.Count);

        try
        {
            var collection = _database.GetCollection<CodeType>("code_types");
            
            // Use upsert to handle updates
            var bulkOps = new List<WriteModel<CodeType>>();
            
            foreach (var codeType in codeTypes)
            {
                var filter = Builders<CodeType>.Filter.Eq(ct => ct.Id, codeType.Id);
                var update = Builders<CodeType>.Update
                    .Set(ct => ct.Name, codeType.Name)
                    .Set(ct => ct.Namespace, codeType.Namespace)
                    .Set(ct => ct.Assembly, codeType.Assembly)
                    .Set(ct => ct.Fields, codeType.Fields)
                    .Set(ct => ct.BSONAttributes, codeType.BSONAttributes)
                    .Set(ct => ct.Nullability, codeType.Nullability)
                    .Set(ct => ct.Discriminators, codeType.Discriminators)
                    .Set(ct => ct.Provenance, codeType.Provenance)
                    .Set(ct => ct.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(ct => ct.CreatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<CodeType>(filter, update) { IsUpsert = true });
            }

            var result = await collection.BulkWriteAsync(bulkOps);
            
            _logger.LogInformation("Wrote {Count} code types to knowledge base", result.UpsertedCount + result.ModifiedCount);
            return (int)(result.UpsertedCount + result.ModifiedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write code types to knowledge base");
            throw;
        }
    }

    /// <summary>
    /// Writes collection mappings to the knowledge base.
    /// </summary>
    /// <param name="collectionMappings">List of collection mappings to write.</param>
    /// <returns>Number of collection mappings written.</returns>
    public async Task<int> WriteCollectionMappingsAsync(List<CollectionMapping> collectionMappings)
    {
        if (collectionMappings.Count == 0)
        {
            return 0;
        }

        _logger.LogDebug("Writing {Count} collection mappings to knowledge base", collectionMappings.Count);

        try
        {
            var collection = _database.GetCollection<CollectionMapping>("collection_mappings");
            
            var bulkOps = new List<WriteModel<CollectionMapping>>();
            
            foreach (var mapping in collectionMappings)
            {
                var filter = Builders<CollectionMapping>.Filter.Eq(cm => cm.Id, mapping.Id);
                var update = Builders<CollectionMapping>.Update
                    .Set(cm => cm.TypeId, mapping.TypeId)
                    .Set(cm => cm.CollectionName, mapping.CollectionName)
                    .Set(cm => cm.ResolutionMethod, mapping.ResolutionMethod)
                    .Set(cm => cm.Confidence, mapping.Confidence)
                    .Set(cm => cm.ResolutionContext, mapping.ResolutionContext)
                    .Set(cm => cm.IsPrimary, mapping.IsPrimary)
                    .Set(cm => cm.Alternatives, mapping.Alternatives)
                    .Set(cm => cm.Provenance, mapping.Provenance)
                    .Set(cm => cm.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(cm => cm.CreatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<CollectionMapping>(filter, update) { IsUpsert = true });
            }

            var result = await collection.BulkWriteAsync(bulkOps);
            
            _logger.LogInformation("Wrote {Count} collection mappings to knowledge base", result.UpsertedCount + result.ModifiedCount);
            return (int)(result.UpsertedCount + result.ModifiedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write collection mappings to knowledge base");
            throw;
        }
    }

    /// <summary>
    /// Writes query operations to the knowledge base.
    /// </summary>
    /// <param name="queryOperations">List of query operations to write.</param>
    /// <returns>Number of query operations written.</returns>
    public async Task<int> WriteQueryOperationsAsync(List<QueryOperation> queryOperations)
    {
        if (queryOperations.Count == 0)
        {
            return 0;
        }

        _logger.LogDebug("Writing {Count} query operations to knowledge base", queryOperations.Count);

        try
        {
            var collection = _database.GetCollection<QueryOperation>("query_operations");
            
            var bulkOps = new List<WriteModel<QueryOperation>>();
            
            foreach (var operation in queryOperations)
            {
                var filter = Builders<QueryOperation>.Filter.Eq(qo => qo.Id, operation.Id);
                var update = Builders<QueryOperation>.Update
                    .Set(qo => qo.OperationType, operation.OperationType)
                    .Set(qo => qo.CollectionId, operation.CollectionId)
                    .Set(qo => qo.Filters, operation.Filters)
                    .Set(qo => qo.Projections, operation.Projections)
                    .Set(qo => qo.Sort, operation.Sort)
                    .Set(qo => qo.Limit, operation.Limit)
                    .Set(qo => qo.Skip, operation.Skip)
                    .Set(qo => qo.AggregationPipeline, operation.AggregationPipeline)
                    .Set(qo => qo.IsTransactional, operation.IsTransactional)
                    .Set(qo => qo.HasReadPreference, operation.HasReadPreference)
                    .Set(qo => qo.HasWriteConcern, operation.HasWriteConcern)
                    .Set(qo => qo.Provenance, operation.Provenance)
                    .Set(qo => qo.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(qo => qo.CreatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<QueryOperation>(filter, update) { IsUpsert = true });
            }

            var result = await collection.BulkWriteAsync(bulkOps);
            
            _logger.LogInformation("Wrote {Count} query operations to knowledge base", result.UpsertedCount + result.ModifiedCount);
            return (int)(result.UpsertedCount + result.ModifiedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write query operations to knowledge base");
            throw;
        }
    }

    /// <summary>
    /// Writes data relationships to the knowledge base.
    /// </summary>
    /// <param name="dataRelationships">List of data relationships to write.</param>
    /// <returns>Number of data relationships written.</returns>
    public async Task<int> WriteDataRelationshipsAsync(List<DataRelationship> dataRelationships)
    {
        if (dataRelationships.Count == 0)
        {
            return 0;
        }

        _logger.LogDebug("Writing {Count} data relationships to knowledge base", dataRelationships.Count);

        try
        {
            var collection = _database.GetCollection<DataRelationship>("data_relationships");
            
            var bulkOps = new List<WriteModel<DataRelationship>>();
            
            foreach (var relationship in dataRelationships)
            {
                var filter = Builders<DataRelationship>.Filter.Eq(dr => dr.Id, relationship.Id);
                var update = Builders<DataRelationship>.Update
                    .Set(dr => dr.SourceTypeId, relationship.SourceTypeId)
                    .Set(dr => dr.TargetTypeId, relationship.TargetTypeId)
                    .Set(dr => dr.RelationshipType, relationship.RelationshipType)
                    .Set(dr => dr.Confidence, relationship.Confidence)
                    .Set(dr => dr.Evidence, relationship.Evidence)
                    .Set(dr => dr.FieldPath, relationship.FieldPath)
                    .Set(dr => dr.IsBidirectional, relationship.IsBidirectional)
                    .Set(dr => dr.Cardinality, relationship.Cardinality)
                    .Set(dr => dr.IsRequired, relationship.IsRequired)
                    .Set(dr => dr.Provenance, relationship.Provenance)
                    .Set(dr => dr.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(dr => dr.CreatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<DataRelationship>(filter, update) { IsUpsert = true });
            }

            var result = await collection.BulkWriteAsync(bulkOps);
            
            _logger.LogInformation("Wrote {Count} data relationships to knowledge base", result.UpsertedCount + result.ModifiedCount);
            return (int)(result.UpsertedCount + result.ModifiedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write data relationships to knowledge base");
            throw;
        }
    }

    /// <summary>
    /// Writes observed schemas to the knowledge base.
    /// </summary>
    /// <param name="observedSchemas">List of observed schemas to write.</param>
    /// <returns>Number of observed schemas written.</returns>
    public async Task<int> WriteObservedSchemasAsync(List<ObservedSchema> observedSchemas)
    {
        if (observedSchemas.Count == 0)
        {
            return 0;
        }

        _logger.LogDebug("Writing {Count} observed schemas to knowledge base", observedSchemas.Count);

        try
        {
            var collection = _database.GetCollection<ObservedSchema>("observed_schemas");
            
            var bulkOps = new List<WriteModel<ObservedSchema>>();
            
            foreach (var schema in observedSchemas)
            {
                var filter = Builders<ObservedSchema>.Filter.Eq(os => os.Id, schema.Id);
                var update = Builders<ObservedSchema>.Update
                    .Set(os => os.CollectionId, schema.CollectionId)
                    .Set(os => os.Schema, schema.Schema)
                    .Set(os => os.TypeFrequencies, schema.TypeFrequencies)
                    .Set(os => os.RequiredFields, schema.RequiredFields)
                    .Set(os => os.StringFormats, schema.StringFormats)
                    .Set(os => os.EnumCandidates, schema.EnumCandidates)
                    .Set(os => os.SampleSize, schema.SampleSize)
                    .Set(os => os.PIIRedacted, schema.PIIRedacted)
                    .Set(os => os.PIIDetections, schema.PIIDetections)
                    .Set(os => os.SamplingConfig, schema.SamplingConfig)
                    .Set(os => os.SampledAt, schema.SampledAt)
                    .Set(os => os.Provenance, schema.Provenance)
                    .Set(os => os.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(os => os.CreatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<ObservedSchema>(filter, update) { IsUpsert = true });
            }

            var result = await collection.BulkWriteAsync(bulkOps);
            
            _logger.LogInformation("Wrote {Count} observed schemas to knowledge base", result.UpsertedCount + result.ModifiedCount);
            return (int)(result.UpsertedCount + result.ModifiedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write observed schemas to knowledge base");
            throw;
        }
    }

    /// <summary>
    /// Writes knowledge base entries to the knowledge base.
    /// </summary>
    /// <param name="knowledgeBaseEntries">List of knowledge base entries to write.</param>
    /// <returns>Number of knowledge base entries written.</returns>
    public async Task<int> WriteKnowledgeBaseEntriesAsync(List<KnowledgeBaseEntry> knowledgeBaseEntries)
    {
        if (knowledgeBaseEntries.Count == 0)
        {
            return 0;
        }

        _logger.LogDebug("Writing {Count} knowledge base entries to knowledge base", knowledgeBaseEntries.Count);

        try
        {
            var collection = _database.GetCollection<KnowledgeBaseEntry>("knowledge_base_entries");
            
            var bulkOps = new List<WriteModel<KnowledgeBaseEntry>>();
            
            foreach (var entry in knowledgeBaseEntries)
            {
                var filter = Builders<KnowledgeBaseEntry>.Filter.Eq(kbe => kbe.Id, entry.Id);
                var update = Builders<KnowledgeBaseEntry>.Update
                    .Set(kbe => kbe.EntityType, entry.EntityType)
                    .Set(kbe => kbe.EntityId, entry.EntityId)
                    .Set(kbe => kbe.SearchableText, entry.SearchableText)
                    .Set(kbe => kbe.Tags, entry.Tags)
                    .Set(kbe => kbe.Relationships, entry.Relationships)
                    .Set(kbe => kbe.RelevanceScore, entry.RelevanceScore)
                    .Set(kbe => kbe.IsActive, entry.IsActive)
                    .Set(kbe => kbe.IsIndexed, entry.IsIndexed)
                    .Set(kbe => kbe.Metadata, entry.Metadata)
                    .Set(kbe => kbe.Provenance, entry.Provenance)
                    .Set(kbe => kbe.LastUpdated, DateTime.UtcNow)
                    .Set(kbe => kbe.UpdatedAt, DateTime.UtcNow)
                    .SetOnInsert(kbe => kbe.CreatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<KnowledgeBaseEntry>(filter, update) { IsUpsert = true });
            }

            var result = await collection.BulkWriteAsync(bulkOps);
            
            _logger.LogInformation("Wrote {Count} knowledge base entries to knowledge base", result.UpsertedCount + result.ModifiedCount);
            return (int)(result.UpsertedCount + result.ModifiedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write knowledge base entries to knowledge base");
            throw;
        }
    }

    /// <summary>
    /// Creates indexes for the knowledge base collections.
    /// </summary>
    /// <returns>Task representing the async operation.</returns>
    public async Task CreateIndexesAsync()
    {
        _logger.LogInformation("Creating indexes for knowledge base collections");

        try
        {
            // Create indexes for code_types collection
            var codeTypesCollection = _database.GetCollection<CodeType>("code_types");
            await codeTypesCollection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<CodeType>(Builders<CodeType>.IndexKeys.Ascending(ct => ct.Name)),
                new CreateIndexModel<CodeType>(Builders<CodeType>.IndexKeys.Ascending(ct => ct.Namespace)),
                new CreateIndexModel<CodeType>(Builders<CodeType>.IndexKeys.Ascending(ct => ct.Provenance.Repository))
            });

            // Create indexes for collection_mappings collection
            var collectionMappingsCollection = _database.GetCollection<CollectionMapping>("collection_mappings");
            await collectionMappingsCollection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<CollectionMapping>(Builders<CollectionMapping>.IndexKeys.Ascending(cm => cm.CollectionName)),
                new CreateIndexModel<CollectionMapping>(Builders<CollectionMapping>.IndexKeys.Ascending(cm => cm.TypeId)),
                new CreateIndexModel<CollectionMapping>(Builders<CollectionMapping>.IndexKeys.Ascending(cm => cm.Provenance.Repository))
            });

            // Create indexes for query_operations collection
            var queryOperationsCollection = _database.GetCollection<QueryOperation>("query_operations");
            await queryOperationsCollection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<QueryOperation>(Builders<QueryOperation>.IndexKeys.Ascending(qo => qo.OperationType)),
                new CreateIndexModel<QueryOperation>(Builders<QueryOperation>.IndexKeys.Ascending(qo => qo.CollectionId)),
                new CreateIndexModel<QueryOperation>(Builders<QueryOperation>.IndexKeys.Ascending(qo => qo.Provenance.Repository))
            });

            // Create indexes for data_relationships collection
            var dataRelationshipsCollection = _database.GetCollection<DataRelationship>("data_relationships");
            await dataRelationshipsCollection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<DataRelationship>(Builders<DataRelationship>.IndexKeys.Ascending(dr => dr.SourceTypeId)),
                new CreateIndexModel<DataRelationship>(Builders<DataRelationship>.IndexKeys.Ascending(dr => dr.TargetTypeId)),
                new CreateIndexModel<DataRelationship>(Builders<DataRelationship>.IndexKeys.Ascending(dr => dr.RelationshipType))
            });

            // Create indexes for observed_schemas collection
            var observedSchemasCollection = _database.GetCollection<ObservedSchema>("observed_schemas");
            await observedSchemasCollection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<ObservedSchema>(Builders<ObservedSchema>.IndexKeys.Ascending(os => os.CollectionId)),
                new CreateIndexModel<ObservedSchema>(Builders<ObservedSchema>.IndexKeys.Ascending(os => os.SampleSize))
            });

            // Create indexes for knowledge_base_entries collection
            var knowledgeBaseEntriesCollection = _database.GetCollection<KnowledgeBaseEntry>("knowledge_base_entries");
            await knowledgeBaseEntriesCollection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<KnowledgeBaseEntry>(Builders<KnowledgeBaseEntry>.IndexKeys.Ascending(kbe => kbe.EntityType)),
                new CreateIndexModel<KnowledgeBaseEntry>(Builders<KnowledgeBaseEntry>.IndexKeys.Ascending(kbe => kbe.EntityId)),
                new CreateIndexModel<KnowledgeBaseEntry>(Builders<KnowledgeBaseEntry>.IndexKeys.Ascending(kbe => kbe.IsActive)),
                new CreateIndexModel<KnowledgeBaseEntry>(Builders<KnowledgeBaseEntry>.IndexKeys.Ascending(kbe => kbe.IsIndexed))
            });

            _logger.LogInformation("Successfully created indexes for knowledge base collections");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create indexes for knowledge base collections");
            throw;
        }
    }
}
