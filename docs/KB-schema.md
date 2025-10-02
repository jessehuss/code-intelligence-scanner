# Knowledge Base Schema Documentation

## Overview

The Code Intelligence Scanner & Knowledge Base Seeder creates a comprehensive knowledge base in MongoDB that captures code intelligence, data relationships, and observed schemas from C#/.NET microservices. This document describes the complete schema structure and relationships.

## Database Structure

The knowledge base is stored in a MongoDB database named `catalog_kb` with the following collections:

- `code_types` - C# POCO classes and their metadata
- `collection_mappings` - Links between C# types and MongoDB collections
- `query_operations` - MongoDB operations found in code
- `data_relationships` - Inferred relationships between data entities
- `observed_schemas` - JSON schemas inferred from sampled MongoDB data
- `knowledge_base_entries` - Normalized searchable facts
- `provenance_records` - Source tracking for all extracted facts

## Collection Schemas

### code_types

Represents C# POCO classes that map to MongoDB documents.

```json
{
  "_id": "ObjectId",
  "name": "string",
  "namespace": "string",
  "assembly": "string",
  "fields": [
    {
      "name": "string",
      "type": "string",
      "isNullable": "boolean",
      "bsonAttributes": [
        {
          "name": "string",
          "value": "string",
          "parameters": {}
        }
      ]
    }
  ],
  "bsonAttributes": [
    {
      "name": "string",
      "value": "string",
      "parameters": {}
    }
  ],
  "nullability": {},
  "discriminators": ["string"],
  "provenance": {
    "repository": "string",
    "filePath": "string",
    "symbol": "string",
    "lineSpan": {
      "start": "number",
      "end": "number"
    },
    "commitSHA": "string",
    "timestamp": "ISODate",
    "extractorVersion": "string"
  }
}
```

**Indexes:**
- `{ "name": 1, "namespace": 1 }` - Unique compound index
- `{ "provenance.repository": 1 }` - Repository-based queries
- `{ "provenance.filePath": 1 }` - File-based queries

### collection_mappings

Links C# types to MongoDB collection names with confidence scores.

```json
{
  "_id": "ObjectId",
  "typeId": "ObjectId", // Reference to code_types._id
  "collectionName": "string",
  "resolutionMethod": "string", // "literal", "constant", "config", "inferred"
  "confidence": "number", // 0.0 - 1.0
  "provenance": {
    "repository": "string",
    "filePath": "string",
    "symbol": "string",
    "lineSpan": {
      "start": "number",
      "end": "number"
    },
    "commitSHA": "string",
    "timestamp": "ISODate",
    "extractorVersion": "string"
  }
}
```

**Indexes:**
- `{ "typeId": 1 }` - Type-based queries
- `{ "collectionName": 1 }` - Collection-based queries
- `{ "confidence": -1 }` - Confidence-based sorting

### query_operations

MongoDB operations found in code with their parameters and context.

```json
{
  "_id": "ObjectId",
  "operationType": "string", // "Find", "Update", "Aggregate", "Replace", "Delete"
  "collectionId": "ObjectId", // Reference to collection_mappings._id
  "filters": {}, // BSON document representing filter conditions
  "projections": {}, // BSON document representing projection
  "sort": {}, // BSON document representing sort criteria
  "limit": "number",
  "skip": "number",
  "aggregationPipeline": [], // Array of BSON documents for aggregation stages
  "provenance": {
    "repository": "string",
    "filePath": "string",
    "symbol": "string",
    "lineSpan": {
      "start": "number",
      "end": "number"
    },
    "commitSHA": "string",
    "timestamp": "ISODate",
    "extractorVersion": "string"
  }
}
```

**Indexes:**
- `{ "collectionId": 1 }` - Collection-based queries
- `{ "operationType": 1 }` - Operation type queries
- `{ "provenance.repository": 1 }` - Repository-based queries

### data_relationships

Inferred relationships between data entities based on code analysis.

```json
{
  "_id": "ObjectId",
  "sourceTypeId": "ObjectId", // Reference to code_types._id
  "targetTypeId": "ObjectId", // Reference to code_types._id
  "relationshipType": "string", // "REFERS_TO", "LOOKUP", "EMBEDDED"
  "confidence": "number", // 0.0 - 1.0
  "evidence": "string", // Description of evidence for the relationship
  "provenance": {
    "repository": "string",
    "filePath": "string",
    "symbol": "string",
    "lineSpan": {
      "start": "number",
      "end": "number"
    },
    "commitSHA": "string",
    "timestamp": "ISODate",
    "extractorVersion": "string"
  }
}
```

**Indexes:**
- `{ "sourceTypeId": 1 }` - Source type queries
- `{ "targetTypeId": 1 }` - Target type queries
- `{ "relationshipType": 1 }` - Relationship type queries
- `{ "confidence": -1 }` - Confidence-based sorting

