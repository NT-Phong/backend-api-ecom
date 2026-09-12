# Catalog Core + Discovery — API Contract và hướng dẫn Frontend

## Mục đích

Tài liệu này là contract frontend cho phần nâng cấp Catalog Core + Discovery: Product readiness cho backoffice, public availability không lộ tồn, canonical slug redirect, và public search/suggestions.

## Scope và trạng thái bằng chứng

- Scope: `Catalog Core + Discovery`; không bao gồm promotion, review/Q&A, wishlist, trust/traceability, CMS, import, PriceList management, marketplace hay public backorder.
- Source map: `ProductsController`, `CatalogProductsController`, `SearchController`, `ProductAvailabilityReadService`, `CatalogReadinessService`, `PublicCatalogSearchStore` và migration `20260830130000_AddCatalogDiscoverySearch`.
- Snapshot: change-set ngày 2026-08-30. Solution build và domain tests đã chạy; PostgreSQL migration apply/rollback, `EXPLAIN`, BFF/browser smoke chưa chạy vì chưa có database `*_test` được cấu hình.
- Source, effective configuration và test/runtime evidence luôn ưu tiên hơn tài liệu này.

## Quy ước dùng chung

- Base path: `/api/v1`.
- Response thành công thông thường: `{ success: true, data, message, timestamp }`.
- Enum được JSON hóa thành string PascalCase, ví dụ `Available`, `Published`, `Public`.
- Các lỗi thông thường trả envelope `success: false`. Riêng redirect slug trả HTTP `301` và header `Location`, không có product envelope.
- UUID, tiền và time phải được FE xử lý đúng kiểu: UUID là string; tiền không dùng JavaScript floating-point để tính tổng; time là UTC ISO-8601.
- Product mutation tiếp tục dùng `concurrencyStamp`; khi `409` phải refetch management detail, không replay request cũ.

---

## 1. Thay đổi DTO public Product: `availability`

### Áp dụng cho endpoint cũ

| Endpoint | Thay đổi additive | Ghi chú FE |
| --- | --- | --- |
| `GET /api/v1/products` | `items[].availability` | Dùng cho card/list/filter hint |
| `GET /api/v1/products/{slug}` | `data.availability` | Dùng cho PDP tổng quát |
| `GET /api/v1/products/{slug}` | `data.variants[].availability` | Dùng cho lựa chọn variant có price hiệu lực |

Đây là thay đổi tương thích: không đổi route, request query hay envelope cũ. FE cũ có thể bỏ qua field mới; FE mới phải coi field có thể thiếu khi đang render cached response cũ trong rollout, rồi fallback là `Unavailable` thay vì đoán `Available`.

### Enum và ý nghĩa

| Value | Ý nghĩa server | UI đề xuất | Có được tự chặn add-cart? |
| --- | --- | --- | --- |
| `Available` | Có variant đang bán được; variant tracked có tồn khả dụng ở MAIN | `Còn hàng` | Không. Đây chỉ là hint UX; server quyết định cuối cùng. |
| `OutOfStock` | Có price hợp lệ nhưng tracked variant không có MAIN level hoặc tồn khả dụng `<= 0` | `Tạm hết hàng` | Có thể disable CTA để giảm request thừa, nhưng vẫn phải xử lý server error. |
| `Unavailable` | Không có variant bán được: inactive, không có effective price, hoặc mode chưa hỗ trợ public | `Chưa khả dụng` | Có thể disable CTA; không thay thế quote/checkout validation. |

### Quy tắc tính đã khóa

1. Variant inactive hoặc không có effective public price là `Unavailable`.
2. Variant `NotTracked` có effective price là `Available`.
3. Variant `Tracked` chỉ `Available` khi tổng `availableQuantity` tại stock location active có code `MAIN` lớn hơn 0.
4. Variant `Tracked` không có MAIN hoặc tồn khả dụng bằng/nhỏ hơn 0 là `OutOfStock`.
5. `Preorder` không được public sell trong scope này nên là `Unavailable`.
6. Product là `Available` nếu có một variant `Available`; nếu không mà có variant `OutOfStock` thì là `OutOfStock`; còn lại là `Unavailable`.
7. `allowBackorder=true` **không** đổi trạng thái sang Available và **không** cho checkout vượt tồn trong V1.

### Dữ liệu không bao giờ có trong public DTO

Không đọc hoặc suy ra `stockedQuantity`, `reservedQuantity`, `availableQuantity`, `incomingQuantity`, inventory item/level/movement, private media URL, price period history hay management concurrency stamp từ public endpoint.

### Ví dụ public list item

Phần dưới đây là `data.items[]` của `GET /api/v1/products`:

