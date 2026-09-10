# Engineering Notes

## Purpose

This repository is a take-home assignment foundation, not a production platform. The goal is to show strong engineering judgment by building a clean, explainable stack that can evolve into the full product without rework.

## Architecture summary

- Frontend: Next.js App Router dashboard shell
- API: ASP.NET Core Web API with controllers and middleware-based error handling
- Persistence: PostgreSQL with EF Core and migrations
- Worker: separate .NET Worker Service that will later own queue polling and execution

## Domain boundaries

Keep the core model explicit:

- Job definition
- Execution request
- Execution attempt

That separation matters because it supports scheduling, retries, history, stale-run recovery, and duplicate-run protection without collapsing everything into one record.

## Conventions

- UUID primary keys everywhere
- UTC timestamps only
- Environment-variable driven configuration
- Validation at the API boundary
- Structured logging in the API and worker
- Separate background worker process instead of embedding execution into the API

## Database approach

- PostgreSQL is the system of record and the queue.
- EF Core manages schema through migrations.
- The initial migration creates the table structure needed for jobs, execution requests, and execution attempts.
- The database shape is prepared for optimistic concurrency, unique active execution constraints, and worker claim patterns.

## API approach

- Controllers are kept thin.
- Business logic lives in application/infrastructure services.
- Global exception handling maps domain and validation errors to clean HTTP responses.
- Swagger is enabled for discoverability and demoability.

## Worker approach

- The worker is a separate process so it can scale and fail independently.
- It is wired for environment-based configuration.
- The current foundation includes scheduler, queue, heartbeat, and stale-reaper service shells so the eventual execution pipeline has the correct home.

## Local development expectations

- Frontend: `npm install`, `npm run dev`, `npm run build`, `npm run lint`
- Backend: `dotnet restore`, `dotnet build`, `dotnet test`
- Infra: `docker compose up --build`

## Verification status in this environment

- Frontend build and lint passed
- Backend build/test could not run because the .NET SDK is not installed in this environment
- Docker Compose validation could not run because Docker is not installed in this environment
