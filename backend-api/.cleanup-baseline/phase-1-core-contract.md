# Phase 1 core-contract boundary

## Implemented safe changes

- Validation is registered once in the MediatR pipeline.
- `ITransactionalRequest` is the transaction opt-in contract for new starter commands.
- The legacy request-level `EnableUnitOfWorkAttribute` check remains temporarily so the existing clone keeps its current behavior until business features are removed.

## Deferred because existing business code still depends on it

- Device, notification, Oxy, scale, SignalR, camera, record, operation, document, and material contracts, including enum and extension-method dependencies used indirectly through call syntax.
- `BaseDto` and `BaseQueryDto`; they have extensive feature-level use and will be removed or redesigned with the business-feature deletion phase.
- `IApplicationDbContext`, `ApplicationDbContext`, EF configurations, migrations, and infrastructure DI.
- Repository audit/current-user cleanup and post-commit domain-event strategy.

## Retained starter boundary

- Domain common primitives, generic repository and Unit of Work contracts.
- Generic result/paging/date-range models, subject to later removal of Ecom error and DTO coupling.
- Validation, logging, performance, error, and transaction pipeline primitives.
- API base controller and generic middleware, subject to later error-contract cleanup.

