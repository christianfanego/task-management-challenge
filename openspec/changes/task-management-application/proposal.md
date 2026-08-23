# Proposal: Task Management Application

## User Story and Goal
As a person tracking work, I want to register, sign in, and manage my own tasks so the first screen gives me a clear list. The goal is a review-ready demonstration balancing behavior, Clean Architecture, testing, and GenAI evidence.

## Main Flow
Register or use the deterministic demo account, sign in, view the list, create/edit/complete/delete owned tasks, and reset the local database when needed.

## Scope
- React/TypeScript client with auth state and responsive task CRUD UI.
- .NET 8 ASP.NET Core controller API, DTOs, Swagger/OpenAPI, SQLite/EF Core.
- Clean Architecture: Domain, Application, Infrastructure, API.
- JWT bearer authentication, secure password hashing, ownership isolation, migrations, idempotent seed/reset documentation.
- `docs/genai-evidence.md`, containing only real prompts, representative outputs, accepted/rejected suggestions, corrections, validation, tests, edge cases, authentication decisions, and final assessment.

## Non-Goals
Collaboration, sharing, notifications, attachments, recurring tasks, realtime updates, admin roles, remote hosting, automatic status changes, and soft deletion.

## Acceptance Criteria
- Registration/login and seeded demo login work locally without exposing passwords.
- Authenticated users can CRUD their own tasks; the UI starts at a simple task list.
- Title is trimmed and 1-120 characters after trimming; optional description is trimmed to at most 2,000 characters, with blank text becoming null; internal whitespace is preserved. Due dates are optional, ISO dates, and may be past. Statuses are exactly `Pending`, `InProgress`, `Completed`.
- UserId comes only from JWT claims. Missing and foreign-owned tasks both return 404.
- Deletion is permanent; status never changes automatically.
- `GET /api/health` is public, database-independent liveness, and returns exactly `{ "status": "ok" }`.
- Seed/reset is deterministic and idempotent, and README explains setup, credentials, migrations/reset, API, tests, and limitations.
- Mandatory backend authentication, frontend authentication, backend task/API, frontend task, architecture-boundary, and dedicated health checks are mapped to non-overlapping acceptance IDs; README, architecture documentation, presentation, and GenAI evidence remain manual/structural reviews. TDD is used where practical.
- Presentation/code review can trace behavior, boundaries, tests, and GenAI decisions to evidence.

## Constraints and Assumptions
Controllers delegate through application boundaries; Infrastructure owns EF Core/SQLite and migrations; DTOs prevent persistence leakage. Frontend consumes OpenAPI and must not trust client-supplied ownership. JWT development configuration must avoid committed secrets. Assumptions: local-only deployment; regular users only; a stable seeded demo identity and representative tasks suffice. The implementation contract is now explicit, with no unresolved product or API decision; environment and tooling risks remain.

## Capabilities
### New Capabilities
- `authentication`: registration, login, password hashing, JWT issuance, and auth state.
- `task-management`: owned task lifecycle, validation, statuses, due dates, and permanent deletion.
- `demo-data-and-documentation`: deterministic seed/reset and review/setup documentation.
- `genai-evidence`: constrained, reproducible evidence document.

### Modified Capabilities
None.

## Delivery, Risks, and Success
Use foundation-first scaffolding followed by independently verified vertical slices. Auto-chain if work exceeds the 400-line review budget. Rollback deletes the feature branch/change and reverts migrations; preserve the OpenSpec audit trail. Genuine environment/tooling risks are the missing .NET SDK, backend/frontend test runners, frontend TypeScript/lint/format/coverage tooling, and browser runtime. Implementation risks remain JWT, SQLite reset, and ownership mistakes and must be addressed by the specified tests. Expected commands are documented as not executed and must not be reported as run until the required tools are available. Success is a locally runnable, review-ready release with documented setup and verified available tests.