```json
{
  "id": "7e2c6213-0c15-42dc-9ae4-0e1464fc58b6",
  "slug": "mat-ong-rung-500g",
  "name": "Mật ong rừng 500g",
  "shortDescription": "Đặc sản địa phương",
  "producer": {
    "id": "6175c1aa-8fb9-4e3e-83c9-dc86fb41d52b",
    "code": "HTX-01",
    "name": "HTX A",
    "description": null,
    "websiteUrl": null
  },
  "primaryCategory": {
    "id": "30b96e86-58b9-4596-87be-0a1e6b41d2c4",
    "name": "Mật ong",
    "slug": "mat-ong",
    "isPrimary": true,
    "displayOrder": 0
  },
  "primaryMedia": null,
  "fromPrice": 150000,
  "currencyCode": "VND",
  "hasEffectivePrice": true,
  "availability": "Available",
  "publishedAt": "2026-08-30T00:00:00Z"
}
```

`hasEffectivePrice` và `availability` không đồng nghĩa: Product có price hợp lệ nhưng tracked stock bằng 0 vẫn là `OutOfStock`.

### TypeScript mapping khuyến nghị

```ts
export type CatalogAvailability = "Available" | "OutOfStock" | "Unavailable";

export type PublicProductItem = {
  id: string;
  slug: string;
  name: string;
  shortDescription: string | null;
  fromPrice: number | null;
  currencyCode: string | null;
  hasEffectivePrice: boolean;
  availability: CatalogAvailability;
  publishedAt: string;
  // producer, primaryCategory, primaryMedia theo public contract hiện có
};

export const availabilityUi: Record<CatalogAvailability, { label: string; canAttemptCart: boolean }> = {
  Available: { label: "Còn hàng", canAttemptCart: true },
  OutOfStock: { label: "Tạm hết hàng", canAttemptCart: false },
  Unavailable: { label: "Chưa khả dụng", canAttemptCart: false }
};
```

**Lưu ý bắt buộc:** trạng thái UI không phải authorization hay reservation. Khi add-cart/checkout trả `400`, `409` hoặc `422`, FE luôn phải dùng lỗi server, refetch cart/product nếu cần và không tự chuyển `OutOfStock` thành `Available`.

---

## 2. Canonical Product slug và HTTP 301

### Endpoint

`GET /api/v1/products/{slug}`

| Tình huống | HTTP | Response | FE phải làm gì |
| --- | --- | --- | --- |
| `slug` là canonical của Product Published | `200` | `ApiResponse<ProductDetailDto>` | Render PDP. |
| `slug` là lịch sử của Product hiện Published | `301` | `Location: /api/v1/products/{canonicalSlug}`; không có JSON body bắt buộc | Thay URL browser/router bằng canonical slug, sau đó render canonical response. |
| Product đích Draft/Review/Paused/Discontinued, history bị soft-delete, hoặc slug không tồn tại | `404` | error envelope | Render not-found; không thử redirect từ `SeoRedirect`. |

### Hướng dẫn BFF/Next.js/browser

1. Browser `fetch` thường tự follow redirect; layer data phải đọc `response.url` hoặc canonical slug cuối cùng nếu cần cập nhật route.
2. BFF/proxy không được tự đổi `301` thành `200` với nội dung cũ, không sửa `Location` và không làm rơi version `/api/v1`.
3. Nếu client tắt auto-redirect để quan sát response, xử lý `301` trước khi parse JSON:

```ts
const response = await fetch(`/api/v1/products/${encodeURIComponent(slug)}`, {
  redirect: "manual"
});

if (response.status === 301) {
  const location = response.headers.get("location");
  if (!location?.startsWith("/api/v1/products/")) throw new Error("Invalid product redirect");
  router.replace(location.replace("/api/v1/products/", "/products/"));
  return;
}
```

4. Không cache permanently response `404` của slug cũ vì Product có thể được publish lại; tôn trọng cache policy môi trường thay vì tự đặt cache vô hạn.
5. Không tự tạo `SeoRedirect` hay redirect client-side từ slug cũ. `ProductSlugHistory` là nguồn canonical của backend.

---

## 3. Backoffice Product readiness

### Endpoint mới

`GET /api/v1/catalog/products/{productId}/readiness`

| Thuộc tính | Contract |
| --- | --- |
| Auth | Bearer token |
| Policies bắt buộc | `catalog.products.read` **và** `inventory.read` |
| Mục đích | Hiển thị readiness trước khi submit/publish; không phải mutation |
| Không trả | số lượng tồn, inventory ledger, media storage key/private URL |

### Response `200`

