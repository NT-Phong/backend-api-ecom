# Clean Architecture Rules

- Presentation maps HTTP/auth context to MediatR and response contracts.
- Application owns validation, authorization, orchestration, transactions, idempotency, and use-case DTOs.
- Domain owns Commerce invariants, state machines, events, and stable failures.
- Infrastructure owns EF/PostgreSQL, cache/storage/messaging, security providers, and external adapters.
- Existing pragmatic Application dependencies are tolerated; do not start broad purity refactors.
- New Commerce business logic must not depend on legacy module vocabulary or infrastructure SDKs.
