# Enterprise Role-Based Management System (ERBMS)

Full-stack RBAC platform built with ASP.NET Core (.NET 9) and React (Vite + TypeScript) using Clean Architecture.

## Features
- Clean Architecture (Domain, Application, Infrastructure, API)
- JWT auth with ASP.NET Core Identity
- RBAC: Users, Roles, Permissions (CRUD)
- Admin approvals for self-signup
- Email availability check and duplicate email handling (409)
- Audit logging, caching, and Swagger
- Frontend admin dashboard with toasts, validation, and search

## Tech Stack
- Backend: ASP.NET Core Web API (.NET 9), EF Core, Identity, Serilog
- Frontend: React, Vite, TypeScript, React Router, Axios
- Data: SQL Server LocalDB (default)

## Architecture Overview
- Domain: entities and core business rules
- Application: DTOs, interfaces, and use cases
- Infrastructure: EF Core, Identity, repositories, logging
- API: controllers, middleware, startup configuration

## Folder Structure
- ERBMS.Domain: entities and value objects
- ERBMS.Application: DTOs, interfaces, services
- ERBMS.Infrastructure: EF Core context, Identity, repositories
- ERBMS.API: Web API entry point and controllers
- ERBMS.Tests: xUnit tests
- client: React + Vite frontend

## Getting Started

### Backend
1. Restore and run migrations:
   - `dotnet ef database update --project ERBMS.Infrastructure --startup-project ERBMS.API`
2. Run API:
   - `dotnet run --project ERBMS.API --urls http://localhost:5125`
3. Swagger:
   - http://localhost:5125/swagger

### Frontend
1. Install deps:
   - `cd client`
   - `npm install`
2. Run client:
   - `npm run dev`
3. App:
   - http://localhost:5173

## Default Credentials (Seed)
- Email: `admin@erbms.local`
- Password: `ChangeMe123!`

## Configuration
- API settings: `ERBMS.API/appsettings.json`
- Default seed admin:
  - `Seed:AdminEmail`
  - `Seed:AdminPassword`
  - `Seed:AdminFullName`
- Client base URL:
  - `Client:Url`

## Approval Flow
- Self-signup creates an inactive user.
- Admin must approve from Users page.
- Admin-created users are active immediately.

## Admin Workflow
1. Sign in as system admin.
2. Go to Users.
3. Review status (Pending/Active) and Approve when ready.
4. Assign or remove roles as needed.

## User Workflow
1. Sign up with email and password.
2. Wait for admin approval.
3. Log in after approval.

## Useful Endpoints
- Auth:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `GET /api/auth/email-available`
- Users:
  - `GET /api/users`
  - `POST /api/users/{id}/approve`
  - `POST /api/users/assign-role`
  - `POST /api/users/remove-role`
  - `POST /api/users/cleanup-identity`

## UI Tips
- Users page has a status filter (All/Pending/Active).
- Use Cleanup Identity for orphaned accounts not visible in the list.

## Frontend Branding
- App title is set in `client/index.html`.
- Favicon lives in `client/public/favicon.svg`.

## Tests
- Run all tests:
   - `dotnet test`

## Docker
- `docker-compose up --build`

## Troubleshooting
- If API fails to start, ensure the database exists and migrations are applied.
- If email shows as already used after deletion, run the cleanup endpoint.
- If login fails with 403, the account is pending approval.

## Notes
- System admin deletion is blocked.
- Self-deletion is blocked.

## License
MIT
