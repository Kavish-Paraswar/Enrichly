# Architecture Overview

## Stack

- Frontend: Next.js, React, TypeScript, Tailwind CSS
- API: ASP.NET Core Web API, .NET 8, C#
- Database: PostgreSQL + Entity Framework Core
- Worker: separate .NET Worker Service

## Core model

- Job: the saved definition of a scheduled or manually runnable task
- Execution request: one logical run of a job
- Execution attempt: a single try under that execution request

## Queue and concurrency strategy

- PostgreSQL is the queue.
- Workers claim execution requests atomically with row locking.
- Active execution requests are protected by a partial unique index.
- Job updates use optimistic concurrency with a version field.
