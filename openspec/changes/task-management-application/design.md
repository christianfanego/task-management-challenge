# Design: Task Management Application

## Technical Approach

Use foundation-first vertical slices for the .NET 8 controller API and React/TypeScript client. Preserve no `displayName`, permanent deletion, local scope, no collaboration.

## Architecture Decisions

| Decision | Choice | Rationale / rejected alternative |
|---|---|---|
| Boundaries | `backend/TaskManagement.sln` contains Domain, Application, Infrastructure, and Api. Domain has no outer references; Application references Domain only; Infrastructure references Application/Domain; Api uses Infrastructure only for composition-root registration. Controllers reference Application contracts/use cases/DTOs, never Infrastructure repositories or EF Core. | Prevents persistence/HTTP leakage; the composition root is the sole API-to-Infrastructure access. |
| Composition | `Api/Program.cs` registers ports, Infrastructure, auth, EF Core, ProblemDetails, Swagger, and controllers. | Wiring stays at the edge. |
| Ownership/security | JWT `sub` is the only task `userId`; repository queries include it. | Prevents ownership leaks through alternate queries; client `userId` is rejected. |
| Persistence | Infrastructure owns `TaskManagementDbContext`, SQLite, migrations, hashing, JWT, and seed/reset commands. | SQLite behavior is contractual, so API tests use SQLite rather than an in-memory substitute. |

## Domain and Application

Domain owns `User`, `TaskItem`, value objects, and `TaskStatus` (`Pending`, `InProgress`, `Completed`). Factories enforce lowercase email, title trim/1-120, description trim/max 2,000 with blank-to-null, ISO date/null, and explicit status. Entities own immutable IDs and UTC timestamps; due dates never change status.

Application owns use cases (`RegisterUser`, `Login`, task CRUD), validation, DTOs, and ports (`IUserRepository`, `ITaskRepository`, `IPasswordHasher`, `ITokenService`, `ICurrentUser`, `IUnitOfWork`, `IClock`). Auth requires non-null fields; email is trimmed, lowercase-normalized, standard-format, max 254. Password is validated exactly as submitted, never silently trimmed, at 12-128 chars with uppercase/lowercase/digit/non-alphanumeric. Duplicate emails, including case variants, map 409; missing-email and wrong-password login share generic 401. PUT requires non-null title/status and present description/dueDate, possibly null; omission is 400. POST alone defaults omitted status to `Pending`. Results map to 400/409/401 and identical missing/foreign 404; entities/exceptions never cross the boundary.

## API and Auth Contracts

Controllers expose `/api/auth/register`, `/api/auth/login`, `/api/health`, `/api/tasks`, and task `{id}` GET/PUT/DELETE. JSON is camelCase; DTOs are `UserDto`, `LoginResponseDto`, `TaskDto`; timestamps are UTC round-trip `Z`. RFC 7807 `application/problem+json` has stable `type/title/status/detail/instance`; 400 adds `errors: {field: string[]}`. Details: 401 `Authentication required or credentials are invalid.`, 409 `The email is already registered.`, task 404 `Task not found.`. Statuses: register 201, login 200, create 201, replace 200, delete 204, health 200 exactly `{ "status": "ok" }`. Auth/register/login/OpenAPI are public; tasks use bearer. Health resolves no auth, database, provider, or readiness service.

Infrastructure issues JWTs for exactly 60 minutes with `sub`, `email`, `iat`, `exp`, and `expiresAt` equal to `exp` in ISO UTC. Validate signature, issuer/audience/configured lifetime; hash passwords with the platform secure password hasher and never log or return hashes. Missing, malformed, invalid-signature, and expired tokens share the generic 401 contract.

## Data and Frontend

EF maps UUIDs, unique normalized email, owner FK/index, status string, nullable fields, UTC converters, and deterministic owner-plus-createdAt/id ordering. SQLite tests use isolated DBs. `backend/tests/TaskManagement.Infrastructure.Tests/SeedResetTests` is the xUnit/FluentAssertions/SQLite data-access suite for the demo-data specification. Run `dotnet test backend/tests/TaskManagement.Infrastructure.Tests/TaskManagement.Infrastructure.Tests.csproj --filter FullyQualifiedName~SeedResetTests`. It asserts empty-DB one-user/three-task counts, fixed UUIDs/timestamps/values/ownership/listed order; repeat-seed no duplicates or changes; and drop/recreate/update/reseed observable counts and exact values. Reset: `dotnet ef database drop --project backend/src/TaskManagement.Infrastructure --startup-project backend/src/TaskManagement.Infrastructure --force`, `dotnet ef database update --project backend/src/TaskManagement.Infrastructure --startup-project backend/src/TaskManagement.Infrastructure`, then `dotnet run --project backend/src/TaskManagement.Infrastructure -- --seed`.

`frontend/` separates `api/` (typed client, ProblemDetails parser, 401 logout), `auth/`, `tasks/`, shared DTO/types, routing, and styles. Session storage holds bearer/expiry; logout, expiry, or 401 clears it and returns to auth. React 19 uses named imports/no manual memoization; TypeScript uses const status objects, flat interfaces, `unknown`, no `any`. Responsive UI covers loading, empty, success, validation, API error, and task 404; delete requires confirmation and never discloses ownership.

## Verification and Work Units

Strict TDD is RED/GREEN/REFACTOR where practical. Blockers are .NET SDK, xUnit/FluentAssertions/WebApplicationFactory/SQLite, Vitest/RTL, frontend quality tools, and browser runtime; unavailable commands are never claimed. Expected root commands: `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~Authentication`, `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~Task`, `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~HealthEndpointTests`, `dotnet test backend/tests/TaskManagement.Architecture.Tests/TaskManagement.Architecture.Tests.csproj`, `dotnet test backend/TaskManagement.sln`. From `frontend/`: `npm test -- --run`, `npm run typecheck`, `npm run lint`, `npm run format:check`, `npm run coverage`. Mappings remain non-overlapping; docs/presentation/GenAI are manual reviews.

Work units are scaffold, domain/application, persistence/seed, auth/API, frontend auth, frontend tasks, and integration/docs. Each includes tests, receipt, runtime/unavailable result, and rollback boundary. Auto-chain at 400 lines; slices are independently reviewable/reversible, never file-type splits.

## Documentation and Evidence

Create `README.md`, architecture docs, presentation, and only-real-evidence `docs/genai-evidence.md`. README covers prerequisites, credentials, migrations/reset, API/Swagger, expected commands, limitations, local scope, and unavailable tooling. Other docs trace boundaries, security, behavior, tests, GenAI, critical evaluation, prompts/outputs, decisions, corrections, validation, and blocked verification.

## Threat Matrix

N/A: no shell/subprocess, VCS/PR automation, executable classification, or process-integration boundary; HTTP routes and operational reset docs are not process integration.

## Migration / Rollout

No production rollout. Local SQLite migrations and destructive reset are documented and reversible at work-unit/branch boundaries. No open questions; confirmed.
