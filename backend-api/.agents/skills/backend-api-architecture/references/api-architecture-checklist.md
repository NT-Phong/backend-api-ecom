# API Architecture Checklist

Use this checklist for backend API reviews.

## Boundary
- Endpoint maps cleanly to one command/query or a small orchestration.
- Controller uses `Mediator.Send`.
- Controller returns `HandleResult` or the established `ApiResponse` pattern.
- Request and response DTOs do not expose internal infrastructure concerns.
- Endpoint has `[Authorize(Policy = Permissions.*)]` unless intentionally anonymous.
- Route ids are copied into commands/queries before validation/handler execution.

## Application
- Command/query has a handler.
- Validator covers required fields and product constraints.
- Mutation path has transaction behavior where needed.
- Handler uses existing abstractions and project error conventions.
- Handler returns `TResult`/`TResult<T>` for handled failures.
- Handler uses `MessageKey` and `ErrorCodes` when suitable constants exist.
- Direct `IApplicationDbContext` use is acceptable where nearby complex queries already use it.

## Domain
- Business invariants live in domain methods or clearly named application services.
- Entity state is not mutated directly from external DTOs when a domain method should own the transition.

## Infrastructure
- Persistence uses existing UnitOfWork/repository patterns.
- Integration concerns stay in Infrastructure.
- Cache keys and SignalR groups match established conventions.
- Redis fallback and memory-cache fallback are considered when changing cache behavior.
- Background workers create scopes before resolving scoped services.

## Verification
- Focused test exists or a focused build command is identified.
- Risks and unverified areas are explicit.
- If no test project exists, the missing automated coverage is reported.
