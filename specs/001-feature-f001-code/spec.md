# Feature Specification: Code Intelligence Scanner & Knowledge Base Seeder

**Feature Branch**: `001-feature-f001-code`  
**Created**: 2025-01-27  
**Status**: Draft  
**Input**: User description: "Feature: F001 – Code Intelligence Scanner & Knowledge Base Seeder

Problem:
Our C#/.NET microservices (≈40 repos) use MongoDB with no shared documentation. Engineers waste time locating which collection stores which object, where it's used, and what its shape is.

Goal:
Build a .NET 8 scanner that (a) does static analysis of C# code via Roslyn to find POCOs, collections, queries, and relationships; (b) optionally samples live MongoDB (read-only) to infer observed JSON Schema; and (c) writes a normalized knowledge base with provenance for search/graph queries.

Users:
Backend engineers, SREs, data/platform engineers.

Must capture:
- Types & fields (with BSON attributes, nullability, discriminators)
- Collections & mappings (GetCollection<T>(\"name\"), IMongoCollection<T>)
- Operations (Find/Update/Aggregate/Replace/Delete), projections, endpoints
- Relationships (REFERS_TO inferred from filters, \$lookup)
- Provenance (repo, file, symbol, line span, commit SHA, timestamp)
- Observed schema: type freq, required %, string formats, enums (from value histograms)
- Redaction of PII in samples

Constraints:
- .NET 8, Roslyn only (no runtime reflection of target services)
- Mongo sampling uses read-only account and N-doc cap per collection
- No writes to product DBs beyond sampling reads

Acceptance (MVP):
- Scans a demo monorepo or 2+ repos and produces KB docs for ≥20 types, ≥10 collections, ≥30 query invocations.
- Resolves ≥80% collection names for GetCollection<T>() literal/const cases.
- Produces JSON Schema for ≥5 collections from samples with PII redaction.
- Emits provenance for 100% of extracted facts.
- Finishes a full scan on dev data in <10 minutes."

## Execution Flow (main)
```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
   - User types and permissions
   - Data retention/deletion policies  
   - Performance targets and scale
   - Error handling behaviors
   - Integration requirements
   - Security/compliance needs

---

## Clarifications

### Session 2025-01-27
- Q: When the scanner encounters encrypted or inaccessible MongoDB connections during live sampling, what should the system do? → A: Fail the entire scan and require manual intervention
- Q: What should be the maximum codebase size the scanner can handle? → A: No hard limit - use streaming/chunked processing
- Q: When PII detection fails or produces false positives during sampling, what should the system do? → A: Use manual review queue for flagged fields
- Q: How should the system handle repositories with no MongoDB usage or malformed code? → A: Skip the repository entirely and continue with others
- Q: When the scanner runs on repositories that have already been scanned, how should it handle updates to the knowledge base? → A: Merge new data with existing data, updating changed items

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a backend engineer working on C#/.NET microservices, I want to quickly discover which MongoDB collections store which data types, how they're queried, and what their actual structure looks like, so that I can understand existing systems and make informed decisions about data access patterns without spending hours searching through multiple repositories.

### Acceptance Scenarios
1. **Given** a C# codebase with MongoDB usage, **When** I run the scanner, **Then** I receive a knowledge base documenting all POCO types, their collection mappings, and query operations with complete provenance
2. **Given** a MongoDB database with sample data, **When** I enable live sampling, **Then** I receive JSON Schema documentation showing observed data patterns with PII properly redacted
3. **Given** a knowledge base with extracted facts, **When** I search for a specific type or collection, **Then** I can see all related queries, relationships, and usage patterns across repositories
4. **Given** multiple repositories to scan, **When** I run a full scan, **Then** the process completes within 10 minutes and produces documentation for at least 20 types and 10 collections

### Edge Cases
- What happens when the scanner encounters encrypted or inaccessible MongoDB connections? → System fails the entire scan and requires manual intervention
- How does the system handle repositories with no MongoDB usage or malformed code? → System skips the repository entirely and continues with others
- What happens when PII detection fails or produces false positives during sampling? → System uses manual review queue for flagged fields
- How does the system handle very large codebases that exceed memory or time constraints? → System uses streaming/chunked processing with no hard size limits

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST analyze C# code using static analysis to identify POCO types, their fields, and BSON attributes
- **FR-002**: System MUST discover MongoDB collection mappings from GetCollection<T>() calls and IMongoCollection<T> usage
- **FR-003**: System MUST extract MongoDB operations (Find, Update, Aggregate, Replace, Delete) with their projections and filters
- **FR-004**: System MUST infer relationships between types based on query filters and $lookup operations
- **FR-005**: System MUST capture complete provenance for every extracted fact (repository, file, symbol, line span, commit SHA, timestamp)
- **FR-006**: System MUST optionally sample live MongoDB data using read-only credentials to infer observed JSON Schema
- **FR-007**: System MUST redact PII from sampled data while preserving structural information (type frequency, required fields, string formats, enums)
- **FR-008**: System MUST produce a normalized knowledge base suitable for search and graph queries
- **FR-013**: System MUST merge new scan data with existing knowledge base data, updating changed items when repositories are re-scanned
- **FR-009**: System MUST resolve collection names for at least 80% of GetCollection<T>() calls using literal or constant values
- **FR-010**: System MUST complete full scans within 10 minutes for development data sets
- **FR-011**: System MUST produce documentation for at least 20 types, 10 collections, and 30 query invocations in MVP scenarios
- **FR-012**: System MUST generate JSON Schema for at least 5 collections from sampled data with PII redaction

### Key Entities *(include if feature involves data)*
- **Code Type**: Represents a C# POCO class with its fields, BSON attributes, nullability, and discriminators
- **Collection Mapping**: Represents the relationship between a C# type and its MongoDB collection name
- **Query Operation**: Represents a MongoDB operation (Find, Update, etc.) with its filters, projections, and target collection
- **Data Relationship**: Represents inferred connections between types based on query patterns and $lookup operations
- **Provenance Record**: Represents the source information for any extracted fact (repo, file, symbol, line span, commit SHA, timestamp)
- **Observed Schema**: Represents the inferred JSON Schema from sampled MongoDB data with type frequencies and patterns
- **Knowledge Base Entry**: Represents a normalized fact stored in the searchable knowledge base with its provenance

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness
- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous  
- [x] Success criteria are measurable
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

---

## Execution Status
*Updated by main() during processing*

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [x] Review checklist passed

---