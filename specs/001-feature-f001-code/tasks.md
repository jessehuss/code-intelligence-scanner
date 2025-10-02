# Tasks: Code Intelligence Scanner & Knowledge Base Seeder

**Input**: Design documents from `/specs/001-feature-f001-code/`
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
   → Core: models, services, CLI commands
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
- **Single project**: `apps/scanner/src/`, `apps/scanner/tests/`
- Paths shown below assume single project structure

## Phase 3.1: Setup
- [x] T001 Create project structure per implementation plan
- [x] T002 Initialize .NET 8 console application with Microsoft.CodeAnalysis.*, MongoDB.Driver, Microsoft.Extensions.* dependencies
- [x] T003 [P] Configure linting and formatting tools (EditorConfig, analyzers)

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [x] T004 [P] Contract test POST /api/v1/scanner/scan in apps/scanner/tests/contract/test_scanner_scan_post.cs
- [x] T005 [P] Contract test GET /api/v1/scanner/status/{scanId} in apps/scanner/tests/contract/test_scanner_status_get.cs
- [x] T006 [P] Contract test POST /api/v1/kb/search in apps/scanner/tests/contract/test_kb_search_post.cs
- [x] T007 [P] Contract test GET /api/v1/kb/types/{typeId} in apps/scanner/tests/contract/test_kb_types_get.cs
- [x] T008 [P] Integration test full scan workflow in apps/scanner/tests/integration/test_full_scan_workflow.cs
- [x] T009 [P] Integration test incremental scan workflow in apps/scanner/tests/integration/test_incremental_scan_workflow.cs
- [x] T010 [P] Integration test MongoDB sampling with PII redaction in apps/scanner/tests/integration/test_mongodb_sampling.cs
- [x] T011 [P] Integration test knowledge base search functionality in apps/scanner/tests/integration/test_kb_search.cs

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [x] T012 [P] CodeType model in apps/scanner/src/Models/CodeType.cs
- [x] T013 [P] CollectionMapping model in apps/scanner/src/Models/CollectionMapping.cs
- [x] T014 [P] QueryOperation model in apps/scanner/src/Models/QueryOperation.cs
- [x] T015 [P] DataRelationship model in apps/scanner/src/Models/DataRelationship.cs
- [x] T016 [P] ProvenanceRecord model in apps/scanner/src/Models/ProvenanceRecord.cs
- [x] T017 [P] ObservedSchema model in apps/scanner/src/Models/ObservedSchema.cs
- [x] T018 [P] KnowledgeBaseEntry model in apps/scanner/src/Models/KnowledgeBaseEntry.cs
- [x] T019 [P] POCOExtractor service in apps/scanner/src/Analyzers/POCOExtractor.cs
- [x] T020 [P] CollectionResolver service in apps/scanner/src/Resolvers/CollectionResolver.cs
- [x] T021 [P] OperationExtractor service in apps/scanner/src/Analyzers/OperationExtractor.cs
- [x] T022 [P] RelationshipInferencer service in apps/scanner/src/Analyzers/RelationshipInferencer.cs
- [x] T023 [P] MongoSampler service in apps/scanner/src/Samplers/MongoSampler.cs
- [x] T024 [P] KnowledgeBaseWriter service in apps/scanner/src/KnowledgeBase/KnowledgeBaseWriter.cs
- [x] T025 [P] IncrementalScanner service in apps/scanner/src/Services/IncrementalScanner.cs
- [x] T026 [P] ScanCommand CLI command in apps/scanner/src/Commands/ScanCommand.cs
- [x] T027 [P] SearchCommand CLI command in apps/scanner/src/Commands/SearchCommand.cs
- [x] T028 [P] GetTypeCommand CLI command in apps/scanner/src/Commands/GetTypeCommand.cs
- [x] T029 [P] Program.cs entry point with DI container setup in apps/scanner/src/Program.cs

## Phase 3.4: Integration
- [x] T030 [P] MongoDB connection and configuration in apps/scanner/src/KnowledgeBase/MongoDbConnection.cs
- [x] T031 [P] Atlas Search index creation and management in apps/scanner/src/KnowledgeBase/AtlasSearchManager.cs
- [x] T032 [P] Configuration binding and validation in apps/scanner/src/Configuration/ScannerConfiguration.cs
- [x] T033 [P] Logging and observability setup in apps/scanner/src/Logging/ScannerLogger.cs
- [x] T034 [P] Error handling and exception management in apps/scanner/src/Services/ErrorHandler.cs
- [x] T035 [P] Performance monitoring and metrics in apps/scanner/src/Services/PerformanceMonitor.cs

