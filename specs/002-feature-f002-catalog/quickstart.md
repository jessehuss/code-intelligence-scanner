# Quickstart: F002 – Catalog API

**Feature**: F002 – Catalog API  
**Date**: 2025-01-02  
**Status**: Complete  

## Overview

The Catalog API provides HTTP endpoints for querying the code intelligence knowledge base. It supports search, collection details, type information, graph relationships, and diff comparisons with performance targets of <300ms P50 for search and <400ms P50 for graph operations.

## Prerequisites

- .NET 8 SDK
- MongoDB instance with knowledge base data
- Redis instance for caching
- Atlas Search configured (optional but recommended)

## Installation

### 1. Clone and Build

```bash
git clone <repository-url>
cd cataloger/apps/catalog-api
dotnet restore
dotnet build
```

### 2. Configuration

Create `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "KnowledgeBase": "mongodb://localhost:27017/catalog_kb",
    "AtlasSearch": "mongodb://localhost:27017/catalog_kb",
    "Redis": "localhost:6379"
  },
  "CatalogApi": {
    "Cache": {
      "DefaultTtl": "00:05:00",
      "SearchTtl": "00:05:00",
      "CollectionTtl": "00:30:00",
      "TypeTtl": "01:00:00",
      "GraphTtl": "00:15:00",
      "DiffTtl": "24:00:00"
    },
    "Performance": {
      "MaxSearchResults": 1000,
      "MaxGraphNodes": 1000,
      "MaxGraphDepth": 5,
      "RequestTimeout": "00:00:30"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3. Environment Variables

```bash
export CATALOG_API__CONNECTIONSTRINGS__KNOWLEDGEBASE="mongodb://localhost:27017/catalog_kb"
export CATALOG_API__CONNECTIONSTRINGS__ATLASSEARCH="mongodb://localhost:27017/catalog_kb"
export CATALOG_API__CONNECTIONSTRINGS__REDIS="localhost:6379"
```

## Running the API

### Development

```bash
dotnet run --project src/CatalogApi.csproj
```

The API will be available at `http://localhost:5000`

### Production

```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet CatalogApi.dll
```

### Docker

```bash
docker build -t catalog-api .
docker run -p 5000:5000 \
  -e CATALOG_API__CONNECTIONSTRINGS__KNOWLEDGEBASE="mongodb://host.docker.internal:27017/catalog_kb" \
  -e CATALOG_API__CONNECTIONSTRINGS__REDIS="host.docker.internal:6379" \
  catalog-api
```

## API Usage

### 1. Search Knowledge Base

Search for entities in the knowledge base:

```bash
# Basic search
curl "http://localhost:5000/search?q=user&kinds=type,collection&limit=50"

# Advanced search with filters
curl "http://localhost:5000/search?q=user&kinds=type,collection&limit=25&offset=10&sortBy=name&sortOrder=asc&filters=%7B%22repository%22%3A%22myapp%22%7D"
```

**Response:**
```json
{
  "results": [
    {
      "id": "type:MyApp.Models.User",
      "entityType": "type",
      "name": "User",
      "description": "User entity representing application users",
      "relevanceScore": 0.95,
      "metadata": {
        "fieldCount": 5,
        "isAbstract": false
      },
      "repository": "myapp",
      "filePath": "src/Models/User.cs",
      "lineNumber": 15,
      "commitSha": "abc123def456",
      "lastModified": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 150,
  "limit": 50,
  "offset": 0,
  "hasMore": true,
  "resultCountsByType": {
    "type": 25,
    "collection": 15,
    "field": 10
  },
  "queryTime": "00:00:00.125"
}
```

### 2. Get Collection Details

Get detailed information about a specific collection:

```bash
curl "http://localhost:5000/collections/vendors"
```

