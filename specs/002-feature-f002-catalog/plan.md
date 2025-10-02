
# Implementation Plan: F002 – Catalog API

**Branch**: `002-feature-f002-catalog` | **Date**: 2025-01-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-feature-f002-catalog/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Build a REST API service that exposes the code intelligence knowledge base via HTTP endpoints for search, collection details, type information, graph relationships, and diff comparisons. The API will provide fast search capabilities using Atlas Search, detailed payloads for collections and types, graph traversal with configurable depth, and commit-based diff functionality. Performance targets: <300ms P50 for search, <400ms P50 for graph operations.

## Technical Context
**Language/Version**: .NET 8 + C#  
**Primary Dependencies**: ASP.NET Core Minimal APIs, MongoDB.Driver, Atlas Search, Redis  
**Storage**: MongoDB "catalog_kb" database, Atlas Search for text queries, Redis for caching  
**Testing**: xUnit, ASP.NET Core Test Host, MongoDB Test Containers  
**Target Platform**: Linux server (Docker container)  
**Project Type**: web (backend API service)  
**Performance Goals**: <300ms P50 search, <400ms P50 graph operations, handle unbounded growth  
**Constraints**: No authentication (internal only), structured error responses, Redis caching with TTL  
**Scale/Scope**: Unbounded knowledge base growth, no concurrent user limits, full observability

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Technology Stack Compliance ✅
- **.NET 8 + C#**: ✅ Using ASP.NET Core Minimal APIs for backend
- **MongoDB.Driver**: ✅ Using for all data access operations
- **No deviations**: ✅ Following specified stack constraints

### Security & Privacy Compliance ✅
- **Read-only credentials**: ✅ API only reads from knowledge base, no writes
- **PII redaction**: ✅ Not applicable (API doesn't store samples)
- **Auditable access**: ✅ All API calls will be logged with provenance

### Observability Compliance ✅
- **Structured logging**: ✅ Full observability requirement (FR-015)
- **Provenance tracking**: ✅ API will log request provenance
- **Searchable logs**: ✅ Using standard .NET logging

### Quality Gates Compliance ✅
- **Unit tests**: ✅ Planned for handlers with fake repositories
- **Integration tests**: ✅ Planned with seeded MongoDB container
- **Data access validation**: ✅ Contract tests for all endpoints

### Developer Experience Compliance ✅
- **Deep links**: ✅ API will provide Git references in responses
- **Copy helpers**: ✅ API will provide MongoDB shell examples
- **Clear documentation**: ✅ OpenAPI contracts and quickstart guide

**Status**: ✅ PASS - All constitutional requirements satisfied

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
apps/catalog-api/
├── src/
│   ├── Models/
│   │   ├── DTOs/
│   │   │   ├── SearchResult.cs
│   │   │   ├── CollectionDetail.cs
│   │   │   ├── TypeDetail.cs
│   │   │   ├── GraphNode.cs
│   │   │   ├── GraphEdge.cs
│   │   │   └── TypeDiff.cs
│   │   └── Requests/
│   │       ├── SearchRequest.cs
│   │       ├── GraphRequest.cs
│   │       └── DiffRequest.cs
│   ├── Handlers/
│   │   ├── SearchHandler.cs
│   │   ├── CollectionsHandler.cs
│   │   ├── TypesHandler.cs
│   │   ├── GraphHandler.cs
│   │   └── DiffHandler.cs
│   ├── Services/
│   │   ├── IKnowledgeBaseService.cs
│   │   ├── KnowledgeBaseService.cs
│   │   ├── ICacheService.cs
│   │   ├── CacheService.cs
│   │   └── IObservabilityService.cs
│   ├── Middleware/
│   │   ├── ErrorHandlingMiddleware.cs
│   │   ├── LoggingMiddleware.cs
│   │   └── CachingMiddleware.cs
│   ├── Configuration/
│   │   ├── ApiConfiguration.cs
│   │   └── CacheConfiguration.cs
│   ├── Program.cs
│   └── appsettings.json
├── tests/
│   ├── unit/
│   │   ├── Handlers/
│   │   ├── Services/
│   │   └── Models/
│   ├── integration/
│   │   ├── test_search_endpoints.cs
│   │   ├── test_collections_endpoints.cs
│   │   ├── test_types_endpoints.cs
│   │   ├── test_graph_endpoints.cs
│   │   └── test_diff_endpoints.cs
│   └── contract/
│       ├── test_search_contract.cs
│       ├── test_collections_contract.cs
│       ├── test_types_contract.cs
│       ├── test_graph_contract.cs
│       └── test_diff_contract.cs
├── Dockerfile
└── README.md
```

**Structure Decision**: Web application structure with separate API service. The catalog-api is a standalone ASP.NET Core service that will be deployed as a Docker container. This structure supports the constitutional requirements for .NET 8, MongoDB.Driver, and comprehensive testing.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - Atlas Search integration patterns for .NET
   - Redis caching best practices for ASP.NET Core
   - MongoDB aggregation pipelines for graph traversal
   - Performance optimization for search endpoints
   - Error handling patterns for REST APIs

2. **Generate and dispatch research agents**:
   ```
   Task: "Research Atlas Search integration patterns for .NET 8"
   Task: "Find Redis caching best practices for ASP.NET Core Minimal APIs"
   Task: "Research MongoDB aggregation pipelines for graph traversal"
   Task: "Find performance optimization patterns for search endpoints"
   Task: "Research error handling patterns for REST APIs with structured responses"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all technical decisions documented

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - SearchResult, CollectionDetail, TypeDetail, GraphNode, GraphEdge, TypeDiff
   - Request/Response DTOs with validation rules
   - Error response structure with HTTP status codes

2. **Generate API contracts** from functional requirements:
   - GET /search → SearchHandler
   - GET /collections/{name} → CollectionsHandler  
   - GET /types/{fqcn} → TypesHandler
   - GET /graph → GraphHandler
   - GET /diff/type/{fqcn} → DiffHandler
   - Output OpenAPI 3.0 schema to `/contracts/catalog-api.yaml`

3. **Generate contract tests** from contracts:
   - test_search_contract.cs, test_collections_contract.cs, test_types_contract.cs
   - test_graph_contract.cs, test_diff_contract.cs
   - Assert request/response schemas, tests must fail initially

4. **Extract test scenarios** from user stories:
   - Search scenario → test_search_endpoints.cs
   - Collection detail scenario → test_collections_endpoints.cs
   - Type detail scenario → test_types_endpoints.cs
   - Graph traversal scenario → test_graph_endpoints.cs
   - Diff comparison scenario → test_diff_endpoints.cs

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType cursor`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - Add ASP.NET Core Minimal APIs, Atlas Search, Redis caching
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/catalog-api.yaml, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each DTO → model creation task [P] 
- Each handler → implementation task
- Each service → implementation task
- Integration tests for each endpoint
- Middleware and configuration setup

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models → Services → Handlers → Middleware → Configuration
- Mark [P] for parallel execution (independent files)
- Setup tasks first (project structure, dependencies)

**Estimated Output**: 30-35 numbered, ordered tasks in tasks.md covering:
- Project setup and dependencies
- Contract tests (5 endpoints)
- DTO models (6 models)
- Service implementations (KnowledgeBase, Cache, Observability)
- Handler implementations (5 handlers)
- Middleware (Error handling, Logging, Caching)
- Configuration and startup
- Integration tests
- Docker and deployment setup

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [x] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `/memory/constitution.md`*
