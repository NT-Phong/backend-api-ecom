# Commerce 9/10 Roadmap

| Batch | Deliverable | Status | Gate |
| --- | --- | --- | --- |
| 1 | Aggregates, invariants, state machines, domain tests | Verified | Domain build/tests |
| 2 | Catalog list/detail and effective price resolver | Not started | Cross-layer build/catalog tests |
| 3 | User/guest Cart APIs | Not started | Token/cart concurrency tests |
| 4 | Preview + transactional CreateOrder | Not started | rollback/idempotency/stock tests |
| 5 | Order/Payment/Shipment lifecycle + cleanup | Not started | transition/cleanup tests |
| 6 | TradeInquiry Application/API + hardening | Domain started | Domain/Application tests |
| 7 | Migration, exclusion, index review | Not started | PostgreSQL migration tests |
| 8 | Staging apply/smoke | Not started | approval before production |

## Locked MVP Decisions

- Reserve during CreateOrder; TTL 30 minutes; MAIN location only.
- VND; active Sale then Public; no retail B2B price.
- COD and manual bank transfer; no gateway.
- Guest cart Secure HttpOnly cookie; DB stores hash only.
- CreateOrder requires Idempotency-Key; same fingerprint returns same order, mismatch conflicts.
- No multi-vendor, settlement, producer portal, or commission.

## Next Safe Batch

After Batch 1 verifies, add public Product list/detail queries, `IEffectivePriceResolver`, filters/paging/sort whitelist, versioned endpoints, and query tests. Do not add a migration in Batch 2.

## CreateOrder Contract

One UnitOfWork transaction resolves idempotency, reloads cart/current prices/MAIN stock, reserves atomically, creates Order/Items/Payment/Reservation/Movement, converts Cart, records result, and commits once. Any error rolls back all. Return handled concurrency conflict; do not retry blindly.

## Final Acceptance

Seven behavioral roots; end-to-end commerce; no oversell; idempotent order/payment; no price overlap; clean rollback; useful indexes; Domain/PostgreSQL tests; reviewed staging migration.