```json
{
  "success": true,
  "data": {
    "productId": "7e2c6213-0c15-42dc-9ae4-0e1464fc58b6",
    "canPublish": true,
    "canSell": true,
    "checks": [
      { "code": "PRODUCER_NOT_PUBLIC_OR_VERIFIED", "passed": true },
      { "code": "PRIMARY_CATEGORY_MISSING_OR_NOT_PUBLIC", "passed": true },
      { "code": "PRIMARY_MEDIA_MISSING_OR_NOT_PUBLIC", "passed": true },
      { "code": "ACTIVE_VARIANT_MISSING", "passed": true },
      { "code": "EFFECTIVE_PRICE_MISSING", "passed": true },
      { "code": "TRACKED_VARIANT_MAIN_LEVEL_MISSING", "passed": true }
    ]
  }
}
```

Mã check dùng tên điều kiện lỗi để ổn định cho analytics/translation. FE phải dùng `passed`, không suy luận pass/fail từ tên code.

| Code | `passed=false` nghĩa là | Ảnh hưởng |
| --- | --- | --- |
| `PRODUCER_NOT_PUBLIC_OR_VERIFIED` | Producer chưa Published hoặc chưa verified | Không publish. |
| `PRIMARY_CATEGORY_MISSING_OR_NOT_PUBLIC` | Thiếu primary category Published | Không publish. |
| `PRIMARY_MEDIA_MISSING_OR_NOT_PUBLIC` | Thiếu primary media Public + Clean | Không publish. |
| `ACTIVE_VARIANT_MISSING` | Không có variant Active | Không publish/sell. |
| `EFFECTIVE_PRICE_MISSING` | Không có effective public price | Không publish/sell. |
| `TRACKED_VARIANT_MAIN_LEVEL_MISSING` | Có tracked active variant chưa khởi tạo MAIN level | Không chặn publish; có thể làm `canSell=false` nếu không có variant Available khác. |

### Hành vi FE backoffice

1. Load readiness cùng lúc hoặc ngay sau management detail; chỉ khi user có cả hai policy.
2. Render `canPublish` là gate cho CTA Publish; vẫn hiển thị lỗi domain từ `POST .../publish` vì state/facts có thể đổi sau khi đọc.
3. Render `canSell` là operational warning, không tự thay Product status.
4. Sau mọi mutation Product/category/media/variant/price/inventory liên quan, invalidate readiness và refetch management detail để lấy `concurrencyStamp` mới.
5. `401` → login/refresh theo auth flow; `403` → ẩn panel readiness; `404` → Product không còn tồn tại.

---

## 4. Product search và discovery

### `GET /api/v1/search/products`

Public endpoint, không cần Bearer.

| Query | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `q` | string | Có | Trimmed, 2–100 ký tự. Search Name, ShortDescription, BrandName; không search SKU. |
| `categorySlug` | string | Không | Lọc theo public category chain. |
| `producerId` | UUID | Không | Lọc Producer public + verified. |
| `minPrice` | decimal | Không | `>= 0`, phải `<= maxPrice`. |
| `maxPrice` | decimal | Không | `>= 0`, phải `>= minPrice`. |
| `availability` | enum | Không | `Available`, `OutOfStock`, `Unavailable`. |
| `sort` | string | Không | `relevance` mặc định, `newest`, `price-asc`, `price-desc`. |
| `page` | integer | Không | `>= 1`, mặc định `1`. |
| `pageSize` | integer | Không | `1–50`, mặc định `20`. |

`relevance` chỉ có nghĩa khi có `q`; endpoint này luôn bắt buộc `q`, vì vậy FE không được dùng `/search/products` như API browse trống. Browse hiện tại tiếp tục dùng `GET /api/v1/products`.

