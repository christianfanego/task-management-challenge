# Task Management Challenge

A full-stack task-management application built with .NET 8, React 19, SQLite, and JWT authentication.

## User Story

As a person tracking work, I want to register, sign in, and manage my own tasks so the first screen gives me a clear list.

## Architecture

- **Backend:** .NET 8 ASP.NET Core Web API with Clean Architecture (Domain, Application, Infrastructure, API layers)
- **Frontend:** React 19 + TypeScript + Vite + Tailwind CSS + react-datepicker
- **Database:** SQLite via Entity Framework Core
- **Authentication:** JWT bearer tokens with BCrypt password hashing
- **Testing:** xUnit + FluentAssertions + WebApplicationFactory + SQLite (backend), Vitest + React Testing Library + happy-dom (frontend)

## Prerequisites

- .NET 8 SDK (`dotnet --info`)
- Node.js 18+ (`node --version`)
- npm (`npm --version`)
- Git

## Quick Start

```bash
# Clone the repository
git clone <repository-url>
cd task-management-challenge

# Backend
dotnet restore backend/TaskManagement.sln
dotnet run --project backend/src/TaskManagement.Api

# Frontend (in a new terminal)
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173` and log in with `demo@example.com` / `DemoPass123!`

## Backend Setup

```bash
# Restore dependencies
dotnet restore backend/TaskManagement.sln

# Build
dotnet build backend/TaskManagement.sln

# Run tests
dotnet test backend/TaskManagement.sln

# Start the API server
dotnet run --project backend/src/TaskManagement.Api
```

The API starts at `http://localhost:5001`.

## Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Run tests
npm test -- --run

# Type check
npm run typecheck

# Lint
npm run lint

# Format check
npm run format:check
```

The frontend starts at `http://localhost:5173` and proxies `/api` requests to the backend.

## Database

The SQLite database is created and seeded automatically on first startup. No manual migration commands are needed.

Demo credentials are pre-loaded:

| Field | Value |
|---|---|
| Email | `demo@example.com` |
| Password | `DemoPass123!` |

## Swagger/OpenAPI

When running in Development mode, Swagger UI is available at:

```
http://localhost:5001/swagger
```

## Test Commands

### Backend

```bash
# All tests
dotnet test backend/TaskManagement.sln

# Domain tests
dotnet test backend/tests/TaskManagement.Domain.Tests

# Application tests
dotnet test backend/tests/TaskManagement.Application.Tests

# Infrastructure tests
dotnet test backend/tests/TaskManagement.Infrastructure.Tests

# API tests
dotnet test backend/tests/TaskManagement.Api.Tests

# Architecture tests
dotnet test backend/tests/TaskManagement.Architecture.Tests
```

### Frontend

```bash
cd frontend
npm test -- --run
```

**Total: 134 tests (81 backend + 53 frontend)**

## Known Limitations

- Local-only deployment; no production configuration
- No soft deletion; task deletion is permanent
- No task sharing or collaboration
- No real-time updates
- No browser runtime for E2E tests
- Architecture test `Controllers_Should_Not_Directly_Reference_Infrastructure_Types` validates field-level dependencies only; assembly-level references at the composition root are expected
- Date picker uses react-datepicker for consistent English locale; native `<input type="date">` follows OS/browser locale
- Backend auto-creates and seeds SQLite database on startup

## Project Structure

```
task-management-challenge/
├── backend/
│   ├── src/
│   │   ├── TaskManagement.Domain/          # Entities, enums, business rules
│   │   ├── TaskManagement.Application/     # Use cases, ports, DTOs
│   │   ├── TaskManagement.Infrastructure/  # EF Core, SQLite, repositories, JWT, BCrypt
│   │   └── TaskManagement.Api/             # Controllers, Program.cs, Swagger, JSON converters
│   └── tests/
│       ├── TaskManagement.Domain.Tests/
│       ├── TaskManagement.Application.Tests/
│       ├── TaskManagement.Infrastructure.Tests/
│       ├── TaskManagement.Architecture.Tests/
│       └── TaskManagement.Api.Tests/
├── frontend/
│   ├── src/
│   │   ├── api/           # API client, auth, tasks
│   │   ├── context/       # Auth context
│   │   ├── components/    # UI components (TaskList, TaskForm, LoginForm, etc.)
│   │   ├── pages/         # Page components
│   │   ├── __tests__/     # Frontend tests
│   │   └── main.tsx       # Entry point
│   └── package.json
├── docs/
│   ├── architecture.md
│   ├── genai-evidence.md
│   └── presentation-outline.md
└── README.md
```
