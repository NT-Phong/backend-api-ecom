# Catalog Product Management — API Report and FE Mapping

**Contract:** current V1 source review, 2026-08-08
**Base URL:** `/api/v1`
**Audience:** Backoffice FE implementing the Product List and Product Editor.

## 1. Outcome

The current backend supports a Product Management page with:

- paged Product list and operational filters;
- Product editor detail, category assignments, media metadata, variants, and price periods;
- a separate option/value read endpoint;
- mutation APIs and a `Draft -> Review -> Published -> Paused` lifecycle;
- optimistic concurrency through a renewed `concurrencyStamp` after every Product mutation.

It does **not** currently provide a Producer management/read API, inventory quantities, a media-library list, media URLs in management detail, or a server-side publish-readiness endpoint. FE must not invent these facts.

## 2. Security and response contract

All Product Management calls require a bearer token. The permission needed depends on the route.

```http
Accept: application/json
Authorization: Bearer <access-token>
```

JSON writes also need:

```http
Content-Type: application/json
```

The application response envelope is:

```json
{
  "success": true,
  "data": {},
  "message": "Success",
  "errorCode": null,
  "validationErrors": null,
  "details": null,
  "timestamp": "2026-08-08T00:00:00Z"
}
```

| HTTP status | FE behavior |
| --- | --- |
| `400` | Render `validationErrors`; keep the form values. |
| `401` | Use the Auth client refresh/login flow. |
| `403` | Hide the feature/action when the policy is absent; show access denied if a deep link is opened. |
| `404` | Show Product/category/media not found and stop the current operation. |
| `409` | Discard automatic retry, refetch Product detail, compare changes, then let the user resubmit with the latest stamp. |
| `422` | Show the domain-rule message, especially lifecycle/publish prerequisite failures. |

## 3. Product Management page: required GET requests

### 3.1 List page — `GET /catalog/products`

**Policy:** `catalog.products.read`
**Purpose:** Product table, status queue, filters, and navigation to the editor.

```http
GET /api/v1/catalog/products?status=Draft&hasPrimaryMedia=false&page=1&pageSize=20
```

| Query parameter | Type | Source behavior | FE control |
| --- | --- | --- | --- |
| `q` | string, max 300 | Case-insensitive name or slug contains search. | Search input. |
| `status` | enum | `Draft`, `Review`, `Published`, `Paused`, `Discontinued`. | Status tabs/filter. |
| `producerId` | UUID | Exact producer ID. | Only show this filter after FE has a valid Producer source; no Producer list endpoint exists today. |
| `categoryId` | UUID | Matches any direct ProductCategory mapping. | Category tree picker. |
| `sku` | string, max 100 | Any variant SKU contains search. | Optional advanced filter. |
| `minPrice`, `maxPrice` | decimal | Filters effective prices at request time. | Price range filter. |
| `createdFrom`, `createdTo` | UTC ISO-8601 | Inclusive creation window. | Date range. |
| `updatedFrom`, `updatedTo` | UTC ISO-8601 | Inclusive update window. | Date range. |
| `hasActiveVariant` | boolean | Product has at least one active variant. | Readiness filter. |
| `hasEffectivePrice` | boolean | Product has at least one current eligible price. | Readiness filter. |
| `hasPrimaryMedia` | boolean | Product has a primary ProductMedia link. It does not prove `Clean + Public`. | Readiness filter, labelled “primary media linked”. |
| `page` | int, minimum 1 | Defaults to 1. | Pagination state. |
| `pageSize` | int, 1–50 | Defaults to 20. | Page-size selector. |

`data` is paginated: `items`, `pageNumber`, `totalPages`, `totalCount`, `pageSize`, `hasPreviousPage`, `hasNextPage`.

Each `items[]` row contains exactly:

```json
{
  "id": "uuid",
  "producerId": "uuid",
  "name": "Product name",
  "slug": "product-slug",
  "status": "Draft",
  "createdAt": "2026-08-08T00:00:00Z",
  "updatedAt": null,
  "primaryCategory": {
    "id": "uuid",
    "name": "Nước mắm",
    "slug": "nuoc-mam",
    "isPrimary": true,
    "displayOrder": 10
  }
}
```

Do not expect description, media, variants, price periods, producer name, inventory quantity, or a `concurrencyStamp` in this list. Open the editor to load those data.

### 3.2 Editor source of truth — `GET /catalog/products/{productId}`

**Policy:** `catalog.products.read`
**Purpose:** Required initial request for `/admin/catalog/products/:id` and the only authoritative refresh after a `409`.

```http
GET /api/v1/catalog/products/00000000-0000-0000-0000-000000000000
```

`data` shape:

```json
{
  "id": "uuid",
  "producerId": "uuid",
  "name": "Product name",
  "slug": "product-slug",
  "shortDescription": null,
  "description": null,
  "usageInstructions": null,
  "storageInstructions": null,
  "warningText": null,
  "metaTitle": null,
  "metaDescription": null,
  "status": "Draft",
  "publishedAt": null,
  "unpublishedAt": null,
  "concurrencyStamp": "uuid",
  "categories": [],
  "media": [],
  "variants": [],
  "pricePeriods": []
}
```

### 3.3 Supporting read APIs

| Request | Policy | Use in Product Management | Returned data |
| --- | --- | --- | --- |
| `GET /catalog/categories/tree` | `catalog.categories.read` | Category picker, one-primary validation, breadcrumb label. Fetch once when opening create/editor. | Recursive `id`, `name`, `slug`, `status`, `displayOrder`, `children`. |
| `GET /catalog/categories/{categoryId}` | `catalog.categories.read` | Optional category detail drawer; not required for normal product editing. | Category management detail and its stamp/counts. |
| `GET /catalog/categories` | `catalog.categories.read` | Category search/list alternative when the tree is too large. | Paginated management categories. |
| `GET /catalog/products/{productId}/options` | `catalog.products.read` | Option/value editor only. Run after Product detail; can be parallel with Category tree. | Options: `id`, `code`, `name`, `displayOrder`, `values[]`. |
| `GET /media/{mediaAssetId}` | `media.read` | Poll an uploaded asset and refresh scan status before attach/set-primary. | `id`, file metadata, `mediaType`, `visibility`, `scanStatus`, `intendedVisibility`. |

Use management routes above, not anonymous `/products` or `/categories`, for staff state. Public routes intentionally hide draft/review content and management metadata.

## 4. Exact editor data mapping

| Editor section | Read source | Fields FE may render/edit | Important boundary |
| --- | --- | --- | --- |
| Basic information | Product detail root | `name`, `slug`, `shortDescription`, `description` | `id`, `status`, timestamps, and stamp are system fields. |
| Usage and safety | Product detail root | `usageInstructions`, `storageInstructions`, `warningText` | Nullable text; do not replace `null` with invented content. |
| SEO | Product detail root | `metaTitle`, `metaDescription` | Nullable; edit through Product details update. |
| Producer | Product detail root/list | `producerId` only | Backend currently supplies no Producer name/detail/list API. FE cannot implement a reliable Producer selector or label from Catalog APIs alone. |
| Lifecycle panel | Product detail root | `status`, `publishedAt`, `unpublishedAt`, `concurrencyStamp` | The stamp is write-only coordination state, never show it as business content. |
| Categories | `categories[]` and Category tree | `id`, `name`, `slug`, `isPrimary`, `displayOrder` | Replacement is whole-collection; exactly one primary. |
| Media gallery | `media[]` | `mediaAssetId`, `originalFileName`, `contentType`, `mediaType`, `visibility`, `scanStatus`, `displayOrder`, `isPrimary`, `caption` | Management DTO has no file URL/storage key. Render preview only from client-selected local file or a separate approved delivery contract. |
| Variants | `variants[]` | `id`, `sku`, `name`, `status`, `inventoryMode`, `allowBackorder`, `barcode`, `weightGrams`, `displayOrder` | No quantity, reserved quantity, or stock location is returned. SKU is immutable after creation. |
| Price periods | `pricePeriods[]` | `id`, `productVariantId`, `amount`, `currencyCode`, `priceType`, `minQuantity`, `effectiveFrom`, `effectiveTo`, `priceListId` | This is management price history. Do not present it as a storefront offer or edit an existing period client-side. |
| Options | Options GET | Option `code`, `name`, ordering; value `value`, ordering | Product detail does not embed the option definitions. |

## 5. FE loading sequence

```text
Product list page
  GET /catalog/categories/tree                 -> cache category labels/tree
  GET /catalog/products?<active filters>       -> render table

Product create page
  GET /catalog/categories/tree                 -> category picker
  Producer selector                            -> blocked: no supported Producer GET endpoint

Product editor page
  GET /catalog/products/{id}                  -> editor source of truth + current stamp
  GET /catalog/categories/tree                -> picker, parallel
  GET /catalog/products/{id}/options          -> option tab, parallel/lazy
  GET /media/{assetId}                        -> only while polling an upload or refreshing one asset
```

Recommended cache keys are separate from public Catalog keys:

```text
catalog-management-products(filter)
catalog-management-product(productId)
catalog-management-category-tree
catalog-management-product-options(productId)
media-metadata(mediaAssetId)
```

Invalidate the Product list and Product detail after a successful Product mutation. Refetch detail after a child-collection mutation so the UI reflects order/state, not merely an optimistic local patch.

## 6. Write APIs required by this page

