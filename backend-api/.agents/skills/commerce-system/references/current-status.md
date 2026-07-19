# Commerce Current Status

Last source review: 2026-07-18

## Verified Evidence

| Evidence | Result |
| --- | --- |
| Static Commerce inventory | 79 entities and 79 EF configurations recorded in the latest source audit |
| ERD structural scan | 80 DBML tables including User and 115 references; official parser confirmation remains unavailable |
| Solution build | `dotnet build Ecom.sln --no-restore`: 0 errors, 13 warnings |
| Domain tests | 18/18 passed, including aggregate and media invariants |
| Media infrastructure tests | 5/5 passed for signature, MIME spoofing, intent, size, visibility, and path traversal |
| `git diff --check` | Passed for the inspected working tree; line-ending warnings only |
| Old Commerce enum-symbol scan | No old symbol references found in the inspected source |

The media policy tests do not use PostgreSQL and do not prove relational, outbox, or rollback behavior.

## Verified Working-Tree Work

- Seven aggregate roots, factories, transitions, domain errors, and events build successfully and have 18 passing domain tests.
- MediaAsset scan/visibility lifecycle, Product media, TradeInquiry attachments, and payment-proof invariants are covered by domain tests.
- Secure upload policy, local quarantine/public/private storage, product media reader, persisted notification service, and outbox conversion source build successfully; outbox interception defaults to disabled until its migration/test gate passes.
- `Tests/Ecom.IntegrationTests` is in the solution; current tests cover file policy only, not PostgreSQL.
- Preserve user-owned changes in `document/DETAILED-ERD.dbml` and `.dbdiagram`.

## Capability Status

| Area | State |
| --- | --- |
| Relational model | Broad Product/Variant/Price/Inventory and transaction-history separation exists |
| Rich Domain Model | Batch 1 verified by solution build and 18 domain tests |
| Media support | Source implemented; file policy verified, PostgreSQL lifecycle unverified |
| Catalog API | Source implemented: public list/detail/categories plus backoffice Product lifecycle; PostgreSQL integration verification pending |
| Cart/checkout/order API | Not implemented |
| Idempotency/concurrency | Planned, not implemented |
| Status/media/outbox migration | Fresh baseline `InitialCommerceSchema` applied to Azure `ecom_dev`; PostgreSQL lifecycle verification remains pending |

## Active Gate

Add PostgreSQL Testcontainers coverage for MediaAsset persistence, outbox atomicity, cleanup idempotency, and storage/DB rollback before treating the applied schema as verified for production.

## Known Risks

1. Enum strings changed in source without a data migration.
2. Catalog CQRS/controllers are source-implemented, but PostgreSQL integration coverage and live API smoke verification remain open.
3. Product publish facts must later be resolved from persisted state.
4. Some root methods accept caller-supplied child collections to avoid changing EF navigation shape in Batch 1.
5. Outbox conversion is implemented but defaults to disabled; PostgreSQL atomicity and an outbox processor remain unverified, and FCM is not registered.
6. Price overlap, inventory locking, idempotency, cleanup, and PostgreSQL tests remain open.

## Score Snapshot

- Relational design: 8.4/10; EF configuration: 8.0/10.
- Domain after unverified Batch 1: estimated 7.0/10.
- Application/API: 3.5/10; production readiness: estimated 5.8/10.
- Overall 9/10 requires every roadmap gate and its evidence.

### 2026-07-18 - Batch 1 and media foundation
- Source: Commerce aggregates plus MediaAsset/ProductMedia/InquiryAttachment lifecycle, secure storage policy, product media reader, notification inbox, and outbox conversion.
- Verification: `dotnet build Ecom.sln --no-restore` passed with 0 errors; Domain tests 18/18; media infrastructure tests 5/5.
- Migration: none generated or applied.
- Remaining risk: PostgreSQL persistence, outbox atomicity, file/DB rollback, cleanup idempotency, authorization/API contracts, and real malware scanning remain open.

### 2026-07-18 - Fresh Azure development schema baseline
- Source: replaced the two historical migration chains and the transient diagnostic migration with `20260718084313_InitialCommerceSchema` plus a regenerated `ApplicationDbContextModelSnapshot`.
- Verification: `dotnet build Presentation/Ecom.API/Ecom.API.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; `dotnet ef migrations has-pending-model-changes` reported no changes; idempotent SQL contained 90 `CREATE TABLE` operations and no `DROP`, `DELETE`, or `TRUNCATE`.
- Migration: `dotnet ef database update` applied `20260718084313_InitialCommerceSchema` to Azure PostgreSQL database `ecom_dev`; follow-up `dotnet ef migrations list` confirmed it as the only applied migration.
- Remaining risk: this is a development baseline, not production/staging evidence; PostgreSQL integration coverage for relational constraints, concurrency, rollback, media lifecycle, and outbox atomicity is still missing.

### 2026-07-19 - Catalog Product API source implementation
- Source: public `GET /api/v1/products`, `GET /api/v1/products/{slug}`, and `GET /api/v1/categories`; effective Sale/Public VND price resolver; Catalog product policy checks; backoffice Product/category/media/variant/price/lifecycle commands; and forward-only VariantPrice overlap migration.
- Verification: API build passed; Domain tests 29/29 passed; `has-pending-model-changes` reported no changes; idempotent SQL contains the `btree_gist` VariantPrice exclusion constraint.
- Remaining risk: migration is not applied; PostgreSQL query/concurrency tests and API authorization smoke tests are not yet available because no Testcontainers fixture is configured and the configured Azure database was unreachable from this environment.
