# Backend Modernization Change-Set Inventory

## Purpose

This inventory protects the existing dirty worktree while the modernization roadmap is implemented. It defines review and future commit boundaries only. It does not stage, commit, discard, or transfer ownership of any existing change.

Baseline snapshot before the G2 file splits on 2026-08-02: 75 status entries: 43 tracked modifications/deletions and 32 untracked files. The current entry count will rise when one grouped source file becomes multiple use-case files; use the change-set manifest rather than the raw count to assess scope. Existing work must be treated as mixed/unassigned unless a reviewer explicitly identifies its owner.

## Rules

- Review and verify one change-set at a time.
- Do not stage all files together.
- Never combine an EF migration or model snapshot with unrelated CQRS, Auth, or configuration work.
- Keep public-contract, authentication, dependency, and runtime-configuration changes behind their explicit approval and verification gates.
- PostgreSQL tests are evidence for the change-set they cover; a skipped database test is not a passing gate.
- The listed paths are a review manifest, not permission to discard files that are not listed.

## CS-01: Dependency and supply-chain repair

**Scope:** package alignment and vulnerable dependency remediation.

- `Core/Ecom.Application/Ecom.Application.csproj`
- `Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj`
- `Presentation/Ecom.API/Ecom.API.csproj`
- `.agents/context/tech-stack.md`

**Required verification:** restore, solution build, full test suite, and `dotnet list Ecom.sln package --vulnerable --include-transitive --no-restore`.

**Current evidence:** source updates exist; dependency verification was recorded separately. CI locked-restore/vulnerability enforcement remains outside this change-set.

## CS-02: Unit of Work and repository correctness

**Scope:** pipeline-owned transaction behavior and generic repository semantics.

- `Core/Ecom.Application/Common/Behaviours/UnitOfWorkBehavior.cs`
- `Core/Ecom.Domain/Interfaces/Repositories/IUnitOfWork.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/UnitOfWork/BaseRepository.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/UnitOfWork/UnitOfWork.cs`
- `Tests/Ecom.IntegrationTests/Application/UnitOfWorkBehaviorTests.cs`
- `Tests/Ecom.IntegrationTests/Persistence/BaseRepositoryQueryTests.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/BaseRepositoryPostgreSqlTests.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/UnitOfWorkPostgreSqlTests.cs`

**Required verification:** focused unit tests, focused PostgreSQL tests against the dedicated external test database, Infrastructure build, and solution build.

**Do not include:** Auth login orchestration, Catalog handlers, or migration files.

## CS-03: PostgreSQL test harness and schema acceptance

**Scope:** external PostgreSQL-only integration fixture, test database safety guard, migration acceptance, and relational constraint coverage.

- `Tests/Ecom.IntegrationTests/PostgreSql/PostgreSqlCollection.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/PostgreSqlFactAttribute.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/PostgreSqlFixture.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/PostgreSqlTestDatabaseGuard.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/PostgreSqlTestDatabaseGuardTests.cs`
- `Tests/Ecom.IntegrationTests/PostgreSql/MigrationPostgreSqlTests.cs`
- `Tests/Ecom.IntegrationTests/Ecom.IntegrationTests.csproj`

**Required verification:** the guard tests pass without credentials; database tests pass only when `ECOM_TEST_POSTGRES` points to a dedicated database ending in `_test` or `_tests` and `ECOM_TEST_ALLOW_RESET=true` is explicitly supplied. Docker/Testcontainers must not be reintroduced.

**Operational gate:** provision and use a least-privilege test principal before treating the PostgreSQL gate as continuously reproducible.

## CS-04: MediaAsset persistence and storage foundation

**Scope:** MediaAsset lifecycle model, storage provider wiring, and the single forward migration.

- `Core/Ecom.Application/Common/Configuration/MediaStorageOptions.cs`
- `Core/Ecom.Application/Common/Interfaces/ICommerceMediaService.cs`
- `Core/Ecom.Application/Common/Services/CommerceMediaService.cs`
- `Core/Ecom.Domain/Entities/Commerce/Catalog/MediaAsset.cs`
- `Infrastructure/Ecom.Infrastructure/Services/AzureBlobStorageService.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce/Catalog/MediaAssetConfiguration.cs`
- `Infrastructure/Ecom.Infrastructure/Migrations/20260731180053_AddMediaAssetProcessingLifecycle.cs`
- `Infrastructure/Ecom.Infrastructure/Migrations/20260731180053_AddMediaAssetProcessingLifecycle.Designer.cs`
- `Infrastructure/Ecom.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`
- `Infrastructure/Ecom.Infrastructure/DependencyInjection.cs`
- `Presentation/Ecom.API/appsettings.json`
- `document/file_storage/DOCUMENT-UPLOAD-IMAGE-DESIGN-REPORT.md`

