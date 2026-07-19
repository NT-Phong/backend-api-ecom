# Catalog Product Current State

Last verified source review: 2026-07-19.

## Delivered source surface

| Area | State | Evidence root |
| --- | --- | --- |
| Public catalog read | Source implemented | `Features/Catalog/Queries/GetProductList`, `GetProductBySlug`, `GetPublicCategories` |
| Backoffice Product lifecycle | Source implemented | `Features/Catalog/Commands` and `CatalogProductsController` |
| Effective price policy | Source implemented | `IEffectivePriceResolver` / `EffectivePriceResolver` |
| Product media public reader | Source implemented | `IProductMediaReader` / `ProductMediaReader` |
| Catalog authorization | Source implemented | `Permissions.CatalogProducts`, HTTP policies, `ICatalogProductAccessService` |
| Variant price overlap constraint | Applied to Azure dev database per EF output | migration `20260719110000` |
| One primary Product media constraint | Applied to Azure dev database per EF output | migration `20260719114222` |

## Last verification evidence

- Domain test command: 31 passed, 0 failed.
- API build command: 0 errors; existing nullable and MessagePack dependency warnings remain.
- User-provided EF output applied both Catalog migrations to `ecom_dev`.

## Known gaps and current behavior

1. `GET /products?categorySlug=...` joins only primary category; direct filtering across every ProductCategory mapping is not implemented.
2. Public/backoffice search use `ToLower().Contains`; planned PostgreSQL `EF.Functions.ILike` is not implemented.
3. Catalog PostgreSQL integration tests for visibility, exclusion constraint, primary media, concurrency, and API authorization are missing.
4. Product option/value and variant-option writes are intentionally deferred.
5. Product delete, producer self-service, category administration, scan-clean bypass, inventory, promotion, cart, checkout, traceability, and external side effects are out of scope.

## Next safe gate

Correct direct category filtering and PostgreSQL search semantics, then add PostgreSQL integration and API authorization coverage before calling the slice staging-ready.
