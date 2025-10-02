# Data Model: F002 – Catalog API

**Feature**: F002 – Catalog API  
**Date**: 2025-01-02  
**Status**: Complete  

## Overview

This document defines the data models for the Catalog API, including request/response DTOs, error handling structures, and validation rules. All models are designed to support the functional requirements while maintaining consistency with the existing knowledge base schema.

## Core Data Transfer Objects (DTOs)

### SearchResult

Represents a search result with entity type, relevance score, and summary data.

```csharp
public class SearchResult
{
    public string Id { get; set; }
    public string EntityType { get; set; } // "type", "collection", "field", "query", "service", "endpoint"
    public string Name { get; set; }
    public string Description { get; set; }
    public double RelevanceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string CommitSha { get; set; }
    public DateTime LastModified { get; set; }
}
```

**Validation Rules**:
- `Id`: Required, non-empty string
- `EntityType`: Required, must be one of the allowed values
- `Name`: Required, non-empty string
- `RelevanceScore`: Required, range 0.0-1.0
- `LastModified`: Required, valid DateTime

### CollectionDetail

Contains declared schema, observed schema, associated types, queries, and relationships for a collection.

```csharp
public class CollectionDetail
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SchemaInfo DeclaredSchema { get; set; }
    public SchemaInfo ObservedSchema { get; set; }
    public List<string> AssociatedTypes { get; set; }
    public List<QueryInfo> RelatedQueries { get; set; }
    public List<RelationshipInfo> Relationships { get; set; }
    public bool HasDrift { get; set; }
    public List<string> DriftFlags { get; set; }
    public int DocumentCount { get; set; }
    public DateTime LastSampled { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string CommitSha { get; set; }
}

public class SchemaInfo
{
    public List<FieldInfo> Fields { get; set; }
    public List<string> RequiredFields { get; set; }
    public Dictionary<string, object> Constraints { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class FieldInfo
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
    public List<string> Attributes { get; set; }
    public string Description { get; set; }
}

public class QueryInfo
{
    public string Operation { get; set; } // "Find", "Update", "Insert", "Delete", "Aggregate"
    public string Filter { get; set; }
    public string Projection { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
}

public class RelationshipInfo
{
    public string Type { get; set; } // "READS", "WRITES", "REFERS_TO"
    public string TargetEntity { get; set; }
    public string Description { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
}
```

**Validation Rules**:
- `Name`: Required, non-empty string
- `DeclaredSchema`: Required, valid SchemaInfo
- `ObservedSchema`: Required, valid SchemaInfo
- `AssociatedTypes`: Required, non-null list
- `RelatedQueries`: Required, non-null list
- `Relationships`: Required, non-null list

### TypeDetail

Contains field definitions, BSON attributes, collection mappings, usage statistics, and change history for a code type.

```csharp
public class TypeDetail
{
    public string FullyQualifiedName { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Description { get; set; }
    public List<FieldDetail> Fields { get; set; }
    public List<string> BsonAttributes { get; set; }
    public List<CollectionMapping> CollectionMappings { get; set; }
    public UsageStatistics UsageStats { get; set; }
    public ChangeSummary ChangeSummary { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string CommitSha { get; set; }
    public DateTime LastModified { get; set; }
}

public class FieldDetail
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
    public List<string> Attributes { get; set; }
    public string Description { get; set; }
    public string DefaultValue { get; set; }
    public List<string> ValidationRules { get; set; }
}

public class CollectionMapping
{
    public string CollectionName { get; set; }
    public string MappingType { get; set; } // "Primary", "Secondary", "Reference"
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
}

public class UsageStatistics
{
    public int QueryCount { get; set; }
    public int RepositoryCount { get; set; }
    public List<string> UsedInRepositories { get; set; }
    public DateTime LastUsed { get; set; }
    public List<string> CommonOperations { get; set; }
}

public class ChangeSummary
{
    public int TotalChanges { get; set; }
    public int AddedFields { get; set; }
    public int RemovedFields { get; set; }
    public int ModifiedFields { get; set; }
    public DateTime LastChange { get; set; }
    public List<string> RecentCommits { get; set; }
}
```

