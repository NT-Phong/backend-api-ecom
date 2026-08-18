# Báo cáo thiết kế trang Admin Management cho FE

> Phạm vi: `Source_Ecom/backend-api`, source snapshot ngày 2026-08-17.
> Mục tiêu: xác định các trang FE nên xây, popup/chức năng đi kèm, hợp đồng API hiện có và backlog API cần bổ sung.

## 1. Cách đọc báo cáo

| Nhãn | Ý nghĩa cho FE |
| --- | --- |
| **NOW** | Có controller/read model phù hợp; có thể làm UI thật sau khi kiểm tra quyền và môi trường. |
| **ACTION-ONLY** | Có mutation nhưng thiếu list/detail quản trị. Không đưa vào navigation production độc lập; chỉ feature-flag hoặc chờ API đọc. |
| **MODEL-ONLY** | Entity/EF relation có trong dự án nhưng chưa có Admin API/use case. Không dựng UI dùng mock như thể đã tích hợp. |
| **FUTURE** | Cần cả thiết kế use case, policy, API/read model, test PostgreSQL và UI. |

`ApiResponse<T>` là envelope chung: `success`, `data`, `message`, `errorCode`, `validationErrors`, `details`, `timestamp`. Enum trả về string, thời gian dùng UTC. FE phải unwrap `data` một lần, giữ `errorCode` và `validationErrors` để hiển thị lỗi trường.

## 2. Kết luận điều hướng

Sidebar nên mở rộng từ ảnh tham chiếu thành các **workspace rõ mục đích**, nhưng chỉ activate route khi nhãn là NOW. Những nhóm chưa có read model không được hiển thị như một page vận hành hoàn chỉnh; chỉ dùng làm group header, menu bị khoá có giải thích, hoặc feature flag cho môi trường demo.

```text
Tổng quan
  Dashboard vận hành                  MODEL-ONLY -> coming soon, không hiển thị KPI giả

Catalog
  Không gian Catalog                  MODEL-ONLY -> group header / dashboard sau này
  Sản phẩm                            NOW
  Danh mục                            NOW
  Media                               Picker NOW trong Product; Media Library MODEL-ONLY
  Nhà sản xuất                        MODEL-ONLY (picker chọn producer có NOW)
  Cơ sở sản xuất / điểm bán           MODEL-ONLY
  Bảng giá                            MODEL-ONLY
  Khuyến mãi / mã giảm giá            MODEL-ONLY

Vận hành thương mại
  Đơn hàng                            ACTION-ONLY -> chưa mở sidebar production
  Đối soát thanh toán                 NOW (SePay chỉ đọc)
  Kho & vị trí kho                    MODEL-ONLY
  Giao vận / thực hiện đơn            ACTION-ONLY -> chỉ đến từ Order sau khi có read model
  Khách hàng / CRM                    MODEL-ONLY
  Đánh giá & Hỏi đáp                  MODEL-ONLY
  B2B / đối tác                       MODEL-ONLY
  Chứng nhận & truy xuất              MODEL-ONLY

Nội dung & tuỳ chỉnh
  Trang / block                        MODEL-ONLY
  Bài viết                             MODEL-ONLY
  Campaign / banner                    MODEL-ONLY
  Điều hướng website                   MODEL-ONLY
  SEO redirect                         MODEL-ONLY
  Theme / giao diện                    FUTURE
  Tuỳ chỉnh checkout                   FUTURE

Cấu hình & cài đặt
  Tổng quan cài đặt                    MODEL-ONLY
  Hồ sơ cửa hàng / localization        MODEL-ONLY
  Checkout / thanh toán / fulfillment  MODEL-ONLY
  Kho / notification / security        MODEL-ONLY
  Tích hợp / retention                 MODEL-ONLY

Hướng dẫn & hỗ trợ
  Help Center / SOP                    FUTURE
  Onboarding wizard                    FUTURE
  Release notes                         FUTURE
  Customer support cases               FUTURE

Quản trị truy cập
  Không gian truy cập                  PARTIAL NOW: landing, không có metrics API
  Người dùng                           NOW
  Vai trò & policy                     PARTIAL NOW: chỉ bật sau khi xử lý blocker policy
  Phiên / security event / audit       MODEL-ONLY

Nhân sự
  Nhân viên / tổ chức / tuyển dụng     FUTURE
  Ca làm / chấm công / nghỉ phép       FUTURE
  Payroll / onboarding                 FUTURE

Báo cáo
  Sales / marketing / operations       MODEL-ONLY
  Export / scheduled report            MODEL-ONLY
```

### 2.1 Quyết định hiển thị menu cho FE

| Cách hiển thị | Áp dụng | Lý do |
| --- | --- | --- |
| **Route hoạt động** | Sản phẩm, Danh mục, Người dùng; Đối soát SePay ở phạm vi read-only. | Có controller/read model source hiện hữu. Vẫn kiểm tra policy và môi trường lúc tích hợp. |
| **Embedded action, không có route riêng** | Media picker trong Product; action order/shipment/payment từ context nếu backend trả action hợp lệ. | Có API hẹp hoặc mutation lẻ, nhưng thiếu library/list/detail vận hành. |
| **Disabled / coming soon có mô tả** | Kho, producer, bảng giá, promotion, CMS, settings, audit, analytics. | Entity/EF model có nhưng thiếu handler/controller/read model. Không gọi đây là chức năng đã triển khai. |
| **Ẩn hoàn toàn ở production** | Theme, Help Center, Support Case, toàn bộ HR. | Cần model/use case/migration hoặc policy chưa được phê duyệt. |

Không tạo dashboard rỗng chỉ vì có card “Không gian Catalog”, “Không gian truy cập” hay “Cài đặt”. Khi chưa có API KPI/configuration read model, card chỉ là group header hoặc route `coming-soon` không có số liệu và không làm giả khả năng lưu dữ liệu. Mục **Cấu hình & cài đặt** không được chứa secret deployment như JWT, connection string, payment key hoặc OTP bypass; chi tiết ở mục 16.

## 3. System design và quan hệ dữ liệu cần FE hiểu

```text
User -- Role -- RolePolicy -- Policy
  |       \-- UserPolicy (override, expiry)
  |-- CustomerProfile / CustomerAddress / UserSession / SecurityEvent

Producer -- Product -- ProductVariant -- VariantPrice
              |  |          |                 \-- PriceList
              |  |          \-- InventoryItem -- InventoryLevel -- StockLocation
              |  |                                 |-- InventoryReservation -- OrderItem
              |  \-- ProductCategory -- Category  \-- InventoryMovement
              \-- ProductMedia -- MediaAsset

Cart -- CartItem -- ProductVariant
Order -- OrderItem (immutable product/price snapshots)
      |-- OrderStatusHistory / OrderNote / OrderDiscount
      |-- Payment -- PaymentTransaction / gateway-or-bank attempts + notifications
      \-- Shipment -- ShipmentItem / ShipmentHistory
```

Các cụm entity đã có nhưng chưa có API quản trị gồm:

- `Promotion`, `Coupon`, `CouponProduct`, `CouponCategory`, `CouponRedemption`.
- `Producer`, `ProducerContact`, `ProductionFacility`, `PointOfSale`, `PointOfSaleProduct`.
- `InventoryItem`, `InventoryLevel`, `InventoryMovement`, `InventoryReservation`, `StockLocation`.
- `Banner`, `Campaign`, `Article`, `Page`, `PageSection`, `NavigationItem`, `SeoRedirect`.
- `Certification`, `TraceProfile`, `TraceLot`, `TraceEvent`, `ProductReview`, `ProductQuestion`, `ProductAnswer`.
- `TradeInquiry`, `TradeInquiryItem`, `TradeInquiryStatusHistory`, `PartnerApplication`.
- `AnalyticsEvent`, `VisitorSession`, `AuditLog`, `SystemSetting`.

Quan hệ quan trọng: giá mua/bán phải gắn với **variant**, không gắn trực tiếp Product; `OrderItem` lưu snapshot lịch sử nên UI Order không được lấy lại tên/giá hiện tại của Catalog để thay cho snapshot; tồn kho khả dụng là dữ liệu `InventoryLevel` (stocked/reserved/incoming), không phải trường Product.

## 4. Quy tắc UX chung bắt buộc

