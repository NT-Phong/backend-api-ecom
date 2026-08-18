# Frontend API Contract

> Nguồn sự thật: controller, request/query, validator và DTO trong source hiện tại. Kiểm tra lần cuối: 2026-08-04. Đây là tài liệu để FE tích hợp, không phải tài liệu mô tả roadmap hay bằng chứng API đã triển khai production.

## 1. Phạm vi và cách dùng

Base URL là URL môi trường do FE cấu hình; mọi URL dưới đây tương đối với base URL. API version nằm trong URL. V1 và V2 là hai contract Auth khác nhau; mỗi màn hình chỉ chọn **một** flow, không trộn access/refresh token giữa chúng.

| Khu vực FE | API đã có | Không suy diễn thành đã có |
| --- | --- | --- |
| Storefront | Public Catalog, OTP V1 | cart, địa chỉ giao hàng, checkout, order, payment, shipment, trade inquiry |
| Backoffice Catalog | product/category/option/variant/price lifecycle | upload file hoặc tạo `MediaAsset` |
| Account/Admin V1 | OTP, profile, user/role management, policy legacy | browser cookie/BFF contract |
| Password V2 | password/session endpoints có feature flag | production email delivery, QR/passkey/Google |

Các phần ``Không suy diễn`` hiện **không có controller trong source**. FE phải giữ mock/BFF nội bộ hoặc chờ contract mới; không gọi trực tiếp PostgreSQL/Azure Blob và không tự tạo URL API giả.

## 2. Quy ước dùng chung

### 2.1 Envelope

Mọi action đi qua `HandleResult` hoặc middleware lỗi đều dùng JSON camelCase theo khung sau:

```ts
type ApiResponse<T> = {
  success: boolean;
  data: T | null;
  message: string | null;
  errorCode: string | null;
  validationErrors: Record<string, string[]> | null;
  details: string | null;
  timestamp: string; // ISO-8601 UTC
};
```

Ví dụ thành công:

```json
{"success":true,"data":{"id":"..."},"message":"Success","errorCode":null,"validationErrors":null,"details":null,"timestamp":"2026-08-04T...Z"}
```

Với mutation trả `TResult` không có data, `data` là `null` và message là `Operation completed successfully`.

### 2.2 HTTP, token, thời gian

| Tình huống | FE xử lý |
| --- | --- |
| API có biểu tượng `Bearer` | gửi `Authorization: Bearer <accessToken>`. 401: xóa session hoặc refresh theo đúng flow đã chọn; 403: ẩn/khóa action, không retry. |
| 400 | đọc `validationErrors` theo từng field; `details` chỉ để fallback. |
| 404 | URL/resource không tồn tại, hoặc Password V2/Demo QR đang bị feature flag tắt. |
| 409 | mutation Catalog bị stale `concurrencyStamp`; GET lại management detail rồi để người dùng quyết định sửa lại. Không tự replay body cũ. |
| 422 | vi phạm invariant nghiệp vụ (ví dụ state/lifecycle không hợp lệ). Hiển thị `message`, sau đó refetch detail nếu là backoffice. |
| 429 | tôn trọng header `Retry-After` nếu có và vô hiệu hoá nút tạm thời. |
| 502/503 | Auth dependency unavailable; hiển thị retry có chủ đích, không coi là sai mật khẩu. |

Enum được serialize dạng chuỗi, phân biệt đúng spelling trong source, ví dụ `Published`, `Paused`, `Sale`, `Tracked`. `DateTime` nhận/trả ISO-8601 UTC (`...Z`). Tiền là JSON number/decimal, không dùng float của UI để tính giá server-side.

### 2.3 Pagination

`GET /products`, `GET /catalog/products`, `GET /catalog/categories` và `GET /auth/users` trả:

```ts
type PaginatedList<T> = {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};
```

