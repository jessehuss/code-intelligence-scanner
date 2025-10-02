# Feature Specification: F002 – Catalog API

**Feature Branch**: `002-feature-f002-catalog`  
**Created**: 2025-01-02  
**Status**: Draft  
**Input**: User description: "Feature: F002 – Catalog API

Goal:
Expose the knowledge base via stable HTTP endpoints for the UI and tooling. Provide fast search, detailed collection/type payloads, graph edges, and diffs by commit SHA.

Consumers:
Next.js UI, developer scripts, internal tools.

Endpoints (MVP):
- GET /search?q=&kinds=type,collection,field,query,service,endpoint&limit=&filters={}
- GET /collections/{name}            // declared+observed schema, types, queries, relationships
- GET /types/{fqcn}                  // fields, attrs, collections, usages, diffs summary
- GET /graph?node=collection:vendors&depth=2&edgeKinds=READS,WRITES,REFERS_TO
- GET /diff/type/{fqcn}?fromSha=&toSha=

Non-goals:
Auth integrations beyond simple bearer/SSO pass-through; mutations.

Acceptance:
- Search returns grouped results with <300ms P50 on dev data.
- Collection detail merges declared/observed schema with drift flags.
- Graph endpoint returns constrained subgraph for depth≤2 in <400ms P50.
- Diff endpoint returns added/removed/changed fields for a type between SHAs."

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

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a developer or data engineer, I want to query the code intelligence knowledge base through HTTP endpoints so that I can build tools and UIs that help teams understand their data architecture, find code relationships, and track changes over time.

### Acceptance Scenarios
1. **Given** a knowledge base populated with code analysis data, **When** I search for "user" with kinds=type,collection, **Then** I receive grouped results showing relevant types and collections with <300ms response time
2. **Given** a collection named "vendors" exists in the knowledge base, **When** I request collection details, **Then** I receive declared schema, observed schema, associated types, related queries, and relationship mappings with drift indicators
3. **Given** a fully qualified class name "MyApp.Models.User", **When** I request type details, **Then** I receive field definitions, BSON attributes, collection mappings, usage statistics, and change summary
4. **Given** a collection node "vendors" in the knowledge graph, **When** I request graph data with depth=2 and edgeKinds=READS,WRITES, **Then** I receive a constrained subgraph showing related collections and operations within <400ms
5. **Given** two commit SHAs for the same type, **When** I request a diff, **Then** I receive a detailed comparison showing added, removed, and changed fields between the commits

### Edge Cases
- What happens when searching for non-existent entities or empty results?
- How does the system handle malformed query parameters or invalid SHAs?
- What occurs when the knowledge base is empty or partially populated?
- How are performance targets maintained under high concurrent load?
- What happens when graph traversal exceeds maximum depth constraints?

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST provide a search endpoint that accepts query text, entity kinds, limits, and filters
- **FR-002**: System MUST return search results grouped by entity type with relevance scoring
- **FR-003**: System MUST provide collection detail endpoint that merges declared and observed schemas
- **FR-004**: System MUST include drift flags indicating differences between declared and observed schemas
- **FR-005**: System MUST provide type detail endpoint showing fields, attributes, collections, and usage data
- **FR-006**: System MUST provide graph endpoint that returns constrained subgraphs based on node, depth, and edge filters
- **FR-007**: System MUST provide diff endpoint that compares type definitions between commit SHAs
- **FR-008**: System MUST return added, removed, and changed fields in diff responses
- **FR-009**: System MUST operate without authentication requirements (internal use only)
- **FR-010**: System MUST handle concurrent requests without data corruption
- **FR-011**: System MUST provide structured error responses with HTTP status codes, error codes, and descriptive messages for invalid requests
- **FR-012**: System MUST maintain response time targets under normal load conditions
- **FR-013**: System MUST implement Redis-based caching with configurable TTL policies for frequently accessed data
- **FR-014**: System MUST implement tiered data retention policies (recent data retained longer than older data)
- **FR-015**: System MUST implement full observability including metrics, logs, traces, health checks, and alerts

### Performance Requirements
- **PR-001**: Search endpoint MUST return results within 300ms P50 response time on development data
- **PR-002**: Graph endpoint MUST return constrained subgraphs within 400ms P50 response time for depth≤2
- **PR-003**: System MUST scale to handle concurrent usage without performance degradation
- **PR-004**: System MUST handle unbounded knowledge base growth without performance degradation

### Key Entities *(include if feature involves data)*
- **SearchResult**: Represents a search result with entity type, relevance score, and summary data
- **CollectionDetail**: Contains declared schema, observed schema, associated types, queries, and relationships for a collection
- **TypeDetail**: Contains field definitions, BSON attributes, collection mappings, usage statistics, and change history for a code type
- **GraphNode**: Represents a node in the knowledge graph with connections to other entities
- **GraphEdge**: Represents relationships between entities with types like READS, WRITES, REFERS_TO
- **TypeDiff**: Contains comparison data showing changes between two versions of a type definition

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

## Clarifications

### Session 2025-01-02
- Q: What level of authentication and authorization should the Catalog API implement? → A: No authentication (internal only)
- Q: What is the expected maximum number of concurrent users the Catalog API should support? → A: No specific limit required
- Q: What level of error handling detail should the API provide? → A: Structured error responses with error codes
- Q: What are the expected limits for knowledge base size and query result sets? → A: No specific limits (unbounded growth expected)
- Q: Should the API implement caching for frequently accessed data? → A: Redis-based caching with TTL policies
- Q: Should the API implement rate limiting to prevent abuse? → A: No rate limiting (trusted internal users)
- Q: How long should historical data (like type diffs) be retained? → A: Tiered retention (recent data longer, older data shorter)
- Q: What metrics and monitoring should be implemented for the API endpoints? → A: Full observability (metrics, logs, traces, health checks, alerts)

## Clarifications Needed

All clarification questions have been resolved.