## Phase 3.5: Polish
- [x] T036 [P] Unit tests for CollectionResolver in apps/scanner/tests/unit/test_collection_resolver.cs
- [x] T037 [P] Unit tests for POCOExtractor in apps/scanner/tests/unit/test_poco_extractor.cs
- [x] T038 [P] Unit tests for OperationExtractor in apps/scanner/tests/unit/test_operation_extractor.cs
- [x] T039 [P] Unit tests for RelationshipInferencer in apps/scanner/tests/unit/test_relationship_inferencer.cs
- [x] T040 [P] Unit tests for MongoSampler in apps/scanner/tests/unit/test_mongo_sampler.cs
- [x] T041 [P] Unit tests for KnowledgeBaseWriter in apps/scanner/tests/unit/test_kb_writer.cs
- [x] T042 [P] Performance tests for large codebase scanning in apps/scanner/tests/performance/test_large_codebase_scan.cs
- [x] T043 [P] Documentation for KB schema in docs/KB-schema.md
- [x] T044 [P] Documentation for PII redaction policies in docs/PII-redaction.md
- [x] T045 [P] GitHub Actions workflow for CI/CD in infra/github-actions/catalog-scan.yml
- [x] T046 [P] README with usage examples and troubleshooting in apps/scanner/README.md
- [x] T047 [P] Configuration examples and templates in apps/scanner/config/
- [x] T048 [P] Docker containerization for deployment in apps/scanner/Dockerfile
- [x] T049 [P] Health checks and monitoring endpoints in apps/scanner/src/Health/HealthChecks.cs
- [x] T050 [P] Integration with external logging systems in apps/scanner/src/Logging/ExternalLogging.cs

## Dependencies
- Tests (T004-T011) before implementation (T012-T029)
- Models (T012-T018) before services (T019-T025)
- Services before CLI commands (T026-T028)
- CLI commands before Program.cs (T029)
- Core implementation before integration (T030-T035)
- Integration before polish (T036-T050)

## Parallel Example
```
# Launch T004-T011 together (Contract and Integration Tests):
Task: "Contract test POST /api/v1/scanner/scan in apps/scanner/tests/contract/test_scanner_scan_post.cs"
Task: "Contract test GET /api/v1/scanner/status/{scanId} in apps/scanner/tests/contract/test_scanner_status_get.cs"
Task: "Contract test POST /api/v1/kb/search in apps/scanner/tests/contract/test_kb_search_post.cs"
Task: "Contract test GET /api/v1/kb/types/{typeId} in apps/scanner/tests/contract/test_kb_types_get.cs"
Task: "Integration test full scan workflow in apps/scanner/tests/integration/test_full_scan_workflow.cs"
Task: "Integration test incremental scan workflow in apps/scanner/tests/integration/test_incremental_scan_workflow.cs"
Task: "Integration test MongoDB sampling with PII redaction in apps/scanner/tests/integration/test_mongodb_sampling.cs"
Task: "Integration test knowledge base search functionality in apps/scanner/tests/integration/test_kb_search.cs"

# Launch T012-T018 together (Model Creation):
Task: "CodeType model in apps/scanner/src/Models/CodeType.cs"
Task: "CollectionMapping model in apps/scanner/src/Models/CollectionMapping.cs"
Task: "QueryOperation model in apps/scanner/src/Models/QueryOperation.cs"
Task: "DataRelationship model in apps/scanner/src/Models/DataRelationship.cs"
Task: "ProvenanceRecord model in apps/scanner/src/Models/ProvenanceRecord.cs"
Task: "ObservedSchema model in apps/scanner/src/Models/ObservedSchema.cs"
Task: "KnowledgeBaseEntry model in apps/scanner/src/Models/KnowledgeBaseEntry.cs"

# Launch T019-T025 together (Service Implementation):
Task: "POCOExtractor service in apps/scanner/src/Analyzers/POCOExtractor.cs"
Task: "CollectionResolver service in apps/scanner/src/Resolvers/CollectionResolver.cs"
Task: "OperationExtractor service in apps/scanner/src/Analyzers/OperationExtractor.cs"
Task: "RelationshipInferencer service in apps/scanner/src/Analyzers/RelationshipInferencer.cs"
Task: "MongoSampler service in apps/scanner/src/Samplers/MongoSampler.cs"
Task: "KnowledgeBaseWriter service in apps/scanner/src/KnowledgeBase/KnowledgeBaseWriter.cs"
Task: "IncrementalScanner service in apps/scanner/src/Services/IncrementalScanner.cs"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts
- Follow constitutional requirements: .NET 8, MongoDB.Driver, Roslyn, complete provenance tracking

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
- [x] Constitutional requirements addressed
- [x] Performance targets included
- [x] Security and privacy requirements covered
- [x] Observability and provenance tracking included
