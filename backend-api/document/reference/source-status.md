# Trạng thái chức năng hiện tại

File này là bảng trạng thái cho Agent bên ngoài. Không cần đọc source để biết capability nào đang dùng được. `Implemented` nghĩa contract/backend slice có trong snapshot tài liệu; runtime mỗi môi trường vẫn cần smoke test. `Partial` nghĩa chỉ một phần flow live. `Foundation` nghĩa mới có model/kiến trúc, không được gọi như API.

Danh sách operation, actor, precondition và state effect đầy đủ nằm tại [Danh mục nghiệp vụ đã triển khai](../07-implemented/IMPLEMENTED-BUSINESS-OPERATIONS.md).

## Implemented API slices

| Slice | Source evidence |
| --- | --- |
| Auth V1/V2, sessions, Demo QR, role-policy | Auth/Session/Demo/Identity controllers và Auth tests |
| Public Product/Category | ProductsController, CategoriesController |
| Backoffice Catalog | CatalogProducts/Options/Categories/Producers controllers |
| Media ProductImage | MediaController, media worker/services/tests |
| Cart, address, checkout, order | Cart/CustomerAddresses/Checkout/Orders controllers |
| SePay hosted + VietQR + IPN/webhook/reconcile | Orders, SePayPayments, ManagementSePayPayments |
| Producer, Inventory, Order ops, Dashboard, System | Management controllers và focused tests |

## Capability chi tiết

| Capability | Status | Có thể thiết kế ngay | Không được giả định |
| --- | --- | --- | --- |
| OTP V1 | Implemented | phone OTP login/register/profile/refresh/logout | production SMS luôn reachable |
| Password V2 | Implemented behind effective config | register/login/recovery/change/setup/refresh/me | email delivery/feature flag luôn bật |
| Session/authorization | Implemented | own logout/revoke, management security, role-policy | passkey/OIDC production |
| Public catalog | Implemented | search/filter/sort/list/detail/category | every product has image/price/stock |
| Catalog management | Implemented | product/category/variant/price/media lifecycle | one-shot save-all API |
| Producer management | Implemented core | create/update/verify/publish/hide/contact/create facility | update/delete facility live |
| Product media | Implemented ProductImage | upload/metadata/retry/delete/attach | all intents/video/document live |
| Cart/guest merge | Implemented | guest/user cart and merge | client price or anonymous ownership bypass |
| Address/checkout | Implemented | address CRUD, standard shipping preview | multiple shipping methods/coupon live |
| Order | Implemented | create/read/cancel/customer + management flow | generic status patch/edit order lines |
| SePay Hosted/VietQR | Implemented source, external gate | create intent, IPN/webhook, reconcile | redirect marks Paid, production merchant configured |
| Inventory | Implemented core | locations, zero-level init, adjust, reservation/ship/return | PO/receipt/transfer/stocktake APIs |
| Dashboard/analytics | Implemented core | overview/order analytics | BI warehouse/real-time finance truth |
| Settings | Partial typed | standard shipping fee only | generic config/secrets editor |
| Audit | Partial | read projections | every mutation automatically audited |

## Entity-present nhưng không được coi là API hoàn chỉnh

Promotion/Coupon; Trust/Certification/Trace; Wishlist/Review/Q&A; Content/CMS; TradeInquiry/Partner; Notification; Analytics. Với Agent bên ngoài, các nhóm này mặc định là `Foundation/ROADMAP`; chỉ dùng API khi [API Catalog](../04-api/API-CATALOG.md) và file domain canonical đã bổ sung contract `Implemented`.

Xem [Future Capabilities](../domains/future-capabilities.md) để lên ý tưởng mà không nhầm thành live contract.

## Evidence boundary

Source và tests có mặt không tự chứng minh test đang pass ở checkout hiện tại. PostgreSQL fixture cần database test riêng; Redis, SePay credentials/public webhook, Blob/scanner, BFF/browser, staging và production cần evidence độc lập.