1. **Permission-first navigation.** Chỉ hiện route/action khi token có policy tương ứng; backend vẫn là nguồn quyết định cuối cùng. Không dùng role name để tự cho phép action.
2. **Catalog mutation queue.** Mọi mutation Product/Category mang `concurrencyStamp`. Lưu stamp mới từ mọi response; chạy tuần tự theo Product. Khi HTTP 409: dừng queue, reload detail, hiện modal so sánh/reapply thủ công; không auto retry body cũ.
3. **Không hiển thị secret.** Password, OTP key, refresh token, JWT, webhook secret, storage key, raw payment payload không vào UI/log FE.
4. **Confirm destructive/financial actions.** Discontinue/hide/delete, refund, cancel, stock adjustment, change role/policy phải có confirmation modal: bản ghi, tác động, lý do bắt buộc nếu backend yêu cầu và người thực hiện.
5. **Timeline/Audit design.** Mỗi flow có status chip, UTC được đổi sang timezone người dùng, actor, reason, timestamp. Hiện chưa phải mọi aggregate có read API timeline; chỉ xây khi DTO có mặt.
6. **Không giả data.** Thiếu field/API thì render unavailable state/feature flag, không dựng stock, promotion, review count, preview URL hoặc readiness giả.

## 5. Trang triển khai ngay: Catalog

### 5.1 `/admin/catalog/products` — Danh sách sản phẩm (**NOW**)

| Thành phần | Thiết kế FE | API/source hiện có |
| --- | --- | --- |
| Bảng chính | Name, slug, Producer ID/summary khi DTO có, status, created/updated, phân trang; filter/search theo contract list. | `GET /api/v1/catalog/products`, policy `catalog.products.read`. |
| Toolbar | Tạo sản phẩm; search; filter trạng thái; refresh. Bulk lifecycle chỉ thêm sau khi có bulk endpoint. | `POST /api/v1/catalog/products`, policy `catalog.products.create`. |
| Row actions | Mở detail. Chỉ hiển thị lifecycle actions trong detail để luôn có `concurrencyStamp`. | Detail có `GET /catalog/products/{id}`. |
| Empty/error | Empty catalog, forbidden, expired token, retry. | Envelope chuẩn. |

**Popup:** không cần popup tạo nhanh. Nút “Tạo sản phẩm” mở route `/new`, không modal, vì tạo Product cần nhiều bước liên quan.

### 5.2 `/admin/catalog/products/new` — Khởi tạo Product Draft (**NOW**)

Mục tiêu là tạo identity của Product trước, nhận `id`, `status=Draft`, `concurrencyStamp`, rồi chuyển ngay sang editor. Producer chọn từ modal picker `GET /api/v1/catalog/producers` và chỉ trả Producer `Published && IsVerified`.

**Popup ProducerPicker:** tìm theo tên/mã, phân trang, hiển thị tên/nhận diện; chọn một Producer. Không tạo/sửa Producer tại đây vì chưa có Producer management API.

### 5.3 `/admin/catalog/products/:id` — Product Editor (**NOW**)

Không có endpoint “Save all”; đây là wizard/tab tuần tự, mỗi tab là mutation riêng.

| Tab | Nội dung và hành động | Popup / guard |
| --- | --- | --- |
| Tổng quan | Name, slug, mô tả, Producer; lưu `PUT /catalog/products/{id}`. | Slug conflict/validation; confirm nếu sửa Published Product vì có thể về Review. |
| Danh mục | Danh sách category, chọn đúng một primary, save replace. | `CategoryTreePicker`; chặn submit khi không có category hoặc nhiều primary. |
| Options | CRUD Option/Value, reorder nếu contract cho phép, gán values cho Variant. | Confirm khi xóa value đang được Variant dùng; backend là authority. |
| Variants | SKU, barcode, name, weight, inventory mode, backorder, trạng thái. Tạo/sửa/pause/activate/discontinue. | `VariantEditor`; `DiscontinueVariantConfirm`. SKU duplicate là field error. |
| Giá | Danh sách periods của Variant: amount, currency, type (`Public`, `Sale`, `B2B`), min qty, effective window, PriceList. | `PricePeriodForm`; overlap/eligibility error. Hiện có **create**, không có edit/delete period: disable actions đó. |
| Media | Upload multipart rồi poll metadata; attach/remove, caption/order, đặt primary. | `MediaUploadDialog`, `MediaScanProgress`, `SetPrimaryConfirm`, `RemoveMediaConfirm`. Không tự dựng Blob URL. |
| Review & publish | checklist facts thực có: Producer hợp lệ, primary category Published, primary media `Clean + Public`, active Variant, effective eligible price. Submit Review/Publish/Pause/Discontinue. | `PublishReadinessDialog`; chỉ là FE checklist, không thay backend validation. `DiscontinueConfirm` bắt buộc. |

**Media workflow:** `POST /api/v1/media` với `ProductImage`, sau đó poll `GET /api/v1/media/{id}` tới khi `Clean + Public`, dùng **stamp mới nhất** để attach. Nếu scan failed, cho retry chỉ khi metadata báo được retry. Upload hiện giới hạn 10 MiB. Media metadata của admin không phải public URL.

### 5.4 `/admin/catalog/categories` — Danh sách/cây Category (**NOW**)

| Thành phần | Hành vi |
| --- | --- |
| Tree + list | Dùng `GET /api/v1/catalog/categories/tree`; list/filter dùng `GET /catalog/categories`. |
| Create | Route detail mới hoặc `CategoryEditor` side panel, mặc định Draft. |
| Status | Chip Draft/Published/Paused/Hidden. Hidden là terminal, không render Restore. |
| Actions | Mở detail; Publish/Pause/Hide chỉ khi có newest stamp. |

### 5.5 `/admin/catalog/categories/:id` — Category Editor (**NOW**)

Fields: name, slug, description, display order, parent. Hiển thị parent, child count, product count/published product count nếu DTO trả về.

**Popup ParentCategoryPicker:** tree searchable, không chọn chính nó/descendant. `PublishConfirm`, `PauseConfirm`, `HideConfirm` phải mô tả dependency: ancestor/cháu/sản phẩm primary có thể khiến backend từ chối.

### 5.6 Media trong Sidebar — triển khai đúng trạng thái

`/admin/catalog/media` **chưa đủ để là Media Library**: backend chỉ upload, read metadata theo ID, retry scan và delete; không có list/search/filter/pagination, không có public preview URL. Trong P0, item “Media” mở **Media workflow guide** hoặc redirect Product Editor > Media; không tạo grid asset giả. Khi có `GET /media` management list, mới mở Media Library thật.

## 6. Trang triển khai ngay: Access Administration

### 6.1 `/admin/access` — Không gian truy cập (**PARTIAL NOW**)

Là landing/navigation cho User và Role/Policy. Không đưa KPI active users, pending access request hay security alerts vì API dashboard chưa tồn tại.

### 6.2 `/admin/access/users` — Người dùng (**NOW, với lưu ý quyền**)

| Thành phần | Hành vi |
| --- | --- |
| List/search | Phone, name, email, status, Role, verified flags, last login, pagination. `GET /api/v1/auth/users`. |
| Create | Modal tạo thành viên: full name, phone, role. User được tạo Active; không hiển thị hay tự phát OTP/password. |
| Edit | Drawer/modal đổi tên, role, active/deactivated. Lưu `PUT /api/v1/auth/admin/users/{id}`. Backend revoke legacy refresh token khi role/status đổi. |
| Delete | Confirm modal, nêu rõ không thể đảo ngược. Không cho xóa SuperAdmin; UI vẫn phải dựa vào 403/400 backend. |
| Assign role | Nên hợp nhất vào Edit, tránh hai flow mâu thuẫn (`UpdateUser` và `user-role/{id}`). |

**Blocker:** các handler create/update/list/assign kiểm tra chính xác role code `ADMIN`, trong khi seeder cũng có `SYSTEM_ADMIN`. FE không được coi SystemAdmin là thay thế hoàn toàn Admin cho tới khi backend thống nhất matrix và có test.

### 6.3 `/admin/access/roles` — Vai trò (**PARTIAL NOW**)

List role, create custom role (name, description, priority), update, delete nếu không phải system role và không có User. Form create chỉ tạo Role; policy mặc định đều off.

**Popup:** `RoleFormDialog`, `DeleteRoleConfirm` (hiển thị số User), `RolePriorityHelp` (số nhỏ = quyền cao hơn).

