# GenAI Evidence

## Tool and Model

- **Tool:** OpenCode with Claude Code agent
- **Session:** task-management-challenge SDD workflow
- **Date:** August 22, 2026

## Prompts and AI-Generated Output

### 1. Challenge Analysis

**Prompt:** Analyze the attached technical interview challenge PDF and extract requirements, ambiguities, and preparation needs.

**AI Output:** Structured analysis of mandatory requirements, preferred practices, deliverables, ambiguities, and SDD kickoff brief.

**Decision:** Accepted. The analysis correctly identified all requirements and ambiguities.

### 2. SDD Initialization Prompt

**Prompt:** Generate a concise English SDD initialization prompt based on the challenge analysis.

**AI Output:** Complete SDD initialization prompt with user story, scope, non-goals, acceptance criteria, backend/frontend/auth/testing/seed/docs/presentation/GenAI evidence requirements.

**Decision:** Accepted with modifications. The prompt was refined through multiple iterations to resolve ambiguities.

### 3. Specification Remediation

**Prompt:** Perform final focused specification correction to resolve implementation-affecting ambiguities.

**AI Output:** Updated specifications with exact field limits, JWT claims, API routes, DTOs, error shapes, test mappings, seed data, and architecture boundaries.

**Decision:** Accepted. The corrected specifications resolved all implementation-affecting ambiguities.

### 4. Backend Implementation

**Prompt:** Implement backend solution and project skeleton, domain entities, application ports, EF Core repositories, and authentication.

**AI Output:** Complete backend with Domain/Application/Infrastructure/API layers, 81 passing tests.

**Decision:** Accepted. The implementation correctly follows Clean Architecture and passes all tests.

### 5. Frontend Implementation

**Prompt:** Implement frontend authentication and task CRUD UI with React 19, TypeScript, and Tailwind CSS.

**AI Output:** Complete frontend with login/register, task CRUD, API client, JWT management, 53 passing tests.

**Decision:** Accepted. The implementation correctly handles all states and passes all quality checks.

### 6. Meta-Prompt for Challenge Analysis

**Prompt:** "Analyze the attached technical interview challenge PDF and extract requirements, ambiguities, and preparation needs..."

**AI Output:** Complete requirements matrix, ambiguities, assumptions, and a refined SDD initialization prompt.

**Decision:** Accepted with manual refinement. The prompt was iterated multiple times to resolve ambiguities before SDD initialization.

### 7. Frontend Bug Fixes

**Prompt:** "Fix the following frontend issues: date format, overdue detection, delete confirmation, edit form scroll..."

**AI Output:** Multiple iterations of fixes, including formatDate, isOverdue, scrollIntoView, react-datepicker integration.

**Decision:** Accepted with corrections. Several fixes initially targeted the wrong file (TaskItem.tsx instead of TaskList.tsx) and required re-application.

## Corrections and Improvements

### 1. TaskStatus Name Collision

**Issue:** `TaskStatus` enum collided with `System.Threading.Tasks.TaskStatus` in test files.

**AI Suggestion:** Use fully-qualified `using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;`

**Decision:** Accepted. Resolved the ambiguity without changing the domain model.

### 2. Architecture Test Over-Restriction

**Issue:** `Api_Should_Reference_Infrastructure_CompositionRoot_Only` failed because the Api project references Infrastructure for DI registration.

**AI Suggestion:** Change to `Controllers_Should_Not_Directly_Reference_Infrastructure_Types` to check field-level dependencies only.

**Decision:** Accepted. The composition root is expected to reference Infrastructure; the test should validate controller independence, not assembly references.

### 3. WSL Filesystem Issue

**Issue:** Vitest hangs on WSL-mounted Windows filesystem (`/mnt/c/`).

**AI Suggestion:** Move the project to native Linux filesystem (`/home/chris/`).

**Decision:** Accepted. Moving to native Linux resolved the hang completely.

### 4. BCrypt Hash Invalid in Seed Data

**Issue:** The TaskSeeder used a hardcoded string "DEMO-PASSWORD-HASH" as the password hash, which is not a valid BCrypt hash. Login failed with SaltParseException.

