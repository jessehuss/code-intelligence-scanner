# Quickstart Guide: Code Intelligence Scanner & Knowledge Base Seeder

**Date**: 2025-01-27  
**Feature**: 001-feature-f001-code  
**Status**: Complete

## Overview

This quickstart guide demonstrates how to use the Code Intelligence Scanner to discover MongoDB usage patterns in C# codebases and build a searchable knowledge base.

## Prerequisites

- .NET 8 SDK installed
- MongoDB instance (local or cloud)
- Read-only access to target repositories
- Read-only MongoDB credentials for sampling (optional)

## Installation

```bash
# Clone the repository
git clone https://github.com/your-org/cataloger.git
cd cataloger

# Build the scanner
dotnet build apps/scanner

# Run tests to verify installation
dotnet test apps/scanner/tests
```

## Basic Usage

### 1. Scan a Single Repository

```bash
# Scan a local repository
dotnet run --project apps/scanner -- \
  --repositories /path/to/your/repo \
  --scan-type full \
  --output-format json \
  --output-file scan-results.json

# Scan a remote repository
dotnet run --project apps/scanner -- \
  --repositories https://github.com/your-org/your-repo.git \
  --scan-type full \
  --output-format json
```

### 2. Scan Multiple Repositories

```bash
# Scan multiple repositories
dotnet run --project apps/scanner -- \
  --repositories /path/to/repo1 /path/to/repo2 https://github.com/org/repo3.git \
  --scan-type full \
  --output-format json \
  --output-file multi-scan-results.json
```

### 3. Enable Live MongoDB Sampling

```bash
# Scan with live data sampling
dotnet run --project apps/scanner -- \
  --repositories /path/to/your/repo \
  --scan-type full \
  --enable-sampling \
  --sampling-config max-documents-per-collection=50 \
  --mongodb-connection-string "mongodb://readonly-user:password@localhost:27017/your-db" \
  --output-format json
```

## Configuration

### Configuration File

Create a `scanner-config.json` file:

```json
{
  "repositories": [
    "/path/to/repo1",
    "/path/to/repo2",
    "https://github.com/org/repo3.git"
  ],
  "scanType": "full",
  "enableSampling": false,
  "samplingConfig": {
    "maxDocumentsPerCollection": 100,
    "piiDetectionEnabled": true,
    "connectionTimeout": 30000
  },
  "outputFormat": "json",
  "outputFile": "scan-results.json",
  "mongodbConnectionString": "mongodb://readonly-user:password@localhost:27017/your-db",
  "knowledgeBaseConnectionString": "mongodb://admin:password@localhost:27017/catalog_kb"
}
```

### Environment Variables

```bash
# Set environment variables
export SCANNER_MONGODB_CONNECTION_STRING="mongodb://readonly-user:password@localhost:27017/your-db"
export SCANNER_KB_CONNECTION_STRING="mongodb://admin:password@localhost:27017/catalog_kb"
export SCANNER_OUTPUT_FORMAT="json"
export SCANNER_ENABLE_SAMPLING="true"
```

## Expected Output

### Scan Results

The scanner produces a JSON file with the following structure:

```json
{
  "scanId": "scan-20250127-143022",
  "status": "completed",
  "startedAt": "2025-01-27T14:30:22Z",
  "completedAt": "2025-01-27T14:35:18Z",
  "duration": 296,
  "results": {
    "typesDiscovered": 25,
    "collectionsMapped": 12,
    "queriesExtracted": 35,
    "relationshipsInferred": 18,
    "schemasObserved": 8,
    "repositories": [
      {
        "repository": "/path/to/your/repo",
        "status": "success",
        "typesDiscovered": 25,
        "collectionsMapped": 12,
        "queriesExtracted": 35
      }
    ]
  }
}
```

### Knowledge Base Entries

The scanner creates entries in the knowledge base with complete provenance:

