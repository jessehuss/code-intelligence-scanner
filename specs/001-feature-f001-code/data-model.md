# Data Model: Code Intelligence Scanner & Knowledge Base Seeder

**Date**: 2025-01-27  
**Feature**: 001-feature-f001-code  
**Status**: Complete

## Entity Definitions

### Code Type
Represents a C# POCO class with its fields, BSON attributes, nullability, and discriminators.

**Fields**:
- `Id`: Unique identifier (string)
- `Name`: Class name (string)
- `Namespace`: Full namespace (string)
- `Assembly`: Assembly name (string)
- `Fields`: List of field definitions
- `BSONAttributes`: List of BSON attribute configurations
- `Nullability`: Nullable reference type information
- `Discriminators`: Type discrimination information
- `Provenance`: Source information (repository, file, symbol, line span, commit SHA, timestamp)

**Validation Rules**:
- Name must be non-empty
- Namespace must be valid C# namespace format
- Fields must have valid C# identifiers
- Provenance must be complete for every instance

### Collection Mapping
Represents the relationship between a C# type and its MongoDB collection name.

**Fields**:
- `Id`: Unique identifier (string)
- `TypeId`: Reference to Code Type (string)
- `CollectionName`: MongoDB collection name (string)
- `ResolutionMethod`: How the collection name was resolved (literal, constant, config, etc.)
- `Confidence`: Resolution confidence score (0.0-1.0)
- `Provenance`: Source information

**Validation Rules**:
- CollectionName must be valid MongoDB collection name
- Confidence must be between 0.0 and 1.0
- TypeId must reference existing Code Type

### Query Operation
Represents a MongoDB operation (Find, Update, etc.) with its filters, projections, and target collection.

**Fields**:
- `Id`: Unique identifier (string)
- `OperationType`: Type of operation (Find, Update, Aggregate, Replace, Delete)
- `CollectionId`: Reference to Collection Mapping (string)
- `Filters`: Query filter expressions
- `Projections`: Field projection specifications
- `Sort`: Sort specifications
- `Limit`: Result limit
- `Skip`: Result skip count
- `AggregationPipeline`: For aggregate operations
- `Provenance`: Source information

**Validation Rules**:
- OperationType must be valid MongoDB operation
- CollectionId must reference existing Collection Mapping
- Filters must be valid MongoDB query syntax

### Data Relationship
Represents inferred connections between types based on query patterns and $lookup operations.

**Fields**:
- `Id`: Unique identifier (string)
- `SourceTypeId`: Source type reference (string)
- `TargetTypeId`: Target type reference (string)
- `RelationshipType`: Type of relationship (REFERS_TO, LOOKUP, EMBEDDED)
- `Confidence`: Relationship confidence score (0.0-1.0)
- `Evidence`: Supporting evidence for the relationship
- `Provenance`: Source information

**Validation Rules**:
- SourceTypeId and TargetTypeId must reference existing Code Types
- RelationshipType must be valid relationship type
- Confidence must be between 0.0 and 1.0

### Provenance Record
Represents the source information for any extracted fact.

**Fields**:
- `Repository`: Repository name or URL (string)
- `FilePath`: File path within repository (string)
- `Symbol`: Symbol name (string)
- `LineSpan`: Line number range (start, end)
- `CommitSHA`: Git commit SHA (string)
- `Timestamp`: Extraction timestamp (DateTime)
- `ExtractorVersion`: Version of the extraction tool (string)

**Validation Rules**:
- Repository must be non-empty
- FilePath must be valid file path
- CommitSHA must be valid Git SHA format
- Timestamp must be valid DateTime

### Observed Schema
Represents the inferred JSON Schema from sampled MongoDB data with type frequencies and patterns.

**Fields**:
- `Id`: Unique identifier (string)
- `CollectionId`: Reference to Collection Mapping (string)
- `Schema`: JSON Schema definition
- `TypeFrequencies`: Frequency of each data type
- `RequiredFields`: List of required field names
- `StringFormats`: Detected string format patterns
- `EnumCandidates`: Potential enum values
- `SampleSize`: Number of documents sampled
- `PIIRedacted`: Whether PII was detected and redacted
- `Provenance`: Source information

**Validation Rules**:
- CollectionId must reference existing Collection Mapping
- Schema must be valid JSON Schema
- SampleSize must be positive integer
- TypeFrequencies must sum to 1.0

### Knowledge Base Entry
Represents a normalized fact stored in the searchable knowledge base with its provenance.

**Fields**:
- `Id`: Unique identifier (string)
- `EntityType`: Type of entity (CodeType, CollectionMapping, QueryOperation, etc.)
- `EntityId`: Reference to the specific entity (string)
- `SearchableText`: Text content for search indexing
- `Tags`: List of search tags
- `Relationships`: List of related entity references
- `LastUpdated`: Last update timestamp (DateTime)
- `Provenance`: Source information

**Validation Rules**:
- EntityType must be valid entity type
- EntityId must reference existing entity
- SearchableText must be non-empty
- LastUpdated must be valid DateTime

## Relationship Definitions

### Primary Relationships
- **Code Type** → **Collection Mapping** (1:many)
- **Collection Mapping** → **Query Operation** (1:many)
- **Code Type** → **Data Relationship** (1:many, both as source and target)
- **Collection Mapping** → **Observed Schema** (1:1)
- **All Entities** → **Provenance Record** (1:1)
- **All Entities** → **Knowledge Base Entry** (1:1)

### Secondary Relationships
- **Query Operation** → **Data Relationship** (evidence source)
- **Observed Schema** → **Data Relationship** (type inference)

## Data Integrity Rules

1. **Referential Integrity**: All foreign key references must point to existing entities
2. **Provenance Completeness**: Every entity must have complete provenance information
3. **Temporal Consistency**: Timestamps must be in chronological order
4. **Confidence Bounds**: All confidence scores must be between 0.0 and 1.0
5. **Schema Validity**: All JSON Schema definitions must be valid
6. **PII Compliance**: No actual PII data may be stored, only structural information

## Indexing Strategy

### Primary Indexes
- All entities: `Id` (unique)
- Code Type: `Name`, `Namespace`
- Collection Mapping: `CollectionName`, `TypeId`
- Query Operation: `OperationType`, `CollectionId`
- Provenance Record: `Repository`, `CommitSHA`, `Timestamp`

### Secondary Indexes
- Knowledge Base Entry: `SearchableText` (text index)
- Data Relationship: `SourceTypeId`, `TargetTypeId`
- Observed Schema: `CollectionId`, `SampleSize`

## Data Retention Policy

- **Active Data**: Keep all current and recent scan results
- **Historical Data**: Archive scans older than 1 year
- **Provenance Data**: Retain indefinitely for audit purposes
- **PII Data**: Never stored, only structural information preserved
