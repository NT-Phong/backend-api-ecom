---
name: commerce-checkout-order-operations
description: Source-first workflow for analyzing, implementing, reviewing, or upgrading Thanh Hoa Commerce Cart, checkout, Order, Payment, Shipment, inventory reservation, guest ownership, idempotency, and their PostgreSQL/API tests. Use whenever a task touches Cart to Order behavior, checkout pricing, stock locking, payment or shipment lifecycle, guest cookies, order ownership, Commerce migrations, or production readiness of this flow.
---

# Commerce Checkout to Order Operations

Use this skill to turn a Commerce request into a bounded, verified change. Treat live source, tests, migrations, and current diff as evidence; never infer runtime or PostgreSQL behavior from a build.

## Fast start

1. Read `AGENTS.md` and `.agents/context/task-router.md`.
2. Run the read-only source map:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/commerce-checkout-order-operations/scripts/scan-commerce-checkout.ps1
   ```

3. Locate the exact route, request, handler, aggregate, EF configuration, migration, and test before proposing a change.
4. Read `references/source-map.md` for the existing slice and `references/production-gates.md` only for migration, concurrency, outbox, or deployment work.
5. Load `commerce-system` for aggregate/schema facts and `backend-api-architecture` only when the boundary includes API, authorization, pipeline, or transaction behavior.

## Execution loop

### 1. Bound the request

Classify it as one of: read/query, cart mutation, checkout preview, order creation, lifecycle transition, persistence/migration, or production verification. State the exact public contract and ownership rule before editing.

Trace a cross-layer flow in this order:

```text
Controller -> MediatR request/validator -> handler/service -> aggregate -> EF/configuration -> migration -> tests
```

Do not turn Buy Now into a separate order flow: add one cart line, preview, then create an order with that line ID.

### 2. Apply invariants

- Keep controllers thin. One use case has one request, validator where needed, and handler.
- Commands implement `ITransactionalRequest`; queries use `QueryNoTracking()` and never start a transaction.
- Let `UnitOfWorkBehavior` perform the only normal commit. Handlers must not call `SaveChangesAsync`, start transactions, or call external services.
- Put transitions in Domain methods. Use IDs across aggregates; do not introduce a broad navigation graph.
- Resolve price, sellability, stock, shipping fee, totals, and payment state on the server. Never accept them from the client.
- Store only the guest-token hash. Never log the token, payment proof, or full recipient data.
- Lock tracked inventory in stable `InventoryItemId` order through `IInventoryReservationStore`; do not replace it with a generic-repository stock update.
- Treat a missing or invalid `checkout.shipping.standardFeeVnd` as a fail-closed checkout error. Do not add a hard-coded fallback.

### 3. Choose the flow

| Task | Required behavior |
| --- | --- |
| Cart | Resolve user or guest principal; do not reserve stock; selected purchased lines soft-delete; convert Cart only when no active lines remain. |
| Preview | Resolve current price, availability, and shipping server-side; return a canonical quote fingerprint. |
| Create order | Claim idempotency before mutation; reload cart and quote; reject changed quote; lock/reserve stock; create immutable snapshots/payment; then clear selected cart lines in one transaction. |
| Expiry/cancel | Release active reservation exactly once and record movement/history. Paid orders require a dedicated refund workflow, never silent cancel. |
| Confirmation/shipping | Confirmation removes expiry only; shipment start consumes reservation and stock; delivery failure changes both Order and Shipment state. |
| Read | Filter public order access by authenticated user or guest order hash; return 404 for a non-owned ID. |

### 4. Respect approval gates

Ask explicit approval before changing public routes/DTOs, permissions, EF migrations/snapshot/DbContext, dependencies, configuration, secrets, infrastructure, or deployment. A feature request does not authorize applying a migration to a shared database.

For migrations, inspect generated SQL, add a forward-only migration, run `has-pending-model-changes`, and record the staging plan. Never edit an applied migration.

### 5. Verify by claim type

| Claim | Minimum evidence |
| --- | --- |
| Domain transition | Domain behavior test |
| CQRS transaction convention | Architecture test plus handler inspection |
| Partial unique/index/lock/rollback/idempotency | Dedicated PostgreSQL integration test on an approved test DB |
| API contract/ownership | API test for 401/403/404 and user/guest isolation |
| Outbox delivery | Atomic persistence test plus worker lease/retry/duplicate-dispatch test |
| Migration safety | Reviewed idempotent SQL, clean build, non-skipped PostgreSQL tests, approved staging apply |

When `ECOM_TEST_POSTGRES` is absent, report PostgreSQL verification as blocked/skipped, not passed; do not apply a migration.

## Required handoff

Report behavior changed, source files changed, commands and exact results, migration status, and remaining production gates. Separate confirmed source/test evidence from assumptions and runtime claims. Update durable Commerce status references only after the corresponding evidence exists.
