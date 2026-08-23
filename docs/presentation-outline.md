# Presentation Outline

## 1. User Story

As a person tracking work, I want to register, sign in, and manage my own tasks so the first screen gives me a clear list.

## 2. Product Problem and Main Flow

### Problem
Users need a simple, secure way to manage personal tasks with ownership isolation.

### Main Flow
1. Register or use demo account
2. Log in with email/password
3. View task list (simple, clear first screen)
4. Create/edit/complete/delete tasks
5. All data is isolated per user

## 3. Architecture and Design Decisions

### Clean Architecture
- **Domain:** Pure business rules (entities, validation)
- **Application:** Use cases and port interfaces
- **Infrastructure:** EF Core, SQLite, JWT, BCrypt
- **API:** Controllers, DTOs, composition root

### Key Decisions
- JWT bearer authentication with 60-minute expiry
- Ownership isolation via JWT claims (never from request body)
- Missing and foreign tasks both return 404
- Full replacement PUT semantics with property-presence validation
- ProblemDetails error responses (RFC 7807)
- Deterministic seed data with fixed UUIDs

### Frontend
- React 19 + TypeScript + Vite + Tailwind
- Centralized API client with JWT handling
- AuthContext for state management
- Loading/empty/success/validation/error states

## 4. Testing Strategy and Results

### Backend (81 tests)
| Layer | Tests | Framework |
|---|---|---|
| Domain | 10 | xUnit |
| Application | 16 | xUnit + Moq |
| Infrastructure | 34 | xUnit + SQLite |
| Architecture | 4 | xUnit + NetArchTest |
| API | 17 | xUnit + WebApplicationFactory |

### Frontend (53 tests)
| Layer | Tests | Framework |
|---|---|---|
| API Client | 12 | Vitest |
| Auth Context | 7 | Vitest + RTL |
| Login Form | 7 | Vitest + RTL |
| Register Form | 6 | Vitest + RTL |
| Task List | 9 | Vitest + RTL |
| Task Form | 9 | Vitest + RTL |
| Harness | 3 | Vitest + RTL |

**Total: 134 tests, all passing.**

## 5. GenAI Usage and Critical Evaluation

### How GenAI Was Used
- Challenge analysis and requirements extraction
- SDD workflow orchestration
- Specification drafting and refinement
- Code generation for boilerplate and tests
- Architecture and design documentation

### Critical Evaluation
- AI correctly identified ambiguities in the challenge
- AI-generated specifications required human review and correction
- AI-generated code required testing and fixes (name collisions, architecture test adjustment)
- AI was not used for security-critical decisions without human validation
- All AI output was verified through automated tests and manual review

### Key Lesson
AI accelerates implementation but requires human judgment for scope, security, architecture, and quality decisions.

## 6. Key Trade-offs and Lessons Learned

### Trade-offs
- SQLite for simplicity vs. PostgreSQL for production
- JWT in localStorage vs. httpOnly cookies
- Permanent deletion vs. soft deletion
- Simple task list vs. advanced filtering/sorting

### Lessons Learned
- WSL filesystem performance impacts tooling
- Clean Architecture boundaries must be tested, not just documented
- TDD with AI acceleration requires careful test design
- Seed data must be deterministic for reproducible demos
- Architecture tests catch violations early

## 7. Live Demo

1. Register a new account
2. Log in with demo credentials
3. Create a task
4. Edit task status
5. Delete a task
6. Show Swagger/OpenAPI
7. Run test suite