Sử dụng `page` (bắt đầu từ 1) và `pageSize` (1..50 ở Catalog). Không gửi tên query cũ như `search`, `categoryId` cho public product, hoặc `sortBy`.

### 2.4 Concurrency cho Catalog management

Mỗi GET management detail trả `concurrencyStamp`. Mọi `PUT`, `PATCH`, `POST` lifecycle và `DELETE` Catalog có thay đổi phải đưa đúng stamp này trong JSON body. ID path được server ghi đè; FE không cần (và không nên tin) `productId`, `categoryId`, `variantId`, `optionId`, `valueId`, `mediaAssetId` trong body.

Sau một mutation thành công, dùng `data.concurrencyStamp` mới cho mutation tiếp theo hoặc GET detail lại. Mọi thay đổi content/variant/option/media/price của product Published đưa product trở về `Review`; UI phải phản ánh điều này thay vì mặc định nó vẫn public.

## 3. Storefront Catalog V1 (anonymous)

### 3.1 Trang danh sách/sản phẩm

| Màn hình | Method / URL | Query | `data` |
| --- | --- | --- | --- |
| Danh sách, tìm kiếm, filter | `GET /api/v1/products` | `q`, `categorySlug`, `producerId`, `minPrice`, `maxPrice`, `sort`, `page`, `pageSize` | `PaginatedList<ProductListItem>` |
| Chi tiết theo slug | `GET /api/v1/products/{slug}` | — | `ProductDetail` |

`sort` chỉ hỗ trợ `newest` (default), `name-asc`, `price-asc`, `price-desc`. Query public chính xác là `q` và `categorySlug` (không phải `search`, `categoryId`). `minPrice <= maxPrice`, giá không âm, `page >= 1`, `pageSize <= 50`.

```ts
type ProductListItem = {
  id: string; slug: string; name: string; shortDescription: string | null;
  producer: ProducerSummary; primaryCategory: CategorySummary | null;
  primaryMedia: ProductMedia | null; fromPrice: number | null; currencyCode: string | null;
  hasEffectivePrice: boolean;
  publishedAt: string;
};
type ProductDetail = {
  id: string; slug: string; name: string; shortDescription: string | null;
  description: string | null; usageInstructions: string | null;
  storageInstructions: string | null; warningText: string | null;
  metaTitle: string | null; metaDescription: string | null;
  producer: ProducerSummary; categories: CategorySummary[]; media: ProductMedia[];
  variants: ProductVariant[]; hasEffectivePrice: boolean; publishedAt: string;
};
type ProducerSummary = { id: string; code: string; name: string; description: string | null; websiteUrl: string | null };
type CategorySummary = { id: string; name: string; slug: string; isPrimary: boolean; displayOrder: number };
type ProductMedia = { mediaAssetId: string; url: string; contentType: string; altText: string | null; caption: string | null; displayOrder: number; isPrimary: boolean };
type ProductVariant = {
  id: string; sku: string; name: string; price: number; currencyCode: string;
  priceType: "Public" | "Sale" | "B2B"; weightGrams: number | null;
  options: { optionId: string; code: string; name: string; valueId: string; value: string }[];
};
```

Public listing/detail trả Product đang `Published` (và chưa bị soft-delete). Ảnh chỉ xuất hiện khi MediaAsset là `Clean + Public`; nếu không có ảnh an toàn, `primaryMedia` là `null` và `media` có thể rỗng. Category không public được trả là `null`/không nằm trong `categories`, nhưng không làm product biến mất. Giá hiệu lực là optional: khi `hasEffectivePrice=false`, `fromPrice`/`currencyCode` là `null` và `variants` có thể rỗng. FE hiển thị placeholder hoặc “Liên hệ”; checkout vẫn phải lấy giá/stock do server xác minh, không suy ra giá từ price period management.

### 3.2 Category public

| Màn hình | Method / URL | `data` |
| --- | --- | --- |
| Menu/category selector | `GET /api/v1/categories` | `PublicCategory[]` |
| Landing category theo slug | `GET /api/v1/categories/{slug}` | `PublicCategory` |

