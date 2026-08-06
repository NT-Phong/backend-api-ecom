# Backend Modernization Execution Tracker

## Goal

Raise the Ecom backend to the repository CQRS, Unit of Work, security, PostgreSQL, and Clean Architecture standards without broad rewrites or unapproved public-contract changes.

## Status Legend

- `[ ]` not started
- `[-]` in progress
- `[x]` completed and verified
- `[!]` blocked or awaiting an approval gate

## Protected Contracts

- Public routes, request/response DTOs, HTTP statuses, and `ApiResponse` shape.
- Authentication, JWT/session behavior, roles, policies, and permissions.
- `ApplicationDbContext`, EF configurations, migrations, and model snapshot.
- Dependencies, runtime configuration, secrets, infrastructure, and deployment.
- Existing legacy behavior outside the Commerce direction.

## B0 - Baseline and Change-Set Protection

- [x] `B0.1` Capture dirty-worktree status before implementation.
- [x] `B0.2` Record current solution build and test baseline from the source review.
- [x] `B0.3` Record migration drift and vulnerable-package evidence from the source review.
- [x] `B0.4` Create this execution tracker.
- [-] `B0.5` Classify the existing worktree into bounded review manifests; do not stage unrelated user changes. The inventory is recorded in `BACKEND-MODERNIZATION-CHANGESET-INVENTORY.md`; staged/committed change-set boundaries remain intentionally open.

Baseline evidence on 2026-07-31:

- `dotnet build Ecom.sln --no-restore`: passed with 0 errors and 31 warnings.
- `dotnet test Ecom.sln --no-build --no-restore`: 70/70 passed.
- `dotnet ef migrations has-pending-model-changes`: failed; model drift exists.
- `dotnet list ... --vulnerable --include-transitive`: vulnerable transitive `MessagePack 2.5.187` found.
- The working tree was already dirty before this tracker and implementation batch.

Implementation evidence:

- `B1` regression test failed 2/2 before the fix and passed 2/2 after the fix.
- `B3` UnitOfWork behavior tests passed 7/7 after adding concurrency and cancellation cases.
- Full solution tests after the first implementation batch: Domain 36/36 and Integration 38/38 passed.
- `Testcontainers.PostgreSql 4.13.0` was used in an earlier batch, then removed after the project decision that Docker is not part of the test strategy.
- The PostgreSQL fixture and external-database safety guard compile; guard tests passed 5/5.
- Azure PostgreSQL database `ecom_integration_tests` was provisioned separately from `ecom_dev`.
- Each integration-test run uses a unique schema, migrates from baseline, truncates only its own tables between tests, and drops only its own schema on disposal.
- PostgreSQL tests passed 17/17 for the database guard, migration baseline, MediaAsset columns, VariantPrice overlap exclusion, repository semantics, soft/hard delete, UnitOfWork commit/rollback, and optimistic concurrency.
- Full solution verification after the Azure PostgreSQL batch: Domain 36/36 and Integration 55/55 passed.
- The pending empty MediaAsset migration was confirmed unapplied, removed with EF tooling, and regenerated as `20260731180053_AddMediaAssetProcessingLifecycle` with five intended columns.
- `has-pending-model-changes` now reports no model drift; reviewed idempotent SQL contains no `DROP TABLE`, `TRUNCATE`, or `DELETE FROM` statements.
- Product and Product Variant lifecycle commands were split from two multi-handler classes into seven focused use-case handlers; command names, namespaces, and controller contracts were preserved.

## B1 - BaseRepository Correctness

- [x] `B1.1` Add a query-generation regression test for `includeDeleted`.
- [x] `B1.2` Make `Query(includeDeleted: true)` use `IgnoreQueryFilters()`.
- [x] `B1.3` Verify `QueryNoTracking(includeDeleted: true)` inherits the corrected behavior.
- [x] `B1.4` Normalize duplicate IDs, reject empty sets, and verify missing-ID behavior.
- [x] `B1.5` Verify soft delete and hard delete against Azure PostgreSQL in B2.
- [ ] `B1.6` Add cancellation tokens to repository reads in a separate compatibility batch.
- [ ] `B1.7` Deprecate string sorting and generic reflection projection after consumer inventory.

Gate:

- No public API or schema change.
- Focused repository tests and Infrastructure build pass.
- PostgreSQL claims remain open until B2.

## B2 - PostgreSQL Integration Foundation

- [x] `B2.1` Add an isolated PostgreSQL fixture for a dedicated external test database only.
- [x] `B2.2` Skip database tests when `ECOM_TEST_POSTGRES` is absent; never fall back to Docker or the runtime database.
- [x] `B2.3` Reject external database names that do not end with `_test` or `_tests`.
- [x] `B2.4` Isolate each run in a unique schema and truncate only owned tables between test cases.
- [x] `B2.5` Add and pass soft-delete, hard-delete, concurrency, rollback, and PostgreSQL constraint tests.
- [x] `B2.6` Migrate an empty Azure test schema to the current model and verify migration history/MediaAsset columns.
- [ ] `B2.7` Create a dedicated least-privilege Azure test runner; current verification used the existing development credential without exposing it to source.

Approval gate:

- Changing the PostgreSQL test provider, credentials, or test dependencies requires explicit approval.

## B3 - Unit of Work Foundation