**Response:**
```json
{
  "name": "vendors",
  "description": "Vendor information collection",
  "declaredSchema": {
    "fields": [
      {
        "name": "id",
        "type": "ObjectId",
        "isRequired": true,
        "isNullable": false,
        "attributes": ["BsonId"],
        "description": "Unique identifier"
      },
      {
        "name": "name",
        "type": "string",
        "isRequired": true,
        "isNullable": false,
        "attributes": ["BsonElement"],
        "description": "Vendor name"
      }
    ],
    "requiredFields": ["id", "name"],
    "constraints": {
      "maxLength": 100
    },
    "lastUpdated": "2024-01-15T10:30:00Z"
  },
  "observedSchema": {
    "fields": [
      {
        "name": "id",
        "type": "ObjectId",
        "isRequired": true,
        "isNullable": false,
        "attributes": ["BsonId"],
        "description": "Unique identifier"
      },
      {
        "name": "name",
        "type": "string",
        "isRequired": true,
        "isNullable": false,
        "attributes": ["BsonElement"],
        "description": "Vendor name"
      },
      {
        "name": "email",
        "type": "string",
        "isRequired": false,
        "isNullable": true,
        "attributes": ["BsonElement"],
        "description": "Vendor email"
      }
    ],
    "requiredFields": ["id", "name"],
    "constraints": {
      "maxLength": 100
    },
    "lastUpdated": "2024-01-15T10:30:00Z"
  },
  "associatedTypes": ["Vendor", "VendorContact"],
  "relatedQueries": [
    {
      "operation": "Find",
      "filter": "{'status': 'active'}",
      "projection": "{'name': 1, 'email': 1}",
      "repository": "myapp",
      "filePath": "src/Services/VendorService.cs",
      "lineNumber": 45
    }
  ],
  "relationships": [
    {
      "type": "READS",
      "targetEntity": "users",
      "description": "Reads user data for validation",
      "repository": "myapp",
      "filePath": "src/Services/VendorService.cs",
      "lineNumber": 67
    }
  ],
  "hasDrift": true,
  "driftFlags": ["missing_field", "type_mismatch"],
  "documentCount": 1250,
  "lastSampled": "2024-01-15T10:30:00Z",
  "repository": "myapp",
  "filePath": "src/Data/VendorRepository.cs",
  "lineNumber": 25,
  "commitSha": "abc123def456"
}
```

### 3. Get Type Details

Get detailed information about a specific type:

```bash
curl "http://localhost:5000/types/MyApp.Models.User"
```

**Response:**
```json
{
  "fullyQualifiedName": "MyApp.Models.User",
  "name": "User",
  "namespace": "MyApp.Models",
  "description": "User entity representing application users",
  "fields": [
    {
      "name": "id",
      "type": "ObjectId",
      "isRequired": true,
      "isNullable": false,
      "attributes": ["BsonId"],
      "description": "Unique identifier",
      "defaultValue": "null",
      "validationRules": ["Required"]
    },
    {
      "name": "email",
      "type": "string",
      "isRequired": true,
      "isNullable": false,
      "attributes": ["BsonElement", "Required"],
      "description": "User email address",
      "defaultValue": "null",
      "validationRules": ["EmailAddress", "MaxLength(100)"]
    }
  ],
  "bsonAttributes": ["BsonIgnoreExtraElements", "BsonDiscriminator"],
  "collectionMappings": [
    {
      "collectionName": "users",
      "mappingType": "Primary",
      "repository": "myapp",
      "filePath": "src/Data/UserRepository.cs",
      "lineNumber": 25
    }
  ],
  "usageStats": {
    "queryCount": 45,
    "repositoryCount": 3,
    "usedInRepositories": ["myapp", "admin", "api"],
    "lastUsed": "2024-01-15T10:30:00Z",
    "commonOperations": ["Find", "Update", "Insert"]
  },
  "changeSummary": {
    "totalChanges": 12,
    "addedFields": 3,
    "removedFields": 1,
    "modifiedFields": 8,
    "lastChange": "2024-01-15T10:30:00Z",
    "recentCommits": ["abc123def456", "def456ghi789"]
  },
  "repository": "myapp",
  "filePath": "src/Models/User.cs",
  "lineNumber": 15,
  "commitSha": "abc123def456",
  "lastModified": "2024-01-15T10:30:00Z"
}
```

### 4. Get Graph Data

Get graph relationships for a specific node:

```bash
# Basic graph query
curl "http://localhost:5000/graph?node=collection:vendors&depth=2&edgeKinds=READS,WRITES"

# Advanced graph query
curl "http://localhost:5000/graph?node=collection:vendors&depth=2&edgeKinds=READS,WRITES,REFERS_TO&maxNodes=100&includeProperties=true"
```

