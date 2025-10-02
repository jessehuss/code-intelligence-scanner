# Tasks: F002 – Catalog API

**Input**: Design documents from `/specs/002-feature-f002-catalog/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, handlers
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Web app**: `apps/catalog-api/src/`, `apps/catalog-api/tests/`
- Paths based on implementation plan structure

## Phase 3.1: Setup
- [x] T001 Create project structure in apps/catalog-api/ with src/ and tests/ directories
- [x] T002 Initialize .NET 8 project with ASP.NET Core Minimal APIs, MongoDB.Driver, Redis, and testing dependencies
- [x] T003 [P] Configure appsettings.json with connection strings and cache TTL settings
- [x] T004 [P] Create Dockerfile for container deployment

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [x] T005 [P] Contract test GET /search in apps/catalog-api/tests/contract/test_search_contract.cs
- [x] T006 [P] Contract test GET /collections/{name} in apps/catalog-api/tests/contract/test_collections_contract.cs
- [x] T007 [P] Contract test GET /types/{fqcn} in apps/catalog-api/tests/contract/test_types_contract.cs
- [x] T008 [P] Contract test GET /graph in apps/catalog-api/tests/contract/test_graph_contract.cs
- [x] T009 [P] Contract test GET /diff/type/{fqcn} in apps/catalog-api/tests/contract/test_diff_contract.cs
- [x] T010 [P] Integration test search endpoint in apps/catalog-api/tests/integration/test_search_endpoints.cs
- [x] T011 [P] Integration test collections endpoint in apps/catalog-api/tests/integration/test_collections_endpoints.cs
- [x] T012 [P] Integration test types endpoint in apps/catalog-api/tests/integration/test_types_endpoints.cs
- [x] T013 [P] Integration test graph endpoint in apps/catalog-api/tests/integration/test_graph_endpoints.cs
- [x] T014 [P] Integration test diff endpoint in apps/catalog-api/tests/integration/test_diff_endpoints.cs

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [x] T015 [P] SearchResult DTO model in apps/catalog-api/src/Models/DTOs/SearchResult.cs
- [x] T016 [P] CollectionDetail DTO model in apps/catalog-api/src/Models/DTOs/CollectionDetail.cs
- [x] T017 [P] TypeDetail DTO model in apps/catalog-api/src/Models/DTOs/TypeDetail.cs
- [x] T018 [P] GraphNode DTO model in apps/catalog-api/src/Models/DTOs/GraphNode.cs
- [x] T019 [P] GraphEdge DTO model in apps/catalog-api/src/Models/DTOs/GraphEdge.cs
- [x] T020 [P] TypeDiff DTO model in apps/catalog-api/src/Models/DTOs/TypeDiff.cs
- [x] T021 [P] SearchRequest model in apps/catalog-api/src/Models/Requests/SearchRequest.cs
- [x] T022 [P] GraphRequest model in apps/catalog-api/src/Models/Requests/GraphRequest.cs
- [x] T023 [P] DiffRequest model in apps/catalog-api/src/Models/Requests/DiffRequest.cs
- [x] T024 [P] ErrorResponse model in apps/catalog-api/src/Models/ErrorResponse.cs
- [x] T025 [P] IKnowledgeBaseService interface in apps/catalog-api/src/Services/IKnowledgeBaseService.cs
- [x] T026 [P] ICacheService interface in apps/catalog-api/src/Services/ICacheService.cs
- [x] T027 [P] IObservabilityService interface in apps/catalog-api/src/Services/IObservabilityService.cs
- [x] T028 KnowledgeBaseService implementation in apps/catalog-api/src/Services/KnowledgeBaseService.cs
- [x] T029 CacheService implementation in apps/catalog-api/src/Services/CacheService.cs
- [x] T030 ObservabilityService implementation in apps/catalog-api/src/Services/ObservabilityService.cs
- [x] T031 SearchHandler implementation in apps/catalog-api/src/Handlers/SearchHandler.cs
- [x] T032 CollectionsHandler implementation in apps/catalog-api/src/Handlers/CollectionsHandler.cs
- [x] T033 TypesHandler implementation in apps/catalog-api/src/Handlers/TypesHandler.cs
- [x] T034 GraphHandler implementation in apps/catalog-api/src/Handlers/GraphHandler.cs
- [x] T035 DiffHandler implementation in apps/catalog-api/src/Handlers/DiffHandler.cs

## Phase 3.4: Integration
- [x] T036 ErrorHandlingMiddleware in apps/catalog-api/src/Middleware/ErrorHandlingMiddleware.cs
- [x] T037 LoggingMiddleware in apps/catalog-api/src/Middleware/LoggingMiddleware.cs
- [x] T038 CachingMiddleware in apps/catalog-api/src/Middleware/CachingMiddleware.cs
- [x] T039 ApiConfiguration in apps/catalog-api/src/Configuration/ApiConfiguration.cs
- [x] T040 CacheConfiguration in apps/catalog-api/src/Configuration/CacheConfiguration.cs
- [x] T041 Program.cs with DI setup, middleware pipeline, and endpoint registration
- [x] T042 Health check endpoints (/health, /health/mongodb, /health/redis, /health/atlas-search)

## Phase 3.5: Polish
- [ ] T043 [P] Unit tests for SearchHandler in apps/catalog-api/tests/unit/Handlers/test_search_handler.cs
- [ ] T044 [P] Unit tests for CollectionsHandler in apps/catalog-api/tests/unit/Handlers/test_collections_handler.cs
- [ ] T045 [P] Unit tests for TypesHandler in apps/catalog-api/tests/unit/Handlers/test_types_handler.cs
- [ ] T046 [P] Unit tests for GraphHandler in apps/catalog-api/tests/unit/Handlers/test_graph_handler.cs
- [ ] T047 [P] Unit tests for DiffHandler in apps/catalog-api/tests/unit/Handlers/test_diff_handler.cs
- [ ] T048 [P] Unit tests for KnowledgeBaseService in apps/catalog-api/tests/unit/Services/test_knowledge_base_service.cs
- [ ] T049 [P] Unit tests for CacheService in apps/catalog-api/tests/unit/Services/test_cache_service.cs
- [ ] T050 [P] Unit tests for DTO models in apps/catalog-api/tests/unit/Models/test_dto_models.cs
- [ ] T051 Performance tests for search endpoint (<300ms P50)
- [ ] T052 Performance tests for graph endpoint (<400ms P50)
- [ ] T053 [P] Update README.md with usage examples
- [ ] T054 [P] Create GitHub Actions workflow for build/test
- [ ] T055 Validate quickstart.md scenarios

## Dependencies
- Tests (T005-T014) before implementation (T015-T035)
- T015-T023 (DTO models) before T025-T027 (service interfaces)
- T025-T027 (service interfaces) before T028-T030 (service implementations)
- T028-T030 (service implementations) before T031-T035 (handlers)
- T031-T035 (handlers) before T036-T042 (integration)
- Implementation before polish (T043-T055)

## Parallel Example
```
# Launch T005-T014 together (all contract and integration tests):
Task: "Contract test GET /search in apps/catalog-api/tests/contract/test_search_contract.cs"
Task: "Contract test GET /collections/{name} in apps/catalog-api/tests/contract/test_collections_contract.cs"
Task: "Contract test GET /types/{fqcn} in apps/catalog-api/tests/contract/test_types_contract.cs"
Task: "Contract test GET /graph in apps/catalog-api/tests/contract/test_graph_contract.cs"
Task: "Contract test GET /diff/type/{fqcn} in apps/catalog-api/tests/contract/test_diff_contract.cs"
Task: "Integration test search endpoint in apps/catalog-api/tests/integration/test_search_endpoints.cs"
Task: "Integration test collections endpoint in apps/catalog-api/tests/integration/test_collections_endpoints.cs"
Task: "Integration test types endpoint in apps/catalog-api/tests/integration/test_types_endpoints.cs"
Task: "Integration test graph endpoint in apps/catalog-api/tests/integration/test_graph_endpoints.cs"
Task: "Integration test diff endpoint in apps/catalog-api/tests/integration/test_diff_endpoints.cs"

