# Feature Specification: F003 – Catalog Explorer UI

**Feature Branch**: `004-feature-f003-catalog`  
**Created**: 2024-12-19  
**Status**: Draft  
**Input**: User description: "Feature: F003 – Catalog Explorer UI

Goal:
A search-first web app to discover Collections, Types, Fields, Queries, Services, and their relationships, with deep links to source lines, drift indicators, and query helpers.

Users:
Backend devs & SREs.

Screens (MVP):
1) Global Search – instant, federated results (kinds grouped) + facets (repo, service, operation, changed_since).
2) Collection Detail – Schema tab (declared vs observed with % presence and drift badges), Types, Queries (with code snippets), Relationships (mini graph).
3) Type Detail – class header, fields/attrs table, collections used, usages, diff summary (between SHAs).
4) Graph View – Cytoscape force graph, filters by edgeKinds and depth, click-through to details.
5) Query Helper – for a selected type/field path, generate Mongo shell and Builders<T> examples.

Constraints:
- Next.js (TypeScript, App Router), Tailwind, shadcn/ui, SWR
- Monaco Editor for code snippets
- Cytoscape.js for graphs
- Keyboard navigation; copy-as buttons; Git deep links

Acceptance:
- Search keystroke-to-result <300ms P50 (dev data, cached)
- Collection and Type pages render in <400ms P50 after cache warm
- Copy-as buttons provide valid examples for Mongo shell and C# Builders<T>"

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

### Session 2024-12-19
- Q: Authentication & Authorization Model → A: No authentication required (open access)
- Q: Concurrent User Scale → A: 10-50 concurrent users
- Q: Error Handling for Broken Source Links → A: Redirect to repository root
- Q: Data Volume Scale Assumptions → A: Up to 10,000 total
- Q: Search Result Limits → A: Show all results with pagination

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As a backend developer or SRE, I want to quickly discover and understand the structure of Collections, Types, Fields, Queries, and Services in my codebase, so that I can efficiently navigate relationships, identify drift between declared and observed schemas, and generate appropriate queries for my work.

### Acceptance Scenarios
1. **Given** I am a backend developer looking for a specific collection, **When** I type in the global search, **Then** I see instant federated results grouped by kind (Collections, Types, Fields, Queries, Services) with relevant facets
2. **Given** I want to understand a collection's schema, **When** I navigate to a Collection Detail page, **Then** I see declared vs observed schema with presence percentages and drift indicators
3. **Given** I need to understand how a type is used, **When** I view a Type Detail page, **Then** I see the class structure, field attributes, collections that use it, and usage patterns
4. **Given** I want to visualize relationships, **When** I open the Graph View, **Then** I see an interactive force graph with filters for edge types and depth
5. **Given** I need to write queries for a specific type/field, **When** I use the Query Helper, **Then** I get valid examples for Mongo shell and C# Builders<T>

### Edge Cases
- What happens when search returns no results?
- How does the system handle large codebases with up to 10,000 entities?
- What occurs when drift indicators show significant schema differences?
- When source code links are broken or unavailable, the system redirects to repository root

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST provide instant global search across Collections, Types, Fields, Queries, and Services with federated results grouped by kind and pagination for all results
- **FR-002**: System MUST display search results with facets for repository, service, operation, and changed_since filters
- **FR-003**: System MUST show Collection Detail pages with schema comparison (declared vs observed), presence percentages, and drift badges
- **FR-004**: System MUST display Type Detail pages with class headers, field/attribute tables, collection usage, and diff summaries between SHAs
- **FR-005**: System MUST provide Graph View with interactive force graph visualization, edge type filters, and depth controls
- **FR-006**: System MUST generate Query Helper examples for Mongo shell and C# Builders<T> based on selected type/field paths
- **FR-007**: System MUST provide deep links to source code lines for all entities
- **FR-008**: System MUST support keyboard navigation throughout the interface
- **FR-009**: System MUST provide copy-as functionality for generated code examples
- **FR-010**: System MUST render search results within 300ms P50 for cached development data
- **FR-011**: System MUST render Collection and Type detail pages within 400ms P50 after cache warm-up
- **FR-012**: System MUST provide valid, executable examples for Mongo shell and C# Builders<T> through copy-as buttons

- **FR-013**: System MUST redirect to repository root when source code links are broken or unavailable
- **FR-014**: System MUST support 10-50 concurrent users without performance degradation
- **FR-015**: System MUST provide open access without authentication requirements

### Non-Functional Requirements
- **NFR-001**: System MUST handle up to 10,000 total entities (Collections, Types, Fields, Queries, Services) without performance degradation
- **NFR-002**: System MUST support 10-50 concurrent users with search response times under 300ms P50
- **NFR-003**: System MUST render Collection and Type detail pages within 400ms P50 after cache warm-up
- **NFR-004**: System MUST provide open access without authentication or authorization requirements

### Key Entities
- **Collection**: Represents a data collection with declared schema, observed schema, presence metrics, and drift indicators
- **Type**: Represents a code type/class with fields, attributes, usage across collections, and version history
- **Field**: Represents individual properties/attributes within types with metadata and usage patterns
- **Query**: Represents database query operations with code snippets and relationship to collections/types
- **Service**: Represents service definitions with their relationships to collections and types
- **Relationship**: Represents connections between entities (collections to types, types to fields, etc.) with edge types and metadata

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