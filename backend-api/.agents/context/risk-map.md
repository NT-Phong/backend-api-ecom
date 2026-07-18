# Risk Map

## Critical Commerce Risks

- Auth/JWT/current-user, customer ownership, roles and permissions.
- Public API/DTO and status vocabularies.
- EF DbContext/configuration/migrations/model snapshot.
- Price overlap and deterministic effective-price selection.
- Inventory reservation, concurrency, overselling, expiry/release/consume lifecycle.
- CreateOrder/payment idempotency and transaction rollback.
- Payment confirmation/refund and webhook verification when gateway scope opens.
- Guest cart token secrecy and PII exposure.
- Append-only transaction/audit histories.

## Safe Rules

- Trace controller -> request -> handler -> aggregate -> persistence before edits.
- Never trust client financial/inventory values.
- Do not retry concurrency blindly.
- Do not publish side effects before commit.
- Use real PostgreSQL tests for relational behavior.
- Never rewrite an applied migration; add an explicit forward migration.

## Protected Files

- Auth/Logout/current-user flows.
- `Permissions.cs`, `ErrorCodes.cs`, message contracts.
- `ApplicationDbContext`, Commerce configurations, migrations and snapshot.
- `appsettings*.json`, secrets, certificates, CI/CD and deployment.

Legacy IoT/SignalR contracts are not project direction, but remain protected existing behavior unless removal is separately approved.
