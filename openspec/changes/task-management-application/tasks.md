# Tasks: Task Management Application

## Review Workload Forecast

| Field | Value |
|---|---|
| Estimated changed lines | 1,300-1,800 authored lines |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | U1; U2; U3; U4; U5; U6; U7; U8 |
| Delivery strategy | auto-chain |
| Chain strategy | pending team decision |

Decision needed before apply: No
Chained PRs recommended: Yes
Chain strategy: pending
400-line budget risk: High
Chain topology decision: required before apply; do not silently select stacked-to-main or feature-branch-chain.

## Work Units

Each unit is RED -> GREEN -> REFACTOR where practical; commands are expected, not executed until tooling exists.

## Task Checklist

- [ ] U1.1 Foundation and toolchain/test-harness setup
- [ ] U2.1 Domain and application use cases
- [ ] U3.1 Persistence, migrations, seed, and reset
- [ ] U4.1 Authentication, API, and health
- [ ] U5.1 Task API and ownership isolation
- [x] U6.1 Frontend authentication
- [x] U7.1 Frontend task management
- [ ] U8.1 Verification, documentation, presentation, and GenAI evidence

| Unit | Scope/outcome; files/areas; dependency; acceptance IDs; focused verification (cwd); rollback; estimate |
|---|---|
| 1.1 Foundation | Depends: none. RED harness admission tests; GREEN create `backend/TaskManagement.sln`, four projects, `frontend/`, package/test configs; REFACTOR conventions. IDs ARCH-REF-001, ARCH-TEST-001. `dotnet test backend/tests/TaskManagement.Architecture.Tests/TaskManagement.Architecture.Tests.csproj` (root); runtime N/A, tools absent. Rollback scaffold; 180 lines. |
| 2.1 Domain/Application | Depends: 1.1. RED `TaskManagement.Domain.Tests`/`Application.Tests`; GREEN Domain/Application entities, ports, DTOs, validators/use cases; REFACTOR boundaries. IDs AUTH-REG-002, TASK-VALID-001/002, TASK-STATUS-001. `dotnet test backend/TaskManagement.sln` (root); runtime N/A. Rollback those projects/tests; 260 lines. |
| 3.1 Persistence/seed | Depends: 1.1-2.1. RED `SeedResetTests` in `TaskManagement.Infrastructure.Tests`; GREEN Infrastructure EF SQLite context/mappings/migrations/hasher, fixed UUID/timestamp seed/reset CLI; REFACTOR ordering. IDs seed-empty/repeat/reset, ARCH-LAYER-001. `dotnet test backend/tests/TaskManagement.Infrastructure.Tests/TaskManagement.Infrastructure.Tests.csproj --filter FullyQualifiedName~SeedResetTests`; `dotnet run --project backend/src/TaskManagement.Infrastructure -- --seed` plus exact documented `dotnet ef` drop/update commands (root), expected only. Rollback Infrastructure/migrations; 220 lines. |
| 4.1 Auth/API/health | Depends: 2.1-3.1. RED `Authentication`/`HealthEndpointTests` in `TaskManagement.Api.Tests`; GREEN `Api/Program.cs` wiring, exact password/JWT/RFC7807 contracts, public auth/Swagger, DB-independent health; REFACTOR. IDs AUTH-REG-001/002, AUTH-AUTH-001, AUTH-PUBLIC-001, AUTH-TOKEN-001/002, HEALTH-001. `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~Authentication` and `~HealthEndpointTests` (root); runtime N/A. Rollback API/auth; 260 lines. |
| 5.1 Task API | Depends: 2.1-4.1. RED `Task` integration suite (WAF/SQLite); GREEN owned CRUD, exact PUT presence/full replacement, 404 indistinguishability, permanent delete; REFACTOR DTO mapping. IDs TASK-CRUD-001/002, TASK-AUTHZ-001, TASK-VALID-001/002, TASK-STATUS-001, TASK-DELETE-001. `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~Task` (root); runtime N/A. Rollback task API/use cases; 220 lines. |
| 6.1 Frontend auth | Depends: 1.1,4.1. RED Vitest/RTL auth suite; GREEN `frontend/api`, `auth`, routing, storage, register/login/logout/expiry/401; REFACTOR React 19/TypeScript rules. IDs UI-AUTH-001/002/003, UI-TOKEN-001. `npm test -- --run` (frontend); browser N/A. Rollback auth areas; 180 lines. |
| 7.1 Frontend tasks | Depends: 5.1-6.1. RED Vitest/RTL task suite; GREEN `frontend/tasks` responsive list/forms/status/delete/404 and loading/empty/success/validation/error states; REFACTOR API errors. IDs UI-STATE-001/002/003, TASK-TEST-FRONTEND-001. `npm test -- --run` (frontend); browser N/A. Rollback tasks/styles; 180 lines. |
| 8.1 Verification/docs | Depends: 1.1-7.1. RED architecture/integration/health/seed-reset admission checks; GREEN non-overlapping suites, `README.md`, architecture docs, presentation, real-only `docs/genai-evidence.md`; REFACTOR reconciliation. IDs AUTH-TEST-BACKEND-001, TASK-TEST-BACKEND-001, ARCH-REF-001, ARCH-LAYER-001, ARCH-TEST-001, DOC-001/002, README-001, ARCH-DOC-001, PRES-001, GENAI-001/002/003/004, GENAI-DOC-001. Run `dotnet test backend/TaskManagement.sln`, architecture test, frontend `npm test -- --run`, `npm run typecheck`, `npm run lint`, `npm run format:check`, `npm run coverage` (root/frontend), expected only; manual review. Rollback docs/tests; 220 lines. |

## TDD/Test Mapping

Keep authentication, task/API, health, seed/reset, frontend, integration, and architecture scenarios in the named suites above without overlap; write each RED test before its production change and record unavailable tooling honestly.
