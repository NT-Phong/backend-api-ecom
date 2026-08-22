# API Catalog V1

Tất cả URL dưới đây tương đối với base URL môi trường. Response nằm trong `ApiResponse<T>`. `Auth` ghi `Public`, `Bearer`, hoặc policy chính. `CSRF` áp dụng cho browser mutation theo controller.

## Security và Authentication

| Method | Route | Auth | Chức năng |
| --- | --- | --- | --- |
| GET | `/api/v1/security/csrf` | Public | phát request token + CSRF cookie |
| POST | `/api/v1/auth/register` | Public | compatibility phone registration |
| POST | `/api/v1/auth/send-otp` | Public | gửi OTP login/register |
| POST | `/api/v1/auth/verify-otp` | Public | xác minh OTP, cấp token/session facts |
| POST | `/api/v1/auth/complete-profile` | Bearer | hoàn thiện profile legacy |
| PATCH | `/api/v1/auth/profile/basic` | `User.Update` | cập nhật full name tối thiểu |
| PATCH | `/api/v1/auth/profile` | `User.Update` | cập nhật profile đầy đủ |
| POST | `/api/v1/auth/refresh-token` | Public | refresh V1 |
| POST | `/api/v1/auth/logout` | Public | revoke refresh/session V1 |
| GET | `/api/v1/auth/me` | `User.Read` | current user V1 |
| POST | `/api/v2/auth/register` | Public | đăng ký Password V2, trả 202 khi accepted |
| POST | `/api/v2/auth/login/password` | Public | login Password V2 |
| POST | `/api/v2/auth/email/verify/confirm` | Public | xác nhận email bằng protected token |
| POST | `/api/v2/auth/password/forgot` | Public | bắt đầu recovery |
| POST | `/api/v2/auth/password/reset` | Public | reset bằng protected token |
| POST | `/api/v2/auth/password/change` | Bearer + CSRF | đổi password |
| POST | `/api/v2/auth/password/setup` | Bearer + CSRF | setup password |
| POST | `/api/v2/auth/refresh` | Public | rotate refresh session V2 |
| GET | `/api/v2/auth/me` | Bearer | current principal V2 |
| GET | `/api/v2/auth/csrf` | Bearer | CSRF token cho authenticated V2 mutation |
| POST | `/api/v2/auth/logout` | Public contract + V2 feature | revoke current/session facts theo body |
| POST | `/api/v2/auth/logout-all` | Bearer | revoke mọi session |
| DELETE | `/api/v2/auth/sessions/{sessionId}` | Bearer | revoke session sở hữu |
| GET/POST | `/api/v1/demo/qr-login/*` | mixed | demo QR handoff, không phải production auth |

## Public catalog

| Method | Route | Query/body | Data |
| --- | --- | --- | --- |
| GET | `/api/v1/products` | `q`, `categorySlug`, `producerId`, `minPrice`, `maxPrice`, `sort`, `page`, `pageSize` | paginated public product items |
| GET | `/api/v1/products/{slug}` | slug | public product detail + variants/media |
| GET | `/api/v1/categories` | none | public categories |
| GET | `/api/v1/categories/{slug}` | slug | public category |
| GET | `/api/v1/catalog/producers` | Bearer + `CatalogProducts.Create`; `q`, paging | verified + published producer picker |
| GET | `/api/v1/catalog/producers/{id}` | Bearer + `CatalogProducts.Create`; id | producer picker detail |

## Catalog management

| Method | Route | Policy | Chức năng |
| --- | --- | --- | --- |
| GET/POST | `/api/v1/catalog/products` | Read/Create | list hoặc tạo Product Draft |
| GET/PUT/DELETE | `/api/v1/catalog/products/{id}` | Read/Update/Discontinue | detail, update, guarded soft-delete |
| PUT | `/catalog/products/{id}/categories` | Update | replace toàn bộ category assignments |
| POST/PATCH/DELETE | `/catalog/products/{id}/media...` | Update | attach/update/primary/remove media |
| POST/PUT | `/catalog/products/{id}/variants...` | Update | create/update variant |
| POST | `/catalog/products/{id}/variants/{variantId}/{pause|activate|discontinue}` | Update | variant lifecycle |
| POST | `/catalog/products/{id}/variants/{variantId}/prices` | Update | append price period |
| POST | `/catalog/products/{id}/{submit-review|publish|pause|discontinue|restore}` | Publish/Discontinue | Product status lifecycle; restore đưa Discontinued về trạng thái hợp lệ |
| GET/POST/PUT/DELETE | `/catalog/products/{id}/options...` | Update | option/value CRUD và variant mapping |
| GET/POST | `/api/v1/catalog/categories` | Read/Create | category list/create |
| GET/PUT/DELETE | `/catalog/categories/{id}` | Read/Update/Deactivate | detail/update/hide |
| GET | `/catalog/categories/tree` | Read | tree |
| POST | `/catalog/categories/{id}/{publish|pause}` | Publish | lifecycle |

## Cart, addresses, checkout và customer orders

| Method | Route | Chức năng |
| --- | --- | --- |
| GET | `/api/v1/cart` | cart hiện tại của guest/user |
| POST | `/api/v1/cart/items` | add variant quantity |
| PATCH/DELETE | `/api/v1/cart/items/{cartItemId}` | change quantity/remove |
| POST | `/api/v1/cart/merge-guest` | merge guest cart sau login |
| GET/POST | `/api/v1/customer/addresses` | list/create address |
| PUT/DELETE | `/api/v1/customer/addresses/{id}` | update/delete address |
| POST | `/api/v1/customer/addresses/{id}/default` | set default |
| POST | `/api/v1/checkout/preview` | server quote + fingerprint |
| GET/POST | `/api/v1/orders` | customer order list/create |
| GET | `/api/v1/orders/{id}` | customer-owned detail |
| POST | `/api/v1/orders/{id}/cancel` | customer cancel theo invariant |
| POST | `/api/v1/orders/{id}/payments/sepay/checkout` | hosted checkout intent |
| POST | `/api/v1/orders/{id}/payments/sepay-vietqr` | VietQR intent |

## Provider payment

| Method | Route | Xác thực | Chức năng |
| --- | --- | --- | --- |
| POST | `/api/v1/payments/sepay/ipn` | `X-Secret-Key` | Hosted Checkout IPN |
| POST | `/api/v1/payments/sepay-bank/webhook` | timestamp + signature trên raw body | bank/VietQR webhook |

## Management

| Nhóm | Route | Chức năng |
| --- | --- | --- |
| Dashboard | `GET /management/dashboard/overview` | KPI tổng hợp |
| Producer | `/management/producers` | list/detail/create/update/verify/publish/hide/contact/facility |
| Inventory | `/management/inventory/levels`, `movements`, `locations` | read/init/adjust/location management |
| Orders | `/management/orders` | list/detail/analytics và lifecycle actions |
| Payment | `GET /management/payments/sepay/reconciliation` | pending/reconciliation view |
| Settings | `GET|PUT /management/settings` | typed standard shipping fee |
| Audit | `GET /management/audit-logs` | audit projection |
| Security | `/management/security/sessions`, `/events` | session/event operations |

## Media

| Method | Route | Chức năng |
| --- | --- | --- |
| POST | `/api/v1/media` | multipart ProductImage upload, tối đa 10 MiB |
| GET | `/api/v1/media/{id}` | metadata/scan state |
| POST | `/api/v1/media/{id}/retry-scan` | retry scan |
| DELETE | `/api/v1/media/{id}` | xóa khi ownership/reference cho phép |
