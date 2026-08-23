# Authentication Specification

## Purpose

Define local registration, login, public access, and authenticated access for regular users.

## Field and Token Rules

- `email` is required, trimmed, lowercase-normalized, standard email format, and at most 254 characters. `password` is required, never silently trimmed, validated exactly as submitted, 12-128 characters, and must contain uppercase, lowercase, digit, and non-alphanumeric characters.
- Duplicate email, including case variants, returns 409. Login for a missing email or wrong password always returns the same generic 401 response.
- JWT bearer tokens expire exactly 60 minutes after issuance and contain `sub`, `email`, `iat`, and `exp`. `expiresAt` is an ISO-8601 UTC string (`DateTimeOffset` round-trip format, ending `Z`) equal to `exp`.

## API Contract

| Route | Request JSON | Success response | Errors |
|---|---|---|---|
| `POST /api/auth/register` | `{email:string, password:string}`; both required, non-null | `201 {id:string UUID, email:string}` | 400, 409 |
| `POST /api/auth/login` | `{email:string, password:string}`; both required, non-null | `200 {accessToken:string, tokenType:"Bearer", expiresAt:string ISO-8601 UTC, user:{id:string UUID,email:string}}` | 400, 401 |
| `GET /api/health` | none | `200 {status:"ok"}` | none |

JSON names are exactly camelCase. Responses never contain passwords, hashes, entities, or client-controlled ownership. All errors are `application/problem+json` RFC 7807 with stable `type`, request-path `instance`, and no secrets or token diagnostics. 400 additionally has `errors: {field:[messages]}`; 401 detail is `Authentication required or credentials are invalid.`; 409 detail is `The email is already registered.`. The 400, 401, and 409 response shapes use the exact `type`, `title`, `status`, `detail`, `instance`, and field-error contracts for their routes.

## Requirements

### Requirement: Register and authenticate users

The system MUST validate registration and login using the fields above, securely hash passwords, issue a JWT on successful login, and return DTOs only. Registration returns 201 with `id` and `email`; login returns 200 with the access token contract. Invalid input returns 400, invalid credentials return generic 401, and duplicate email returns 409.

#### Scenario: Register then log in (AUTH-REG-001)
- GIVEN a new registration with a valid email and password
- WHEN the client registers and logs in with the same credentials
- THEN registration returns 201, login returns a 60-minute bearer token, and neither response contains password data

#### Scenario: Reject duplicate or invalid registration (AUTH-REG-002)
- GIVEN an existing email in any letter case or invalid registration fields
- WHEN registration is submitted
- THEN the API returns 409 or 400 respectively and does not reveal password data

#### Scenario: Invalid credentials and expired token (AUTH-AUTH-001)
- GIVEN wrong login credentials or a token past its `exp` claim
- WHEN the client logs in or calls a protected route
- THEN login or the protected route returns generic 401 with no sensitive data

### Requirement: Public endpoints remain public

Registration, login, and OpenAPI MUST be public; protected task endpoints MUST require a valid bearer token. Health is specified and mapped separately.

#### Scenario: Access authentication or documentation endpoints (AUTH-PUBLIC-001)
- GIVEN no bearer token
- WHEN the client calls registration, login, or OpenAPI
- THEN the endpoint responds according to its contract rather than with an authentication challenge

#### Scenario: Call a protected endpoint anonymously (AUTH-TOKEN-001)
- GIVEN no bearer token, an invalid token, or an expired token
- WHEN the client calls a protected task endpoint
- THEN the API returns 401 and does not disclose task data

#### Scenario: Health is database-independent (HEALTH-001)
- GIVEN no bearer token and no registered SQLite provider, DbContext, or database connection in the test host
- WHEN `GET /api/health` is called
- THEN it returns HTTP 200 and exactly `{ "status": "ok" }`
- AND the test proves SQLite/database readiness is not accessed or required

### Requirement: Frontend authentication state is observable

The frontend MUST provide registration, login, logout, token-expiry, loading, success, and actionable error states. A 401 clears auth state and returns the user to authentication without exposing response internals.

#### Scenario: Login controls task access (UI-AUTH-001)
- GIVEN valid credentials
- WHEN the user submits login
- THEN the UI shows progress, stores usable auth state, and can load the user’s task list

#### Scenario: Authentication failure is actionable (UI-AUTH-002)
- GIVEN rejected credentials
- WHEN authentication fails
- THEN the UI shows a clear generic error without exposing secrets

#### Scenario: Expired authentication returns to login (UI-TOKEN-001)
- GIVEN an expired session or protected request returning 401
- WHEN the frontend handles the response
- THEN it clears auth state and offers a path to authenticate again

#### Scenario: Logout removes access (UI-AUTH-003)
- GIVEN an authenticated frontend session
- WHEN the user logs out
- THEN local auth state is cleared and the next protected request omits the former token

### Requirement: Token rejection is non-disclosing

The API MUST return the exact generic RFC 7807 401 shape for missing, malformed, invalid-signature, and expired bearer tokens.

#### Scenario: Reject invalid bearer tokens (AUTH-TOKEN-002)
- GIVEN a missing, malformed, invalid-signature, or expired bearer token
- WHEN a protected route is called
- THEN it returns the generic 401 ProblemDetails shape and no task data

### Requirement: Authentication tests are mandatory

Backend authentication tests MUST use xUnit, FluentAssertions, WebApplicationFactory, and SQLite. Frontend authentication tests MUST use Vitest and React Testing Library. Expected commands are not executed until the documented tools exist.

#### Scenario: Verify backend authentication coverage (AUTH-TEST-BACKEND-001)
- GIVEN the backend authentication suite and required tools
- WHEN its expected command runs
- THEN it covers registration, login, password/email rules, JWT claims/expiry, and token rejection

#### Scenario: Verify frontend authentication coverage (AUTH-TEST-FRONTEND-001)
- GIVEN the frontend authentication suite and required tools
- WHEN its expected command runs
- THEN it covers registration, login, logout, token expiry, and 401 handling

## Acceptance Test Mapping

| Scenario ID | Intended project/suite | Working directory | Framework/tool | Expected command (not executed) |
|---|---|---|---|---|
| AUTH-REG-001, AUTH-REG-002, AUTH-AUTH-001, AUTH-PUBLIC-001, AUTH-TOKEN-001, AUTH-TOKEN-002, AUTH-TEST-BACKEND-001 | `backend/tests/TaskManagement.Api.Tests` | repository root | xUnit, FluentAssertions, WebApplicationFactory, SQLite | `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~Authentication` |
| HEALTH-001 | `backend/tests/TaskManagement.Api.Tests/HealthEndpointTests` | repository root | xUnit, WebApplicationFactory | Expected, not executed while .NET is unavailable: `dotnet test backend/tests/TaskManagement.Api.Tests/TaskManagement.Api.Tests.csproj --filter FullyQualifiedName~HealthEndpointTests` |
| UI-AUTH-001, UI-AUTH-002, UI-TOKEN-001, UI-AUTH-003, AUTH-TEST-FRONTEND-001 | `frontend/` authentication suite | `frontend/` | Vitest, React Testing Library | `npm test -- --run` |

The frontend MUST not retry with another identity or expose response internals.