**Response:**
```json
{
  "centerNode": {
    "id": "collection:vendors",
    "entityType": "collection",
    "name": "vendors",
    "description": "Vendor information collection",
    "properties": {
      "documentCount": 1250,
      "lastUpdated": "2024-01-15"
    },
    "incomingEdges": [
      {
        "id": "edge:123",
        "sourceNodeId": "type:VendorService",
        "targetNodeId": "collection:vendors",
        "edgeType": "WRITES",
        "description": "Writes vendor data",
        "properties": {
          "frequency": "high",
          "lastUsed": "2024-01-15"
        },
        "repository": "myapp",
        "filePath": "src/Services/VendorService.cs",
        "lineNumber": 67,
        "commitSha": "abc123def456",
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ],
    "outgoingEdges": [
      {
        "id": "edge:124",
        "sourceNodeId": "collection:vendors",
        "targetNodeId": "type:Vendor",
        "edgeType": "REFERS_TO",
        "description": "References vendor type",
        "properties": {
          "frequency": "high",
          "lastUsed": "2024-01-15"
        },
        "repository": "myapp",
        "filePath": "src/Data/VendorRepository.cs",
        "lineNumber": 25,
        "commitSha": "abc123def456",
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ],
    "repository": "myapp",
    "filePath": "src/Data/VendorRepository.cs",
    "lineNumber": 25,
    "commitSha": "abc123def456"
  },
  "nodes": [
    {
      "id": "collection:vendors",
      "entityType": "collection",
      "name": "vendors",
      "description": "Vendor information collection",
      "properties": {
        "documentCount": 1250,
        "lastUpdated": "2024-01-15"
      },
      "incomingEdges": [],
      "outgoingEdges": [],
      "repository": "myapp",
      "filePath": "src/Data/VendorRepository.cs",
      "lineNumber": 25,
      "commitSha": "abc123def456"
    }
  ],
  "edges": [
    {
      "id": "edge:123",
      "sourceNodeId": "type:VendorService",
      "targetNodeId": "collection:vendors",
      "edgeType": "WRITES",
      "description": "Writes vendor data",
      "properties": {
        "frequency": "high",
        "lastUsed": "2024-01-15"
      },
      "repository": "myapp",
      "filePath": "src/Services/VendorService.cs",
      "lineNumber": 67,
      "commitSha": "abc123def456",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalNodes": 25,
  "totalEdges": 45,
  "queryTime": "00:00:00.250"
}
```

### 5. Get Type Diff

Compare type definitions between two commits:

```bash
# Basic diff
curl "http://localhost:5000/diff/type/MyApp.Models.User?fromSha=abc123def456&toSha=def456ghi789"

# Advanced diff with options
curl "http://localhost:5000/diff/type/MyApp.Models.User?fromSha=abc123def456&toSha=def456ghi789&includeFieldDetails=true&includeAttributeChanges=true"
```

**Response:**
```json
{
  "fullyQualifiedName": "MyApp.Models.User",
  "fromCommitSha": "abc123def456",
  "toCommitSha": "def456ghi789",
  "addedFields": [
    {
      "fieldName": "email",
      "fieldType": "string",
      "changeType": "Added",
      "oldValue": "null",
      "newValue": "string",
      "description": "Added email field for user authentication"
    }
  ],
  "removedFields": [
    {
      "fieldName": "username",
      "fieldType": "string",
      "changeType": "Removed",
      "oldValue": "string",
      "newValue": "null",
      "description": "Removed username field in favor of email"
    }
  ],
  "modifiedFields": [
    {
      "fieldName": "name",
      "fieldType": "string",
      "changeType": "Modified",
      "oldValue": "string",
      "newValue": "string",
      "description": "Modified name field validation rules"
    }
  ],
  "attributeChanges": [
    {
      "attributeName": "BsonElement",
      "changeType": "Added",
      "oldValue": "null",
      "newValue": "email",
      "description": "Added BsonElement attribute for email field"
    }
  ],
  "diffGeneratedAt": "2024-01-15T10:30:00Z",
  "repository": "myapp",
  "filePath": "src/Models/User.cs"
}
```

### 6. Health Check

Check API health status:

```bash
curl "http://localhost:5000/health"
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0"
}
```

## Error Handling

The API returns structured error responses following RFC 7807 (Problem Details):

```json
{
  "type": "https://api.cataloger.dev/errors/validation",
  "title": "Validation Error",
  "status": 400,
  "detail": "The 'q' parameter is required and cannot be empty",
  "instance": "/search",
  "errorCode": "VALIDATION_ERROR",
  "extensions": {
    "field": "q",
    "value": ""
  },
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "abc123def456"
}
```

## Performance Considerations

### Caching

The API implements Redis-based caching with configurable TTL policies:

- **Search results**: 5 minutes
- **Collection details**: 30 minutes
- **Type details**: 1 hour
- **Graph data**: 15 minutes
- **Type diffs**: 24 hours

### Rate Limiting

No rate limiting is implemented (internal use only).

### Response Times

- **Search**: <300ms P50
- **Graph operations**: <400ms P50 for depth≤2
- **Other endpoints**: <200ms P50

## Monitoring and Observability

### Logging

Structured logging with correlation IDs:

```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "Information",
  "message": "Search request completed",
  "traceId": "abc123def456",
  "spanId": "def456ghi789",
  "properties": {
    "endpoint": "/search",
    "query": "user",
    "kinds": ["type", "collection"],
    "resultCount": 25,
    "responseTime": "125ms"
  }
}
```

### Metrics

Key metrics tracked:

- Request count and duration by endpoint
- Cache hit/miss ratios
- MongoDB query performance
- Error rates by type
- Memory and CPU usage

### Health Checks

Health check endpoints:

- `/health` - Overall API health
- `/health/mongodb` - MongoDB connection
- `/health/redis` - Redis connection
- `/health/atlas-search` - Atlas Search availability

## Testing

### Contract Tests

Run contract tests to validate API contracts:

```bash
dotnet test tests/contract/
```

### Integration Tests

Run integration tests with test containers:

```bash
dotnet test tests/integration/
```

### Load Testing

Example load test with Apache Bench:

```bash
# Search endpoint load test
ab -n 1000 -c 10 "http://localhost:5000/search?q=user&kinds=type,collection&limit=50"

# Graph endpoint load test
ab -n 1000 -c 10 "http://localhost:5000/graph?node=collection:vendors&depth=2"
```

## Troubleshooting

### Common Issues

1. **MongoDB Connection Failed**
   - Check connection string format
   - Verify MongoDB is running
   - Check network connectivity

2. **Redis Connection Failed**
   - Check Redis server status
   - Verify connection string
   - Check firewall settings

3. **Atlas Search Not Available**
   - Verify Atlas Search configuration
   - Check search index status
   - Review search query syntax

4. **Performance Issues**
   - Check cache hit ratios
   - Monitor MongoDB query performance
   - Review response time metrics

### Debug Mode

Enable debug logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Cataloger.CatalogApi": "Trace"
    }
  }
}
```

### Profiling

Enable profiling for performance analysis:

```bash
dotnet run --project src/CatalogApi.csproj --environment Development --profiling
```

## Security Considerations

### Data Access

- Read-only access to knowledge base
- No authentication required (internal use only)
- All API calls logged for audit purposes

### Input Validation

- All user input validated and sanitized
- SQL injection prevention (MongoDB injection)
- XSS prevention in string fields
- Request size limits enforced

### Error Information

- No sensitive data in error messages
- No internal system paths in responses
- No stack traces in production
- Sanitized user input in all fields

## Contributing

### Development Setup

1. Fork the repository
2. Create a feature branch
3. Make changes following the constitution
4. Add tests for new functionality
5. Run all tests and ensure they pass
6. Submit a pull request

### Code Style

Follow the project's code style guidelines:

- Use .NET 8 and C# 12 features
- Follow ASP.NET Core best practices
- Implement proper error handling
- Add comprehensive logging
- Write unit and integration tests

### Testing Requirements

- Unit tests for all handlers and services
- Integration tests for all endpoints
- Contract tests for API validation
- Performance tests for critical paths

## Support

For issues and questions:

- Check the troubleshooting section
- Review the API documentation
- Open an issue in the repository
- Contact the development team

## License

This project is licensed under the MIT License - see the LICENSE file for details.