### 6.4 `/admin/access/roles/:id/policies` — Ma trận Policy (**BLOCKED pending backend fix**)

Đề xuất UI: module-grouped checkbox matrix, search policy, diff unsaved, confirm impact, read-only policy code. Current endpoints gồm đọc policy toàn hệ thống, đọc theo role và adjust role policy.

Không release UI này trước khi sửa hai blocker:

1. `IdentityController` dùng policy name `ADMIN`, nhưng policy provider kiểm tra JWT claim `policy` trùng chính xác; `Permissions.GetAll()` không seed policy code `ADMIN`. Route có thể không truy cập được.
2. `RoleSeeder` chạy mỗi startup và chỉ grant policy cho `SYSTEM_ADMIN`, `ADMIN`, `MANAGER`, `USER`; custom role không thuộc bốn code này sẽ bị soft-delete policy ở lần seed sau.

Future contract bắt buộc: version/audit event cho role policy mutation, invalidation/revocation session/token của user bị ảnh hưởng, và API render access matrix. Đây là thay đổi bảo mật, phải có backend acceptance test trước FE release.

## 7. Vận hành thương mại: trạng thái và trang cần có

### 7.1 `/admin/operations/orders` — Order Queue (**ACTION-ONLY**)

Project có các command quản lý nhưng **không có management list/detail GET**. Vì vậy chưa xây queue thật.

Khi API đọc bổ sung, page cần có:

- Search order number, customer, phone, SKU; filter order/payment/fulfillment status, payment method, date range, location; saved views.
- Table: number, placed at, customer snapshot, amount, payment status, fulfillment status, shipment tracking, assignee/flag.
- Bulk selection chỉ dành cho action mà backend có batch endpoint; không loop POST từ FE.
- Row click mở Order Detail; không đưa dữ liệu PII thừa vào list.

### 7.2 `/admin/operations/orders/:id` — Order Detail & fulfillment (**ACTION-ONLY**)

Data page phải lấy từ `Order`, immutable `OrderItem`, `OrderStatusHistory`, `OrderNote`, `Payment`, `PaymentTransaction`, `Shipment`, `ShipmentItem`, `ShipmentHistory`, `InventoryReservation`.

Các action backend hiện có:

| Action | Policy | Popup bắt buộc |
| --- | --- | --- |
| Confirm / Cancel order | `orders.manage` | Cancel reason + warning release/reversal effect. |
| Verify bank transfer | `payments.verify` | So khớp amount, provider reference, evidence; warning chống verify hai lần. |
| Refund payment | `payments.refund` | Amount, reason, original payment/transaction; không vượt amount paid. |
| Prepare shipment | `shipments.manage` | Confirm pick/pack stage. |
| Start shipment | `shipments.manage` | Carrier, tracking code, confirmation inventory reservation sẽ consume. |
| Complete / Delivery failed | `shipments.manage` | Delivery-failed reason; complete confirmation. |

Order và Shipment có state machine riêng. Không cho FE tự set status bằng dropdown; chỉ render action phù hợp state trả về bởi backend. Cần backlog API: management list/detail, timeline DTO, notes, document/packing-slip, order edit/return policy trước khi làm các popup tương ứng.

### 7.3 `/admin/operations/payments/reconciliation` — Đối soát SePay (**NOW, read-only**)

`GET /api/v1/management/payments/sepay/reconciliation`, policy `payments.verify`. Thiết kế: filter date, trạng thái reconciliation, tổng amount, payment/transaction rows, provider reference, link Order khi DTO có, export button chỉ active khi backend export API xuất hiện.

Không hiển thị raw webhook secret/payload. Refund là action trong Order Detail, không phải batch button của reconciliation.

### 7.4 `/admin/operations/inventory` — Tồn kho (**MODEL-ONLY**)

Entity đã đủ nền: `InventoryItem → InventoryLevel → StockLocation`; `InventoryMovement` append-only và `InventoryReservation` liên kết `OrderItem`. Đây là trang P1 quan trọng để tránh oversell, nhưng hiện chưa có controller/use case.

| Page con tương lai | Cần có | Popup |
| --- | --- | --- |
| Inventory overview | available = stocked - reserved, incoming, low-stock, filter location/variant | none; drill down detail |
| Variant stock detail | balances, movements, reservations, related orders | `AdjustStock`, lý do bắt buộc |
| Receive | receive theo location/variant, partial receive | `ReceiveStock`, scan barcode tùy phase |
| Transfer | origin/destination, lines, shipment, partial received/rejected | `CreateTransfer`, `ReceiveTransfer`, `CancelTransfer` |
| Stock locations | CRUD operational location | `LocationEditor`, deactivate confirmation |

Backend cần lock/concurrency, append-only movement, permission tách `inventory.read`, `inventory.adjust`, `inventory.transfer`, không để FE gửi available quantity tùy ý.

## 8. Trang tương lai đã có data model

| Nhóm/page | Entity neo dữ liệu | UI/page + popup cần có | Điều kiện trước FE |
| --- | --- | --- | --- |
| Producers | Producer, ProducerContact, ProductionFacility, PointOfSale, PointOfSaleProduct | list/detail; create/edit producer; verify/publish/pause; facility/POS drawers; contact editor | CRUD, lifecycle, verification policy, public visibility read model. |
| Price lists | PriceList, VariantPrice | list/detail price list; effective window, status; attach price periods | PriceList CRUD/API, overlap validation, audit. |
| Promotions & coupons | Promotion, Coupon, CouponProduct/Category, CouponRedemption, OrderDiscount | promotion list/editor; condition builder; coupon generate/import; redemption monitor; pause/end modals | promotion evaluation in checkout, eligibility read API, transaction-safe redemption. |
| Content & storefront | Campaign, Banner, Article, Page, PageSection, NavigationItem, SeoRedirect | campaign board; banner editor/media picker; article/page editor; navigation tree; redirect manager | CMS CRUD, scheduling, preview/draft/publish, sanitization, SEO policy. |
| Trust & traceability | Certification/Evidence, ProductCertification, ProducerCertification, TraceProfile/Lot/Event/Evidence | certificate review; trace profile/lot timeline; evidence viewer; approve/reject dialogs | moderation lifecycle, evidence access control, public projection. |
| Review & Q&A | ProductReview/Media, ProductQuestion/Answer | moderation queue; review detail; hide/restore; answer editor | customer submission/read model, moderation commands, anti-abuse policy. |
| B2B | TradeInquiry/Item/StatusHistory/Attachment, PartnerApplication | inquiry queue/detail; assign owner; quote/won/lost/closed; attachment viewer; partner approval | list/detail, assignment, status transition, PII access policy, notification/outbox. |
| Customers | User, CustomerProfile, CustomerAddress, Order, Wishlist | searchable customer 360; addresses; order history; notes/tags/consent | customer management read API, PII policy, audit, no direct password/token data. |
| Notifications | Notification, UserNotification | template/event list, delivery inbox, recipient state | notification management/read APIs; token privacy; delivery audit. |
| Analytics | AnalyticsEvent, VisitorSession | sales/funnel/catalog/operations dashboard, date range, drilldown/export | aggregate read models, timezone/currency definitions, retention/consent. |
| System settings/audit | SystemSetting, AuditLog, SecurityEvent, UserSession | settings groups, audit search/detail, session revoke, security event view | protected settings API, immutable audit read model, session-admin policy. |

## 9. Page specification cho các mục chưa có API

### Dashboard (`/admin` hoặc `/admin/analytics/overview`) — **FUTURE**

Không dùng Product/Order raw list để FE tự tính KPI. Backend phải trả aggregate theo timezone/currency: GMV, paid/refunded, order count, AOV, orders by lifecycle, fulfillment SLA, low-stock, pending moderation/B2B, payment reconciliation exceptions. Cần date-range picker, compare period, drill-down link, empty state "no data", và export server-side.

### Producer 360 (`/admin/producers/:id`) — **FUTURE**

Header có verification/public status; tabs Profile, Contacts, Facilities, Points of sale, Products, Certifications, Audit. Popup `VerifyProducer`, `PauseProducer`, `FacilityEditor`, `PointOfSaleEditor`. Product create chỉ được dùng Producer đã hợp lệ; do đó Producer workflow nên được ưu tiên trước khi mở catalog cho staff rộng hơn.

### Customer 360 (`/admin/customers/:id`) — **FUTURE**

