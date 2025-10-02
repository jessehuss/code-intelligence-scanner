# Implementation Plan: Code Intelligence Scanner & Knowledge Base Seeder

**Branch**: `001-feature-f001-code` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-feature-f001-code/spec.md`

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
Build a .NET 8 console application that performs static analysis of C# code via Roslyn to discover MongoDB usage patterns, optionally samples live MongoDB data with PII redaction, and creates a normalized knowledge base with complete provenance tracking for search and graph queries.

## Technical Context
**Language/Version**: .NET 8, C# 12  
**Primary Dependencies**: Microsoft.CodeAnalysis.* (Roslyn), MongoDB.Driver, Microsoft.Extensions.*  
**Storage**: MongoDB "catalog_kb" database with Atlas Search index  
**Testing**: xUnit, MongoDB test containers for integration tests  
**Target Platform**: Cross-platform console application (Windows, Linux, macOS)  
**Project Type**: single - console application with modular components  
**Performance Goals**: Complete full scans within 10 minutes for development data sets, handle large codebases via streaming/chunked processing  
**Constraints**: Read-only MongoDB credentials, PII redaction mandatory, complete provenance tracking, no runtime reflection of target services  
**Scale/Scope**: 40+ repositories, ≥20 types, ≥10 collections, ≥30 query invocations in MVP

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **Technology Stack Constraints**: Using .NET 8 + C# for backend services, MongoDB.Driver for data access, Roslyn for static analysis - compliant

✅ **Security & Privacy**: Read-only MongoDB credentials for sampling, PII redaction mandatory, only structural information preserved - compliant

✅ **Observability**: Complete provenance tracking for every extracted fact (repo, file, symbol, line span, commit SHA, timestamp) - compliant

✅ **Quality Gates**: Unit tests for resolvers, integration tests against seeded MongoDB - compliant

✅ **Developer Experience**: Deep links to Git, copy-as helpers for Mongo shell and Builders<T> examples - compliant

## Project Structure

### Documentation (this feature)
```
specs/001-feature-f001-code/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
apps/
└── scanner/
    ├── src/
    │   ├── Analyzers/           # Roslyn analyzers for C# code
    │   ├── Samplers/            # MongoDB data sampling
    │   ├── KnowledgeBase/       # Knowledge base operations
    │   ├── Models/              # Data models and entities
    │   ├── Resolvers/           # Collection name and field path resolvers
    │   ├── Services/            # Core business logic
    │   └── Program.cs           # Console application entry point
    └── tests/
        ├── unit/                # Unit tests for resolvers and services
        ├── integration/         # Integration tests with MongoDB
        └── contract/            # Contract tests for APIs

docs/
├── KB-schema.md         # Knowledge base schema documentation
└── PII-redaction.md     # PII redaction policies and procedures

infra/
└── github-actions/
    └── catalog-scan.yml # CI/CD pipeline for scanning
```

**Structure Decision**: Single console application with modular architecture. The scanner is a self-contained tool that can be run locally or in CI/CD pipelines.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - Roslyn static analysis patterns for MongoDB usage
   - MongoDB sampling strategies and PII detection
   - Knowledge base schema design
   - Performance optimization for large codebases

2. **Generate and dispatch research agents**:
   ```
   Task: "Research Roslyn static analysis patterns for MongoDB usage in C# codebases"
   Task: "Find best practices for MongoDB data sampling with PII redaction"
   Task: "Research knowledge base schema design for code intelligence systems"
   Task: "Find performance optimization patterns for large codebase analysis"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Code Type, Collection Mapping, Query Operation, Data Relationship
   - Provenance Record, Observed Schema, Knowledge Base Entry
   - Validation rules from requirements and clarifications

2. **Generate API contracts** from functional requirements:
   - Scanner configuration and execution endpoints
   - Knowledge base query and search endpoints
   - Use standard REST patterns for web API components

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
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

No violations detected - all constitutional requirements are met.

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