### observed_schemas

JSON schemas inferred from sampled MongoDB data with PII redaction.

```json
{
  "_id": "ObjectId",
  "collectionId": "ObjectId", // Reference to collection_mappings._id
  "schema": {}, // JSON Schema definition
  "typeFrequencies": {
    "string": "number",
    "number": "number",
    "boolean": "number",
    "object": "number",
    "array": "number"
  },
  "requiredFields": ["string"],
  "stringFormats": {
    "fieldName": "string" // "email", "date-time", "uri", etc.
  },
  "enumCandidates": {
    "fieldName": ["string"] // Array of possible enum values
  },
  "sampleSize": "number",
  "piiRedacted": "boolean",
  "provenance": {
    "repository": "string",
    "filePath": "string",
    "symbol": "string",
    "lineSpan": {
      "start": "number",
      "end": "number"
    },
    "commitSHA": "string",
    "timestamp": "ISODate",
    "extractorVersion": "string"
  }
}
```

**Indexes:**
- `{ "collectionId": 1 }` - Collection-based queries
- `{ "sampleSize": -1 }` - Sample size sorting
- `{ "piiRedacted": 1 }` - PII redaction queries

### knowledge_base_entries

Normalized searchable facts for text search and graph queries.

```json
{
  "_id": "ObjectId",
  "entityType": "string", // "CodeType", "CollectionMapping", "QueryOperation", etc.
  "entityId": "ObjectId", // Reference to the actual entity's _id
  "searchableText": "string", // Normalized text for search
  "tags": ["string"], // Categorization tags
  "relationships": ["ObjectId"], // References to other KnowledgeBaseEntry _ids
  "lastUpdated": "ISODate",
  "provenance": {
    "repository": "string",
    "filePath": "string",
    "symbol": "string",
    "lineSpan": {
      "start": "number",
      "end": "number"
    },
    "commitSHA": "string",
    "timestamp": "ISODate",
    "extractorVersion": "string"
  }
}
```

**Indexes:**
- `{ "entityType": 1 }` - Entity type queries
- `{ "entityId": 1 }` - Entity ID queries
- `{ "tags": 1 }` - Tag-based queries
- `{ "lastUpdated": -1 }` - Recency sorting

### provenance_records

Source tracking for all extracted facts with Git integration.

```json
{
  "_id": "ObjectId",
  "repository": "string",
  "filePath": "string",
  "symbol": "string",
  "lineSpan": {
    "start": "number",
    "end": "number"
  },
  "commitSHA": "string",
  "timestamp": "ISODate",
  "extractorVersion": "string"
}
```

**Indexes:**
- `{ "repository": 1 }` - Repository-based queries
- `{ "filePath": 1 }` - File-based queries
- `{ "commitSHA": 1 }` - Commit-based queries
- `{ "timestamp": -1 }` - Temporal queries

## Atlas Search Indexes

The knowledge base includes Atlas Search indexes for full-text search capabilities:

### knowledge_base_entries_search

```json
{
  "mappings": {
    "fields": {
      "searchableText": {
        "type": "string",
        "analyzer": "lucene.standard"
      },
      "entityType": {
        "type": "string",
        "analyzer": "lucene.keyword"
      },
      "tags": {
        "type": "string",
        "analyzer": "lucene.standard"
      },
      "provenance.repository": {
        "type": "string",
        "analyzer": "lucene.keyword"
      },
      "provenance.filePath": {
        "type": "string",
        "analyzer": "lucene.keyword"
      }
    }
  }
}
```

### code_types_search

```json
{
  "mappings": {
    "fields": {
      "name": {
        "type": "string",
        "analyzer": "lucene.standard"
      },
      "namespace": {
        "type": "string",
        "analyzer": "lucene.keyword"
      },
      "fields.name": {
        "type": "string",
        "analyzer": "lucene.standard"
      },
      "fields.type": {
        "type": "string",
        "analyzer": "lucene.keyword"
      }
    }
  }
}
```

## Data Relationships

The knowledge base captures several types of relationships:

### 1. Type-to-Collection Mappings
- **One-to-One**: Each `CodeType` can have multiple `CollectionMapping` entries
- **Resolution Methods**: Literal strings, constants, configuration values, or inferred names
- **Confidence Scoring**: Based on resolution method and evidence quality

### 2. Operation-to-Collection Links
- **Many-to-One**: Multiple `QueryOperation` entries can reference the same `CollectionMapping`
- **Operation Types**: Find, Update, Insert, Delete, Aggregate operations
- **Parameter Extraction**: Filters, projections, sort criteria, and aggregation pipelines

### 3. Data Entity Relationships
- **Foreign Key References**: Inferred from field names and types
- **Lookup Operations**: Detected from `$lookup` aggregation stages
- **Embedded Documents**: Identified from nested object types
- **Confidence Scoring**: Based on evidence strength and consistency

