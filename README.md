# Job Automation Platform

This repository is the working foundation for the Enrichly HR take-home assignment. It is intentionally scoped to prove the platform, architecture, and developer experience without implementing the full product yet.

## Stack

- Frontend: Next.js, React, TypeScript, Tailwind CSS
- Backend: ASP.NET Core Web API, .NET 8, C#
- Database: PostgreSQL with Entity Framework Core
- Worker: separate .NET Worker Service

## What is included

- A responsive frontend dashboard shell
- An ASP.NET Core API project with DI, global error handling, validation, structured logging, Swagger, and health checks
- A separate worker service project
- PostgreSQL schema and an initial EF Core migration
- Docker Compose for local development
- Foundation documentation for architecture and engineering decisions

## Repository layout

- `frontend/` - Next.js dashboard shell and Tailwind configuration
- `backend/` - API, domain, infrastructure, migration, and worker projects
- `tests/` - initial test project scaffold
- `docs/` - reference documentation for the implementation
- `docker-compose.yml` - local development services
- `.env.example` - environment variable template

## Local setup

1. Install the prerequisites:
	- Node.js 20+ and npm
	- .NET 8 SDK
	- Docker Desktop
2. Copy `.env.example` to `.env` if you want local overrides in one place.
3. Install frontend dependencies:

```bash
cd frontend
npm install
```

4. Start the frontend during development:

```bash
npm run dev
```

5. Start the full local stack:

```bash
docker compose up --build
```

## Local URLs

- Frontend: `http://localhost:3000`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- Database health: `http://localhost:8080/health/db`

## Backend commands

```bash
cd backend
dotnet restore JobAutomationPlatform.sln
dotnet build JobAutomationPlatform.sln
dotnet test JobAutomationPlatform.sln
```

## Frontend commands

```bash
cd frontend
npm install
npm run build
npm run lint
```

## Environment variables

The application is configured through environment variables. The important ones are:

- `ConnectionStrings__Default` - PostgreSQL connection string
- `ASPNETCORE_ENVIRONMENT` - API environment
- `ASPNETCORE_URLS` - API listen address
- `Worker__PollingIntervalSeconds` - worker queue polling interval
- `NEXT_PUBLIC_API_BASE_URL` - frontend API base URL

## Design goals

- Keep the stack easy to explain in an interview
- Use PostgreSQL as the queue and source of truth
- Keep the codebase small and maintainable
- Prefer explicit, readable implementation over extra abstraction

## Current scope

This foundation is complete for startup and developer workflow, but it intentionally does not yet include the product features for job CRUD, execution retries, scheduling, or authentication.