Tabs Profile, Addresses, Orders, Support/B2B, Consent, Security summary. Không cho sửa password/token. Mọi xem PII phải có policy riêng và AuditLog. `DeactivateCustomer` cần modal lý do và mô tả ảnh hưởng phiên đăng nhập/checkout.

### Trust & trace (`/admin/trust`) — **FUTURE**

Queue chứng nhận/trace evidence/review moderation; filters state/producer/product/date; detail phải ghi reason approval/rejection và audit actor. Public trace page chỉ dùng projection đã approved, không leak internal evidence/metadata.

## 10. Workflow chuẩn tham chiếu thực tế và tác động lên thiết kế

| Quy trình thực tế | Áp dụng cho dự án |
| --- | --- |
| Product cần trạng thái tách "chưa hoàn thiện", "bán được", "ngừng bán"; status là control point cho storefront. | Giữ Product `Draft → Review → Published → Paused/Discontinued`; UI Review checklist, không publish thẳng từ form tổng quan. |
| Order management bao gồm xem/tra cứu, payment, fulfillment, return/refund; fulfillment là pick-pack-ship, không chỉ đổi status. | Ưu tiên management order read model và timeline trước; `StartShipment` phải là action nghiệp vụ có carrier/tracking và inventory consumption. |
| Inventory transfer/receive tách thỏa thuận, movement vật lý và received quantity; partial receive/reject giữ số liệu chính xác. | Các entity inventory hiện có là nền tảng; thêm transfer/receiving aggregate/API thay vì chỉ thêm input "số lượng tồn" vào Product. |
| Reconciliation cần liên kết payout/batch với transaction, totals, fee, exception và export. | SePay reconciliation page là read-only P0; bổ sung date/filter/export/error state, ledger/payment transaction detail ở P1. |
| Staff access cần least privilege, deny by default, enforcement server-side và audit thay đổi quyền. | Chỉ FE-gate để UX; backend phải thống nhất SystemAdmin/Admin, custom role persistence, session invalidation và audit trước role-policy UI. |

## 11. Backlog API tối thiểu theo phase

### Phase 0 — hoàn tất UI an toàn từ API hiện có

- Product list/new/editor và Category list/editor.
- Product media upload/poll/attach; Producer picker.
- User list/create/edit/delete; role list/create/edit/delete, nhưng ẩn Role Policy page đến khi blocker được sửa.
- SePay reconciliation read-only.

### Phase 1 — khép kín vận hành đơn hàng

- `GET /management/orders` paginated/filterable; `GET /management/orders/{id}` với snapshots, payment, shipment, histories, permissible actions.
- Order notes/timeline DTO, server-generated packing/invoice document, payment/refund history.
- `GET /management/media` list/search/filter + authorized preview contract.
- Correct and test access matrix, role-policy persistence, session/token invalidation, SecurityEvent/AuditLog.

### Phase 2 — merchant operations

- Producer CRUD + verification; price list CRUD; Inventory overview/movement/adjust/reservation/transfer; StockLocation CRUD.
- Promotion/coupon lifecycle and checkout application; customer 360 read model.

### Phase 3 — growth, trust and content

- CMS/SEO, reviews/Q&A moderation, traceability/certification, B2B inquiry case management, notifications.
- Analytics aggregate/read models and server-side exports.

## 12. Acceptance checklist cho FE trước khi release một trang

- [ ] Route và policy thực sự có trong backend; test account có claim tương ứng.
- [ ] Request/response/error/empty/pagination/loading states đã map từ `ApiResponse<T>`.
- [ ] Không render field không nằm trong DTO.
- [ ] Mutation có optimistic/concurrency plan; Product/Category dùng newest stamp và 409 flow.
- [ ] Action tài chính/destructive có reason/confirm và refresh state sau response.
- [ ] PII, secrets, storage key, payment webhook payload không vào DOM/log/analytics FE.
- [ ] Browser smoke với API thật, session quyền thật, media scan thật và PostgreSQL môi trường phù hợp.

## 13. DB capability matrix: table vật lý, quan hệ và page

### 13.1 Quy ước vật lý và tác động tới FE

Mọi entity kế thừa `BaseEntity` được EF map mặc định thành bảng `Tbl_<Entity>` (ví dụ `Tbl_Product`, `Tbl_Order`, `Tbl_User`), có UUID `Id`, số nội bộ `No`, `CreatedAt/By`, `UpdatedAt/By`, soft-delete (`IsDeleted`, `DeletedAt/By`) và `ConcurrencyStamp`. Vì vậy:

- Page list dùng DTO/read model, không lấy trực tiếp schema table về browser.
- Mọi admin mutation mới phải trả stamp/version mới nếu record dùng optimistic concurrency.
- “Delete” trên UI mặc định phải hiểu là soft-delete/hide/discontinue cho tới khi API nêu rõ hard-delete; không cho FE suy diễn xóa vật lý.
- `AuditLog.BeforeData`/`AfterData` và `SystemSetting.Value` là `jsonb`; viewer phải có JSON tree/redaction, không render raw secrets.

### 13.2 Identity, customer và workforce access

| Table chính | Quan hệ thực tế | Page nghiệp vụ có thể neo dữ liệu | Mức API |
| --- | --- | --- | --- |
| `Tbl_User` | 1 User → 1 Role; có `UserPolicy`, refresh/session/device/security records; profile cơ bản, status/lockout/verification. | Account/staff access list, customer account summary, own profile. | User management NOW; customer/staff 360 chưa có. |
| `Tbl_Role`, `Tbl_Policy`, `Tbl_RolePolicy`, `Tbl_UserPolicy` | Role có nhiều User/RolePolicy; UserPolicy là override có thời hạn. | Role list, policy matrix, access review. | Partial; role-policy release bị blocker. |
| `Tbl_UserSession`, `Tbl_SessionRefreshToken`, `Tbl_JwtRefreshToken`, `Tbl_UserDeviceToken`, `Tbl_SecurityEvent` | User → sessions/tokens/devices/security events. | Session security, device revoke, login/security timeline. | MODEL-ONLY cho admin. |
| `Tbl_CustomerProfile`, `Tbl_CustomerAddress`, `Tbl_AdministrativeArea` | CustomerProfile 1:1 User; User có nhiều address, một default; address → area. | Customer 360, consent, address book. | Customer tự quản addresses NOW; staff customer page chưa có. |

**Điểm quyết định:** `Tbl_User` không có `DepartmentId`, `ManagerId`, `EmploymentContractId`, `EmploymentStatus`, `WorkScheduleId`, salary, attendance hoặc leave balance. Một User có role không phải là một Employee record.

### 13.3 Catalog, supplier/producer và storefront merchandise

| Table chính | Quan hệ thực tế | Page/feature cần có | Mức API |
| --- | --- | --- | --- |
| `Tbl_Producer`, `Tbl_ProducerContact` | Producer → nhiều Product và contacts. | Producer list/detail, contact management, verification. | Picker read NOW; CRUD MODEL-ONLY. |
| `Tbl_ProductionFacility`, `Tbl_PointOfSale`, `Tbl_PointOfSaleProduct` | Producer → facilities/POS; POS ↔ Product. | Facility/POS management, local availability page. | MODEL-ONLY. |
| `Tbl_Category`, `Tbl_ProductCategory` | Category có hierarchy; Product ↔ Category, exactly one primary by domain rule. | Category tree/editor. | NOW. |
| `Tbl_Product`, `Tbl_ProductSlugHistory` | Product → Producer; status/lifecycle, slug history. | Product list/editor, publish review, redirect/history panel future. | NOW except slug history view. |
| `Tbl_ProductVariant`, `Tbl_ProductOption`, `Tbl_ProductOptionValue`, `Tbl_ProductVariantOptionValue` | Product → variants/options; variant ↔ selected option values. | Variant matrix, SKU/barcode editor, option editor. | NOW. |
| `Tbl_VariantPrice`, `Tbl_PriceList` | Variant → price periods; optional price list. | Price period editor, price list administration. | Price period create NOW; PriceList CRUD MODEL-ONLY. |
| `Tbl_MediaAsset`, `Tbl_ProductMedia` | Product ↔ MediaAsset, media primary/order/caption; scan/visibility lifecycle on asset. | Product media tab, Media Library/asset governance. | Tab NOW; library MODEL-ONLY. |

### 13.4 Commercial transaction, payment, shipment và inventory

