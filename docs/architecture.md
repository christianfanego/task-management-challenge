# Architecture

## Clean Architecture

The backend follows Clean Architecture with four layers, each with a single responsibility:

```
┌─────────────────────────────────────┐
│  API Layer (TaskManagement.Api)     │  HTTP controllers, DTOs, Swagger
├─────────────────────────────────────┤
│  Application Layer                  │  Use cases, ports (interfaces), DTOs
│  (TaskManagement.Application)       │
├─────────────────────────────────────┤
│  Infrastructure Layer               │  EF Core, SQLite, repositories, JWT, BCrypt
│  (TaskManagement.Infrastructure)    │
├─────────────────────────────────────┤
│  Domain Layer (TaskManagement.Domain)│  Entities, enums, business rules
└─────────────────────────────────────┘
```

### Dependency Direction

```
API → Application → Domain
API → Infrastructure → Application → Domain
```

- **Domain** references nothing outer. Pure business rules.
- **Application** references only Domain. Defines ports (interfaces) for infrastructure.
- **Infrastructure** references Application and Domain. Implements ports.
- **API** references Application and Infrastructure. Wires dependencies at the composition root (Program.cs). Controllers reference only Application services, never repositories or EF Core.

### Layer Responsibilities

| Layer | Responsibilities |
|---|---|
| **Domain** | `TaskItem`, `User`, `TaskStatus` entities. Business rules (validation, ownership). No external dependencies. |
| **Application** | `ITaskRepository`, `IUserRepository`, `IPasswordHasher`, `IJwtTokenGenerator` ports. `RegisterUserService`, `LoginUserService`, `TaskService` use cases. |
| **Infrastructure** | `AppDbContext`, EF Core mappings, `TaskRepository`, `UserRepository`, `BCryptPasswordHasher`, `JwtTokenGenerator`, `TaskSeeder`. |
| **API** | `AuthController`, `TaskController`, `Program.cs` (composition root), DTOs, Swagger configuration, JWT bearer setup, health endpoint. |

## Frontend Structure

```
frontend/src/
├── api/
│   ├── types.ts          # Shared types and DTOs
│   ├── client.ts         # Fetch wrapper with JWT handling
│   ├── auth.ts           # Register/login API functions
│   └── tasks.ts          # Task CRUD API functions
├── context/
│   └── AuthContext.tsx    # Auth state management
├── components/
│   ├── LoginForm.tsx     # Login form
│   ├── RegisterForm.tsx  # Register form
│   ├── TaskList.tsx      # Task list view
│   ├── TaskItem.tsx      # Single task display
│   ├── TaskForm.tsx      # Create/edit form
│   ├── ProtectedRoute.tsx # Auth guard
│   └── Layout.tsx        # Navigation bar
├── pages/
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   └── TasksPage.tsx
└── App.tsx               # React Router setup
```

### API Integration

- The frontend uses a centralized `apiClient` that attaches the JWT bearer token to all requests.
- On 401 responses, the client clears the token and redirects to login.
- Token and user info are stored in `localStorage`.
- JWT expiry is checked on application mount.

### State Management

- Auth state is managed through React Context (`AuthContext`).
- Task state is managed locally in `TasksPage` with loading/empty/error/success states.
- Form state is managed locally in form components.

## API Contract

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login and receive JWT |
| GET | `/api/health` | No | Health check (no DB dependency) |
| GET | `/api/tasks` | Yes | List authenticated user's tasks |
| POST | `/api/tasks` | Yes | Create task for authenticated user |
| GET | `/api/tasks/{id}` | Yes | Get task by ID (ownership enforced) |
| PUT | `/api/tasks/{id}` | Yes | Update task (ownership enforced) |
| DELETE | `/api/tasks/{id}` | Yes | Delete task (ownership enforced) |

### Error Responses

All errors use RFC 7807 `ProblemDetails`:

```json
{
  "type": "https://example.invalid/problems/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more fields are invalid.",
  "instance": "/api/tasks",
  "errors": { "title": ["Title is required."] }
}
```