```ts
type PublicCategory = {
  id: string; parentId: string | null; name: string; slug: string;
  description: string | null; displayOrder: number;
};
```

## 4. Backoffice Catalog V1

Tất cả route phần này cần Bearer token và policy ghi trong bảng. `Content-Type: application/json` cho body. `GET` không cần stamp; mọi mutation cần stamp trừ create product/category.

### 4.1 Product quản trị

| Màn hình / action | Method / URL | Policy | Body FE gửi | `data` thành công |
| --- | --- | --- | --- | --- |
| List | `GET /api/v1/catalog/products` | `CatalogProducts.Read` | query bên dưới | `PaginatedList<CatalogProductListItem>` |
| Detail | `GET /api/v1/catalog/products/{productId}` | `CatalogProducts.Read` | — | `CatalogProductManagement` |
| Tạo nháp | `POST /api/v1/catalog/products` | `CatalogProducts.Create` | `CreateProduct` | `ProductManagementResult` |
| Sửa thông tin | `PUT /api/v1/catalog/products/{productId}` | `CatalogProducts.Update` | `UpdateProduct` | `ProductManagementResult` |
| Thay toàn bộ category | `PUT /api/v1/catalog/products/{productId}/categories` | `CatalogProducts.Update` | `ReplaceCategories` | `ProductManagementResult` |
| Gửi duyệt | `POST /api/v1/catalog/products/{productId}/submit-review` | `CatalogProducts.Publish` | `{ concurrencyStamp }` | `ProductManagementResult` |
| Publish | `POST /api/v1/catalog/products/{productId}/publish` | `CatalogProducts.Publish` | `{ concurrencyStamp }` | `ProductManagementResult` |
| Pause | `POST /api/v1/catalog/products/{productId}/pause` | `CatalogProducts.Publish` | `{ concurrencyStamp }` | `ProductManagementResult` |
| Discontinue | `POST /api/v1/catalog/products/{productId}/discontinue` | `CatalogProducts.Discontinue` | `{ concurrencyStamp }` | `ProductManagementResult` |
| Delete compatibility | `DELETE /api/v1/catalog/products/{productId}` | `CatalogProducts.Discontinue` | `{ concurrencyStamp }` | `ProductManagementResult` |

List query: `q`, `status` (`Draft|Review|Published|Paused|Discontinued`), `producerId`, `categoryId`, `sku`, `minPrice`, `maxPrice`, `createdFrom`, `createdTo`, `updatedFrom`, `updatedTo`, `hasActiveVariant`, `hasEffectivePrice`, `hasPrimaryMedia`, `page`, `pageSize`.

```ts
type CreateProduct = {
  producerId: string; name: string; slug: string; shortDescription?: string | null;
  description?: string | null; usageInstructions?: string | null;
  storageInstructions?: string | null; warningText?: string | null;
  metaTitle?: string | null; metaDescription?: string | null;
};
type UpdateProduct = CreateProduct & { concurrencyStamp: string };
type ReplaceCategories = {
  concurrencyStamp: string;
  categories: { categoryId: string; isPrimary: boolean }[]; // >= 1, unique, exactly one primary
};
type ProductManagementResult = { id: string; slug: string; status: string; concurrencyStamp: string };
```

`ProductManagementResult.id` là product ID. `DELETE` không phải hard delete; action dùng command Discontinue, nên UI hiển thị là ngừng kinh doanh. Không đổi slug/name/category/variant/media/price lúc `Discontinued`.

