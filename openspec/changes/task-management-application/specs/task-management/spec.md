# Task Management Specification

## Purpose

Define the authenticated, permanently deletable lifecycle of tasks owned by one regular user.

## Field, Update, and Error Rules

Titles are required, trimmed, and stored at 1-120 characters. Descriptions are optional on POST, nullable, trimmed when non-null, max 2,000 characters after trim, and blank becomes null; internal whitespace is preserved. Due dates are optional and nullable ISO `YYYY-MM-DD`; past dates are valid. Status is required on PUT, defaults to `Pending` only when omitted on POST, and is exactly `Pending`, `InProgress`, or `Completed`.

`PUT` is full replacement. Property presence is checked independently: `title` and `status` MUST be present and non-null; `description` and `dueDate` MUST be present and MAY be null. Omitted latter properties are 400; explicit null clears them. POST may omit or send null for `description` and `dueDate`. No request accepts `userId`.

## API Contract

| Route | Request JSON | Success response | Errors |
|---|---|---|---|
| `GET /api/tasks` | none | `200 TaskDto[]` | 401 |
| `POST /api/tasks` | `{title:string, description:string|null?, status:string?, dueDate:string|null?}` | `201 TaskDto` | 400, 401 |
| `GET /api/tasks/{id}` | none | `200 TaskDto` | 401, 404 |
| `PUT /api/tasks/{id}` | `{title:string, description:string|null, status:string, dueDate:string|null}` | `200 TaskDto` | 400, 401, 404 |
| `DELETE /api/tasks/{id}` | none | `204` empty body | 401, 404 |

`TaskDto` is `{id:string UUID, title:string, description:string|null, status:string, dueDate:string|null, createdAt:string ISO-8601 UTC, updatedAt:string ISO-8601 UTC}`. Names are exactly camelCase. Errors are `application/problem+json` RFC 7807 with `type,title,status,detail,instance`; 400 adds `errors: {field:[messages]}`; 404 detail is `Task not found.` for absent and foreign IDs; 401 detail is `Authentication required or credentials are invalid.`. Messages never reveal ownership, persistence details, hashes, or token diagnostics.

## Requirements

### Requirement: Manage only owned tasks
The system MUST list, create, retrieve, full-update, and permanently delete tasks for the authenticated user only. UserId MUST come from the JWT.

#### Scenario: List and create owned tasks (TASK-CRUD-001)
- GIVEN an authenticated user
- WHEN the user lists tasks or creates a valid task
- THEN only that user’s tasks are returned and the created task is owned by the JWT identity

#### Scenario: Retrieve, update, and delete an owned task (TASK-CRUD-002)
- GIVEN an authenticated user’s task
- WHEN the user retrieves, updates, or deletes it
- THEN the operation succeeds and deletion removes it permanently

#### Scenario: Foreign task is indistinguishable from missing (TASK-AUTHZ-001)
- GIVEN a foreign or nonexistent identifier
- WHEN the authenticated user retrieves, updates, or deletes it
- THEN the API returns 404 with no ownership disclosure

### Requirement: Enforce task validation and explicit status
The system MUST enforce exact trimming, bounds, date format, status values, PUT property presence, and no automatic status changes.

#### Scenario: Accept valid task values (TASK-VALID-001)
- GIVEN valid title, optional description, allowed status, and nullable ISO date
- WHEN a task is created or fully replaced
- THEN trimmed values and explicit status/date are persisted

#### Scenario: Reject invalid values (TASK-VALID-002)
- GIVEN invalid title, description, date, status, or PUT property presence
- WHEN a task is created or updated
- THEN field-level 400 feedback is returned and no invalid task is persisted

#### Scenario: Preserve explicit status (TASK-STATUS-001)
- GIVEN a task with any allowed status and an elapsed due date
- WHEN time passes or the task is read or updated without a status change
- THEN its status remains unchanged

### Requirement: Frontend renders task lifecycle states
The frontend MUST provide authenticated list and CRUD interactions with loading, empty, success, validation, API error, and 404 states, including explicit permanent-delete intent.

#### Scenario: Render list outcomes (UI-STATE-001)
- GIVEN an authenticated session
- WHEN tasks are loading, absent, successfully returned, or fail to load
- THEN the UI renders distinct loading, empty, success, or error feedback

#### Scenario: Validate and persist task forms (UI-STATE-002)
- GIVEN invalid form data or a valid create/update action
- WHEN the user submits it
- THEN invalid data is rejected visibly and valid changes show progress and server results

#### Scenario: Show task request errors (UI-STATE-003)
- GIVEN a task request returns a generic error or 404
- WHEN the frontend handles the response
- THEN it shows actionable error or not-found feedback without false success

#### Scenario: Delete is permanent (TASK-DELETE-001)
- GIVEN an authenticated user has a task
- WHEN the user confirms deletion and then retrieves that ID
- THEN DELETE returns 204 and retrieval returns 404

### Requirement: Task acceptance tests are mandatory
Backend task/API tests MUST use xUnit, FluentAssertions, WebApplicationFactory, and SQLite. Frontend task tests MUST use Vitest and React Testing Library. Expected commands are not executed until the documented tools exist.

#### Scenario: Verify backend task/API coverage (TASK-TEST-BACKEND-001)
- GIVEN the backend task/API suite and required tools
- WHEN its expected command runs
- THEN it covers CRUD, validation, ownership, status, and permanent deletion

#### Scenario: Verify frontend task coverage (TASK-TEST-FRONTEND-001)
- GIVEN the frontend task suite and required tools
- WHEN its expected command runs
- THEN it covers list, form, loading, empty, success, validation, error, and 404 states

## Acceptance Test Mapping

| Scenario ID | Intended project/suite | Working directory | Framework/tool | Expected command (not executed) |
|---|---|---|---|---|
| TASK-CRUD-001, TASK-CRUD-002, TASK-AUTHZ-001, TASK-VALID-001, TASK-VALID-002, TASK-STATUS-001, TASK-DELETE-001, TASK-TEST-BACKEND-001 | `backend/tests/TaskManagement.Api.Tests` | repository root | xUnit, FluentAssertions, WebApplicationFactory, SQLite | `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~Task` |
| UI-STATE-001, UI-STATE-002, UI-STATE-003, TASK-TEST-FRONTEND-001 | `frontend/` task suite | `frontend/` | Vitest, React Testing Library | `npm test -- --run` |