**AI Suggestion:** Generate the hash at runtime using BCrypt.Net.BCrypt.HashPassword("DemoPass123!") instead of hardcoding it.

**Decision:** Accepted. The seed now generates a valid BCrypt hash on every startup.

### 5. Database Not Created on Startup

**Issue:** The application started without creating the SQLite database or tables. All API calls failed with "no such table: Users".

**AI Suggestion:** Add context.Database.EnsureCreated() and TaskSeeder.Seed(context) to Program.cs during startup.

**Decision:** Accepted. The database is now auto-created and seeded on first run.

### 6. Tailwind CSS Not Installed

**Issue:** Frontend components used Tailwind CSS classes (e.g., "bg-red-50", "text-gray-500") but the tailwindcss package was not installed. The UI rendered as plain unstyled HTML.

**AI Suggestion:** Install tailwindcss and @tailwindcss/vite, configure Vite plugin, create index.css with @import "tailwindcss".

**Decision:** Accepted. The UI now renders with proper styling.

### 7. Vite Proxy Port Mismatch

**Issue:** The Vite dev server proxy was configured to forward /api requests to port 5000, but the backend ran on port 5001. All frontend API calls failed.

**AI Suggestion:** Change the proxy target from "http://localhost:5000" to "http://localhost:5001" in vite.config.ts.

**Decision:** Accepted. Frontend now correctly proxies to the backend.

### 8. Date Timezone Shift Bug

**Issue:** When users selected a date (e.g., Jan 2), the task list showed it as one day later (Jan 3). The API returned dates without timezone ("2026-01-02T00:00:00"), and System.Text.Json deserialized them as local time (Kind=Unspecified), causing a day shift.

**AI Suggestion:** Configure JSON serializer with DateTimeZoneHandling.Utc in Program.cs to treat all unspecified DateTimes as UTC.

**Decision:** Identified but pending implementation. The root cause was confirmed through API response analysis.

### 9. TaskItem.tsx Was Never Rendered

**Issue:** The AI modified TaskItem.tsx with formatDate, isOverdue, and overdue styling, but the actual component rendering tasks was TaskList.tsx. TaskItem.tsx was never imported or used.

**AI Suggestion:** Move all formatting and overdue logic from TaskItem.tsx to TaskList.tsx. Delete TaskItem.tsx.

**Decision:** Accepted. All visual improvements now work correctly in the rendered component.

### 10. Native Date Picker Locale

**Issue:** The <input type="date"> displayed month names in the OS/browser locale (Spanish), not English. The lang="en" attribute did not override the native date picker.

**AI Suggestion:** Replace native date input with react-datepicker, a styled calendar component that always displays in English.

**Decision:** Accepted. The date picker now shows English month names consistently.

## Edge Cases and Validation Decisions

### 1. Task Validation
- Title: required, trimmed, 1-120 characters
- Description: optional, trimmed, max 2,000 characters, blank becomes null
- Status: exactly Pending, InProgress, Completed
- Due dates: optional, past dates allowed, no automatic status changes

### 2. Authentication
- Email: required, trimmed, lowercase-normalized, max 254 characters
- Password: 12-128 characters, uppercase/lowercase/digit/non-alphanumeric, never silently trimmed
- JWT: 60-minute expiry, required claims sub/email/iat/exp
- Invalid credentials: generic 401 response (no disclosure)

### 3. Ownership Isolation
- UserId extracted from JWT claims only, never from request body
- Missing and foreign-owned tasks both return 404 (no disclosure)

### 4. Seed Data
- Fixed UUIDs and UTC timestamps for deterministic, idempotent seeding
- One demo user and three tasks
- Reset drops/recreates schema and reseeds

## Final Assessment

The GenAI tool was used as a planning and implementation accelerator, not as a replacement for human judgment. Key human decisions included:

- Scope and feature prioritization
- Architecture boundary validation
- Security decisions (JWT expiry, ownership isolation, 404 concealment)
- Test strategy and coverage targets
- Documentation structure and content
- Quality gate enforcement

All AI-generated code was reviewed, tested, and corrected where necessary. The final implementation passes 134 tests (81 backend + 53 frontend) and all quality checks.