**Required verification:** no pending model changes, reviewed idempotent SQL, PostgreSQL migration/MediaAsset tests, and a separate approved staging-apply gate.

**Do not combine:** this change-set must never be committed together with an unreviewed runtime connection string or storage secret.

## CS-05: Catalog domain, CQRS and public/backoffice API

**Scope:** Product/Category/Option/Variant/Media use cases, effective price reads, permissions, controllers, and CQRS structure.

- `Core/Ecom.Application/Features/Catalog/**`
- `Core/Ecom.Application/Common/Interfaces/IEffectivePriceResolver.cs`
- `Core/Ecom.Application/Common/Services/EffectivePriceResolver.cs`
- `Core/Ecom.Application/DependencyInjection.cs`
- `Core/Ecom.Domain/Entities/Commerce/Catalog/Product.cs`
- `Core/Ecom.Domain/Entities/Commerce/Catalog/Category.cs`
- `Core/Ecom.Domain/Entities/Commerce/Catalog/ProductOption.cs`
- `Core/Ecom.Domain/Entities/Commerce/Catalog/ProductOptionValue.cs`
- `Core/Ecom.Domain/Entities/Commerce/Catalog/ProductVariantOptionValue.cs`
- `Core/Ecom.Domain/Constants/Permissions.cs`
- `Core/Ecom.Domain/Enums/Commerce/CommerceEnums.cs`
- `Presentation/Ecom.API/Controllers/V1/CatalogProductsController.cs`
- `Presentation/Ecom.API/Controllers/V1/CategoriesController.cs`
- `Presentation/Ecom.API/Controllers/V1/CatalogCategoriesController.cs`
- `Presentation/Ecom.API/Controllers/V1/CatalogProductOptionsController.cs`
- `Tests/Ecom.Domain.Tests/Commerce/ProductAndCartTests.cs`
- `Tests/Ecom.Domain.Tests/Commerce/CategoryTests.cs`
- `Tests/Ecom.IntegrationTests/Application/CqrsArchitectureTests.cs`
- `Tests/Ecom.IntegrationTests/Catalog/CatalogApiFactory.cs`
- `Tests/Ecom.IntegrationTests/Catalog/CatalogApiAuthorizationTests.cs`
- `document/architecture/catalog/CATALOG-PRODUCT-API-FE-AGENT-GUIDE.md`

**Required verification:** Catalog unit/architecture tests, API build, authorization and response-contract tests, plus the Catalog PostgreSQL suite. API acceptance tests use a PostgreSQL fixture and must not be treated as passed when skipped.

**Current sub-boundaries:** Product/Product Variant lifecycle, Product Options, Categories, Product Media, and category query splits are source-complete. The CQRS architecture verifier allows no grouped Catalog handler files. Auth PasswordManagement and Demo QR are intentionally outside this Catalog change-set.

## CS-06: Auth V2 password-login transaction correction

**Scope:** the Password Login orchestration and its independent failed-login audit mutation.

- `Core/Ecom.Application/Features/AuthV2/Login/PasswordLoginCommand.cs`
- `Core/Ecom.Application/Features/AuthV2/Login/CompletePasswordLoginCommand.cs`
- `Core/Ecom.Application/Features/AuthV2/Login/RecordFailedPasswordLoginCommand.cs`
- Auth-specific cases in `Tests/Ecom.IntegrationTests/Application/UnitOfWorkBehaviorTests.cs`

**Required verification:** successful commit, handled-failure rollback, exception rollback, nested transaction ownership, and PostgreSQL session/credential evidence. Refresh/logout/replay remain a later Auth V2 batch.

**Do not include:** PasswordManagement grouping refactor, CORS, JWT configuration, or roles/policy changes.

## CS-07: Presentation and runtime configuration