```ts
type CatalogProductListItem = {
  id: string; producerId: string; name: string; slug: string; status: string;
  createdAt: string; updatedAt: string | null; primaryCategory: CategorySummary | null;
};
type CatalogProductManagement = {
  id: string; producerId: string; name: string; slug: string;
  shortDescription: string | null; description: string | null;
  usageInstructions: string | null; storageInstructions: string | null; warningText: string | null;
  metaTitle: string | null; metaDescription: string | null;
  status: string; publishedAt: string | null; unpublishedAt: string | null;
  concurrencyStamp: string; categories: CategorySummary[];
  media: CatalogProductMedia[]; variants: CatalogProductVariant[]; pricePeriods: CatalogVariantPrice[];
};
type CatalogProductMedia = { mediaAssetId: string; originalFileName: string; contentType: string; mediaType: string; visibility: string; scanStatus: string; displayOrder: number; isPrimary: boolean; caption: string | null };
type CatalogProductVariant = { id: string; sku: string; name: string; status: string; inventoryMode: string; allowBackorder: boolean; barcode: string | null; weightGrams: number | null; displayOrder: number };
type CatalogVariantPrice = { id: string; productVariantId: string; amount: number; currencyCode: string; priceType: string; minQuantity: number; effectiveFrom: string; effectiveTo: string | null; priceListId: string | null };
```

### 4.2 Media: chỉ attach asset đã tồn tại

| Action | Method / URL | Body |
| --- | --- | --- |
| Attach | `POST /api/v1/catalog/products/{productId}/media` | `{ concurrencyStamp, mediaAssetId, displayOrder, makePrimary, caption }` |
| Đổi vị trí/caption | `PATCH /api/v1/catalog/products/{productId}/media/{mediaAssetId}` | `{ concurrencyStamp, displayOrder, caption }` |
| Chọn primary | `POST /api/v1/catalog/products/{productId}/media/{mediaAssetId}/primary` | `{ concurrencyStamp }` |
| Gỡ link | `DELETE /api/v1/catalog/products/{productId}/media/{mediaAssetId}` | `{ concurrencyStamp }` |

Tất cả cần `CatalogProducts.Update`, trả `ProductManagementResult`, `displayOrder >= 0`, `caption` tối đa 500 ký tự. `mediaAssetId` phải là asset đã tồn tại và public-usable khi làm primary. Source hiện không có endpoint upload/create `MediaAsset`; FE không gửi multipart/base64 vào các route trên.

### 4.3 Variant, price và option

| Action | Method / URL | Body |
| --- | --- | --- |
| Tạo variant | `POST /api/v1/catalog/products/{productId}/variants` | `{ concurrencyStamp, sku, name, inventoryMode, allowBackorder, barcode, weightGrams, displayOrder }` |
| Sửa variant | `PUT /api/v1/catalog/products/{productId}/variants/{variantId}` | `{ concurrencyStamp, name, barcode, weightGrams, displayOrder, inventoryMode, allowBackorder }` |
| Pause/activate/discontinue variant | `POST .../variants/{variantId}/pause`, `/activate`, `/discontinue` | `{ concurrencyStamp }` |
| Tạo price period | `POST /api/v1/catalog/products/{productId}/variants/{variantId}/prices` | `{ concurrencyStamp, amount, priceType, effectiveFrom, effectiveTo, priceListId, currencyCode, minQuantity }` |
| Lấy options | `GET /api/v1/catalog/products/{productId}/options` | — |
| Tạo option | `POST /api/v1/catalog/products/{productId}/options` | `{ concurrencyStamp, code, name, displayOrder }` |
| Sửa/xóa option | `PUT` / `DELETE` `.../options/{optionId}` | update: `{ concurrencyStamp, name, displayOrder }`; delete: `{ concurrencyStamp }` |
| Tạo value | `POST .../options/{optionId}/values` | `{ concurrencyStamp, value, displayOrder }` |
| Sửa/xóa value | `PUT` / `DELETE` `.../options/{optionId}/values/{valueId}` | update: `{ concurrencyStamp, value, displayOrder }`; delete: `{ concurrencyStamp }` |
| Gán values cho variant | `PUT /api/v1/catalog/products/{productId}/variants/{variantId}/option-values` | `{ concurrencyStamp, optionValueIds }` |

