# Demo Data and Documentation Specification

## Purpose

Define reproducible local demo data, reset behavior, setup documentation, and review readiness.

## Fixed Seed Data and Commands

The demo user is UUID `00000000-0000-0000-0000-000000000001`, email `demo@example.com`, password `DemoPass123!`, and fixed `createdAt`/`updatedAt` `2026-01-01T08:00:00Z`. Tasks are inserted in listed order, all owned by that UUID:

| UUID | title | description | status | dueDate | createdAt / updatedAt |
|---|---|---|---|---|---|
| `00000000-0000-0000-0000-000000000101` | Prepare weekly review | Summarize completed work | Pending | 2026-01-15 | `2026-01-01T09:00:00Z` / same |
| `00000000-0000-0000-0000-000000000102` | Ship task API | Verify ownership and validation | InProgress | 2026-02-01 | `2026-01-02T09:00:00Z` / same |
| `00000000-0000-0000-0000-000000000103` | Archive January notes | null | Completed | null | `2026-01-03T09:00:00Z` / same |

Expected, not executed, root-relative commands are `dotnet run --project backend/src/TaskManagement.Infrastructure -- --seed`, then reset with `dotnet ef database drop --project backend/src/TaskManagement.Infrastructure --startup-project backend/src/TaskManagement.Infrastructure --force`, `dotnet ef database update --project backend/src/TaskManagement.Infrastructure --startup-project backend/src/TaskManagement.Infrastructure`, and the seed command. Reset drops/recreates schema, seeds, and exposes counts of one user/three tasks with fixed values.

Expected, not executed, verification commands are `dotnet test backend/TaskManagement.sln` from repository root; from `frontend/`, `npm test -- --run`, `npm run typecheck`, `npm run lint`, `npm run format:check`, and `npm run coverage`. They remain unexecuted because the .NET SDK, backend/frontend test runners, frontend TypeScript/lint/format/coverage tooling, and browser runtime are unavailable.

## Requirements

### Requirement: Seed deterministic demo data idempotently
The system MUST provide exactly one stable demo user and three documented tasks. Repeated seeding MUST neither duplicate nor alter records.

#### Scenario: Seed an empty local database
- GIVEN an empty local database
- WHEN the expected seed command runs
- THEN one demo user and exactly three documented tasks exist with fixed ownership and values

#### Scenario: Repeat seeding
- GIVEN the seeded database
- WHEN the seed operation runs again
- THEN counts and all values remain unchanged

### Requirement: Reset is documented and deterministic
The project MUST document prerequisites, migrations, intentional demo credential, reset, API/Swagger, expected tests, limitations, and local-only scope.

#### Scenario: Reset and reseed
- GIVEN changed or deleted local task data
- WHEN documented reset and seed steps run
- THEN records return to exactly one user and three deterministic tasks

#### Scenario: Missing prerequisites are explicit (DOC-001)
- GIVEN required SDKs, runners, quality tools, or browser runtime are unavailable
- WHEN a reviewer follows the documentation
- THEN each unavailable capability and consequence is identified without claiming a pass

### Requirement: Support presentation and code review
Documentation MUST make behavior, Clean Architecture, DTO/security decisions, tests, ownership, persistence, limitations, and evidence traceable.

#### Scenario: Reviewer traces acceptance behavior (DOC-002)
- GIVEN README, OpenAPI, tests, and project structure
- WHEN a reviewer inspects the application
- THEN requested flows and boundaries trace to observable evidence

#### Scenario: Verify README content (README-001)
- GIVEN the repository-root README
- WHEN a reviewer performs a manual/structural review
- THEN it contains setup, credentials, migrations/reset, API/Swagger, expected commands, limitations, local-only scope, and unavailable-tool risks without claiming execution

#### Scenario: Verify architecture documentation (ARCH-DOC-001)
- GIVEN repository-root architecture documentation
- WHEN a reviewer performs a manual/structural review
- THEN it identifies layer responsibilities, dependency direction, DTO/ownership boundaries, security decisions, and architecture checks

#### Scenario: Verify presentation coverage (PRES-001)
- GIVEN repository-root presentation materials
- WHEN a reviewer performs a manual/structural review
- THEN they find the informal user story, product flow, design decisions, technical architecture, demonstrated functionality, testing strategy, GenAI usage, and critical evaluation

### Requirement: Establish meaningful verification with TDD
The project MUST use strict TDD where practical and provide the specified backend, frontend, data-access, and architecture checks. Expected commands are not verification claims.

#### Scenario: Verify seed and data-access behavior
- GIVEN the infrastructure integration suite and required tools
- WHEN its expected command runs
- THEN it verifies deterministic seed, idempotence, reset, and fixed values

#### Scenario: Verify architecture references (ARCH-REF-001)
- GIVEN the architecture test project
- WHEN its expected command runs
- THEN forbidden project references and dependency direction are verified

#### Scenario: Verify architecture responsibilities (ARCH-LAYER-001)
- GIVEN the architecture test project
- WHEN its expected command runs
- THEN controller, application, and infrastructure responsibilities are verified with NetArchTest.Rules

#### Scenario: Verify architecture test admission (ARCH-TEST-001)
- GIVEN the architecture test project and repository structure
- WHEN the focused architecture command runs
- THEN the required boundary tests are discoverable and reportable

#### Scenario: Test-first vertical slice
- GIVEN a planned behavior slice
- WHEN implementation proceeds
- THEN a failing test is established first where practical, followed by passing implementation and refactoring

## Acceptance Test Mapping

| Scenario ID | Intended project/suite | Working directory | Framework/tool | Expected command or review status |
|---|---|---|---|---|
| DOC-001 | Repository documentation review | repository root | Manual/structural review | Manual/structural review, not an automated test; inspect prerequisites, unavailable tools, and claims |
| DOC-002 | Repository documentation review | repository root | Manual/structural review | Manual/structural review, not an automated test; inspect traceability and limitations |
| README-001 | Repository root README | repository root | Manual/structural review | Manual/structural review, not an automated test; inspect the exact scenario checklist |
| ARCH-DOC-001 | Repository architecture documentation | repository root | Manual/structural review | Manual/structural review, not an automated test; inspect the exact scenario checklist |
| PRES-001 | Repository presentation materials | repository root | Manual/structural review | Manual/structural review, not an automated test; inspect the exact scenario checklist |
| ARCH-REF-001, ARCH-LAYER-001, ARCH-TEST-001 | `backend/tests/TaskManagement.Architecture.Tests` | repository root | NetArchTest.Rules and project-reference checks | Expected, not executed: `dotnet test backend/tests/TaskManagement.Architecture.Tests/TaskManagement.Architecture.Tests.csproj` |

## Clean Architecture and Environment Requirements

Domain references no outer project. Application may reference Domain, not Infrastructure/API, and owns use cases/ports. Infrastructure may reference Application/Domain and owns EF Core, SQLite, migrations, hashing, and JWT implementation. API may reference Infrastructure only in its composition root; controllers handle HTTP binding, authorization, delegation, and DTO mapping, never repositories or EF Core.

The environment lacks the .NET SDK, backend/frontend test runners, frontend TypeScript/lint/format/coverage tooling, and browser runtime. All expected commands above are clearly not executed. No specification check claims a command has run.