**Scope:** controller base behavior, error mapping, and runtime settings that have public/security impact.

- `Presentation/Ecom.API/Controllers/BaseController.cs`
- `Presentation/Ecom.API/Middleware/ErrorHandlingMiddleware.cs`
- `Presentation/Ecom.API/appsettings.json`

**Required verification:** route/response characterization tests and approved security review. CORS, trusted proxies, health, metrics, Swagger, HTTPS, and HSTS are intentionally deferred to G4 because they require real deployment topology and access decisions.

## CS-08: Guidance and durable status

**Scope:** architecture rules and tracker/documentation only.

- `AGENTS.md`
- `.agents/skills/backend-api-architecture/SKILL.md`
- `.agents/skills/backend-api-architecture/references/cqrs-unit-of-work-standard.md`
- `.agents/skills/catalog-product-system/SKILL.md`
- `.agents/skills/commerce-system/references/current-status.md`
- `document/architecture/BACKEND-MODERNIZATION-TASKS.md`
- this inventory

**Required verification:** review against live source and the actual test output of the change-set being documented. Documentation cannot mark a runtime gate complete by itself.

## CS-09: Cart, checkout, order, and fulfilment vertical slice

**Scope:** guest and authenticated cart ownership, customer addresses, server-resolved preview, idempotent order creation, inventory reservation, order/payment/shipment transitions, and their forward-only persistence additions.

- `Core/Ecom.Application/Common/Commerce/**`
- `Core/Ecom.Application/Common/Interfaces/ICommerceCheckoutServices.cs`
- `Core/Ecom.Application/Common/Services/CheckoutPricingService.cs`
- `Core/Ecom.Application/Features/Commerce/**`
- `Core/Ecom.Domain/Entities/Commerce/{Customer,Inventory,Ordering,System}/**`
- `Core/Ecom.Domain/Enums/Commerce/CommerceEnums.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce/Ordering/{IdempotencyRecordConfiguration.cs,OrderConfiguration.cs}`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/ApplicationDbContext.cs`
- `Infrastructure/Ecom.Infrastructure/Migrations/{20260806065505_AddCommerceCheckoutIdempotency.*,20260806070313_AddGuestOrderOwnership.*,ApplicationDbContextModelSnapshot.cs}`
- `Infrastructure/Ecom.Infrastructure/Services/{CartPrincipalResolver.cs,IdempotencyStore.cs,InventoryReservationStore.cs,OrderNumberGenerator.cs,ReservationExpiryWorker.cs}`
- `Infrastructure/Ecom.Infrastructure/DependencyInjection.cs`
- `Infrastructure/Ecom.Infrastructure/Seeding/RoleSeeder.cs`
- `Presentation/Ecom.API/Controllers/V1/{CartController.cs,CheckoutController.cs,CustomerAddressesController.cs,ManagementOrdersController.cs,OrdersController.cs}`
- `Presentation/Ecom.API/Program.cs`
- `Tests/Ecom.Domain.Tests/Commerce/{InventoryTests.cs,OrderTests.cs,ProductAndCartTests.cs}`
- `document/api/COMMERCE-CHECKOUT-V1-AGENT-GUIDE.md`
- `.agents/skills/commerce-checkout-order-operations/**`

**Required verification:** solution build, focused Domain/Application tests, API ownership/authorization tests, and PostgreSQL acceptance for idempotency, stock locking, rollback, migration, guest isolation, and lifecycle transitions. A skipped PostgreSQL/API test is not a passing gate.

**Do not combine:** media scanning/storage implementation, Catalog CQRS split, Auth V2 login orchestration, or package remediation.

## Execution order

```text
CS-08 guidance and status
  -> CS-01 dependency verification (already source-complete)
  -> CS-03 dedicated PostgreSQL evidence
  -> CS-02 repository/UoW acceptance and CS-05 Catalog acceptance
  -> CS-04 MediaAsset storage/lifecycle
  -> CS-05 Catalog CQRS bounded splits
  -> CS-06 Auth V2 acceptance
  -> CS-09 Cart-to-Order acceptance
  -> later G4 through G7 change-sets
```

## Current handling decision

`B0.5` is ready for local commit preparation: the manifest is complete and each selected group must be staged independently after its review. Source/local verification does not replace the protected PostgreSQL, runtime, migration-apply, or security gates listed above.