**Validation Rules**:
- `FullyQualifiedName`: Required, non-empty string
- `Name`: Required, non-empty string
- `Fields`: Required, non-null list
- `CollectionMappings`: Required, non-null list
- `UsageStats`: Required, valid UsageStatistics
- `ChangeSummary`: Required, valid ChangeSummary

### GraphNode

Represents a node in the knowledge graph with connections to other entities.

```csharp
public class GraphNode
{
    public string Id { get; set; }
    public string EntityType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public List<GraphEdge> IncomingEdges { get; set; }
    public List<GraphEdge> OutgoingEdges { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string CommitSha { get; set; }
}
```

**Validation Rules**:
- `Id`: Required, non-empty string
- `EntityType`: Required, non-empty string
- `Name`: Required, non-empty string
- `IncomingEdges`: Required, non-null list
- `OutgoingEdges`: Required, non-null list

### GraphEdge

Represents relationships between entities with types like READS, WRITES, REFERS_TO.

```csharp
public class GraphEdge
{
    public string Id { get; set; }
    public string SourceNodeId { get; set; }
    public string TargetNodeId { get; set; }
    public string EdgeType { get; set; } // "READS", "WRITES", "REFERS_TO"
    public string Description { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string CommitSha { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Validation Rules**:
- `Id`: Required, non-empty string
- `SourceNodeId`: Required, non-empty string
- `TargetNodeId`: Required, non-empty string
- `EdgeType`: Required, must be one of the allowed values
- `CreatedAt`: Required, valid DateTime

### TypeDiff

Contains comparison data showing changes between two versions of a type definition.

```csharp
public class TypeDiff
{
    public string FullyQualifiedName { get; set; }
    public string FromCommitSha { get; set; }
    public string ToCommitSha { get; set; }
    public List<FieldChange> AddedFields { get; set; }
    public List<FieldChange> RemovedFields { get; set; }
    public List<FieldChange> ModifiedFields { get; set; }
    public List<AttributeChange> AttributeChanges { get; set; }
    public DateTime DiffGeneratedAt { get; set; }
    public string Repository { get; set; }
    public string FilePath { get; set; }
}

public class FieldChange
{
    public string FieldName { get; set; }
    public string FieldType { get; set; }
    public string ChangeType { get; set; } // "Added", "Removed", "Modified"
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string Description { get; set; }
}

