# AGENTS.md

## Purpose

Guide agents building the Thanh Hoa Commerce backend API. Detailed routing lives under `.agents/`; this file is the bootstrap.

## Project Direction

The target product is a public commerce platform for local products with catalog, producer profiles, trust/traceability, cart, checkout, orders, payment, shipment, CMS, engagement, B2B inquiries, reporting, and administration.

The repository still contains legacy aquaculture/IoT modules. Treat them as existing source, not as the direction for new work. Do not extend those modules unless the user explicitly reopens that scope.

Architecture remains pragmatic Clean Architecture with CQRS/MediatR:

```text
Presentation -> Application -> Domain
Infrastructure -> Application/Domain abstractions
```

## Startup

1. Read `.agents/context/task-router.md`.
2. Select one primary skill from the router.
3. Load only the reference matching the current task boundary.
4. Use `.agents/context/quick-scan.md` when starting from a route, entity, error, or feature name.
5. Reuse already-loaded stable context during the same task.

## Workflow

1. Classify: design, debugging, refactoring, review, or implementation.
2. Bound the task to route/controller, request, handler, aggregate, repository/query, configuration, or integration.
3. Search before opening files and inspect a nearby pattern.
4. Make the smallest safe change and preserve public contracts unless approved.
5. Run the narrowest safe build/test when the environment permits it.
6. Report summary, files, verification, risks, and the next roadmap gate.
7. Update Commerce references only when durable status or contracts change.

## Backend Standards

- Keep controllers thin; use MediatR commands/queries and FluentValidation.
- Put business invariants and state transitions in Domain methods.
- Use existing `TResult`, `ApiResponse`, `MessageKey`, and `ErrorCodes` conventions.
- Use UnitOfWork/transactions for multi-write commerce operations.
- Keep one Commerce entity and one EF configuration per file.
- Keep cross-aggregate references ID-based; avoid large bidirectional navigation graphs.
- Never trust client price, stock, discount, payment result, or order totals.
- Use PostgreSQL integration tests for constraints, concurrency, and migrations; do not use EF InMemory for those claims.

## Approval Gates

Require explicit approval before changing public APIs, auth/permissions, EF migrations/snapshot/DbContext/configurations, dependencies, runtime configuration, secrets, infrastructure, or deployment.

Never log secrets, JWT/refresh tokens, guest cart tokens, payment credentials, connection strings, private customer data, or raw uploads.

## Delegation

Keep requirements, decisions, and integration in the main task. Delegate only independent workstreams such as read-only exploration, test-gap analysis, security review, or verification. Avoid parallel edits to the same aggregate, schema, migration, or API contract; keep delegation depth at one.

## Verification

Run verification in proportion to risk and record exact commands/results. A blocked or skipped check is not a pass. For Commerce, follow `.agents/skills/commerce-system/references/implementation-roadmap.md`; never apply a migration without explicit approval and staging evidence.

## Final Response

- Summary
- Files changed
- Verification run
- Risks / follow-ups
