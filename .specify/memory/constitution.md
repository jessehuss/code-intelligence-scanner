<!--
Sync Impact Report:
- Version change: 2.1.1 → 1.0.0 (new constitution for cataloger project)
- Modified principles: All principles replaced with cataloger-specific ones
- Added sections: Security & Privacy, Observability, Quality Gates, Developer Experience, Schedules
- Removed sections: None (template structure preserved)
- Templates requiring updates: ✅ plan-template.md, ✅ spec-template.md, ✅ tasks-template.md
- Follow-up TODOs: None
-->

# Cataloger Constitution

## Core Principles

### I. Technology Stack Constraints
MUST use .NET 8 + C# for backend services; MongoDB.Driver for all data access operations; Roslyn for static analysis capabilities; Next.js (TypeScript) for UI components; ASP.NET Core Minimal APIs for backend endpoints. No deviations from this stack without explicit constitutional amendment.

### II. Security & Privacy (NON-NEGOTIABLE)
MUST use read-only MongoDB credentials for sampling operations; MUST redact PII in any stored examples; MUST allow only shape/type/length/format in samples - no actual data content. All data access patterns must be auditable and reversible.

### III. Observability (NON-NEGOTIABLE)
MUST log provenance for every extracted fact: repository, file path, symbol name, commit SHA, and timestamp. All logging must be structured and searchable. No fact extraction without complete provenance tracking.

### IV. Quality Gates
MUST have unit tests for resolvers (collection name inference, filter extraction); MUST have integration tests against a seeded MongoDB instance; MUST validate all data access patterns before production deployment.

### V. Developer Experience
MUST provide deep links to Git (file + line references); MUST provide "copy as" helpers for Mongo shell and Builders<T> examples; MUST maintain clear documentation for all data access patterns and resolvers.

## Security & Privacy Requirements

All data sampling operations MUST use read-only credentials. PII redaction is mandatory for any stored examples. Only structural information (shape, type, length, format) may be preserved in samples. All data access must be auditable with complete provenance tracking.

## Development Workflow

### Schedules
MUST perform incremental scan on every push; MUST perform nightly full scan; MUST perform weekly integrity checks. All scans must be automated and report results to the development team.

### Code Review Process
All PRs MUST verify compliance with security constraints, observability requirements, and quality gates. Complexity must be justified against the core principles. Use constitution.md for runtime development guidance.

## Governance

This constitution supersedes all other development practices. Amendments require documentation, approval from the development team, and a migration plan for existing code. All development work must comply with these principles.

**Version**: 1.0.0 | **Ratified**: 2025-01-27 | **Last Amended**: 2025-01-27