public class AttributeChange
{
    public string AttributeName { get; set; }
    public string ChangeType { get; set; } // "Added", "Removed", "Modified"
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string Description { get; set; }
}
```

**Validation Rules**:
- `FullyQualifiedName`: Required, non-empty string
- `FromCommitSha`: Required, non-empty string
- `ToCommitSha`: Required, non-empty string
- `AddedFields`: Required, non-null list
- `RemovedFields`: Required, non-null list
- `ModifiedFields`: Required, non-null list
- `DiffGeneratedAt`: Required, valid DateTime

## Request Models

### SearchRequest

```csharp
public class SearchRequest
{
    public string Query { get; set; }
    public List<string> Kinds { get; set; } // "type", "collection", "field", "query", "service", "endpoint"
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
    public Dictionary<string, object> Filters { get; set; }
    public string SortBy { get; set; } = "relevance";
    public string SortOrder { get; set; } = "desc";
}
```

**Validation Rules**:
- `Query`: Required, non-empty string, max length 500
- `Kinds`: Optional, must contain only allowed values
- `Limit`: Range 1-1000
- `Offset`: Minimum 0
- `SortBy`: Must be one of "relevance", "name", "lastModified"
- `SortOrder`: Must be "asc" or "desc"

### GraphRequest

```csharp
public class GraphRequest
{
    public string Node { get; set; } // Format: "collection:vendors" or "type:MyApp.Models.User"
    public int Depth { get; set; } = 2;
    public List<string> EdgeKinds { get; set; } // "READS", "WRITES", "REFERS_TO"
    public int MaxNodes { get; set; } = 100;
    public bool IncludeProperties { get; set; } = false;
}
```

**Validation Rules**:
- `Node`: Required, non-empty string, must match pattern "type:name" or "collection:name"
- `Depth`: Range 1-5
- `EdgeKinds`: Optional, must contain only allowed values
- `MaxNodes`: Range 10-1000

### DiffRequest

```csharp
public class DiffRequest
{
    public string FullyQualifiedName { get; set; }
    public string FromCommitSha { get; set; }
    public string ToCommitSha { get; set; }
    public bool IncludeFieldDetails { get; set; } = true;
    public bool IncludeAttributeChanges { get; set; } = true;
}
```

**Validation Rules**:
- `FullyQualifiedName`: Required, non-empty string
- `FromCommitSha`: Required, non-empty string, must be valid SHA
- `ToCommitSha`: Required, non-empty string, must be valid SHA

## Response Models

### SearchResponse

```csharp
public class SearchResponse
{
    public List<SearchResult> Results { get; set; }
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public bool HasMore { get; set; }
    public Dictionary<string, int> ResultCountsByType { get; set; }
    public TimeSpan QueryTime { get; set; }
}
```

### GraphResponse

```csharp
public class GraphResponse
{
    public GraphNode CenterNode { get; set; }
    public List<GraphNode> Nodes { get; set; }
    public List<GraphEdge> Edges { get; set; }
    public int TotalNodes { get; set; }
    public int TotalEdges { get; set; }
    public TimeSpan QueryTime { get; set; }
}
```

## Error Models

### ErrorResponse

```csharp
public class ErrorResponse
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public string Detail { get; set; }
    public string Instance { get; set; }
    public string ErrorCode { get; set; }
    public Dictionary<string, object> Extensions { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
}
```

**Validation Rules**:
- `Type`: Required, URI format
- `Title`: Required, non-empty string
- `Status`: Required, valid HTTP status code
- `Detail`: Required, non-empty string
- `Timestamp`: Required, valid DateTime
- `TraceId`: Required, non-empty string

## Validation Rules Summary

### Common Validation Rules
- All string fields: Non-empty, trimmed
- All lists: Non-null, may be empty
- All DateTime fields: Valid DateTime values
- All numeric fields: Within specified ranges
- All enum fields: Must match allowed values

### Business Logic Validation
- Entity types must match predefined values
- Commit SHAs must be valid format
- File paths must be valid format
- Line numbers must be positive integers
- Relevance scores must be between 0.0 and 1.0

### Performance Constraints
- Search queries limited to 500 characters
- Graph depth limited to 5 levels
- Maximum 1000 results per search
- Maximum 1000 nodes per graph query
- Request timeouts at 30 seconds

## Data Relationships

### Entity Relationships
- SearchResult → Repository, FilePath, LineNumber
- CollectionDetail → AssociatedTypes, RelatedQueries, Relationships
- TypeDetail → CollectionMappings, UsageStats, ChangeSummary
- GraphNode → IncomingEdges, OutgoingEdges
- GraphEdge → SourceNodeId, TargetNodeId
- TypeDiff → AddedFields, RemovedFields, ModifiedFields

### Cross-Entity References
- All entities reference Repository, FilePath, LineNumber, CommitSha
- Graph edges connect nodes by ID references
- Collection mappings reference type definitions
- Query operations reference collections and types

## Caching Strategy

### Cache Keys
- Search results: `search:{query}:{kinds}:{limit}:{offset}`
- Collection details: `collection:{name}`
- Type details: `type:{fqcn}`
- Graph data: `graph:{node}:{depth}:{edgeKinds}`
- Type diffs: `diff:{fqcn}:{fromSha}:{toSha}`

### Cache TTL
- Search results: 5 minutes
- Collection details: 30 minutes
- Type details: 1 hour
- Graph data: 15 minutes
- Type diffs: 24 hours

### Cache Invalidation
- On knowledge base updates
- On new scan results
- On schema changes
- Manual cache clear endpoints

## Security Considerations

### Data Exposure
- No sensitive data in error messages
- No internal system paths in responses
- No stack traces in production
- Sanitized user input in all fields

### Input Validation
- All user input validated and sanitized
- SQL injection prevention (MongoDB injection)
- XSS prevention in string fields
- Rate limiting on expensive operations

### Access Control
- Internal use only (no authentication required)
- Read-only access to knowledge base
- Audit logging for all operations
- Request correlation IDs for tracking
