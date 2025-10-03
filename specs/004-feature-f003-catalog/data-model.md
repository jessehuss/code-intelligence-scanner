# Data Model: F003 – Catalog Explorer UI

**Date**: 2024-12-19  
**Feature**: Catalog Explorer UI  
**Phase**: 1 - Design & Data Model

## Entity Definitions

### Collection
Represents a data collection with schema information and usage metrics.

**Fields**:
- `id: string` - Unique identifier
- `name: string` - Collection name
- `declaredSchema: SchemaField[]` - Declared schema fields
- `observedSchema: SchemaField[]` - Observed schema from sampling
- `presenceMetrics: PresenceMetrics` - Field presence statistics
- `driftIndicators: DriftIndicator[]` - Schema drift indicators
- `types: TypeReference[]` - Types used in this collection
- `queries: QueryReference[]` - Queries operating on this collection
- `relationships: Relationship[]` - Relationships to other entities
- `provenance: ProvenanceRecord` - Source information

**Validation Rules**:
- `name` must be non-empty string
- `declaredSchema` and `observedSchema` must be valid arrays
- `presenceMetrics` must have valid percentage values (0-100)

### Type
Represents a code type/class with field definitions and usage information.

**Fields**:
- `id: string` - Unique identifier (FQCN)
- `name: string` - Type name
- `namespace: string` - Namespace/package
- `fields: Field[]` - Type fields/properties
- `attributes: Attribute[]` - Type attributes/annotations
- `collections: CollectionReference[]` - Collections using this type
- `usages: Usage[]` - Usage patterns across codebase
- `diffSummary: DiffSummary` - Changes between SHAs
- `relationships: Relationship[]` - Relationships to other entities
- `provenance: ProvenanceRecord` - Source information

**Validation Rules**:
- `id` must be valid FQCN format
- `fields` must be valid array of Field objects
- `attributes` must be valid array of Attribute objects

### Field
Represents individual properties/attributes within types.

**Fields**:
- `id: string` - Unique identifier
- `name: string` - Field name
- `type: string` - Field type
- `isRequired: boolean` - Whether field is required
- `defaultValue: any` - Default value if any
- `attributes: Attribute[]` - Field attributes/annotations
- `usagePatterns: UsagePattern[]` - How field is used
- `provenance: ProvenanceRecord` - Source information

**Validation Rules**:
- `name` must be non-empty string
- `type` must be valid type identifier

### Query
Represents database query operations with code snippets.

**Fields**:
- `id: string` - Unique identifier
- `name: string` - Query name/description
- `operation: QueryOperation` - Query operation type
- `codeSnippet: string` - Code snippet
- `language: string` - Programming language
- `collections: CollectionReference[]` - Collections queried
- `types: TypeReference[]` - Types involved
- `relationships: Relationship[]` - Relationships to other entities
- `provenance: ProvenanceRecord` - Source information

**Validation Rules**:
- `operation` must be valid QueryOperation enum value
- `codeSnippet` must be non-empty string
- `language` must be valid language identifier

### Service
Represents service definitions with their relationships.

**Fields**:
- `id: string` - Unique identifier
- `name: string` - Service name
- `type: ServiceType` - Service type
- `collections: CollectionReference[]` - Collections used
- `types: TypeReference[]` - Types used
- `endpoints: Endpoint[]` - Service endpoints
- `relationships: Relationship[]` - Relationships to other entities
- `provenance: ProvenanceRecord` - Source information

**Validation Rules**:
- `name` must be non-empty string
- `type` must be valid ServiceType enum value

### Relationship
Represents connections between entities.

**Fields**:
- `id: string` - Unique identifier
- `sourceId: string` - Source entity ID
- `targetId: string` - Target entity ID
- `edgeKind: EdgeKind` - Relationship type
- `weight: number` - Relationship strength (0-1)
- `metadata: Record<string, any>` - Additional relationship data
- `provenance: ProvenanceRecord` - Source information

**Validation Rules**:
- `sourceId` and `targetId` must reference valid entities
- `edgeKind` must be valid EdgeKind enum value
- `weight` must be between 0 and 1

## Supporting Types

### SchemaField
Represents a field in a schema definition.