### 4. Schema Observations
- **Type Inference**: From sampled MongoDB documents
- **PII Redaction**: Automatic detection and redaction of sensitive data
- **Format Detection**: Email, date-time, URI patterns
- **Enum Candidates**: Value frequency analysis for potential enumerations

## Query Patterns

### Common Queries

1. **Find all types in a repository:**
```javascript
db.code_types.find({ "provenance.repository": "my-repo" })
```

2. **Find collection mappings for a type:**
```javascript
db.collection_mappings.find({ "typeId": ObjectId("...") })
```

3. **Find operations on a collection:**
```javascript
db.query_operations.find({ "collectionId": ObjectId("...") })
```

4. **Find relationships for a type:**
```javascript
db.data_relationships.find({ 
  $or: [
    { "sourceTypeId": ObjectId("...") },
    { "targetTypeId": ObjectId("...") }
  ]
})
```

5. **Search for text across all entities:**
```javascript
db.knowledge_base_entries.aggregate([
  { $search: { 
    index: "knowledge_base_entries_search",
    text: { query: "user", path: ["searchableText", "tags"] }
  }}
])
```

### Graph Queries

For graph-based analysis, the knowledge base can be exported to Neo4j:

```cypher
// Find all types and their relationships
MATCH (t:CodeType)-[r:DATA_RELATIONSHIP]->(t2:CodeType)
RETURN t, r, t2

// Find the path from a type to its collection
MATCH (t:CodeType)-[:MAPS_TO]->(c:CollectionMapping)
RETURN t, c

// Find all operations on a collection
MATCH (c:CollectionMapping)<-[:OPERATES_ON]-(o:QueryOperation)
RETURN c, o
```

## Data Quality and Validation

### Validation Rules

1. **Required Fields**: All entities must have valid provenance information
2. **Confidence Scores**: Must be between 0.0 and 1.0
3. **Object References**: All foreign key references must be valid
4. **Timestamp Consistency**: Provenance timestamps must be reasonable
5. **Version Tracking**: Extractor version must be present and valid

### Data Integrity

1. **Referential Integrity**: Foreign key references are validated
2. **Uniqueness Constraints**: Compound indexes ensure uniqueness
3. **Data Consistency**: Regular integrity checks validate relationships
4. **Provenance Tracking**: Every fact includes complete source information

## Performance Considerations

### Indexing Strategy

1. **Compound Indexes**: Optimize for common query patterns
2. **Partial Indexes**: Filter out irrelevant data
3. **Text Indexes**: Enable full-text search capabilities
4. **TTL Indexes**: Automatic cleanup of old data

### Query Optimization

1. **Projection**: Only return required fields
2. **Limit**: Restrict result sets for large queries
3. **Sort**: Use indexed fields for sorting
4. **Aggregation**: Use aggregation pipelines for complex queries

### Storage Optimization

1. **Compression**: Enable MongoDB compression
2. **Sharding**: Distribute large datasets across shards
3. **Archiving**: Move old data to archive collections
4. **Cleanup**: Regular cleanup of orphaned records

## Security and Privacy

### PII Protection

1. **Automatic Detection**: Heuristic-based PII detection
2. **Redaction**: Replace PII with placeholder values
3. **Audit Trail**: Track all PII redaction activities
4. **Compliance**: Ensure GDPR and other privacy regulations

### Access Control

1. **Read-Only Access**: Limit write access to authorized users
2. **Role-Based Access**: Different access levels for different users
3. **Audit Logging**: Track all access and modifications
4. **Encryption**: Encrypt sensitive data at rest and in transit

## Maintenance and Monitoring

### Regular Maintenance

1. **Index Rebuilding**: Rebuild indexes for optimal performance
2. **Data Cleanup**: Remove orphaned and outdated records
3. **Schema Updates**: Update schemas as code evolves
4. **Backup and Recovery**: Regular backups and recovery testing

### Monitoring

1. **Performance Metrics**: Track query performance and response times
2. **Data Quality**: Monitor data accuracy and completeness
3. **Usage Patterns**: Analyze query patterns and optimize accordingly
4. **Error Tracking**: Monitor and alert on data quality issues

## Future Enhancements

### Planned Features

1. **Real-time Updates**: Live synchronization with code changes
2. **Advanced Analytics**: Machine learning-based relationship detection
3. **API Integration**: RESTful APIs for knowledge base access
4. **Visualization**: Graph visualization tools for data relationships
5. **Export Formats**: Support for various export formats (JSON, CSV, GraphML)

### Scalability Improvements

1. **Horizontal Scaling**: Support for multiple MongoDB instances
2. **Caching**: Redis-based caching for frequently accessed data
3. **Streaming**: Real-time data streaming for large datasets
4. **Partitioning**: Data partitioning strategies for large collections