# Launch T015-T023 together (all DTO models):
Task: "SearchResult DTO model in apps/catalog-api/src/Models/DTOs/SearchResult.cs"
Task: "CollectionDetail DTO model in apps/catalog-api/src/Models/DTOs/CollectionDetail.cs"
Task: "TypeDetail DTO model in apps/catalog-api/src/Models/DTOs/TypeDetail.cs"
Task: "GraphNode DTO model in apps/catalog-api/src/Models/DTOs/GraphNode.cs"
Task: "GraphEdge DTO model in apps/catalog-api/src/Models/DTOs/GraphEdge.cs"
Task: "TypeDiff DTO model in apps/catalog-api/src/Models/DTOs/TypeDiff.cs"
Task: "SearchRequest model in apps/catalog-api/src/Models/Requests/SearchRequest.cs"
Task: "GraphRequest model in apps/catalog-api/src/Models/Requests/GraphRequest.cs"
Task: "DiffRequest model in apps/catalog-api/src/Models/Requests/DiffRequest.cs"
Task: "ErrorResponse model in apps/catalog-api/src/Models/ErrorResponse.cs"

# Launch T025-T027 together (service interfaces):
Task: "IKnowledgeBaseService interface in apps/catalog-api/src/Services/IKnowledgeBaseService.cs"
Task: "ICacheService interface in apps/catalog-api/src/Services/ICacheService.cs"
Task: "IObservabilityService interface in apps/catalog-api/src/Services/IObservabilityService.cs"

# Launch T043-T050 together (unit tests):
Task: "Unit tests for SearchHandler in apps/catalog-api/tests/unit/Handlers/test_search_handler.cs"
Task: "Unit tests for CollectionsHandler in apps/catalog-api/tests/unit/Handlers/test_collections_handler.cs"
Task: "Unit tests for TypesHandler in apps/catalog-api/tests/unit/Handlers/test_types_handler.cs"
Task: "Unit tests for GraphHandler in apps/catalog-api/tests/unit/Handlers/test_graph_handler.cs"
Task: "Unit tests for DiffHandler in apps/catalog-api/tests/unit/Handlers/test_diff_handler.cs"
Task: "Unit tests for KnowledgeBaseService in apps/catalog-api/tests/unit/Services/test_knowledge_base_service.cs"
Task: "Unit tests for CacheService in apps/catalog-api/tests/unit/Services/test_cache_service.cs"
Task: "Unit tests for DTO models in apps/catalog-api/tests/unit/Models/test_dto_models.cs"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts
- Follow constitutional requirements: .NET 8, MongoDB.Driver, structured logging, read-only access

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts**:
   - Each contract file → contract test task [P]
   - Each endpoint → implementation task
   
2. **From Data Model**:
   - Each entity → model creation task [P]
   - Relationships → service layer tasks
   
3. **From User Stories**:
   - Each story → integration test [P]
   - Quickstart scenarios → validation tasks

4. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Dependencies block parallel execution

## Validation Checklist
*GATE: Checked by main() before returning*

- [x] All contracts have corresponding tests
- [x] All entities have model tasks
- [x] All tests come before implementation
- [x] Parallel tasks truly independent
- [x] Each task specifies exact file path
- [x] No task modifies same file as another [P] task