Các action bảng này đều cần `CatalogProducts.Update`. Kết quả: tạo variant trả `{ variantId, productId, concurrencyStamp }`; tạo price trả `{ variantPriceId, productId, concurrencyStamp }`; các action còn lại trả `ProductManagementResult`. Giá yêu cầu `amount >= 0`, `currencyCode` đủ 3 ký tự, `minQuantity >= 1`, và `effectiveTo > effectiveFrom` nếu có. Variant dùng `inventoryMode`: `NotTracked|Tracked|Preorder`; status: `Active|Paused|Discontinued`. Không được dùng `B2B` làm kỳ vọng giá public.

```ts
type ProductOption = { id: string; code: string; name: string; displayOrder: number; values: { id: string; value: string; displayOrder: number }[] };
```

### 4.4 Category quản trị

| Action | Method / URL | Policy | Body |
| --- | --- | --- | --- |
| List | `GET /api/v1/catalog/categories` | `CatalogCategories.Read` | query: `q`, `status`, `parentId`, `hasChildren`, `hasProducts`, `hasPublishedProducts`, `sort`, `page`, `pageSize` |
| Detail | `GET /api/v1/catalog/categories/{categoryId}` | `CatalogCategories.Read` | — |
| Tree | `GET /api/v1/catalog/categories/tree` | `CatalogCategories.Read` | — |
| Create | `POST /api/v1/catalog/categories` | `CatalogCategories.Create` | `{ parentId, name, slug, description, displayOrder }` |
| Update | `PUT /api/v1/catalog/categories/{categoryId}` | `CatalogCategories.Update` | `{ concurrencyStamp, parentId, name, slug, description, displayOrder }` |
| Publish/Pause | `POST .../{categoryId}/publish` / `/pause` | `CatalogCategories.Publish` | `{ concurrencyStamp }` |
| Hide | `DELETE /api/v1/catalog/categories/{categoryId}` | `CatalogCategories.Deactivate` | `{ concurrencyStamp }` |

`status` là `Draft|Published|Paused|Hidden`; `sort` là `displayOrder` (default), `name`, `createdAt` hoặc `updatedAt`; `pageSize` 1..100. `displayOrder >= 0`; `name <= 200`, `slug <= 250`, `description <= 10000`. Create/update trả `CatalogCategoryManagement`; tree trả `{ id, name, slug, status, displayOrder, children }[]` lồng nhau. Hide là ẩn (không khẳng định hard delete).

```ts
type CatalogCategoryManagement = {
  id: string; parentId: string | null; name: string; slug: string; description: string | null;
  displayOrder: number; status: string; concurrencyStamp: string;
  parent: { id: string; name: string; slug: string } | null;
  childrenCount: number; productCount: number; publishedProductCount: number;
  createdAt: string | null; updatedAt: string | null;
};
```

## 5. Product Image upload V1

Các route Media yêu cầu Bearer token và policy riêng. V1 chỉ nhận `ProductImage`; asset mới luôn trả `Pending` và **không có URL public** cho đến khi worker scan/promote thành `Clean + Public`.

| Action | Method / URL | Policy | Request / response |
| --- | --- | --- | --- |
| Upload | `POST /api/v1/media` | `media.upload` | `multipart/form-data`: `file`, `intent=ProductImage`, `altText?`; tối đa 10 MB; trả **201** `MediaAssetResult` |
| Metadata | `GET /api/v1/media/{mediaAssetId}` | `media.read` | Chỉ uploader hoặc `media.manage`; trả intent, target visibility, scan status và metadata, không URL |
| Delete pending/unattached | `DELETE /api/v1/media/{mediaAssetId}` | `media.delete` | Chỉ uploader hoặc `media.manage`; asset đã gắn Product trả 409 `MEDIA_IN_USE` |

