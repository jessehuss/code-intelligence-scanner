# Code Intelligence Scanner & Knowledge Base Seeder

A .NET 8 console application that performs static analysis of C#/.NET microservices to extract code intelligence, infer data relationships, and build a comprehensive knowledge base for MongoDB collections and operations.

## Features

- **Static Code Analysis**: Uses Roslyn to analyze C# code and extract POCOs, collections, and operations
- **MongoDB Sampling**: Samples live MongoDB data to infer observed JSON schemas
- **PII Redaction**: Automatically detects and redacts personally identifiable information
- **Knowledge Base**: Builds a searchable knowledge base with provenance tracking
- **Incremental Scanning**: Supports full, incremental, and integrity scan modes
- **Performance Monitoring**: Built-in performance monitoring and metrics
- **Atlas Search Integration**: Full-text search capabilities using MongoDB Atlas Search

## Quick Start

### Prerequisites

- .NET 8 SDK
- MongoDB 7.0+
- Git

### Installation

1. Clone the repository:
```bash
git clone https://github.com/your-org/cataloger.git
cd cataloger
```

2. Build the application:
```bash
cd apps/scanner/src
dotnet build
```

3. Run the scanner:
```bash
dotnet run -- --help
```

### Basic Usage

#### Scan a Repository
```bash
dotnet run -- scan --repository /path/to/repo --output-format json
```

#### Search the Knowledge Base
```bash
dotnet run -- search --query "user collection" --limit 10
```

#### Get Type Information
```bash
dotnet run -- get-type --type-id "507f1f77bcf86cd799439011"
```

## Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "KnowledgeBase": {
    "ConnectionString": "mongodb://localhost:27017/catalog_kb",
    "DatabaseName": "catalog_kb"
  },
  "MongoSampling": {
    "Enabled": true,
    "MaxDocumentsPerCollection": 1000,
    "MaxCollections": 100,
    "TimeoutSeconds": 300
  },
  "PIIRedaction": {
    "Enabled": true,
    "PIIFieldNames": ["email", "phone", "ssn", "token", "key"],
    "RedactionValue": "[REDACTED]"
  }
}
```

### Environment Variables
```bash
export KNOWLEDGE_BASE__CONNECTION_STRING="mongodb://localhost:27017/catalog_kb"
export MONGO_SAMPLING__ENABLED="true"
export PII_REDACTION__ENABLED="true"
```

## Command Reference

### scan
Initiates a code intelligence scan operation.

```bash
dotnet run -- scan [OPTIONS]
```

**Options:**
- `--repository <PATH>`: Path to the repository to scan
- `--scan-type <TYPE>`: Type of scan (full, incremental, integrity)
- `--output-format <FORMAT>`: Output format (json, yaml, csv)
- `--log-level <LEVEL>`: Log level (Debug, Information, Warning, Error)
- `--max-concurrent-files <NUMBER>`: Maximum number of files to process concurrently
- `--exclude-directories <DIRS>`: Directories to exclude from scanning

**Examples:**
```bash
# Full scan of a repository
dotnet run -- scan --repository /path/to/repo --scan-type full

# Incremental scan with custom concurrency
dotnet run -- scan --repository /path/to/repo --scan-type incremental --max-concurrent-files 10

# Scan with specific log level
dotnet run -- scan --repository /path/to/repo --log-level Debug
```

### search
Searches the knowledge base for entities and relationships.

```bash
dotnet run -- search [OPTIONS]
```

**Options:**
- `--query <TEXT>`: Search query text
- `--entity-types <TYPES>`: Entity types to filter by
- `--repositories <REPOS>`: Repositories to filter by
- `--limit <NUMBER>`: Maximum number of results
- `--offset <NUMBER>`: Number of results to skip

**Examples:**
```bash
# Search for user-related entities
dotnet run -- search --query "user" --limit 20

# Search for specific entity types
dotnet run -- search --query "collection" --entity-types "CollectionMapping,QueryOperation"

# Search within specific repositories
dotnet run -- search --query "order" --repositories "my-repo,other-repo"
```

### get-type
Retrieves detailed information about a specific code type.

```bash
dotnet run -- get-type [OPTIONS]
```

**Options:**
- `--type-id <ID>`: ID of the type to retrieve
- `--type-name <NAME>`: Name of the type to retrieve
- `--namespace <NAMESPACE>`: Namespace of the type
- `--include-fields`: Include field information
- `--include-relationships`: Include relationship information

**Examples:**
```bash
# Get type by ID
dotnet run -- get-type --type-id "507f1f77bcf86cd799439011"

# Get type by name and namespace
dotnet run -- get-type --type-name "User" --namespace "MyApp.Models"

