# Research: Code Intelligence Scanner & Knowledge Base Seeder

**Date**: 2025-01-27  
**Feature**: 001-feature-f001-code  
**Status**: Complete

## Research Tasks Executed

### 1. Roslyn Static Analysis Patterns for MongoDB Usage

**Task**: Research Roslyn static analysis patterns for MongoDB usage in C# codebases

**Decision**: Use Microsoft.CodeAnalysis.CSharp for syntax tree analysis with specific patterns for MongoDB operations

**Rationale**: 
- Roslyn provides comprehensive C# syntax analysis capabilities
- Can detect ClassDeclarationSyntax with BSON attributes
- Can identify IMongoDatabase.GetCollection<T>() calls and IMongoCollection<T> usage
- Supports lambda expression analysis for field path extraction
- Enables data-flow analysis for string constant resolution

**Alternatives considered**:
- Reflection-based analysis: Rejected due to constitutional constraint (no runtime reflection)
- External tools (NDepend, SonarQube): Rejected as they don't provide the granular control needed
- Custom parser: Rejected as too complex and error-prone

### 2. MongoDB Data Sampling with PII Redaction

**Task**: Find best practices for MongoDB data sampling with PII redaction

**Decision**: Implement heuristic-based PII detection with manual review queue for flagged fields

**Rationale**:
- Field name heuristics (email, phone, ssn, token, key, address, name, ip, jwt, credit)
- Value pattern detection via regex for common PII formats
- Manual review queue for ambiguous cases
- Preserve structural information (type frequency, required fields, string formats, enums)
- Use read-only credentials with configurable document limits per collection

**Alternatives considered**:
- Machine learning-based PII detection: Rejected as too complex for MVP
- Complete data redaction: Rejected as it would eliminate useful schema information
- No PII detection: Rejected due to constitutional security requirements

### 3. Knowledge Base Schema Design

**Task**: Research knowledge base schema design for code intelligence systems

**Decision**: MongoDB-based schema with separate collections for different entity types and relationships

**Rationale**:
- Primary store: MongoDB "catalog_kb" with collections for nodes, edges, schemas, provenance
- Secondary: Atlas Search index "kb_search" for text queries
- Optional: Neo4j for graph queries (behind feature flag)
- Support for incremental updates and merge operations
- Complete provenance tracking for every fact

**Alternatives considered**:
- Single collection with embedded documents: Rejected as it would limit query flexibility
- Graph database only: Rejected as it would complicate text search capabilities
- Relational database: Rejected as it doesn't match the document-oriented nature of the data

### 4. Performance Optimization for Large Codebases

**Task**: Find performance optimization patterns for large codebase analysis

**Decision**: Implement streaming/chunked processing with no hard size limits

**Rationale**:
- Process files in chunks to avoid memory exhaustion
- Use async/await patterns for I/O operations
- Implement incremental scanning based on commit SHA tracking
- Support for parallel processing of independent files
- Configurable timeouts and resource limits

**Alternatives considered**:
- Hard size limits: Rejected as it would limit scalability
- Single-threaded processing: Rejected as it would be too slow for large codebases
- Complete in-memory processing: Rejected as it would cause memory issues

## Technical Decisions Summary

| Decision Area | Chosen Approach | Key Benefits |
|---------------|-----------------|--------------|
| Static Analysis | Roslyn with pattern matching | Comprehensive C# analysis, no runtime reflection |
| PII Detection | Heuristic-based with manual review | Balanced security and utility, constitutional compliance |
| Knowledge Base | MongoDB with Atlas Search | Flexible queries, text search, incremental updates |
| Performance | Streaming/chunked processing | Scalable, memory-efficient, no hard limits |
| Error Handling | Fail fast for critical issues, skip for non-critical | Clear failure modes, continued operation where possible |

## Implementation Notes

- All decisions align with constitutional requirements
- Performance targets: <10 minutes for development data sets
- Security: Read-only credentials, mandatory PII redaction
- Observability: Complete provenance tracking for every extracted fact
- Quality: Unit tests for resolvers, integration tests with MongoDB

## Next Steps

Research complete. Ready for Phase 1 design and contract generation.
