# Catalog Product Review Checklist

## Trace a request

1. Identify route, query/command, or entity.
2. Inspect controller, validator, handler, service/interface, domain method, EF configuration, migration, nearest test.
3. Search Catalog paths first; expand into Producer, Media, Pricing, or Auth only when the relation requires it.

## Public read

- Anonymous `ApiResponse<T>`/`PaginatedList<T>` contract?
- Product/Producer/primary Category/active Variant/effective price/safe Media facts enforced?
- No management fields, storage keys, raw internal price history, or N+1 loading?
- Page bounds, sort whitelist, `404`, slug-only detail retained?

## Backoffice mutation

- Route ID overrides body ID?
- `ITransactionalRequest` and FluentValidation present?
- HTTP and handler policy both enforced?
- Product stamp and relationship ownership checked?
- Aggregate method—not direct mutation—owns invariant?
- New concurrency stamp returned?

## Persistence and delivery

- Focused configuration, delete policy, and active-row unique constraint correct?
- Price history append-only and PostgreSQL exclusion preserved?
- Migration forward-only with reviewed idempotent SQL?
- PostgreSQL integration test proves relational/concurrency claim; API test proves 401/403/policy success?

## Evidence levels

| Claim | Minimum evidence |
| --- | --- |
| Source implemented | symbols + build |
| Domain invariant | focused domain test |
| Constraint/concurrency | real PostgreSQL integration test |
| Authorization | API anonymous/401/403/success test |
| Staging ready | reviewed migration, staging apply, smoke evidence |

## Fast searches

```powershell
rg -n "GetProductList|GetProductBySlug|GetCatalogProduct|CatalogProducts" Core/Ecom.Application Presentation/Ecom.API
rg -n "IEffectivePriceResolver|IProductMediaReader|ICatalogProductAccessService" Core/Ecom.Application
rg -n "Publish\(|ReplaceCategories|AttachMedia|EnsureContentCanBeChanged|EnsurePricingCanBeChanged" Core/Ecom.Domain/Entities/Commerce
rg -n "ProductCategory|ProductMedia|ProductVariant|VariantPrice" Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations Infrastructure/Ecom.Infrastructure/Migrations
rg -n "Catalog|ProductAndCart|VariantPrice|ProductMedia" Tests
```