| Table chính | Quan hệ thực tế | Page/feature cần có | Mức API |
| --- | --- | --- | --- |
| `Tbl_Cart`, `Tbl_CartItem` | Cart owner XOR User/guest-token hash; CartItem → Variant. | Customer cart support/abandon cart. | Customer flow NOW; staff support MODEL-ONLY. |
| `Tbl_Order`, `Tbl_OrderItem`, `Tbl_OrderStatusHistory`, `Tbl_OrderNote`, `Tbl_OrderDiscount` | Order owner XOR User/guest; immutable product/price snapshots; histories/notes/discount facts. | Order queue/detail, timeline, manual note, returns/refund. | Customer reads NOW; staff actions ACTION-ONLY. |
| `Tbl_Payment`, `Tbl_PaymentTransaction` | 1 Order → 1 Payment; Payment → many immutable transactions. | Payment detail/refund/reconciliation. | Verify/refund action + SePay reconciliation; detail MODEL-ONLY. |
| `Tbl_PaymentGatewayAttempt`, `Tbl_PaymentGatewayNotification`, `Tbl_PaymentBankQrAttempt`, `Tbl_PaymentBankQrWebhookNotification` | Attempt/notification audit per payment, provider idempotency/replay facts. | Payment exception queue, webhook audit, settlement/reconciliation drilldown. | Reconciliation partial; other pages MODEL-ONLY. |
| `Tbl_Shipment`, `Tbl_ShipmentItem`, `Tbl_ShipmentHistory` | Shipment → order lines/history; order and shipment status are separate. | Pick-pack-ship detail, carrier/tracking, delivery exception. | Actions ACTION-ONLY; staff read model missing. |
| `Tbl_StockLocation`, `Tbl_InventoryItem`, `Tbl_InventoryLevel`, `Tbl_InventoryReservation`, `Tbl_InventoryMovement` | Variant → InventoryItem; level unique by item/location; reservation → OrderItem; movement append-only. | Inventory overview, location, adjustment, receive, transfer, reservation audit. | MODEL-ONLY. |
| `Tbl_IdempotencyRecord` | Records command fingerprint/result/expiry. | Operations support, duplicate-order diagnosis; never expose client-wide. | No admin read API. |

### 13.5 Engagement, trust, content, B2B, observability

| Table group | Quan hệ thực tế | Page/feature cần có | Mức API |
| --- | --- | --- | --- |
| `Tbl_Promotion`, `Tbl_Coupon`, junctions, `Tbl_CouponRedemption` | Promotion/coupon target Product/Category and redemptions link Order/User. | Campaign/promotion/coupon, redemption audit. | MODEL-ONLY. |
| `Tbl_Certification` + evidence + product/producer/facility junctions | Certificates can attach to Product, Producer, Facility. | Compliance certificate review and evidence viewer. | MODEL-ONLY. |
| `Tbl_TraceProfile`, `Tbl_TraceLot`, `Tbl_TraceEvent`, evidence | Trace profile → lots → events/evidence. | Traceability editor, lot timeline, public projection approval. | MODEL-ONLY. |
| `Tbl_ProductReview`, media, question/answer, wishlist | Product/User/OrderItem-linked review and Q&A; wishlist customer personalization. | Moderation queue, answer workflow, review media safety. | MODEL-ONLY. |
| `Tbl_Page`, sections/products, article/category map, campaign/banner/navigation/redirect | CMS content and navigation use MediaAsset/product relations. | CMS, banner scheduler, navigation tree, redirect manager. | MODEL-ONLY. |
| `Tbl_TradeInquiry`, items/history/attachments, `Tbl_PartnerApplication` | B2B case has requested items, status history, internal attachment; applicant may link User. | B2B CRM queue, quote pipeline, partner approval. | MODEL-ONLY. |
| `Tbl_Notification`, `Tbl_UserNotification` | Notification → per-user inbox/delivery state. | Admin notification operations/template/delivery audit. | MODEL-ONLY. |
| `Tbl_VisitorSession`, `Tbl_AnalyticsEvent` | Anonymous/user-linked web analytics. | Marketing/commerce dashboards and funnels. | MODEL-ONLY. |
| `Tbl_AuditLog`, `Tbl_SystemSetting`, `Tbl_OutboxMessage` | Administrative audit, protected setting, integration outbox. | System operations, audit/search, outbox failure queue. | MODEL-ONLY. |

## 14. Quản lý website bán hàng thực tế: sitemap đầy đủ

Đây là taxonomy vận hành một website commerce, không phải danh sách page được phép build ngay. Nhãn quyết định việc bật menu.

| Workspace | Page | Mục tiêu vận hành | Popup/chức năng đi kèm | DB/API hiện trạng |
| --- | --- | --- | --- | --- |
| Tổng quan | Commerce dashboard | GMV, orders, payment exception, fulfillment SLA, low stock, moderation/B2B inbox. | Date range, compare period, drilldown, export. | Analytics/Order entities có; API aggregate chưa có. |
| Catalog | Products | Quản trị hàng hóa, lifecycle, variants, price, media. | Product editor 7 tabs, publish/discontinue confirmations. | NOW. |
| Catalog | Categories | Taxonomy storefront. | Parent picker, publish/pause/hide confirmations. | NOW. |
| Catalog | Media library | Tái sử dụng/kiểm duyệt asset, scan exception. | Upload, retry scan, delete/usage impact. | Asset model có; list API thiếu. |
| Sourcing | Producers | Quản lý thương hiệu/nhà sản xuất, contacts, verify. | Producer/facility/POS editor; verify/pause. | MODEL-ONLY except picker. |
| Sourcing | Facilities & points of sale | Nơi sản xuất/bán, availability theo nơi. | Map/location form, hours, product binding. | MODEL-ONLY. |
| Merchandising | Price lists | Retail/B2B/time-window pricing. | Price list editor, schedule conflict. | MODEL-ONLY. |
| Merchandising | Promotions & coupons | Rule, eligibility, code, redemption. | Rule builder, coupon generator/import, pause/end. | MODEL-ONLY. |
| Operations | Orders | Search, payment/fulfillment/exception queues. | Cancel, verify payment, refund, notes, print docs. | ACTION-ONLY mutations. |
| Operations | Fulfillment | Pick/pack/ship, carrier/tracking, failure/retry. | Start shipment, delivery failed, shipment document. | ACTION-ONLY mutations. |
| Operations | Payments | Reconciliation, provider attempts, webhook exceptions/refunds. | Refund confirmation, mismatch resolution, CSV export. | Reconciliation NOW; rest MODEL-ONLY. |
| Operations | Inventory | Stock by location, incoming/reserved, movement, transfer. | Adjust/receive/transfer/stocktake. | MODEL-ONLY. |
| CRM | Customers | Customer 360, consent, order history, support. | Deactivate/merge/PII access confirmation. | MODEL-ONLY. |
| CRM | Reviews & Q&A | Moderate public trust content. | Approve/hide, answer, abuse escalation. | MODEL-ONLY. |
| B2B | Inquiries | Assign/quote/win/lost/close commercial leads. | Assignment, quote form, close reason, attachment viewer. | MODEL-ONLY. |
| Trust | Certifications | Verify evidence and decide public visibility. | Approve/reject with reason, expiration alert. | MODEL-ONLY. |
| Trust | Traceability | Lot event timeline / QR public view governance. | Lot editor, event/evidence modal, recall/hold. | MODEL-ONLY. |
| Content | Pages, articles, banners | Controlled content lifecycle, campaign schedule. | Editor, media picker, preview, publish/schedule. | MODEL-ONLY. |
| Content | Navigation & redirects | Website menu and SEO redirect safety. | Tree editor, redirect loop checker, publish preview. | MODEL-ONLY. |
| Analytics | Sales/marketing/operations reports | Decision support; no client-derived financial aggregates. | Filters, scheduled/server export, drill-down. | MODEL-ONLY. |
| System | Access, users, roles | Staff identity and authorization. | User editor, role form, access matrix after blockers fixed. | Users/Roles partial NOW. |
| System | Audit, sessions, settings, outbox | Security/operational governance. | Session revoke, setting confirmation, JSON diff, replay/retry only if backend supports. | MODEL-ONLY. |

## 15. Workforce / HR: blueprint thực tế và gap database

### 15.1 Phân biệt Access Management với Human Resources

