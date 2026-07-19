# Catalog Product API — Guide for FE and AI Agents

**Status:** implemented V1 contract.  
**Audience:** storefront FE, backoffice FE, and AI agents generating client code.  
**Base URL:** `/api/v1`.

This document describes only endpoints that exist in the current Catalog Product slice. Do not invent upload, category administration, producer administration, product-option, inventory, promotion, cart, or checkout calls from this document.

## 1. Choose the correct API boundary

| FE use case | Use this route family | Authentication | Never use it for |
| --- | --- | --- | --- |
| Customer storefront: browse product cards | `/products` | None | Draft/review products or editing product data |
| Customer storefront: product page | `/products/{slug}` | None | Backoffice editing or lookup by product ID |
| Storefront category navigation | `/categories` | None | Creating/editing categories |
| Staff backoffice: product list/detail/editing | `/catalog/products` | Bearer token plus Catalog policy | Customer-facing product rendering |

Public APIs intentionally expose only purchasable/public data. Backoffice APIs expose draft states, raw price periods and media moderation metadata; they must **never** be called by a public storefront page.

## 2. Common HTTP and response contract

### Headers

- All requests send `Accept: application/json`.
- JSON mutation requests send `Content-Type: application/json`.
- Backoffice requests send `Authorization: Bearer <access-token>`.
- Public endpoints must not require a token.

### Envelope

Every successful or handled-error response uses this shape (properties are camelCase in JSON):

```json
{
  "success": true,
  "data": {},
  "message": "Success",
  "errorCode": null,
  "validationErrors": null,
  "details": null,
  "timestamp": "2026-07-19T10:00:00Z"
}
```

For a failure, do not read `data`; display a suitable message from `message` or field messages from `validationErrors`.

| Status | Meaning for FE | Required client behavior |
| --- | --- | --- |
| `400` | Request validation/domain input failure | Highlight `validationErrors`; do not retry automatically |
| `401` | No usable token | Send user to login / refresh session according to the auth client flow |
| `403` | Logged in but missing Catalog policy | Hide the action and show access-denied state |
| `404` | Product/slug/related entity not found or not public | Show not-found; do not reveal a backoffice reason on storefront |
| `409` | Duplicate slug/SKU or stale `concurrencyStamp` | Refetch management detail, replace local state, let staff reapply changes |
| `500` | Unexpected server error | Show generic failure; do not retry a mutation blindly |

### Pagination

List responses use `data` with:

```json
{
  "items": [],
  "pageNumber": 1,
  "totalPages": 1,
  "totalCount": 0,
  "pageSize": 20,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## 3. Storefront APIs — read only

### `GET /products`

**Purpose:** render a product grid/search-result page. It returns only Published products whose producer, primary category, media, active variants, and effective pricing meet public rules.

| Query parameter | Type | Meaning |
| --- | --- | --- |
| `q` | string, max 200 | Searches product name and short description |
| `categorySlug` | string | Filters by the current implementation's **primary** ProductCategory slug; it does not expand category children |
| `producerId` | UUID | Filters one producer |
| `minPrice`, `maxPrice` | decimal | Filters the product's effective `fromPrice` |
| `sort` | string | `newest` (default), `name-asc`, `price-asc`, `price-desc` |
| `page` | integer | Starts at `1` |
| `pageSize` | integer | `1..50`, default `20` |

Example:

```http
GET /api/v1/products?q=mat%20ong&categorySlug=dac-san&sort=price-asc&page=1&pageSize=12
```

Map `data.items[]` to the product card. `primaryMedia` can be `null`; render a local placeholder, not a storage key.

```json
{
  "id": "uuid",
  "slug": "mat-ong-rung",
  "name": "Mật ong rừng",
  "shortDescription": "...",
  "producer": { "id": "uuid", "code": "HTX-01", "name": "...", "description": null, "websiteUrl": null },
  "primaryCategory": { "id": "uuid", "name": "Đặc sản", "slug": "dac-san", "isPrimary": true, "displayOrder": 1 },
  "primaryMedia": { "mediaAssetId": "uuid", "url": "https://...", "contentType": "image/webp", "altText": null, "caption": null, "displayOrder": 0, "isPrimary": true },
  "fromPrice": 120000,
  "currencyCode": "VND",
  "publishedAt": "2026-07-19T10:00:00Z"
}
```

`fromPrice` is the lowest valid effective price among active variants. It is display data only; it is not a checkout quote.

### `GET /products/{slug}`

**Purpose:** canonical storefront product detail page. Pass the `slug` from list data or the URL. Never pass a product UUID, and do not attempt a slug-history fallback.

```http
GET /api/v1/products/mat-ong-rung
```

The response contains product content, public categories, public clean media, and only Active variants that have an effective price. Each variant includes its current effective `price`, `currencyCode`, `priceType`, weight, and configured option values.

Use this endpoint—not the management detail endpoint—to populate SEO metadata, gallery, variant selector, and public price display.

### `GET /categories`

**Purpose:** category navigation/filter source for the public catalog.

Returns a **flat**, Published category collection sorted by `displayOrder`, then `name`:

```json
{
  "id": "uuid",
  "parentId": null,
  "name": "Đặc sản",
  "slug": "dac-san",
  "description": "...",
  "displayOrder": 1
}
```

Build a tree in FE only if the UI needs one; do not assume that the API validates category-parent cycles. Use a category `slug` as the value passed to `GET /products?categorySlug=...`.

## 4. Backoffice access and concurrency

All routes below begin with `/api/v1/catalog/products` and require a token plus the stated policy.

| Policy | Allows |
| --- | --- |
| `catalog.products.read` | Management list and management detail |
| `catalog.products.create` | Create draft Product |
| `catalog.products.update` | Details, category mapping, media, variant and price mutation; variant lifecycle |
| `catalog.products.publish` | Submit review, publish, pause Product |
| `catalog.products.discontinue` | Discontinue Product |

### Concurrency rule — mandatory

Every Product mutation body contains `concurrencyStamp` from the latest management detail or preceding mutation result.

1. Call `GET /catalog/products/{productId}` before editing.
2. Send its `data.concurrencyStamp` in exactly one mutation.
3. Replace the locally held stamp with `data.concurrencyStamp` from that success response.
4. On `409`, reload management detail. Never resend the old payload automatically.

The Product stamp is the version for all child mutations too: category mapping, media, variants, prices and lifecycle actions.

## 5. Backoffice read APIs

### `GET /catalog/products`

**Policy:** `catalog.products.read`.  
**Purpose:** management table; includes all Product statuses.

Query: `q` (name/slug), `status` (`Draft`, `Review`, `Published`, `Paused`, `Discontinued`), `producerId`, `page`, `pageSize` (`1..50`).

Use `primaryCategory` only as a display summary. Open a product with its UUID using the detail endpoint.

### `GET /catalog/products/{productId}`

**Policy:** `catalog.products.read`.  
**Purpose:** single source of truth for a staff edit screen.

It returns Product fields, `concurrencyStamp`, all category mappings, all attached media metadata, variants, and all price periods. It intentionally does **not** return private storage keys or a staff-side media URL.

## 6. Backoffice write APIs

All examples below omit optional fields only when their value is not needed. UUIDs are JSON strings.

### Product identity and content

| Method and path | Policy | Purpose |
| --- | --- | --- |
| `POST /catalog/products` | create | Creates a `Draft` product |
| `PUT /catalog/products/{productId}` | update | Replaces editable Product details; may change slug if it remains unique |

Create body has no concurrency stamp:

```json
{
  "producerId": "uuid",
  "name": "Mật ong rừng",
  "slug": "mat-ong-rung",
  "shortDescription": "...",
  "description": "...",
  "usageInstructions": null,
  "storageInstructions": null,
  "warningText": null,
  "metaTitle": "...",
  "metaDescription": "..."
}
```

Update uses the same editable fields plus `concurrencyStamp`. The path `productId` is authoritative; do not rely on a body Product ID.

### Categories — replace the entire mapping

`PUT /catalog/products/{productId}/categories` — policy `catalog.products.update`.

```json
{
  "concurrencyStamp": "uuid",
  "categories": [
    { "categoryId": "uuid-primary", "isPrimary": true },
    { "categoryId": "uuid-secondary", "isPrimary": false }
  ]
}
```

This is **replace-all**, not add-one. Always send every category to retain. At least one existing Category is required, IDs cannot repeat, and exactly one mapping must have `isPrimary: true`.

There is no Category create/update endpoint in this Catalog Product API. Obtain valid IDs from the authorized category-management source; public `GET /categories` contains only Published categories and is not an admin editor source.

### Media — attach only already trusted MediaAsset IDs

| Method and path | Meaning |
| --- | --- |
| `POST /catalog/products/{id}/media` | Attach an existing MediaAsset |
| `PATCH /catalog/products/{id}/media/{mediaAssetId}` | Change link display order/caption |
| `POST /catalog/products/{id}/media/{mediaAssetId}/primary` | Make attached media the primary image/media |
| `DELETE /catalog/products/{id}/media/{mediaAssetId}` | Remove the Product-to-media link |

Attach example:

```json
{
  "concurrencyStamp": "uuid",
  "mediaAssetId": "uuid",
  "displayOrder": 0,
  "makePrimary": true,
  "caption": "Mặt trước sản phẩm"
}
```

The referenced asset must already be **Clean + Public** if `makePrimary` is true. This API does not upload a file, scan malware, mark a file clean, or make an asset public. Do not build a staff bypass button for those operations. A Published Product cannot remove its primary media.

For `PATCH`, send `{ "concurrencyStamp": "uuid", "displayOrder": 1, "caption": "..." }`. For primary and delete, send `{ "concurrencyStamp": "uuid" }`; DELETE deliberately has a JSON body in this contract.

### Variants

| Method and path | Meaning |
| --- | --- |
| `POST /catalog/products/{id}/variants` | Create sellable variant; SKU becomes immutable |
| `PUT /catalog/products/{id}/variants/{variantId}` | Update non-SKU details and inventory policy |
| `POST /catalog/products/{id}/variants/{variantId}/pause` | Pause variant |
| `POST /catalog/products/{id}/variants/{variantId}/activate` | Activate variant |
| `POST /catalog/products/{id}/variants/{variantId}/discontinue` | Terminal variant state |

Create example:

```json
{
  "concurrencyStamp": "uuid",
  "sku": "HONEY-500G",
  "name": "Hũ 500g",
  "inventoryMode": "Tracked",
  "allowBackorder": false,
  "barcode": null,
  "weightGrams": 500,
  "displayOrder": 0
}
```

Valid `inventoryMode`: `NotTracked`, `Tracked`, `Preorder`. A discontinued Product or Variant cannot be mutated. Product option/value creation and variant-option mapping are intentionally out of scope; do not generate a combinations UI yet.

### Prices — append history, never edit it

`POST /catalog/products/{id}/variants/{variantId}/prices` — policy `catalog.products.update`.

```json
{
  "concurrencyStamp": "uuid",
  "amount": 120000,
  "priceType": "Public",
  "effectiveFrom": "2026-07-20T00:00:00Z",
  "effectiveTo": null,
  "priceListId": null,
  "currencyCode": "VND",
  "minQuantity": 1
}
```

Rules:

- This appends a `VariantPrice`; there is no update/delete price endpoint.
- `effectiveTo`, when supplied, must be later than `effectiveFrom`.
- `priceListId`, if supplied, must exist.
- The database rejects overlapping periods for the same variant, price list and price type.
- Public storefront uses only valid `Sale` then `Public` VND prices with `minQuantity = 1`; it never shows `B2B` price. A B2B-only variant cannot satisfy Product publish requirements.

### Product lifecycle

Each operation sends only `{ "concurrencyStamp": "uuid" }`.

| Method and path | Policy | Transition and FE usage |
| --- | --- | --- |
| `POST /catalog/products/{id}/submit-review` | publish | `Draft` or `Paused` → `Review`; use when staff finishes content |
| `POST /catalog/products/{id}/publish` | publish | `Review` → `Published`; server validates all public facts |
| `POST /catalog/products/{id}/pause` | publish | `Published` → `Paused`; removes from storefront |
| `POST /catalog/products/{id}/discontinue` | discontinue | Final state; no Product delete endpoint |

Publish succeeds only when all persisted facts are true: Producer is Published and verified; primary Category is Published; primary MediaAsset is Clean + Public; at least one active Variant exists; and an active Variant has an effective eligible Sale/Public price. The UI may pre-check these facts, but must rely on the API result as authoritative.

## 7. Recommended FE workflows

### Storefront

1. Fetch `/categories` for navigation.
2. Fetch `/products` for a grid, filters and paging.
3. Navigate using `item.slug`; fetch `/products/{slug}` for the detail view.
4. Use returned `variant.id` for a future cart API only. Cart APIs are not available in this slice.

### Backoffice create and publish

1. Create Product. Store returned `id` and `concurrencyStamp`.
2. Replace category mappings with one primary category.
3. Obtain a pre-scanned MediaAsset ID from the trusted media workflow; attach a Clean + Public asset as primary.
4. Create one or more active variants.
5. Append at least one effective VND `Public` or `Sale` price with `minQuantity: 1`.
6. Submit for review, then publish.
7. After every success, persist the returned concurrency stamp before the next request.

## 8. Explicit non-goals for this FE slice

- No Product hard-delete.
- No producer self-service or producer/category management endpoints here.
- No media scan/clean/public bypass endpoint.
- No Product option/value or generated variant-combination UI.
- No traceability, review, promotion, inventory availability, cart, checkout, payment, shipment, or external side-effect API.
- Do not send a client-calculated price, stock, discount, or total as an authoritative value.

## 9. AI agent implementation guardrails

- Generate separate `catalogPublicApi` and `catalogBackofficeApi` clients; do not share backoffice DTOs into storefront components.
- Treat UUIDs as opaque strings. Store slug only for public URLs; use Product UUID only for management routes.
- Keep all timestamps as ISO-8601 UTC strings and decimal money values as JSON numbers; render VND locally, for example `120000` → `120.000 ₫`.
- Never log bearer tokens, private media storage keys, or raw backoffice price history in public analytics.
- When a route is absent from this document, stop and request a backend contract instead of guessing its path or payload.

## 10. Source of truth

- Controllers: `Presentation/Ecom.API/Controllers/V1/ProductsController.cs`, `CategoriesController.cs`, `CatalogProductsController.cs`.
- Public DTOs: `Core/Ecom.Application/Features/Catalog/Common/CatalogDtos.cs`.
- Management DTOs/commands: `Core/Ecom.Application/Features/Catalog/Common/CatalogManagementDtos.cs` and `Core/Ecom.Application/Features/Catalog/Commands/`.
- Public eligibility and price behavior: `GetProductListQueryHandler.cs`, `GetProductBySlugQueryHandler.cs`, and `EffectivePriceResolver.cs`.

When this guide conflicts with a running API response, preserve the response as evidence and ask the backend team to reconcile the contract before changing FE behavior.
