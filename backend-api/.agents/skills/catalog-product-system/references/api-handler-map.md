# Catalog Product API, Handler, and Service Map

## Public read

| Route | Query/handler | Services | Purpose |
| --- | --- | --- | --- |
| `GET /api/v1/products` | `GetProductListQuery` | effective price + bulk primary media | Public grid/filter/paging |
| `GET /api/v1/products/{slug}` | `GetProductBySlugQuery` | effective price + public media | Canonical product detail |
| `GET /api/v1/categories` | `GetPublicCategoriesQuery` | none | Flat published navigation |

Public controllers: `ProductsController.cs`, `CategoriesController.cs`.

## Backoffice

| Purpose | Query/command | Policy |
| --- | --- | --- |
| Management list/detail | `GetCatalogProductListQuery`, `GetCatalogProductByIdQuery` | `catalog.products.read` |
| Create Product | `CreateProductCommand` | `catalog.products.create` |
| Details/categories/media/variants/prices | commands under `Features/Catalog/Commands` | `catalog.products.update` |
| Submit review/publish/pause | lifecycle commands | `catalog.products.publish` |
| Discontinue | lifecycle command | `catalog.products.discontinue` |

`CatalogProductsController` binds route IDs with `with { ... }` and calls Mediator only.

## Services/interfaces

| Interface | Implementation | Responsibility |
| --- | --- | --- |
| `IEffectivePriceResolver` | `EffectivePriceResolver` | Shared price eligibility/precedence |
| `IProductMediaReader` | `ProductMediaReader` | Safe public URL DTO and bulk primary media |
| `ICatalogProductAccessService` | `CatalogProductAccessService` | Handler policy assertion |
| `ICommerceMediaService` | `CommerceMediaService` | Trusted MediaAsset attach lifecycle |

All register scoped in `Core/Ecom.Application/DependencyInjection.cs`.

## Command sequence

Validate -> authorize -> load Product -> check stamp -> validate relationship ownership -> domain method -> UnitOfWork persistence -> return renewed stamp. Stale stamp maps to `409` via the established `ALREADY_EXISTS` response contract.

Public FE uses slug/public DTOs; backoffice uses Product UUID/management DTOs. FE calls backend HTTPS only and never Azure PostgreSQL directly.
