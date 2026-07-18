# Architecture

## Dependency Direction

```text
Presentation -> Application -> Domain
Infrastructure -> Application/Domain abstractions
```

Domain owns Commerce aggregates, invariants, state machines, events, and stable business failures. Application owns use cases, validation, authorization, transactions, orchestration, and DTOs. Infrastructure owns EF/PostgreSQL and external adapters. Presentation maps HTTP to MediatR and response contracts.

## Commerce Rules

- Aggregate methods are the only mutation surface.
- Cross-aggregate operations such as CreateOrder are Application orchestration inside UnitOfWork.
- Reads may use optimized projections through `IApplicationDbContext` or a dedicated query abstraction.
- OrderItem and histories retain immutable transaction truth.
- External notifications occur after commit or through an outbox.

## Request Flow

```text
Controller -> Mediator -> validation/authorization/UnitOfWork -> handler
-> aggregate/query/service -> EF/PostgreSQL -> TResult -> ApiResponse
```

Do not refactor unrelated legacy modules while building Commerce slices.
