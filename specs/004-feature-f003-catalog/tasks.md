# Tasks: F003 – Catalog Explorer UI

**Input**: Design documents from `/specs/004-feature-f003-catalog/`
**Prerequisites**: plan.md, research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → Extract: Next.js 14+ (App Router), TypeScript, Tailwind, shadcn/ui, SWR, Monaco Editor, Cytoscape.js
2. Load design documents:
   → data-model.md: 6 entities (Collection, Type, Field, Query, Service, Relationship)
   → contracts/: 4 contract test files + 1 OpenAPI spec
   → research.md: Technology decisions and performance requirements
3. Generate tasks by category:
   → Setup: Next.js project, dependencies, linting
   → Tests: Contract tests, integration tests
   → Core: API client, components, pages
   → Integration: Error handling, performance
   → Polish: Testing, deployment
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Tests before implementation (TDD)
   → Components can be parallel [P]
5. Number tasks sequentially (T001-T030)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Web app**: `apps/catalog-explorer/` following monorepo pattern
- **Components**: `src/components/` for reusable UI components
- **Pages**: `src/app/` for Next.js App Router pages
- **Tests**: `tests/` for all test files

## Phase 3.1: Setup
- [x] T001 Create Next.js project structure in apps/catalog-explorer/
- [x] T002 Initialize Next.js 14+ with TypeScript, Tailwind CSS, and shadcn/ui
- [x] T003 [P] Configure ESLint, Prettier, and TypeScript strict mode
- [x] T004 [P] Set up Vitest, React Testing Library, and Playwright
- [x] T005 [P] Configure environment variables and API client setup

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [x] T006 [P] Contract test search API in tests/contract/test_search_contract.cs
- [x] T007 [P] Contract test collection API in tests/contract/test_collection_contract.cs
- [x] T008 [P] Contract test graph API in tests/contract/test_graph_contract.cs
- [x] T009 [P] Contract test query helper API in tests/contract/test_query_helper_contract.cs
- [x] T010 [P] Integration test global search flow in tests/e2e/test_search_flow.spec.ts
- [x] T011 [P] Integration test collection detail flow in tests/e2e/test_collection_detail.spec.ts
- [x] T012 [P] Integration test type detail flow in tests/e2e/test_type_detail.spec.ts
- [x] T013 [P] Integration test graph visualization flow in tests/e2e/test_graph_flow.spec.ts

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [x] T014 [P] API client with TypeScript types in src/lib/api.ts
- [x] T015 [P] TypeScript type definitions in src/lib/types.ts
- [x] T016 [P] Utility functions in src/lib/utils.ts
- [x] T017 [P] SearchBar component with debouncing in src/components/SearchBar.tsx
- [x] T018 [P] KindResults component for grouped results in src/components/KindResults.tsx
- [x] T019 [P] FacetPanel component for filtering in src/components/FacetPanel.tsx
- [x] T020 [P] SchemaTable component for schema comparison in src/components/SchemaTable.tsx
- [x] T021 [P] DriftBadge component for drift indicators in src/components/DriftBadge.tsx
- [x] T022 [P] CodeSnippet component with Monaco Editor in src/components/CodeSnippet.tsx
- [x] T023 [P] MiniGraph component with Cytoscape in src/components/MiniGraph.tsx
- [x] T024 [P] QueryHelper component for code generation in src/components/QueryHelper.tsx
- [x] T025 [P] Custom hooks for search, SWR, and keyboard navigation in src/hooks/
- [ ] T026 Global search page with routing in src/app/search/page.tsx
- [ ] T027 Collection detail page with tabs in src/app/collections/[name]/page.tsx
- [ ] T028 Type detail page with comprehensive info in src/app/types/[fqcn]/page.tsx
- [ ] T029 Graph visualization page with filters in src/app/graph/page.tsx
- [ ] T030 Root layout with navigation and error boundaries in src/app/layout.tsx

## Phase 3.4: Integration
- [ ] T031 Error boundaries and loading states throughout application
- [ ] T032 Toast notifications for API errors and user feedback
- [ ] T033 Deep linking to Git repositories using provenance data
- [ ] T034 Performance optimization for 300ms search and 400ms page loads
- [ ] T035 Accessibility features and keyboard navigation
- [ ] T036 SWR caching and background revalidation setup

## Phase 3.5: Polish
- [ ] T037 [P] Unit tests for all components in tests/components/
- [ ] T038 [P] Unit tests for custom hooks in tests/hooks/
- [ ] T039 [P] Unit tests for utility functions in tests/lib/
- [ ] T040 Performance testing with Lighthouse CI
- [ ] T041 End-to-end testing with Playwright for all user flows
- [ ] T042 Dockerfile and deployment configuration
- [ ] T043 GitHub Actions workflow for CI/CD
- [ ] T044 Documentation and README updates

