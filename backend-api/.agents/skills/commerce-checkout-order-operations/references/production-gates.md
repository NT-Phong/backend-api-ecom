# Production gates for Cart to Order

Use this checklist before recommending deployment. It is a gate list, not proof that a gate has passed.

## Required configuration and schema

- Provision private `SystemSetting` key `checkout.shipping.standardFeeVnd` with a valid non-negative VND value. Missing or malformed data must fail checkout.
- Review and apply pending Commerce migrations only to an approved staging target. Keep Catalog/Media migration work isolated in review and rollout.
- Seed and verify `orders.read`, `orders.manage`, `payments.verify`, and `shipments.manage` assignments in the target database.

## Required PostgreSQL scenarios

- Partial unique active `CartItem`, guest/user cart merge race, and guest cookie isolation.
- Parallel stock lock with no oversell; retryable concurrency handling; CreateOrder rollback at each mutation boundary.
- Idempotency replay, mismatch, processing race, and expiry behavior.
- Reservation expiry/release exactly once; confirm hold; ship consume; cancel/refund transitions.
- Migration application and expected constraints/indexes on a dedicated test database.

## Outbox and observability

The existing interceptor can persist domain events atomically only when enabled. Do not claim durable external delivery until a processor has explicit claim/lease, retry/backoff, duplicate-safe dispatch, dead-letter handling, telemetry, and PostgreSQL tests.

Never emit guest token, payment proof, phone number, or full address in logs. Prefer order ID and idempotency record ID as correlation values.
