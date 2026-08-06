# CQRS, Handler, and Unit of Work Standard

## Scope and source of truth

This is the required pattern for new Ecom backend work. Live source, tests, current diff, and command output remain authoritative. `ITransactionalRequest` is the current mutation convention; `[EnableUnitOfWork]` and `ExecuteInTransactionAsync` exist only for legacy compatibility.

```text
HTTP Controller
  -> MediatR Command or Query
  -> Current user / authorization / validation behaviors
  -> UnitOfWorkBehavior for ITransactionalRequest only
  -> Handler
  -> Aggregate method or focused application service
  -> Repository tracks changes
  -> UnitOfWorkBehavior SaveChanges + Commit once
  -> TResult -> ApiResponse
```

## Use-case structure

Create one folder per use case. A command/query has one request and one handler. Add a validator when the request accepts input that needs structural validation.

```text
Features/<BoundedContext>/<Concern>/
  Commands/
    CreateX/
      CreateXCommand.cs
      CreateXCommandValidator.cs
      CreateXCommandHandler.cs
  Queries/
    GetX/
      GetXQuery.cs
      GetXQueryHandler.cs
      GetXDto.cs
  Services/
```

Do not group unrelated CRUD requests into one `*Commands.cs` file or implement multiple `IRequestHandler<,>` interfaces in one handler. Existing grouped code is a migration target, not a template.

## Controller and request rules

- Controller actions bind only route, query, and body; copy route IDs with immutable `with { ... }` expressions and call `Mediator.Send` once.
- Commands change state and implement `IRequest<TResult<TResponse>>, ITransactionalRequest`.
- Queries do not implement `ITransactionalRequest`; they use `QueryNoTracking()` unless tracking is necessary for an explicit reason.
- Storefront/public DTOs expose only public facts. Management DTOs can expose operational state and `ConcurrencyStamp`; never reuse them for anonymous routes.

## Validation, handler, Domain, and service rules

- FluentValidation verifies input shape: required fields, format, ranges, collection uniqueness, and maximum lengths.
- The handler verifies database facts: authorization, existence, ownership, uniqueness that needs a query, and concurrency stamp.
- Domain methods own aggregate invariants and state changes. The handler does not set related state property-by-property.
- A focused application service is appropriate for reusable orchestration such as access/concurrency loading or hierarchy traversal. Do not create generic CRUD services.
- Expected business failures return `TResult.Failure`. Unexpected exceptions are logged and thrown so the exception and transaction pipeline can handle them. Never return `ex.Message` to clients.

## Transaction ownership

For a new mutation:

1. `ITransactionalRequest` activates `UnitOfWorkBehavior` after authorization and validation.
2. The behavior opens the transaction and records whether it owns it.
3. The handler only invokes repository `InsertAsync`, `UpdateAsync`, or `DeleteAsync`; it does not save or manage a transaction.
4. A successful owner request calls `CommitTransactionAsync`, which saves and commits once.
5. A handled failed `IResult`, concurrency failure, cancellation, or exception rolls back and clears tracked state when the behavior owns the transaction.
6. Nested transactional requests join the active transaction and do not commit or roll back the outer request.

Do not call network, cache invalidation, email, payment, IoT, or other external side effects while the database transaction is open. Use a post-commit handler or outbox when delivery must be durable.

## Legacy migration

- Request-level `[EnableUnitOfWork]` remains supported only to protect existing code during migration.
- Handler-level attributes are not a transaction contract and must not be added.
- `ExecuteInTransactionAsync` remains available for existing callers but must not appear in new handlers.
- Migrate one bounded context/use case at a time: marker first, remove manual save/transaction calls, add tests, build, then proceed.

## Test gate

Every change to transaction behavior needs focused tests for:

- non-transactional query bypass;
- transactional success commits exactly once;
- handled `TResult` failure rolls back and clears tracking;
- exception and concurrency rollback behavior;
- nested transaction ownership.

Use PostgreSQL integration tests, not EF InMemory, for real multi-write rollback, EF constraints, concurrency, migrations, and transaction isolation claims.
