
# Implementation Plan: F003 – Catalog Explorer UI

**Branch**: `004-feature-f003-catalog` | **Date**: 2024-12-19 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-feature-f003-catalog/spec.md`

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
A search-first web application for backend developers and SREs to discover Collections, Types, Fields, Queries, and Services in their codebase. Features include instant global search with federated results, detailed entity views with schema comparison and drift indicators, interactive graph visualization, and query helper tools. Built with Next.js 14+ (App Router), TypeScript, Tailwind, and shadcn/ui, integrating with the F002 Catalog API.

## Technical Context
**Language/Version**: TypeScript 5.0+, Next.js 14+ (App Router)  
**Primary Dependencies**: Next.js, TypeScript, Tailwind CSS, shadcn/ui, SWR, Monaco Editor, Cytoscape.js  
**Storage**: N/A (consumes F002 Catalog API)  
**Testing**: Vitest, React Testing Library, Playwright  
**Target Platform**: Web browser (modern browsers)  
**Project Type**: web (frontend application)  
**Performance Goals**: Search results <300ms P50, page renders <400ms P50 after cache warm  
**Constraints**: 10-50 concurrent users, up to 10,000 entities, open access (no auth)  
**Scale/Scope**: 5 main screens, 10-50 concurrent users, up to 10,000 total entities

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Technology Stack Compliance
✅ **PASS**: Using Next.js (TypeScript) for UI components as required by constitution  
✅ **PASS**: Frontend application consuming backend API (F002 Catalog API)  
✅ **PASS**: No deviation from approved stack

### Security & Privacy Compliance
✅ **PASS**: No authentication required (open access) - no sensitive data handling  
✅ **PASS**: Read-only access to catalog data via API  
✅ **PASS**: No data storage in frontend application

### Observability Compliance
✅ **PASS**: Deep links to Git (file + line references) provided via provenance data  
✅ **PASS**: All data sourced from F002 API with provenance tracking

### Quality Gates Compliance
✅ **PASS**: Unit tests planned with Vitest/RTL for components  
✅ **PASS**: Integration tests planned with Playwright for user flows  
✅ **PASS**: Lighthouse budget for performance validation

### Developer Experience Compliance
✅ **PASS**: Deep links to Git provided via provenance data from API  
✅ **PASS**: "Copy as" helpers for Mongo shell and Builders<T> examples  
✅ **PASS**: Clear documentation planned for all components and patterns

**Overall Status**: ✅ **CONSTITUTION COMPLIANT** - No violations detected

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
apps/catalog-explorer/
├── src/
│   ├── app/                    # Next.js App Router pages
│   │   ├── search/            # Global search page
│   │   ├── collections/[name]/ # Collection detail pages
│   │   ├── types/[fqcn]/      # Type detail pages
│   │   ├── graph/             # Graph visualization page
│   │   └── layout.tsx         # Root layout
│   ├── components/            # Reusable UI components
│   │   ├── ui/               # shadcn/ui components
│   │   ├── SearchBar.tsx     # Global search component
│   │   ├── KindResults.tsx   # Search results by kind
│   │   ├── FacetPanel.tsx    # Search facets
│   │   ├── SchemaTable.tsx   # Schema comparison table
│   │   ├── DriftBadge.tsx    # Drift indicator badge
│   │   ├── CodeSnippet.tsx   # Monaco editor wrapper
│   │   ├── MiniGraph.tsx     # Cytoscape mini graph
│   │   └── QueryHelper.tsx   # Query generation helper
│   ├── lib/                  # Utilities and configurations
│   │   ├── api.ts           # F002 API client
│   │   ├── utils.ts         # Common utilities
│   │   └── types.ts         # TypeScript type definitions
│   └── hooks/               # Custom React hooks
│       ├── useSearch.ts     # Search functionality
│       ├── useSWR.ts        # Data fetching
│       └── useKeyboard.ts   # Keyboard navigation
├── public/                   # Static assets
├── tests/                   # Test files
│   ├── components/         # Component tests (Vitest/RTL)
│   ├── e2e/               # End-to-end tests (Playwright)
│   └── __mocks__/         # Test mocks
├── package.json
├── next.config.js
├── tailwind.config.js
├── tsconfig.json
└── Dockerfile
```

**Structure Decision**: Web application structure with Next.js App Router. The frontend application will be located in `apps/catalog-explorer/` following the existing monorepo pattern. Components are organized by functionality with clear separation between UI components, utilities, and custom hooks.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType cursor`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract test → contract test implementation task [P]
- Each component → component creation task [P]
- Each page route → page implementation task
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation
- Dependency order: API client → Components → Pages → Integration
- Mark [P] for parallel execution (independent files/components)

**Task Categories**:
1. **Foundation Tasks** (5-8 tasks): Project setup, API client, types, utilities
2. **Component Tasks** (8-12 tasks): UI components, hooks, shared functionality [P]
3. **Page Tasks** (5-7 tasks): Route pages, layouts, navigation
4. **Integration Tasks** (3-5 tasks): End-to-end tests, performance validation
5. **Polish Tasks** (2-3 tasks): Error handling, accessibility, deployment

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**Key Implementation Focus**:
- Next.js App Router with Server Components
- SWR for data fetching and caching
- shadcn/ui component library integration
- Monaco Editor for code snippets
- Cytoscape.js for graph visualization
- Performance optimization for 300ms search, 400ms page loads
- Accessibility and keyboard navigation
- Error boundaries and graceful failure handling

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
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `/memory/constitution.md`*