**Fields**:
- `name: string` - Field name
- `type: string` - Field type
- `isRequired: boolean` - Whether field is required
- `isArray: boolean` - Whether field is an array
- `nestedFields: SchemaField[]` - Nested fields for complex types

### PresenceMetrics
Represents field presence statistics.

**Fields**:
- `totalSamples: number` - Total number of samples
- `presentSamples: number` - Number of samples where field is present
- `presencePercentage: number` - Percentage of presence (0-100)
- `lastUpdated: Date` - Last update timestamp

### DriftIndicator
Represents schema drift between declared and observed schemas.

**Fields**:
- `fieldName: string` - Field name
- `driftType: DriftType` - Type of drift
- `severity: DriftSeverity` - Severity level
- `description: string` - Human-readable description
- `suggestedAction: string` - Suggested remediation

### Attribute
Represents type or field attributes/annotations.

**Fields**:
- `name: string` - Attribute name
- `value: any` - Attribute value
- `parameters: Record<string, any>` - Attribute parameters

### Usage
Represents how a type is used across the codebase.

**Fields**:
- `context: string` - Usage context
- `frequency: number` - Usage frequency
- `locations: Location[]` - Usage locations

### DiffSummary
Represents changes between different SHAs.

**Fields**:
- `fromSha: string` - Source SHA
- `toSha: string` - Target SHA
- `addedFields: Field[]` - Added fields
- `removedFields: Field[]` - Removed fields
- `modifiedFields: Field[]` - Modified fields
- `changeCount: number` - Total number of changes

### ProvenanceRecord
Represents source information for entities.

**Fields**:
- `repository: string` - Repository URL
- `filePath: string` - File path
- `lineNumber: number` - Line number
- `commitSha: string` - Commit SHA
- `timestamp: Date` - Extraction timestamp
- `extractor: string` - Extraction tool/process

## Enums

### QueryOperation
- `FIND` - Find/select operations
- `INSERT` - Insert operations
- `UPDATE` - Update operations
- `DELETE` - Delete operations
- `AGGREGATE` - Aggregation operations

### ServiceType
- `API` - REST API service
- `GRAPHQL` - GraphQL service
- `GRPC` - gRPC service
- `BACKGROUND` - Background job service
- `SCHEDULED` - Scheduled task service

### EdgeKind
- `USES` - Entity uses another entity
- `CONTAINS` - Entity contains another entity
- `REFERENCES` - Entity references another entity
- `IMPLEMENTS` - Entity implements another entity
- `EXTENDS` - Entity extends another entity

### DriftType
- `MISSING_FIELD` - Field missing in observed schema
- `EXTRA_FIELD` - Extra field in observed schema
- `TYPE_MISMATCH` - Type mismatch between schemas
- `REQUIRED_MISMATCH` - Required/optional mismatch

### DriftSeverity
- `LOW` - Minor drift, no immediate action needed
- `MEDIUM` - Moderate drift, should be addressed
- `HIGH` - Significant drift, requires immediate attention
- `CRITICAL` - Critical drift, may cause failures

## State Transitions

### Collection State
- `DISCOVERED` → `ANALYZED` → `MONITORED`
- `ANALYZED` → `DRIFT_DETECTED` → `RESOLVED`

### Type State
- `DISCOVERED` → `ANALYZED` → `USAGE_TRACKED`
- `USAGE_TRACKED` → `CHANGED` → `IMPACT_ASSESSED`

## Data Flow

1. **Discovery**: Entities are discovered through code analysis
2. **Analysis**: Schema and relationships are extracted
3. **Monitoring**: Drift and usage patterns are tracked
4. **Presentation**: Data is presented in the UI with real-time updates

## Validation Rules

### Global Rules
- All entities must have valid provenance records
- All IDs must be unique within their entity type
- All timestamps must be valid ISO 8601 dates
- All percentages must be between 0 and 100

### Relationship Rules
- Relationships must be bidirectional (if A relates to B, B relates to A)
- Relationship weights must be normalized (0-1 range)
- Circular relationships are allowed but should be flagged

### Schema Rules
- Declared schemas should be consistent with observed schemas
- Drift indicators should be actionable and specific
- Presence metrics should be based on sufficient sample sizes
