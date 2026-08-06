---
name: backend-api-architecture
description: Analyze, design, implement, review, debug, or refactor the Ecom backend API across controllers, MediatR CQRS, validation, authorization, UnitOfWork, EF Core/PostgreSQL, error contracts, security, performance, testing, and Clean Architecture boundaries. Use for cross-cutting backend flows or when a task is not primarily a Commerce domain-model question.
---

# Backend API Architecture

## Start

Read `AGENTS.md` and `.agents/context/task-router.md`. Identify the smallest route, request, handler, aggregate, persistence, or integration boundary before loading a reference.

## Reference Routing

- API contract or full request flow: `references/api-architecture-checklist.md`.
- Dependency direction or refactoring boundary: `references/dependency-boundaries.md` and `references/clean-architecture-rules.md`.
- Security, auth, privacy, or secret handling: `references/security-review.md` plus `.agents/context/risk-map.md`.
- Planned implementation and verification: `references/execution-workflow.md`.
- CQRS, handler, transaction ownership, and test conventions: `references/cqrs-unit-of-work-standard.md`.
- Commerce behavior or schema: also use `commerce-system`, loading only its matching reference.

## Workflow

1. Search the exact route, symbol, error, or test.
2. Trace Presentation -> Application -> Domain -> Infrastructure.
3. Check validation, authorization, transactions, error mapping, security, and tests.
4. Preserve public contracts and protected configuration unless explicitly approved.
5. Prefer aggregate methods and focused use-case services over controller/handler business logic.
6. Run the narrowest relevant build/test when permitted and report evidence.

## Standards

- Keep versioned controllers thin and use MediatR requests.
- Use FluentValidation and stable `TResult`/`ApiResponse` errors.
- New mutations implement `ITransactionalRequest`; `UnitOfWorkBehavior` owns begin/commit/rollback and the single normal `SaveChangesAsync` call.
- Use one request, validator where applicable, and handler per use case. Keep queries non-transactional and no-tracking by default.
- `[EnableUnitOfWork]` and `ExecuteInTransactionAsync` are legacy bridges only; do not introduce new consumers.
- Keep HTTP and Infrastructure concerns out of Domain.
- Add abstractions only for a real boundary, duplication, or testability need.
- Use PostgreSQL integration tests for constraints, concurrency, and migrations.
- Never log secrets, guest tokens, payment credentials, or PII.