`MediaAssetResult` gồm `id`, `originalFileName`, `contentType`, `sizeBytes`, `mediaType`, `visibility`, `scanStatus`, `intendedVisibility`. FE poll metadata sau upload; chỉ gọi endpoint Catalog attach hiện có khi `scanStatus = Clean` và `visibility = Public`.

## 6. Authentication V1: OTP và quản trị legacy

Base: `/api/v1/auth`. Đây là flow phone-first hiện hữu. `POST /register` là compatibility route; client mới dùng `send-otp` cho cả đăng ký và đăng nhập.

| Màn hình/action | Method / URL | Auth | Body/query | `data` quan trọng |
| --- | --- | --- | --- | --- |
| Nhập số điện thoại | `POST /send-otp` | anonymous | `{ phoneNumber, loginSource? }` | `expiresInSeconds`, `canResendAt`, `message`, `isPending`, `status`; `otpCode` chỉ Development |
| Xác thực OTP | `POST /verify-otp` | anonymous | `{ phoneNumber, otpCode }` | token, role/policies, profile state bên dưới |
| Compatibility register | `POST /register` | anonymous | `{ phoneNumber }` | `userId`, `phoneNumber`, `isProfileCompleted`, `status`, `expiresIn`; `testOtp` chỉ Development |
| Basic profile tùy chọn | `PATCH /profile/basic` | `User.Update` | `{ fullName }` | `{ userId, fullName, profileState }` |
| Full profile legacy | `POST /complete-profile` | Bearer | `{ fullName, email?, address?, avatarId? }` | profile completed result |
| Sửa profile | `PATCH /profile` | `User.Update` | `{ fullName?, email?, address?, avatarId? }` | profile result |
| Current user | `GET /me` | `User.Read` | — | `CurrentUser` |
| Refresh V1 | `POST /refresh-token` | anonymous | `{ refreshToken }` | refresh token pair + expiry |
| Logout V1 | `POST /logout` | anonymous | `{ refreshToken, logoutAllDevices?, fcmToken?, sessionId? }` | `null` |
| Delete own account | `DELETE /delete-account` | `User.Delete` | `{ phoneNumber, selectedReasons, otherReasonNote?, otpCode }` | `string` |

`verify-otp` trả:

```ts
type VerifyOtpResult = {
  userId: string; phoneNumber: string; isProfileCompleted: boolean;
  canSkipProfile: boolean; profileState: string; loginStatus: string;
  accessToken: string; refreshToken: string;
  accessTokenExpiresAt: string; refreshTokenExpiresAt: string;
  roleCode: string | null; roleId: string | null; roleName: string | null; policies: string[];
};
type CurrentUser = {
  userId: string; phoneNumber: string; address: string | null; email: string | null;
  fullName: string | null; avatarId: string | null; status: string;
  roleId: string | null; roleCode: string | null; roleName: string | null; policies: string[];
  lastLoginAt: string | null; phoneNumberConfirmed: boolean; emailConfirmed: boolean;
  canSkipProfile: boolean; profileState: string;
};
```

Sau OTP, `LoginStatus = OPTIONAL_BASIC_PROFILE`, `CanSkipProfile = true`, `ProfileState = BASIC_PROFILE_MISSING` nghĩa là FE có thể mở bước nhập tên rồi cho skip; không coi đó là checkout/profile-full đã hoàn thành. OTP Development `0000` không phải production contract.

### 6.1 User/role management V1

