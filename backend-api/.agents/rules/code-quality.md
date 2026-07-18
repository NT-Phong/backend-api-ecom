# Code Quality

- Thin controllers, focused handlers, behavioral aggregates.
- One Commerce entity and configuration per file.
- No speculative abstractions or unrelated refactors.
- No direct state mutation or client-authoritative financial/inventory values.
- Deterministic price selection and explicit transaction/idempotency boundaries.
- Append-only transaction histories.
- Tests target behavior and real PostgreSQL semantics where relational behavior matters.