# Get type with all details
dotnet run -- get-type --type-name "User" --include-fields --include-relationships
```

## Advanced Usage

### Custom PII Detection

Create a custom PII detector by implementing `IPIIDetector`:

```csharp
public class CustomPIIDetector : IPIIDetector
{
    public bool Detect(string fieldName, object fieldValue)
    {
        // Custom PII detection logic
        return fieldName.Contains("custom") && fieldValue is string;
    }
}
```

### Custom Collection Resolver

Implement a custom collection resolver:

```csharp
public class CustomCollectionResolver : CollectionResolver
{
    public override CollectionMapping ResolveCollectionName(CodeType codeType, string? collectionName)
    {
        // Custom collection resolution logic
        return base.ResolveCollectionName(codeType, collectionName);
    }
}
```

### Performance Tuning

Optimize performance for large codebases:

```json
{
  "Scanning": {
    "MaxConcurrentRepositories": 3,
    "MaxConcurrentFiles": 15,
    "MaxFileSizeMB": 5
  },
  "Performance": {
    "Enabled": true,
    "MaxMemoryUsageMB": 2048,
    "MaxCpuUsagePercent": 80
  }
}
```

## Troubleshooting

### Common Issues

#### 1. MongoDB Connection Failed
```
Error: MongoDB connection failed
Solution: Check connection string and ensure MongoDB is running
```

#### 2. Out of Memory
```
Error: OutOfMemoryException
Solution: Reduce MaxConcurrentFiles or increase available memory
```

#### 3. PII Detection False Positives
```
Error: Too many false positives in PII detection
Solution: Adjust PII detection patterns in configuration
```

#### 4. Slow Performance
```
Error: Scan taking too long
Solution: Enable incremental scanning or reduce concurrency
```

### Debug Mode

Enable debug logging for troubleshooting:

```bash
dotnet run -- scan --repository /path/to/repo --log-level Debug
```

### Performance Profiling

Enable performance monitoring:

```bash
dotnet run -- scan --repository /path/to/repo --enable-performance-monitoring
```

## Development

### Project Structure
```
apps/scanner/
├── src/                    # Source code
│   ├── Models/            # Data models
│   ├── Analyzers/         # Code analysis services
│   ├── Resolvers/         # Collection resolution
│   ├── Samplers/          # MongoDB sampling
│   ├── KnowledgeBase/     # Knowledge base services
│   ├── Services/          # Core services
│   ├── Commands/          # CLI commands
│   └── Program.cs         # Entry point
├── tests/                 # Test projects
│   ├── unit/             # Unit tests
│   ├── integration/      # Integration tests
│   ├── performance/      # Performance tests
│   └── contract/         # Contract tests
└── README.md             # This file
```

### Building from Source

1. Clone the repository:
```bash
git clone https://github.com/your-org/cataloger.git
cd cataloger
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build --configuration Release
```

4. Run tests:
```bash
dotnet test
```

### Running Tests

#### Unit Tests
```bash
dotnet test apps/scanner/tests/unit/
```

#### Integration Tests
```bash
dotnet test apps/scanner/tests/integration/
```

#### Performance Tests
```bash
dotnet test apps/scanner/tests/performance/
```

#### Contract Tests
```bash
dotnet test apps/scanner/tests/contract/
```

### Code Style

The project uses EditorConfig for consistent code style:

```ini
# C# files
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_indent_block_contents = true
csharp_indent_braces = false
```

### Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## API Reference

### Models

#### CodeType
Represents a C# POCO class with metadata.

```csharp
public class CodeType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public List<FieldDefinition> Fields { get; set; }
    public List<BsonAttributeDefinition> BSONAttributes { get; set; }
    public ProvenanceRecord Provenance { get; set; }
}
```

#### CollectionMapping
Links a C# type to a MongoDB collection.

```csharp
public class CollectionMapping
{
    public string Id { get; set; }
    public string TypeId { get; set; }
    public string CollectionName { get; set; }
    public string ResolutionMethod { get; set; }
    public double Confidence { get; set; }
    public ProvenanceRecord Provenance { get; set; }
}
```

#### QueryOperation
Represents a MongoDB operation found in code.

```csharp
public class QueryOperation
{
    public string Id { get; set; }
    public string OperationType { get; set; }
    public string CollectionId { get; set; }
    public object? Filters { get; set; }
    public object? Projections { get; set; }
    public ProvenanceRecord Provenance { get; set; }
}
```

### Services

#### POCOExtractor
Extracts POCO classes from C# code using Roslyn.

```csharp
public class POCOExtractor
{
    public List<CodeType> ExtractPOCOs(SyntaxTree syntaxTree, Compilation compilation);
}
```

#### CollectionResolver
Resolves MongoDB collection names from code.

```csharp
public class CollectionResolver
{
    public CollectionMapping ResolveCollectionName(CodeType codeType, string? collectionName);
}
```

#### MongoSampler
Samples MongoDB data and infers schemas.

```csharp
public class MongoSampler
{
    public Task<ObservedSchema> SampleCollection(string collectionName, int sampleSize);
}
```

## Performance

### Benchmarks

Typical performance on a modern development machine:

- **Small Repository** (100 files): 30-60 seconds
- **Medium Repository** (500 files): 2-5 minutes
- **Large Repository** (1000+ files): 5-15 minutes

### Optimization Tips

1. **Use Incremental Scanning**: Only scan changed files
2. **Adjust Concurrency**: Balance CPU and memory usage
3. **Exclude Directories**: Skip unnecessary directories
4. **Enable Caching**: Cache analysis results
5. **Monitor Performance**: Use built-in performance monitoring

### Memory Usage

Typical memory usage patterns:

- **Base Memory**: 50-100 MB
- **Per File**: 1-5 MB
- **Peak Memory**: 500-2000 MB (depending on codebase size)

## Security

### PII Protection

The scanner automatically detects and redacts PII:

- **Email Addresses**: Detected by field names and patterns
- **Phone Numbers**: US phone number formats
- **Social Security Numbers**: XXX-XX-XXXX format
- **Credit Card Numbers**: Luhn algorithm validation
- **IP Addresses**: IPv4 and IPv6 formats

### Access Control

- **Read-Only MongoDB Access**: Uses read-only credentials for sampling
- **No Data Modification**: Never modifies source code or production data
- **Audit Logging**: Comprehensive audit trails
- **Encryption**: Data encrypted at rest and in transit

## Monitoring

### Metrics

The scanner provides comprehensive metrics:

- **Scan Duration**: Total time for scan operations
- **Files Processed**: Number of files analyzed
- **Types Extracted**: Number of POCO classes found
- **Operations Found**: Number of MongoDB operations
- **Memory Usage**: Peak and average memory consumption
- **Error Count**: Number of errors encountered

### Logging

Structured logging with multiple levels:

- **Debug**: Detailed debugging information
- **Information**: General operational information
- **Warning**: Warning conditions
- **Error**: Error conditions

### Health Checks

Built-in health check endpoints:

- **MongoDB Connection**: Database connectivity
- **Memory Usage**: Memory consumption
- **CPU Usage**: CPU utilization
- **Disk Space**: Available disk space

## Deployment

### Docker

Build and run with Docker:

```bash
# Build image
docker build -t cataloger-scanner .