| Area | Route | Policy | Minimum body fields |
| --- | --- | --- | --- |
| Create draft | `POST /catalog/products` | `catalog.products.create` | `producerId`, content fields. |
| Update details | `PUT /catalog/products/{id}` | `catalog.products.update` | `concurrencyStamp` plus all editable content fields. |
| Replace categories | `PUT /catalog/products/{id}/categories` | `catalog.products.update` | `concurrencyStamp`, complete `categories[]`. |
| Upload image | `POST /media` multipart | `media.upload` | `file`, `intent=ProductImage`, optional `altText`. |
| Attach image | `POST /catalog/products/{id}/media` | `catalog.products.update` | `concurrencyStamp`, `mediaAssetId`, `displayOrder`, `makePrimary`, `caption`. |
| Update/primary/delete image | media PATCH/POST/DELETE product routes | `catalog.products.update` | Current `concurrencyStamp`. |
| Create/update/lifecycle variant | product variant routes | `catalog.products.update` | Current `concurrencyStamp`; SKU only on create. |
| Add price period | `POST /catalog/products/{id}/variants/{variantId}/prices` | `catalog.products.update` | Current stamp, amount, type, UTC time window, VND, min quantity. |
| Manage options | product option/value routes | `catalog.products.update` | Current stamp; replacement sends all final selected option values. |
| Submit review/publish/pause | lifecycle routes | `catalog.products.publish` | Current `concurrencyStamp`. |
| Discontinue | `POST /catalog/products/{id}/discontinue` or `DELETE /catalog/products/{id}` | `catalog.products.discontinue` | Current `concurrencyStamp`. |

## 7. Concurrency protocol

Every mutation after create uses exactly one current Product stamp.

```text
GET detail
  -> store data.concurrencyStamp
  -> send one mutation with that stamp
  -> take response.data.concurrencyStamp as the next stamp
  -> refetch detail after collection changes or a 409
```

Serialise actions per Product. For example, wait for attach-media to return before adding a price or submitting review. A response from successful Product mutation is:

```json
{
  "id": "uuid",
  "slug": "product-slug",
  "status": "Review",
  "concurrencyStamp": "new-uuid"
}
```

For variant/price writes, the response carries the child ID and the renewed Product stamp. Replace the editor stamp with that returned value.

## 8. Publish readiness and lifecycle

The permitted Product state transitions are:

```text
Draft -> Review -> Published -> Paused -> Review
                         \
                          -> Discontinued
```

`Discontinued` is terminal. Any successful details/category/media/variant/price mutation on a Published Product returns it to `Review`; show this consequence before saving.

Enable the **Publish** button only after this client-side checklist is visibly confirmed:

1. Product status is `Review`.
2. Its Producer is `Published` and verified. This cannot be proved from current Product Management GET APIs.
3. Exactly one primary category exists and its Category status is `Published`.
4. One primary media item has `visibility = Public` and `scanStatus = Clean`.
5. At least one variant is `Active`.
6. At least one variant has a currently effective eligible price.

The server repeats these checks on `POST /catalog/products/{id}/publish`; client-side readiness is only guidance. There is no dedicated readiness endpoint, so do not infer readiness from `hasPrimaryMedia=true` or from the public product list.

## 9. Backend gaps that must remain explicit in FE

| Desired UI feature | Current API status | FE handling |
| --- | --- | --- |
| Producer selector/name/profile/verification | No Producer management read route/controller in current source. | Disable the selector or use a separately approved producer contract; do not query PostgreSQL from FE. |
| Stock quantity and availability | Not returned by Product/Variant management DTOs. | Do not show “in stock”, quantity or warehouse controls. |
| Product image URL in backoffice | Not included in management ProductMedia DTO. | Use metadata/status; request a dedicated signed/proxied preview contract if required. |
| Media library browsing | Only media-by-ID GET is available. | Upload a file then poll its returned ID; no “pick existing media” screen. |
| Price edit/delete | No current route. | Append a new time period; do not fake in-place editing. |
| Publish readiness aggregate | No endpoint. | Use the checklist, but retain server response as authority. |
| Product reviews, certification, traceability, promotion, shipping, recommendations | Outside this Catalog management contract. | Keep these sections unavailable/feature-flagged. |

## 10. Acceptance checks for FE

1. A user with only `catalog.products.read` can view the table/detail/options but cannot see mutation controls.
2. A stale second tab gets `409`, refetches detail, and never auto-replays a mutation.
3. Category save sends the complete category collection and exactly one `isPrimary=true`.
4. Image attach is disabled until media metadata says `Clean + Public`.
5. The product Publish button is disabled when a price, active variant, primary media, or producer readiness is missing.
6. Saving a Published Product renders the returned `Review` state and removes it from public-cache assumptions until it is republished.
7. No Product Management request or state store uses direct PostgreSQL, storage keys, public product DTOs, or assumed inventory data.

## 11. Source boundaries

This report is based on the current Catalog controller, queries, management DTOs, lifecycle commands, and media endpoint. It is source-contract documentation; it does not prove PostgreSQL deployment state, media worker availability, permission assignment, or production media storage behavior.