Access Management trả lời **"ai được dùng hệ thống và được làm action nào"**. HR trả lời **"ai là nhân viên, thuộc phòng ban nào, làm lịch nào, làm bao nhiêu giờ, nghỉ phép/được trả lương ra sao"**. Không gộp hai khái niệm vào `Tbl_User`:

- Một customer cũng là User nhưng không phải Employee.
- Một Employee có thể chưa được cấp tài khoản hệ thống.
- Một User/staff account có thể là service/integration account, không có hợp đồng lao động.
- Role là authorization; job title/department/manager là organization data.

Các hệ thống HR thực tế tách employee records, departments, contracts, attendance/time-off/planning và payroll work entries. Employee được tuyển/hired qua pipeline riêng; attendance và leave là input cho timesheet/payroll, không phải field phụ của User.

### 15.2 Bảng còn thiếu bắt buộc nếu mở HR

| Aggregate/bảng đề xuất | Relationship tối thiểu | Vì sao cần tách khỏi `Tbl_User` |
| --- | --- | --- |
| `Employee` | optional `UserId` unique; `DepartmentId`, `JobPositionId`, `ManagerEmployeeId`, work email/phone, employment status, start/end date. | Hồ sơ nhân sự, không phải identity. |
| `Department` | parent department; manager employee. | Org chart, headcount, approval route, cost center. |
| `JobPosition` | department, employment type, schedule, headcount target. | Tuyển dụng và phân quyền nghiệp vụ không cùng một khái niệm. |
| `EmploymentContract` | employee, position, start/end, contract type, salary structure, legal state. | Lịch sử hợp đồng/audit, không ghi đè vào User. |
| `WorkSchedule` / `ShiftTemplate` / `ShiftAssignment` | employee/location/date/time/role; break. | Planning/roster, capacity fulfillment. |
| `Attendance` | employee, check-in/out, source/device/location, approved correction. | Work time và compliance; immutable correction trail. |
| `LeaveType`, `LeaveBalance`, `LeaveRequest`, `LeaveApproval` | employee, date window, approver, state. | Leave lifecycle và balance không nằm trong session login. |
| `TimesheetEntry` / `WorkEntry` | employee, project/task or shift, source attendance/leave, hours, approval. | Cầu nối attendance/leave sang payroll hoặc cost analysis. |
| `PayrollPeriod`, `PayrollRun`, `Payslip` | employee contract, work entries, adjustment, approval, payment reference. | Bảo mật tài chính, legal/tax localization và immutable history. |
| `RecruitmentJob`, `Applicant`, `ApplicationStage`, `Interview`, `Offer` | job → applicants → stage history; hired applicant → employee. | Tuyển dụng có PII riêng, retention/access riêng. |
| `OnboardingPlan`, `OnboardingTask`, `EmployeeAssetAssignment`, `OffboardingChecklist` | employee, owner, due date, completion. | Provision/revoke equipment/system access có kiểm soát. |

Không tạo migration hay HR screen trong phase hiện tại nếu chưa có quyết định nghiệp vụ về pháp lý, payroll vendor, timezone/ca làm, dữ liệu sinh trắc/định vị và retention PII.

### 15.3 Sitemap HR thực tế và popup

| Page tương lai | Nội dung | Popup/action | Permission đề xuất |
| --- | --- | --- | --- |
| `/admin/hr/people` | Directory nhân viên: code, department, job, manager, employment status, work location. | `EmployeeEditor`, deactivate/offboard confirmation. | `hr.employees.read/manage` |
| `/admin/hr/people/:id` | Profile, contracts, schedule, attendance, leave, equipment, onboarding/offboarding, access account link. | Contract editor, manager change, link/unlink User, PII access reason. | record-level HR policy |
| `/admin/hr/organization` | Department tree, manager, headcount, job positions. | Department/JobPosition editor; re-parent confirm. | `hr.organization.manage` |
| `/admin/hr/recruitment/jobs` | Job requisitions, target headcount, recruiter, publish state. | Job editor, publish/unpublish. | `hr.recruitment.manage` |
| `/admin/hr/recruitment/applicants` | Kanban stage New → screen → interview → offer → hired/rejected; stage history. | Applicant form, schedule interview, offer/reject reason, convert-to-employee. | `hr.recruitment.manage` |
| `/admin/hr/planning` | Shift calendar, staffing demand theo warehouse/order operations. | Create/copy shift, conflict/overlap dialog, swap request. | `hr.schedule.manage` |
| `/admin/hr/attendance` | Live checked-in, late/missing checkout, correction queue. | Kiosk/check-in correction, manual attendance reason/approval. | `hr.attendance.read/manage` |
| `/admin/hr/time-off` | Leave balance/calendar/approval queue. | Request, approve/reject, cancel, conflict alert. | `hr.leave.read/approve` |
| `/admin/hr/timesheets` | Work entries by employee/project/shift; approval. | Edit/correction with audit, approve/reject. | `hr.timesheet.read/approve` |
| `/admin/hr/payroll` | Period, work-entry exceptions, payroll run, payslip status. | Lock period, approve run, adjustment/reversal. | `hr.payroll.*`, strict separation of duties |
| `/admin/hr/onboarding` | Checklists for equipment, training, account/role provision; offboarding revoke. | Assign task, complete with evidence, revoke access confirmation. | `hr.onboarding.manage` |
| `/admin/hr/reports` | Headcount, turnover, attendance, leave, recruitment funnel. | Date filter, anonymized/export controls. | `hr.reports.read` |

### 15.4 Workforce workflow liên kết commerce

```text
Recruitment -> Applicant -> Offer -> Employee -> Contract + User account (optional)
                                      |          \-> Role/policies (access only)
                                      |-> Department / Job / Manager
                                      |-> Schedule -> Attendance / Leave -> WorkEntry -> Payroll
                                      \-> Onboarding -> equipment + training + operational access

Commerce operations demand
  Order volume / warehouse capacity -> Shift planning
  Staff assigned to pick-pack-ship -> shipment/order actor audit
  Inventory adjustment/refund/policy change -> named actor + reason + approval/audit
```

Với scope commerce hiện tại, phase HR hợp lý đầu tiên là **Employee directory + Department/Job + staff-to-User linking + onboarding/offboarding access checklist**. Attendance/payroll chỉ nên mở sau khi có policy pháp lý và DB aggregates nêu trên.

## 16. Cấu hình, cài đặt và tuỳ chỉnh website

### 16.1 Phân tầng cấu hình — không đưa mọi thứ vào `appsettings`

`appsettings.*`, environment variables, connection string, JWT key, OTP controlled-test bypass, payment secret, Blob/SMTP credential là **runtime secret/infrastructure configuration**. Chúng không được đưa vào trang Admin, không trả về API và không ghi vào `Tbl_SystemSetting`.

`Tbl_SystemSetting` hiện có `SettingKey`, `Value`, `IsPublic`, `Description`; `Value` được EF lưu JSONB. Đây là nền tảng cho **business configuration không bí mật**, nhưng source scan hiện không có controller/handler/query cho aggregate này. Vì vậy FE không được tự giả định có endpoint setting.

| Tầng | Ví dụ | Nơi lưu/chủ sở hữu | UI admin | Ràng buộc bắt buộc |
| --- | --- | --- | --- | --- |
| Secret/infrastructure | connection string, JWT signing key, SePay secret, Blob key, scanner credential, OTP bypass | deployment secret store + `appsettings` theo môi trường | Không có UI; chỉ deployment runbook. | Never expose/read-back/log; thay đổi qua DevOps, rotate/audit. |
| System runtime non-secret | URL công khai, rate-limit threshold, feature rollout, maintenance switch | `SystemSetting` nếu ứng dụng đọc được động; nếu không phải config deploy. | Settings page chỉ sau khi có allow-list backend. | Key registry, typed JSON schema, default, validation, audit, rollback. |
| Business operation | shop profile, contact, order cutoff, delivery zone, COD policy, invoice note | `SystemSetting` hoặc aggregate chuyên biệt khi quy tắc phức tạp. | Form theo nhóm, không phải JSON editor mặc định. | RBAC, maker-checker cho tài chính/fulfillment, effective time. |
| Storefront presentation | homepage sections, menu, banner, SEO, theme token | `Page`, `PageSection`, `NavigationItem`, `Banner`, `Campaign`, `SeoRedirect`; theme hiện chưa có model. | CMS/Customization. | Draft/preview/publish/schedule, media safety, cache invalidation, audit. |