- [x] `B3.1` Add behavior tests for `DbUpdateConcurrencyException`.
- [x] `B3.2` Add behavior tests for cancellation rollback.
- [x] `B3.3` Add PostgreSQL multi-write rollback coverage.
- [x] `B3.4` Ensure successful mutations save/commit exactly once.
- [x] `B3.5` Ensure handled `IResult` failures rollback and clear tracking.
- [x] `B3.6` Keep nested transaction ownership with the outer request.
- [ ] `B3.7` Mark legacy transaction helpers obsolete after consumer migration.
- [x] `B3.8` Reduce normal transaction logs to Debug while retaining operational warnings/errors.

## B4 - CQRS Migration

- [x] `B4.1` Migrate Catalog mutations to `ITransactionalRequest` only.
- [x] `B4.2` Remove handler/service `SaveChangesAsync` from Catalog paths.
- [x] `B4.3` Split Product Options into one use case and handler per folder.
- [x] `B4.4` Split Categories into one use case and handler per folder.
- [x] `B4.5` Split Product Media into one use case and handler per folder.
- [x] `B4.6` Split Product and Variant lifecycle handlers.
- [x] `B4.7` Make Catalog queries non-transactional and no-tracking by default.
- [x] `B4.8` Add an architecture verifier for new CQRS violations.

## B5 - Authentication and Security

- [!] `B5.1` Convert Password Login to pipeline-owned transaction semantics.
- [!] `B5.2` Verify refresh rotation, replay handling, logout, and ownership.
- [!] `B5.3` Audit log redaction for tokens, credentials, PII, and payment data.
- [!] `B5.4` Externalize and validate CORS and trusted-proxy configuration.
- [!] `B5.5` Restrict detailed health, metrics, and Swagger exposure.
- [!] `B5.6` Review media quarantine, scanning, promotion, and cleanup compensation.

Approval gate:

- Authentication, authorization, security configuration, and public behavior require explicit approval before code changes.

## B6 - Controller and Error Contract

- [ ] `B6.1` Characterize existing route and response contracts with tests.
- [ ] `B6.2` Move controllers to explicit `ISender` injection.
- [ ] `B6.3` Enforce bind/copy route ID/send/map-only controller actions.
- [ ] `B6.4` Consolidate model validation and exception response generation.
- [!] `B6.5` Introduce a distinct concurrency error code.
- [ ] `B6.6` Keep public and management DTOs separated.

Approval gate:

- Error-code, status, route, and response-envelope changes require public-contract approval.

## B7 - Base Entity and Clean Architecture

- [ ] `B7.1` Encapsulate audit, soft-delete, and concurrency mutation.
- [ ] `B7.2` Retain the existing `No` column until a separately approved schema decision.
- [ ] `B7.3` Keep Domain independent from EF, ASP.NET, Redis, Azure, and Firebase implementations.
- [ ] `B7.4` Freeze legacy IoT modules; do not extend them for new Commerce work.
- [ ] `B7.5` Move files only when their bounded context is actively migrated.
- [ ] `B7.6` Enforce one Commerce entity and one EF configuration per file.

## B8 - Migration, Dependency, and Supply-Chain Gates

- [x] `B8.1` Reconcile the unapplied empty MediaAsset migration and model snapshot.
- [x] `B8.2` Generate and review forward-only idempotent SQL without applying it to `ecom_dev`.
- [x] `B8.3` Align EF Core and Npgsql patch versions.
- [x] `B8.4` Upgrade the SignalR/MessagePack dependency chain to a patched version.
- [!] `B8.5` Add vulnerability and locked-restore checks to CI.

Approval gate:

- Migration, snapshot, dependency, and CI/runtime configuration changes require explicit approval.

## B9 - Outbox, Observability, and Runtime Reliability

- [ ] `B9.1` Verify outbox creation is atomic with business writes.
- [ ] `B9.2` Add idempotent worker processing and retry ownership.
- [ ] `B9.3` Keep external side effects post-commit.
- [ ] `B9.4` Standardize trace IDs and structured operational logs.
- [ ] `B9.5` Add query and endpoint performance baselines before caching.

## B10 - Commerce Delivery Roadmap

- [-] `B10.1` Catalog PostgreSQL and authorization acceptance tests are implemented; dedicated external PostgreSQL execution remains required before completion.
- [-] `B10.2` User/guest Cart with hashed guest tokens is source-implemented; guest-isolation and merge-race PostgreSQL acceptance remain required.
- [-] `B10.3` Server-resolved checkout preview is source-implemented; API/relational acceptance remains required.
- [-] `B10.4` Idempotent transactional CreateOrder and stock reservation are source-implemented; PostgreSQL lock, replay, mismatch, race, and rollback acceptance remain required.
- [-] `B10.5` Order, Payment, Shipment, and reservation-expiry lifecycle handlers are source-implemented; transition/ownership/runtime acceptance remains required.
- [ ] `B10.6` Complete TradeInquiry Application/API.
- [ ] `B10.7` Run reviewed staging migration and smoke gates.

## Required Verification per Batch

1. Focused tests for the changed behavior.
2. Narrow project build.
3. Solution build when cross-layer source changes.
4. PostgreSQL tests for relational, transaction, concurrency, and migration claims.
5. `git diff --check` and an in-scope diff review.
6. Update this tracker only when the corresponding evidence passes.