| Action | Method / URL | Policy | Body/query |
| --- | --- | --- | --- |
| List users | `GET /api/v1/auth/users` | `UsersManage.Read` | `userId`, `searchText`, `page`, `pageSize` |
| Create user | `POST /api/v1/auth/admin/create-user` | `UsersManage.Create` | `{ fullName, phoneNumber, roleId }` |
| Update user | `PUT /api/v1/auth/admin/users/{id}` | `UsersManage.Update` | `{ fullName, roleId, isActive }` |
| Delete user | `DELETE /api/v1/auth/admin/users/{id}` | `UsersManage.Delete` | — |
| List roles | `GET /api/v1/auth/admin/roles` | `Roles.Read` | — |
| Create role | `POST /api/v1/auth/admin/roles` | `Roles.Create` | `{ name, description?, priority }` |
| Update role | `PUT /api/v1/auth/admin/roles/{id}` | `Roles.Update` | `{ name, description?, priority }` |
| Delete role | `DELETE /api/v1/auth/admin/roles/{id}` | `Roles.Delete` | — |
| Gán role user | `PUT /api/v1/auth/user-role/{id}` | `Roles.AssignRole` | `{ newRoleId }` |

Các API role/policy là security-sensitive: sau khi đổi role/policy, FE không tự giả định token hiện tại đã mang claims mới. Refetch `/me` hoặc buộc re-auth theo product decision/deployed behavior.

### 6.2 Identity policy controller (legacy route riêng)

Controller này **không** nằm dưới `/auth`: vì dùng route mặc định của `BaseController`, URL source là `/api/v1/Identity/...` (ASP.NET route matching không phân biệt hoa/thường, nhưng FE nên ghi URL lowercase nhất quán chỉ sau khi xác nhận deployed gateway).

| Method / URL | Policy | Body/query |
| --- | --- | --- |
| `GET /api/v1/Identity/Policies` | `Roles.Admin` | `moduleName`, `searchText` (và các field phân trang kế thừa `BaseQueryDto` nếu endpoint deployed yêu cầu) |
| `GET /api/v1/Identity/{roleId}/Policies` | `RolePolicies.Read` | — |
| `POST /api/v1/Identity/{roleId}/adjust-role-policy` | `Roles.Admin` | `{ policies: string[] }` (GUID policy IDs) |

## 7. Authentication V2: password/session (feature-flagged)

Base: `/api/v2/auth`. Khi `PasswordAuthenticationV2` tắt, các route có guard trả 404 `PasswordAuthenticationV2Disabled`; vì vậy FE chỉ bật UI V2 khi environment/deployment đã xác nhận flag. Không dùng V2 cho browser-cookie/BFF: source hiện là mobile-token-first.

| Action | Method / URL | Auth | Body | Kết quả |
| --- | --- | --- | --- | --- |
| Register | `POST /register` | anonymous | `{ username, email, password }` | **202** `{ status: "Accepted", developmentVerificationToken? }` |
| Confirm email | `POST /email/verify/confirm` | anonymous | `{ token }` | `null` |
| Password login | `POST /login/password` | anonymous | `{ identifier, password, deviceId, rememberMe }` | session/token pair |
| Refresh | `POST /refresh` | anonymous | `{ refreshToken }` | session/token pair |
| Me | `GET /me` | Bearer | — | session identity |
| Logout one session | `POST /logout` | anonymous | `{ refreshToken, fcmToken?, sessionId? }` | `null` |
| Logout all | `POST /logout-all` | Bearer | `{ refreshToken?, fcmToken?, sessionId? }` | `null` |
| Revoke session | `DELETE /sessions/{sessionId}` | Bearer | — | `null` |
| Forgot password | `POST /password/forgot` | anonymous | `{ email }` | **202** Accepted |
| Reset password | `POST /password/reset` | anonymous | `{ token, newPassword }` | `null` |
| Change password | `POST /password/change` | Bearer + CSRF | `{ currentPassword, newPassword }` | `null` |
| Setup password | `POST /password/setup` | Bearer + CSRF | `{ username, password }` | `null` |
| Get CSRF token | `GET /csrf` | Bearer | — | `{ token }` |

```ts
type PasswordLoginResult = {
  sessionId: string; accessToken: string; refreshToken: string;
  accessTokenExpiresAt: string; refreshTokenExpiresAt: string;
};
type V2Me = {
  userId: string; sessionId: string; username: string | null; role: string | null;
  policies: string[]; idleExpiresAt: string; absoluteExpiresAt: string;
};
```