#### Response `200`

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "slug": "mat-ong-rung-500g",
        "name": "Mật ong rừng 500g",
        "shortDescription": "Đặc sản địa phương",
        "producer": { "id": "uuid", "code": "HTX-01", "name": "HTX A", "description": null, "websiteUrl": null },
        "primaryCategory": { "id": "uuid", "name": "Mật ong", "slug": "mat-ong", "isPrimary": true, "displayOrder": 0 },
        "primaryMedia": null,
        "fromPrice": 150000,
        "currencyCode": "VND",
        "hasEffectivePrice": true,
        "availability": "Available",
        "publishedAt": "2026-08-30T00:00:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "categories": [{ "id": "uuid", "name": "Mật ong", "slug": "mat-ong", "count": 1 }],
    "producers": [{ "id": "uuid", "name": "HTX A", "slug": null, "count": 1 }],
    "availability": [{ "availability": "Available", "count": 1 }]
  }
}
```

#### Facet semantics quan trọng

- Mỗi facet count được tính sau **tất cả** filter đang gửi, gồm text, category, producer, price và availability.
- Khi FE đã gửi `availability=Available`, availability facet thông thường chỉ còn `Available`; đây là hành vi đúng, không phải backend thiếu count của các trạng thái khác.
- `categories` có thể có nhiều category cho một Product; count là số Product distinct.
- Không lấy facet ở page hiện tại để tự tính; phải dùng các mảng facet do server trả.
- `items` là public product item; không thêm SKU/variant price periods hoặc tồn bằng client join.

#### Luồng FE khuyến nghị

1. Debounce input 250–400 ms; chỉ gọi khi `q.trim().length >= 2`.
2. Dùng `URLSearchParams`, không tự nối string query; reset `page=1` khi đổi bất kỳ filter/sort.
3. Cancel request cũ bằng `AbortController` hoặc query-library cancellation để tránh response cũ ghi đè query mới.
4. Key cache phải chứa toàn bộ `q`, filter, sort, page và pageSize.
5. `400` do query invalid: hiển thị validation/message, không retry. `200` với `items=[]` là no-result bình thường.
6. Dùng server result cho thứ tự relevance; không sort lại `items` theo name ở client.

### `GET /api/v1/search/suggestions`

| Thuộc tính | Contract |
| --- | --- |
| Auth | Public |
| Query | `q` bắt buộc, 2–100 ký tự |
| Giới hạn | tối đa 8 kết quả |
| Data | `slug`, `name`, `primaryMedia` public/clean hoặc `null` |
| Không có | price, availability, SKU, inventory, analytics hay personalized ranking |

Ví dụ `data`:

```json
[
  {
    "slug": "mat-ong-rung-500g",
    "name": "Mật ong rừng 500g",
    "primaryMedia": null
  }
]
```

Suggestion click phải điều hướng bằng `slug` qua PDP canonical; không dùng name để tạo route.

---

## 5. TypeScript contract cho search

```ts
export type ProductSearchSort = "relevance" | "newest" | "price-asc" | "price-desc";

export type SearchFacet = {
  id: string;
  name: string;
  slug: string | null;
  count: number;
};

export type AvailabilityFacet = {
  availability: CatalogAvailability;
  count: number;
};

export type ProductSearchResult = {
  items: PublicProductItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  categories: SearchFacet[];
  producers: SearchFacet[];
  availability: AvailabilityFacet[];
};

export type ProductSuggestion = {
  slug: string;
  name: string;
  primaryMedia: ProductMedia | null;
};
```

Do `ProductSearchResult` không dùng shape `PaginatedList` cũ, FE phải đọc `page` thay vì `pageNumber`, và tự tính `totalPages = Math.ceil(totalCount / pageSize)` khi cần.

---

## 6. Audit và invalidation đối với FE/backoffice

- Các mutation Catalog được ghi vào `AuditLog` với `entityName=CatalogProduct` và `entityId=productId`, kể cả thay đổi child như media/variant/price.
- Đây không phải public stream và không làm thay đổi response mutation hiện có.
- Nếu backoffice có màn audit dùng endpoint management audit hiện hữu, filter bằng Product UUID với `CatalogProduct`; không dùng Product slug làm audit key.
- Sau mutation success, invalidation tối thiểu:
  1. management detail/list Product;
  2. readiness của Product;
  3. public list/detail/search cache nếu mutation đã đi qua publish/review/pause hoặc canonical slug đổi;
  4. route theo slug cũ/canonical slug mới.

## 7. Checklist tích hợp FE

- [ ] Nâng public product types để bắt buộc xử lý `availability`.
- [ ] Không public hoặc hiển thị số tồn từ management DTO ở storefront.
- [ ] Không coi `Available` là reservation; vẫn xử lý kết quả add-cart/checkout server.
- [ ] Thêm 301-aware PDP/BFF routing, giữ `/api/v1` trong `Location`.
- [ ] Chỉ gọi readiness từ backoffice khi có cả hai policies; 403 không retry.
- [ ] Thêm search page dùng endpoint mới; browse page vẫn dùng `/products`.
- [ ] Dùng server facets, không tính từ items page.
- [ ] Debounce/cancel suggestion và search request; encode mọi query value.
- [ ] Sau mutation, invalidate readiness, management caches, public caches và slug route phù hợp.
- [ ] QA các case: Available, OutOfStock, Unavailable; old slug 301; 404; invalid `q`; no-result; category/producer/price/availability filters; 401/403 readiness.

## Liên kết liên quan

- [API conventions](API-CONVENTIONS.md)
- [API Catalog V1](API-CATALOG.md)
- [Catalog domain](../domains/catalog.md)
- [Request/response examples](REQUEST-RESPONSE-EXAMPLES.md)
