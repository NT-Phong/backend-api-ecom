# Safety Constraints

## Approval Required

- Public routes/DTOs/handler signatures.
- Auth, current-user, permissions, roles/policies.
- DbContext, entity configurations, migrations, snapshot, dependencies.
- Payment/inventory/idempotency contracts, runtime configuration, CI/CD/deployment.
- Destructive operations.

## Never

- Log secrets, JWT/refresh tokens, guest cart tokens, payment credentials, PII, connection strings, or raw uploads.
- Rewrite applied migrations or generated snapshots manually.
- Hide failures with broad catches.
- Disable tests/analyzers to pass a gate.
- Mix unrelated legacy cleanup into a Commerce slice.