`username`: 4..32, letter/number/`.`/`_`/`-`, có ít nhất một letter, không có `@`, không toàn số. Password thường 15..128 ký tự; Development có thể cho min 5 theo config. Không log/render lại password, access token, refresh token, verification token. `developmentVerificationToken` chỉ xuất hiện ở Development khi option cho phép; production chưa có email provider/outbox chứng minh delivery.

`/password/change` và `/password/setup` yêu cầu CSRF token: gọi `GET /api/v2/auth/csrf`, sau đó gửi `X-CSRF-TOKEN: <data.token>` và cookie antiforgery do server trả. Đây là contract action hiện tại; toàn bộ V2 vẫn cần feature flag/deployment confirmation trước khi FE public UI.

## 8. Demo QR V1: chỉ Development, không phải sản phẩm

Base `/api/v1/demo/qr-login`, chỉ hoạt động khi `Environment=Development` và `DemoQrLoginOptions.Enabled=true`; ngoài ra trả 404 `DemoQrLoginDisabled`.

| Method / URL | Auth | Mục đích |
| --- | --- | --- |
| `POST /start` | anonymous | khởi tạo demo QR |
| `GET /{id}/status` | anonymous | polling trạng thái demo |
| `POST /{id}/approve` | Bearer | phê duyệt demo |
| `POST /{id}/reject` | Bearer | từ chối demo |
| `GET /{id}/approval-page` | anonymous | HTML demo do API trả |

Không đưa flow này vào storefront production, không coi nó là QR login V2.

## 9. Mapping theo luồng FE

```text
Storefront
  categories -> products (categorySlug/q/page) -> product detail -> [chưa có Cart/Checkout API]

Backoffice Catalog
  GET management detail -> giữ concurrencyStamp
    -> sửa product/category/media/variant/option/price
    -> nhận stamp mới hoặc 409 -> GET detail lại
    -> submit-review -> publish

OTP V1
  send-otp -> verify-otp -> lưu token pair -> GET /me
                         -> OPTIONAL_BASIC_PROFILE ? PATCH /profile/basic : tiếp tục

Password V2 (chỉ khi flag bật)
  register -> confirm email -> login/password -> token pair -> GET /me
                                         -> refresh / logout / session revoke
```

## 10. Checklist trước khi FE merge

- Đặt base URL theo environment; không hard-code URL DB/storage.
- Chuẩn hoá mọi call qua `ApiResponse<T>` và không coi HTTP 200 là đủ nếu `success=false`.
- Store access/refresh token theo security model của FE; tuyệt đối không log token trong console/analytics.
- Dùng đúng casing query: `q`, `categorySlug`, `searchText`, `page`, `pageSize`.
- Cache management detail kèm `concurrencyStamp`; mutation nối tiếp lấy stamp trả về gần nhất.
- Không render hoặc gọi Cart/Checkout/Order/Payment/Shipment/Trade Inquiry như API đã tồn tại.
- Xác nhận Swagger/deployed base URL và feature flags trước UAT; source/build không tự chứng minh runtime hoặc production availability.

## 11. Trace source để cập nhật contract

Khi contract đổi, cập nhật guide từ các boundary sau (không chỉ controller):

- Controller: `Presentation/Ecom.API/Controllers/`
- Catalog request/DTO: `Core/Ecom.Application/Features/Catalog/`
- Auth V1/V2 request/DTO: `Core/Ecom.Application/Features/Auth/`, `Core/Ecom.Application/Features/AuthV2/`
- Envelope/error/serialization: `Core/Ecom.Application/Common/Models/ApiResponse.cs`, `Presentation/Ecom.API/Controllers/BaseController.cs`, `Presentation/Ecom.API/Middleware/ErrorHandlingMiddleware.cs`, `Presentation/Ecom.API/Extensions/ServiceExtensions.cs`