```json
{
  "id": "type-user-001",
  "entityType": "CodeType",
  "name": "User",
  "namespace": "MyApp.Models",
  "fields": [
    {
      "name": "Id",
      "type": "ObjectId",
      "isNullable": false,
      "bsonAttributes": [{"name": "BsonId", "value": "true"}]
    },
    {
      "name": "Email",
      "type": "string",
      "isNullable": false,
      "bsonAttributes": [{"name": "BsonElement", "value": "email"}]
    }
  ],
  "collectionMappings": [
    {
      "collectionName": "users",
      "resolutionMethod": "literal",
      "confidence": 1.0
    }
  ],
  "provenance": {
    "repository": "/path/to/your/repo",
    "filePath": "src/Models/User.cs",
    "symbol": "User",
    "lineSpan": {"start": 5, "end": 25},
    "commitSHA": "abc123def456",
    "timestamp": "2025-01-27T14:30:22Z"
  }
}
```

## Search and Query

### Search the Knowledge Base

```bash
# Search for types containing "User"
dotnet run --project apps/scanner -- search \
  --query "User" \
  --entity-types CodeType \
  --limit 10

# Search for collection mappings
dotnet run --project apps/scanner -- search \
  --query "users" \
  --entity-types CollectionMapping \
  --repositories /path/to/your/repo
```

### Get Detailed Type Information

```bash
# Get detailed information about a specific type
dotnet run --project apps/scanner -- get-type \
  --type-id "type-user-001" \
  --include-relationships \
  --include-queries
```

## Advanced Features

### Incremental Scanning

```bash
# Perform incremental scan (only changed files)
dotnet run --project apps/scanner -- \
  --repositories /path/to/your/repo \
  --scan-type incremental \
  --last-commit-sha "abc123def456"
```

### Integrity Check

```bash
# Perform integrity check (validate existing knowledge base)
dotnet run --project apps/scanner -- \
  --scan-type integrity \
  --repositories /path/to/your/repo
```

### Custom PII Detection

```bash
# Use custom PII detection patterns
dotnet run --project apps/scanner -- \
  --repositories /path/to/your/repo \
  --enable-sampling \
  --pii-patterns-file custom-pii-patterns.json
```

## Troubleshooting

### Common Issues

1. **MongoDB Connection Failed**
   ```
   Error: Failed to connect to MongoDB
   Solution: Verify connection string and credentials
   ```

2. **Repository Access Denied**
   ```
   Error: Repository access denied
   Solution: Check repository permissions and authentication
   ```

3. **PII Detection Warnings**
   ```
   Warning: PII detected in field 'email'
   Solution: Review manual review queue for flagged fields
   ```

### Debug Mode

```bash
# Enable debug logging
dotnet run --project apps/scanner -- \
  --repositories /path/to/your/repo \
  --log-level debug \
  --verbose
```

### Performance Tuning

```bash
# Optimize for large codebases
dotnet run --project apps/scanner -- \
  --repositories /path/to/your/repo \
  --max-parallel-files 4 \
  --chunk-size 1000 \
  --memory-limit 2GB
```

## Integration with CI/CD

### GitHub Actions

```yaml
name: Code Intelligence Scan
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Run Scanner
        run: |
          dotnet run --project apps/scanner -- \
            --repositories . \
            --scan-type incremental \
            --output-format json \
            --output-file scan-results.json
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: scan-results
          path: scan-results.json
```

## Next Steps

1. **Explore Results**: Review the generated knowledge base entries
2. **Search Patterns**: Use the search functionality to find specific patterns
3. **Integrate**: Incorporate the scanner into your development workflow
4. **Customize**: Configure PII detection and sampling parameters
5. **Scale**: Set up automated scanning for multiple repositories

## Support

For issues and questions:
- Check the troubleshooting section above
- Review the logs for detailed error information
- Consult the API documentation for advanced usage
- Contact the development team for support
