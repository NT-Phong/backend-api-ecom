# Entity graphs theo bounded context

Tài liệu này là bản đồ quan hệ dành cho AI Agent không đọc source. Mỗi hộp là một entity lưu trữ; mũi tên mô tả quan hệ nghiệp vụ, không ngụ ý mọi navigation đều được khai báo hai chiều trong ORM.

## Producer, Catalog, Pricing và Media

```mermaid
erDiagram
  Producer ||--o{ ProducerContact : has
  Producer ||--o{ ProductionFacility : operates
  Producer ||--o{ Product : owns
  Product ||--o{ ProductCategory : classified_by
  Category ||--o{ ProductCategory : contains
  Category ||--o{ Category : parent_of
  Product ||--o{ ProductOption : defines
  ProductOption ||--o{ ProductOptionValue : allows
  Product ||--o{ ProductVariant : sells_as
  ProductVariant ||--o{ ProductVariantOptionValue : configured_by
  ProductOptionValue ||--o{ ProductVariantOptionValue : selected_by
  ProductVariant ||--o{ VariantPrice : priced_by
  PriceList ||--o{ VariantPrice : groups
  Product ||--o{ ProductMedia : presents
  MediaAsset ||--o{ ProductMedia : attached_as
  Product ||--o{ ProductSlugHistory : redirects_from
```

- `Product` giữ nội dung và lifecycle; `ProductVariant` mới là đơn vị mua hàng và sở hữu SKU.
- Giá là bản ghi `VariantPrice` theo khoảng hiệu lực, currency, loại giá và minimum quantity; không nhận giá từ client khi mua.
- `MediaAsset` đi qua scan/process độc lập. `ProductMedia` chỉ là liên kết, thứ tự, caption và primary.
- Publish Product cần Producer public, category/media chính hợp lệ, ít nhất một active variant và effective public price. Tồn dương không phải publish gate.

## Inventory, Cart và Order

```mermaid
erDiagram
  ProductVariant ||--o| InventoryItem : tracked_as
  InventoryItem ||--o{ InventoryLevel : balanced_at
  StockLocation ||--o{ InventoryLevel : stores
  InventoryItem ||--o{ InventoryMovement : ledger
  StockLocation ||--o{ InventoryMovement : occurs_at
  Cart ||--o{ CartItem : contains
  ProductVariant ||--o{ CartItem : selected
  Cart ||--o| Order : converted_to
  Order ||--|{ OrderItem : snapshots
  ProductVariant ||--o{ OrderItem : source_variant
  OrderItem ||--o{ InventoryReservation : reserves
  InventoryLevel ||--o{ InventoryReservation : allocated_from
  Order ||--o{ OrderStatusHistory : records
  Order ||--o{ OrderNote : annotated_by
  Order ||--o{ OrderDiscount : snapshots
  IdempotencyRecord }o--o| Order : resolves_to
```

- `InventoryLevel` là balance theo cặp item-location; available được server tính từ stocked/reserved, không lưu theo số client gửi.
- `InventoryMovement` là ledger append-only. Adjust, Allocate, Release, Ship và Return phải để lại dấu vết phù hợp.
- `CartItem.Id` chọn line checkout; `ProductVariantId` chọn hàng hóa. Hai ID không thay thế nhau.
- `OrderItem` giữ snapshot tên/SKU/đơn giá/số lượng tại thời điểm mua; lịch sử đơn không đọc lại catalog hiện tại.
- Preview chỉ báo giá. Create Order mới khóa, kiểm quote và tạo reservation trong transaction.

## Payment và Shipment

```mermaid
erDiagram
  Order ||--|| Payment : paid_by
  Payment ||--o{ PaymentTransaction : records
  Payment ||--o{ PaymentGatewayAttempt : initiates
  PaymentGatewayAttempt ||--o{ PaymentGatewayNotification : receives
  Payment ||--o{ PaymentBankQrAttempt : creates
  PaymentBankQrAttempt ||--o{ PaymentBankQrWebhookNotification : receives
  Order ||--o| Shipment : fulfilled_by
  Shipment ||--|{ ShipmentItem : contains
  OrderItem ||--o{ ShipmentItem : fulfills
  Shipment ||--o{ ShipmentHistory : records
```

- Hosted redirect/VietQR attempt chỉ là ý định thanh toán, không chứng minh `Paid`.
- IPN/webhook phải xác thực, đối chiếu reference/order/amount/currency và deduplicate trước khi đổi trạng thái.
- Refund chỉ tác động payment ledger. Hoàn tồn là nghiệp vụ Return riêng sau khi hàng thực sự được nhận và chấp nhận.

## Identity, customer và ownership

```mermaid
erDiagram
  ApplicationUser ||--o| CustomerProfile : has
  ApplicationUser ||--o{ CustomerAddress : owns
  ApplicationUser ||--o{ AuthSession : opens
  ApplicationUser ||--o{ SecurityEvent : generates
  ApplicationUser ||--o{ Cart : owns
  ApplicationUser ||--o{ Order : places
  AdministrativeArea ||--o{ CustomerAddress : classifies
  AdministrativeArea ||--o{ ProductionFacility : locates
```

Guest Cart/Order dùng guest principal được bảo vệ, không tạo `ApplicationUser` giả. Sau login, merge guest cart khóa và hợp nhất vào user cart; chỉ clear guest ownership sau khi transaction thành công.

## Foundation graph — chưa phải live API

```mermaid
flowchart LR
  Producer --> Trust[Certification / Evidence]
  Product --> Trace[Trace profile / Lot / Event]
  Product --> Engage[Wishlist / Review / Q&A]
  Product --> Promo[Promotion / Coupon]
  Product --> Content[Page / Article / Banner / Navigation]
  Product --> B2B[Trade inquiry / Partner application]
  Order --> Notify[Notification]
  Order --> Analytics[Analytics projections]
```

Các entity này có thể tồn tại trong model/DbContext nhưng mặc định là `Foundation/ROADMAP`. Không được sáng tạo endpoint, UI action hoặc cam kết runtime nếu [API Catalog](../04-api/API-CATALOG.md) chưa công bố capability tương ứng.

## Quy tắc dùng graph

1. Thiết kế mua hàng bắt đầu từ `ProductVariant`, rồi resolve effective price và inventory.
2. Thiết kế lịch sử giao dịch dùng snapshot/ledger, không join ngược để lấy giá hoặc tên hiện tại.
3. Cross-aggregate mutation phải qua use case/domain invariant; không sửa status/balance trực tiếp.
4. Quan hệ vật lý, delete behavior, index và concurrency cuối cùng phải được xác nhận bằng migration PostgreSQL khi thay schema.