### 16.2 Trang Settings cần có cho admin

| Route/page | Nội dung chi tiết | Popup/action đi kèm | Entity/API hiện trạng | Yêu cầu backend trước khi FE bật |
| --- | --- | --- | --- | --- |
| `/admin/settings/overview` | Danh sách nhóm setting, last change, environment label, cảnh báo pending publish. | Confirm thay đổi; compare current/default; rollback version. | `SystemSetting` MODEL-ONLY. | `GET /management/settings`, key registry, masked value DTO, audit query. |
| `/admin/settings/store-profile` | Tên thương hiệu, logo, hotline/email, địa chỉ, giờ phục vụ, social links, default SEO. | Media picker; validate contact/URL; preview header/footer. | Có thể lưu một số JSON setting; chưa có contract. | Typed `StoreProfileSetting` command/query, public projection cache-safe. |
| `/admin/settings/localization` | Timezone, currency display, locale, date/number format, VAT display, supported language. | Effective-from confirmation; preview receipt/order. | Chưa có aggregate/API. | Schema/versioned setting; không hồi tố Order snapshot. |
| `/admin/settings/checkout` | Guest checkout, order cutoff, minimum order, address requirements, cancel window, customer message/terms. | Policy editor, legal text link, activate at time. | Cart/Order entity có nhưng policy configuration chưa có API. | Server-side rule evaluator; versioned policy; checkout preview reads effective policy. |
| `/admin/settings/payment` | Bật/tắt payment method đã hỗ trợ, bank transfer instructions, refund/operator policy, reconciliation cut-off. | Dual approval; test-mode badge; settlement mismatch acknowledgement. | Payment entities + SePay reconciliation có; provider setting contract chưa có. | Secret remains deployment-only; public non-secret config endpoint; role separation. |
| `/admin/settings/fulfillment` | Warehouse/location default, handling SLA, carrier/service options, delivery-failed/retry policy, packing/invoice note. | Cutoff/schedule editor, use-in-orders impact. | Inventory/Shipment models exist; setting/API missing. | Stock-location and shipment read/write APIs; avoid changing historical shipment facts. |
| `/admin/settings/inventory` | Reservation expiry, low-stock threshold, stocktake/adjust approval threshold. | Warning/risk confirmation; effective date. | Inventory model exists; no management API. | Typed policy + inventory job configuration; audit and approval. |
| `/admin/settings/notifications` | Event-to-template mapping, recipient policy, in-app/email/SMS/push enablement, quiet hours. | Template editor/preview/send test; recipient preview. | Notification/UserNotification model exists; no template/channel entity/API. | Add `NotificationTemplate`/channel config or deliberately scope to code-owned templates. |
| `/admin/settings/security` | Session duration, forced MFA/OTP policy, password/lockout policy, staff IP restriction, access review cadence. | Revoke sessions; require own re-auth; export access review. | Session/security entities exist; system policy mutation surface incomplete. | Do not place secret here; policy evaluator + SecurityEvent/AuditLog integration. |
| `/admin/settings/integrations` | Read-only integration health: payment webhook, storage/scanner, mail/SMS, analytics, outbox. | Retry/replay only for idempotent supported jobs; copy correlation ID. | `OutboxMessage`/attempt entities model-only. | Health/read models, redaction, retry state machine; no generic “resend”. |
| `/admin/settings/data-retention` | Retention for analytics, customer PII, audit, media quarantine, exports; deletion/anonymization schedule. | Impact preview, legal approval, scheduled-job confirmation. | No retention policy/API. | Legal decision, background jobs, immutable audit, scoped purge commands. |

**Không làm** một màn hình cho nhập tự do `SettingKey`/raw JSON cho mọi role. Chỉ `SYSTEM_ADMIN` mới xem technical key registry; operator dùng form typed. `IsPublic=true` cũng không đồng nghĩa trả toàn bộ setting ra storefront: backend phải allow-list từng key và trả DTO public riêng.

### 16.3 CMS và storefront customization — entity đã có nhưng chưa có vận hành

| Page | Dữ liệu/quan hệ đã có | FE cần hiển thị và thao tác | Popup/workflow | Trạng thái source |
| --- | --- | --- | --- | --- |
| `/admin/content/pages` | `Page` có title/slug/status/publish/meta; `PageSection` thuộc Page; `PageSectionProduct` liên kết Product. | Page list, status, URL, SEO, scheduled publish; section count. | Create/edit page, duplicate, unsaved-change guard, archive; preview draft. | MODEL-ONLY; Page/Section còn không có public factory/mutation/handler/controller. |
| `/admin/content/pages/:id/editor` | Section type/title/content/order/status, product blocks. | Block editor: hero, rich text, product collection, CTA, FAQ; reorder; mobile/desktop preview. | Add block, product picker, media picker, schedule/publish confirmation, conflict refresh. | MODEL-ONLY; phải thiết kế structured block schema, sanitation và revision. |
| `/admin/content/articles` | `Article`, `ArticleCategory`, `ArticleCategoryMap`, cover `MediaAsset`, author User. | Editorial calendar, category/tag, SEO, cover, author, publish state. | Article editor, revision/preview, scheduled publish/unpublish. | MODEL-ONLY. |
| `/admin/content/campaigns` | `Campaign` → `Banner`; date window/status; banner has media/alt/target/order. | Campaign calendar, banner placement, schedule collision, impression metrics (sau này). | Campaign editor, banner editor, media replacement, stop campaign, preview slot. | MODEL-ONLY; validate date range and media clean/public. |
| `/admin/content/navigation` | `NavigationItem` parent/page/target URL/order/status. | Header/footer/mobile menu tree; internal Page link or external URL. | Tree reorder, link picker, deactivate, preview; circular-parent prevention. | MODEL-ONLY. |
| `/admin/content/seo/redirects` | `SeoRedirect` source/target/status code/active. | Search/filter redirect, hit count when analytics exists. | Create/edit, bulk import dry-run, loop/chain conflict dialog, deactivate. | MODEL-ONLY; server must normalize paths and prevent loop/open redirect. |
| `/admin/customization/theme` | Không có `Theme`/revision/asset binding entity. | Brand colors, typography, radius, layout options, custom CSS only if approved. | Token editor, accessibility contrast check, live preview, publish/rollback. | FUTURE — không dùng raw `SystemSetting` thay schema/revision. |
| `/admin/customization/checkout` | Page/entity riêng chưa có. | Logo, checkout copy, legal links, allow-listed layout choices. | Preview, publish, revert. | FUTURE — business checkout policy thuộc Settings, visual configuration thuộc Theme. |

CMS cần có **Draft → Review → Published/Archived**, revision/author/audit và preview token. `ContentStatus` hiện trong entity không tự tạo ra các workflow này; phải có command, validation, authorization và public reader/cache invalidation tương ứng.

## 17. Hướng dẫn vận hành, onboarding và support nội bộ

### 17.1 Help Center là một module quản trị riêng

Các bài `Article`/`Page` hiện phù hợp để public content; chúng **không đủ** cho knowledge base nội bộ vì thiếu audience staff-only, version, owner, acknowledgement/read receipt, SOP approval và liên kết role/task. Do đó cần module Guide riêng, hoặc mở rộng CMS có các field/access controls này.

| Page | Mục tiêu | Chức năng/popup | Data hiện có/gap |
| --- | --- | --- | --- |
| `/admin/help` | Landing theo role: việc cần làm hôm nay, SOP mới, release note. | Search, bookmark, “đã đọc/xác nhận”, feedback helpful/not helpful. | Không có Guide/acknowledgement entity/API. |
| `/admin/help/guides` | Danh sách SOP: tạo/publish sản phẩm, xử lý đơn, hoàn tiền, kiểm kho, xử lý media, bảo mật. | Filter role/module/status; print/export controlled. | Có thể tái dùng rich content nhưng phải thêm audience/version. |
| `/admin/help/guides/:slug` | Nội dung từng hướng dẫn, prerequisite, checklist và liên kết page. | Version history, acknowledge, report outdated, attach media/file. | FUTURE. |
| `/admin/help/manage` | Editor cho owner/approver: draft, review, published, retired. | Guide editor, reviewer assignment, publish/revoke, acknowledgement report. | FUTURE. |
| `/admin/help/release-notes` | Thông báo thay đổi API/UI/policy tới đúng role. | Compose, target role, schedule, read report. | Có `Notification` nền tảng nhưng chưa có template/audience workflow. |
| `/admin/support/customer-cases` | Tiếp nhận customer issue liên quan Order/Payment/Shipment/Product. | Create case from order, internal note, attach evidence, assign/escalate, close reason. | Không có SupportCase/CaseMessage entity; không ngụy trang `OrderNote` thành full helpdesk. |