# Run container
docker run -d \
  --name scanner \
  -e KNOWLEDGE_BASE__CONNECTION_STRING="mongodb://mongo:27017/catalog_kb" \
  cataloger-scanner
```

### Kubernetes

Deploy to Kubernetes:

```bash
# Apply manifests
kubectl apply -f k8s/

# Check deployment
kubectl get pods -l app=scanner
```

### CI/CD

GitHub Actions workflow:

```yaml
name: Scanner CI/CD
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet test
```

## Support

### Documentation

- [Knowledge Base Schema](docs/KB-schema.md)
- [PII Redaction Policies](docs/PII-redaction.md)
- [API Documentation](docs/api/)
- [Architecture Guide](docs/architecture/)

### Community

- **GitHub Issues**: Report bugs and request features
- **Discussions**: Ask questions and share ideas
- **Wiki**: Community-maintained documentation

### Commercial Support

For commercial support and consulting:

- **Email**: support@example.com
- **Phone**: +1 (555) 123-4567
- **Website**: https://example.com/support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

### Version 1.0.0
- Initial release
- Basic code analysis capabilities
- MongoDB sampling and schema inference
- PII detection and redaction
- Knowledge base creation
- CLI interface

### Version 1.1.0 (Planned)
- Advanced relationship inference
- Real-time scanning capabilities
- Enhanced performance monitoring
- Additional export formats
- Graph visualization tools

## Roadmap

### Short Term (3 months)
- [ ] Performance optimizations
- [ ] Additional PII detection patterns
- [ ] Enhanced error handling
- [ ] More comprehensive tests

### Medium Term (6 months)
- [ ] Real-time scanning
- [ ] Advanced analytics
- [ ] Machine learning integration
- [ ] API endpoints

### Long Term (12 months)
- [ ] Multi-language support
- [ ] Cloud-native deployment
- [ ] Advanced visualization
- [ ] Enterprise features