## Dependencies
- Tests (T006-T013) before implementation (T014-T030)
- T014 (API client) blocks T017-T030 (components and pages)
- T015 (types) blocks T014, T017-T030
- T016 (utils) blocks T017-T030
- T025 (hooks) blocks T026-T030 (pages)
- Implementation before integration (T031-T036)
- Integration before polish (T037-T044)

## Parallel Execution Examples

### Phase 3.2: Contract Tests (T006-T013) - Can run in parallel
```bash
# Launch all contract tests together:
Task: "Contract test search API in tests/contract/test_search_contract.cs"
Task: "Contract test collection API in tests/contract/test_collection_contract.cs"
Task: "Contract test graph API in tests/contract/test_graph_contract.cs"
Task: "Contract test query helper API in tests/contract/test_query_helper_contract.cs"
Task: "Integration test global search flow in tests/e2e/test_search_flow.spec.ts"
Task: "Integration test collection detail flow in tests/e2e/test_collection_detail.spec.ts"
Task: "Integration test type detail flow in tests/e2e/test_type_detail.spec.ts"
Task: "Integration test graph visualization flow in tests/e2e/test_graph_flow.spec.ts"
```

### Phase 3.3: Core Components (T017-T025) - Can run in parallel
```bash
# Launch all component development together:
Task: "SearchBar component with debouncing in src/components/SearchBar.tsx"
Task: "KindResults component for grouped results in src/components/KindResults.tsx"
Task: "FacetPanel component for filtering in src/components/FacetPanel.tsx"
Task: "SchemaTable component for schema comparison in src/components/SchemaTable.tsx"
Task: "DriftBadge component for drift indicators in src/components/DriftBadge.tsx"
Task: "CodeSnippet component with Monaco Editor in src/components/CodeSnippet.tsx"
Task: "MiniGraph component with Cytoscape in src/components/MiniGraph.tsx"
Task: "QueryHelper component for code generation in src/components/QueryHelper.tsx"
Task: "Custom hooks for search, SWR, and keyboard navigation in src/hooks/"
```

### Phase 3.5: Unit Tests (T037-T039) - Can run in parallel
```bash
# Launch all unit tests together:
Task: "Unit tests for all components in tests/components/"
Task: "Unit tests for custom hooks in tests/hooks/"
Task: "Unit tests for utility functions in tests/lib/"
```

## Task Details

### T001: Create Next.js project structure
- Create `apps/catalog-explorer/` directory
- Initialize Next.js 14+ with App Router
- Set up basic folder structure per plan.md
- Configure TypeScript and Tailwind CSS

### T002: Initialize dependencies
- Install Next.js, TypeScript, Tailwind CSS
- Add shadcn/ui component library
- Install SWR for data fetching
- Add Monaco Editor and Cytoscape.js
- Configure package.json scripts

### T003-T005: Configuration tasks [P]
- ESLint, Prettier, TypeScript strict mode
- Vitest, React Testing Library, Playwright
- Environment variables and API client setup

### T006-T013: Contract and Integration Tests [P]
- Each contract test file from contracts/ directory
- Integration tests for all 5 user flows from quickstart.md
- Tests must fail initially (no implementation yet)

### T014-T016: Foundation (API client, types, utils) [P]
- Typed API client with error handling
- TypeScript definitions for all entities
- Utility functions for common operations

### T017-T025: UI Components [P]
- All 8 main components from plan.md
- Custom hooks for search, data fetching, keyboard nav
- Each component in separate file for parallel development

### T026-T030: Pages and Layout
- 4 main pages: search, collections/[name], types/[fqcn], graph
- Root layout with navigation and error boundaries
- Sequential due to shared dependencies

### T031-T036: Integration
- Error handling, performance, accessibility
- Deep linking, caching, user feedback
- Sequential due to cross-cutting concerns

### T037-T044: Polish
- Comprehensive testing, deployment, documentation
- Unit tests can be parallel [P]
- Performance and deployment tasks sequential

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Follow TDD approach: tests first, then implementation
- All components use shadcn/ui base components
- Performance targets: 300ms search, 400ms page loads
- Support for 10-50 concurrent users, up to 10,000 entities

## Validation Checklist
- [x] All contracts have corresponding tests (4 contract tests)
- [x] All entities have model tasks (6 entities in types.ts)
- [x] All tests come before implementation
- [x] Parallel tasks truly independent
- [x] Each task specifies exact file path
- [x] No task modifies same file as another [P] task
- [x] User flows from quickstart.md covered in integration tests
- [x] Performance requirements from research.md included
- [x] Technology stack from plan.md properly utilized