### 17.2 Onboarding wizard theo từng loại quản trị

| Wizard | Bước | Điều kiện hoàn thành | Server capability còn thiếu |
| --- | --- | --- | --- |
| Initial store setup | Store profile → catalog category → producer → product draft → media clean → price → publish → payment/fulfillment policy → test order. | Không còn blocking readiness; owner xác nhận public preview. | Settings/Producer/Inventory/operations management APIs. |
| Catalog operator | Role grant → media policy → product draft → variants/options → price overlap check → review/publish. | Product Published, audit actor, no validation failure. | Catalog có phần lớn; producer admin/media library/list/readiness gaps còn mở. |
| Order/warehouse staff | Link Employee/User → role → location/shift → open order queue → pick/pack/ship → exception SOP. | Access least privilege, named actor and shipment history. | HR, order-management read queue, inventory/shipment APIs. |
| Security admin | Staff account → MFA/OTP policy → role/policy review → session revoke/recovery → quarterly access certification. | Explicit approval/audit and no secret shown. | Access-policy defects need repair; setting/audit/session admin APIs. |

Hướng dẫn không thay thế validation server-side. Mỗi guide nên deep-link tới page, nhưng page vẫn phải kiểm tra permission, lifecycle, concurrency stamp và input policy.

## 18. Backlog API/handler: entity tồn tại nhưng FE chưa thể gọi

### 18.1 Quy tắc trạng thái

- **NOW**: controller + command/query source tồn tại; vẫn cần runtime/PostgreSQL proof nếu chưa chạy.
- **ACTION-ONLY**: có mutation lẻ nhưng thiếu management list/detail/read model, nên chưa tạo page vận hành hoàn chỉnh.
- **MODEL-ONLY**: table/entity/configuration tồn tại nhưng không có controller/Application feature phục vụ quản trị trong source scan ngày 17-08-2026.
- **FUTURE**: không có model đủ nghĩa nghiệp vụ; cần discovery và migration được phê duyệt.

### 18.2 Ma trận ưu tiên để biến database thành admin product

| Priority | Bounded context/entity đã có | Page cần mở | API/handler tối thiểu cần bổ sung | Không được làm tắt |
| --- | --- | --- | --- | --- |
| P0 | `SystemSetting`, `AuditLog`, `UserSession`, `SecurityEvent`, `OutboxMessage` | Settings, Audit, Session/security, Integration health. | Typed settings get/update by key group; audit query; session list/revoke; health/outbox read. | Không expose secret, raw token, PII, hay generic outbox replay. |
| P0 | `Order`, `OrderItem`, histories/notes, `Payment`, `Shipment` | Order management queue/detail/exception tabs. | Management list/detail/filter/export-safe query; notes history; action availability/read model. | Không dùng customer-owner endpoints cho admin; không cho FE tự suy trạng thái. |
| P0 | `InventoryItem`, `InventoryLevel`, `Reservation`, `Movement`, `StockLocation` | Stock dashboard, location, receive/adjust/stocktake. | List/detail/ledger; receive/adjust with reason/approval; reservation expiry read; stock location CRUD. | Không allow client direct quantity update; movement append-only, lock/transaction server-side. |
| P0 | `Producer`, contacts/facilities/POS/POS products | Producer workspace. | Management CRUD/list/detail, verify/pause, facility/POS/product binding queries. | Picker API hiện có không phải backoffice CRUD. |
| P1 | `PriceList`, `VariantPrice`, `Promotion`, `Coupon`, junctions/redemptions | Price/promotion/coupon pages. | CRUD/schedule/readiness, eligibility/read model, redemption report. | Không trust calculated discount/price from FE; enforce time overlap/usage server-side. |
| P1 | `Page`, section/product, `Article`, `Campaign`, `Banner`, `NavigationItem`, `SeoRedirect` | CMS + customization pages ở mục 16.3. | Draft CRUD/revision/preview/publish/schedule, public reader, cache invalidation. | Không publish arbitrary HTML; sanitize rich content and validate redirect/media. |
| P1 | `MediaAsset`, `ProductMedia` | Media library, usage/scan/moderation queue. | Asset list/filter/paging/usage ref; public-safe URL reader; delete impact. | Không expose private/quarantine URL; upload endpoint không thay thế library API. |
| P1 | `CustomerProfile`, addresses, `Wishlist`, reviews/Q&A | Customer 360, moderation, customer support view. | Customer list/detail privacy-scoped; review/question moderation/answer; consent handling. | Không show PII by default; no unrestricted export. |
| P1 | Certification/trace entities | Compliance and traceability. | CRUD/review/approval/expiry queries, trace public projection, evidence reader. | Không public unverified certificate/evidence or alter historical trace event. |
| P1 | `TradeInquiry`, items/history/attachment, `PartnerApplication` | B2B CRM/partner onboarding. | Queue/detail, assign/status transitions/quote data/attachment policy. | Không overload Order; preserve immutable status history. |
| P2 | `Notification`, `UserNotification` | Notification template/delivery operations. | Inbox/admin query, targeted send command, preference/read status, template model if approved. | No broad user broadcast without targeting/audit/rate control. |
| P2 | `VisitorSession`, `AnalyticsEvent` | Dashboard/funnel/report export. | Server aggregate read models, retention-aware export, consent filter. | Không aggregate sensitive/raw client data in browser. |
| P2 | HR aggregates proposed at mục 15 | Workforce pages. | New approved schema + CQRS/API per aggregate. | Không add HR columns haphazardly to `User`. |

### 18.3 Chuẩn contract chung cho mọi trang MODEL-ONLY

1. `GET` paged list: filter/sort allow-list, total, cursor/page contract, tenant/role scope enforced server-side.
2. `GET /{id}`: detail DTO gồm relation cần render và `concurrencyStamp`; không lộ secret/private media/PII quá mức.
3. `POST`/`PUT` command: validator, permission, lifecycle transition, idempotency khi action có side effect; update gửi `concurrencyStamp` và trả `409` có hướng dẫn refresh/reapply.
4. Actions riêng (`publish`, `pause`, `approve`, `refund`, `adjust`, `retry`): command chuyên biệt, reason/evidence/actor/audit; không dùng generic status update.
5. History/audit: read-only timeline để FE giải thích trạng thái; destructive operation cần soft-delete/archival policy hoặc bị cấm đối với dữ liệu tài chính/kho.
6. API chỉ được đánh dấu NOW sau source, authorization test và tối thiểu một PostgreSQL/API smoke phù hợp; entity EF không phải là API contract.

## 19. Nguồn đối chiếu quy trình

- Shopify: [Product details and status](https://help.shopify.com/en/manual/products/details/product-details-page/), [Managing orders](https://help.shopify.com/en/manual/fulfillment/managing-orders), [Fulfilling orders](https://help.shopify.com/en/manual/fulfillment/fulfilling-orders), [Inventory transfers](https://help.shopify.com/en/manual/products/inventory/inventory-transfers/creating-and-managing-transfers).
- Stripe: [Refunds](https://docs.stripe.com/refunds), [Payout reconciliation](https://docs.stripe.com/reports/payout-reconciliation?locale=en-GB), [Reporting and reconciliation](https://docs.stripe.com/plan-integration/get-started/reporting-reconciliation?locale=en-GB).
- OWASP: [Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html), [Logging vocabulary for authorization changes](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Vocabulary_Cheat_Sheet.html).
- Odoo HR: [Employees](https://www.odoo.com/documentation/18.0/applications/hr/employees.html), [Recruitment flow](https://www.odoo.com/documentation/17.0/applications/hr/recruitment/recruitment-flow.html), [Check in and out](https://www.odoo.com/documentation/17.0/applications/hr/attendances/check_in_check_out.html), [Work entries](https://www.odoo.com/documentation/18.0/applications/hr/payroll/work_entries.html).

Các nguồn trên dùng để tham chiếu workflow và control; API/DTO/source của dự án vẫn là nguồn quyết định cho triển khai FE.
