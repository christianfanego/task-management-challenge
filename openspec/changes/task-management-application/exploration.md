## Exploration: task-management-application

### Current State
The repository is a greenfield checkout on `main` with no commits and no application, solution, project, package, or test files. Only `.atl/` and the OpenSpec initialization artifacts exist. `openspec/config.yaml` records the confirmed target: React with TypeScript, .NET 8 ASP.NET Core controller API, SQLite with EF Core, JWT bearer authentication, secure password hashing, Clean Architecture, DTOs, Swagger/OpenAPI, strict TDD, and local-only task management.

Available environment tooling is limited to Node.js `22.22.1`, npm `9.2.0`, Git, curl, and Docker. The `dotnet` CLI is not installed. No frontend manifest or test configuration exists, and OpenSpec reports no test runner, framework, unit/integration/API/data-access/frontend-auth-CRUD/E2E capability, coverage tool, linter, type checker, or formatter. No browser executable was detected for local browser verification.

### Affected Areas
- `backend/` or equivalent solution root — future .NET 8 Clean Architecture solution, projects, dependency boundaries, configuration, migrations, and API host.
- `frontend/` or equivalent client root — future React/TypeScript application, authentication state, task CRUD UI, API client, and frontend tests.
- `tests/` or project-local test projects — future domain, application, data-access, API, integration, and frontend auth/CRUD coverage.
- `openspec/config.yaml` — existing source of SDD constraints; implementation must keep strict TDD and the required 404 ownership behavior.
- Seed/reset documentation and local configuration — future deterministic demo user/task data and idempotent database reset workflow.

### Approaches
1. **Foundation-first scaffold, then vertical slices** — install prerequisites, create the solution/test harness, establish Clean Architecture boundaries and shared contracts, then implement authentication followed by task CRUD and frontend integration in independently tested slices.
   - Pros: Makes the missing toolchain and architecture boundaries explicit early; supports strict TDD; keeps backend and frontend contracts aligned; allows each slice to be verified and chained under the 400-line review budget.
   - Cons: Requires an initial setup slice before visible product behavior; API/frontend contract decisions must be kept synchronized.
   - Effort: Medium

2. **Feature-first prototype, then structural cleanup** — build a minimal end-to-end task flow quickly, then extract layers, security, persistence abstractions, and comprehensive tests.
   - Pros: Produces an early demonstrable path and can expose UX/API assumptions quickly.
   - Cons: Conflicts with the confirmed Clean Architecture and strict TDD goals; risks insecure ownership checks, persistence leakage, and costly restructuring; depends on the same unavailable .NET and test prerequisites.
   - Effort: High

### Recommendation
Use the foundation-first scaffold followed by vertical slices. The later solution-creation phase should begin by installing and verifying the .NET 8 SDK, selecting a supported Node package manager setup, and adding backend/frontend test and quality tooling. Then create separate Domain, Application, Infrastructure, and API projects plus a React/TypeScript client, with tests established before production behavior. Implement JWT login and password hashing first, derive `UserId` only from authenticated claims, and make every task query/update/delete scoped by that identity so missing and unauthorized resources both produce 404. Add SQLite/EF Core migrations and idempotent deterministic seed/reset tooling, then complete the frontend auth and task CRUD flow against the documented OpenAPI contract.

The work should be split into independently verifiable units: environment and scaffold; domain/application task rules; persistence and seed/reset; authentication/API; frontend auth and CRUD; and integration/quality hardening. Forecast review size before apply; if any unit risks exceeding 400 changed lines, use the already selected auto-chain strategy rather than starting oversized work.

### Blockers and Prerequisites
- Install the .NET 8 SDK and confirm `dotnet --info`, restore, build, and test commands.
- Decide and install the frontend test stack during solution creation, at minimum a unit/component runner and browser-capable auth/CRUD test strategy; no such tooling is currently present.
- Add frontend TypeScript compiler, linter, formatter, and package scripts; none are currently configured.
- Add a browser runtime or documented alternative if browser-level verification is required; no browser executable was detected.
- Establish local configuration and secret handling for JWT signing without committing development secrets.

### Risks
- Toolchain installation or network restrictions can delay all implementation and verification.
- JWT signing configuration and password hashing need secure development defaults without making local setup unusable.
- Ownership isolation must be enforced in the application/data-access path, not only by controller route checks, or unauthorized task access may leak through alternate queries.
- SQLite behavior, migrations, and reset semantics can diverge between test and local environments unless the database lifecycle is explicit.
- API DTO validation must preserve bounded title/description rules while allowing past due dates and preventing automatic status transitions.
- A broad full-stack first change can exceed the 400-line review budget; work-unit boundaries and verification are required before apply.

### Ready for Proposal
Yes. Exploration is complete for the greenfield baseline. The next phase can create a proposal without additional product clarification, but implementation must remain paused until the .NET SDK and the selected test/quality toolchain are available and verified.
