# Commerce end-to-end flows

## Product publish

Producer hợp lệ → Product Draft → categories/options → variants → effective prices → upload/scan/attach media → inventory setup nếu tracked → submit review → publish. Mỗi mutation dùng stamp mới; sửa nội dung/giá có thể đưa Published về Review theo domain rule.

```mermaid
flowchart TD
  P0[Create Producer Draft] --> P1[Verify Producer]
  P1 --> P2[Publish Producer]
  P2 --> D[Create Product Draft]
  D --> C[Assign categories]
  C --> O[Create options and values]
  O --> V[Create variant and option mapping]
  V --> PR[Append effective price]
  PR --> M[Upload, scan Clean, attach media]
  M --> I{InventoryMode Tracked?}
  I -- yes --> IL[Initialize level then Adjust stock]
  I -- no --> R[Submit review]
  IL --> R
  R --> PUB[Publish]
  PUB --> SF[Visible in storefront if public read conditions pass]
```

Failure gates: Producer chưa verified/published; category hierarchy không public; không có active variant; không có effective public price; primary media chưa Clean/Public; stale stamp. Tracked inventory chưa setup có thể khiến variant chưa sẵn sàng mua/fulfill nhưng không phải Product publish gate. Không bỏ qua gate bằng cách sửa trực tiếp status trong database.

## Authentication và cart ownership

```mermaid
sequenceDiagram
  participant FE as Browser/FE
  participant API as Auth API
  participant CART as Cart API
  FE->>CART: Add variant as guest
  CART-->>FE: guest cookie + CartDto
  FE->>API: OTP verify or password login
  API-->>FE: access/refresh session
  FE->>CART: POST merge-guest + Bearer + CSRF/cookies
  CART->>CART: lock guest/user carts and merge
  CART-->>FE: user CartDto, clear guest principal on success
```

Nếu login thành công nhưng merge thất bại, guest cart cookie không được clear trước commit. FE hiển thị user session vẫn hợp lệ và cho retry merge có kiểm soát/refetch.

## Cart đến Order

```mermaid
flowchart LR
  V[Active Variant + effective price] --> C[CartItem]
  C --> Q[Checkout preview]
  Q --> F[Quote fingerprint]
  F --> O[Create Order + Idempotency-Key]
  O --> L[Lock tracked inventory]
  L --> S[OrderItem snapshots + Payment + Reservations]
  S --> P[Commit]
```

Preview không reserve. CreateOrder khóa/kiểm lại quote, tạo snapshot và Allocate reservation trong transaction.

```mermaid
sequenceDiagram
  participant FE
  participant PRE as Checkout Preview
  participant ORD as Create Order
  participant DB as PostgreSQL
  FE->>PRE: cartItemIds + recipient + payment
  PRE->>DB: read cart, variants, prices, shipping setting
  PRE-->>FE: lines/totals/fingerprint/expiry
  FE->>ORD: same facts + fingerprint + Idempotency-Key
  ORD->>DB: begin idempotency + lock cart
  ORD->>DB: recalculate quote and compare
  ORD->>DB: lock levels and reserve tracked items
  ORD->>DB: insert Order/Items/History/Payment/Movements/Reservations
  ORD->>DB: update cart + complete idempotency + commit
  ORD-->>FE: OrderSummary
```

Nếu quote/fingerprint/availability đổi, toàn transaction thất bại và FE preview lại. Nếu response mất sau commit, retry cùng key/payload trả kết quả trước đó thay vì tạo order thứ hai.

## Payment/fulfillment

Order Pending → COD/BankTransfer/SePay/SePayVietQr. Hosted redirect hoặc QR chỉ khởi tạo attempt. Validated IPN/webhook/manual verification mới thay đổi payment theo invariant. Management xác nhận order → prepare/start/complete shipment; cancel/expiry release reservation; ship consume stock; receive return tạo Return movement. Refund không tự restock.

```mermaid
sequenceDiagram
  participant FE
  participant API
  participant SEP as SePay
  participant DB
  FE->>API: create checkout/VietQR intent
  API->>DB: lock order/payment, create or reuse attempt
  API-->>FE: hosted form or QR facts
  FE->>SEP: user completes payment
  SEP->>API: signed IPN/webhook
  API->>API: authenticate, validate order/reference/amount/currency
  API->>DB: deduplicate notification and mark Paid or NeedsReconciliation
  FE->>API: GET order status
  API-->>FE: authoritative PaymentStatus
```

## Fulfillment và inventory ledger

```mermaid
flowchart LR
  CO[Create Order] -->|Allocate| RA[Reservation Active]
  RA -->|Cancel/Expiry: Release| RR[Released]
  RA -->|Ship: Ship movement| RC[Consumed]
  RC --> DEL[Delivered]
  RC --> DF[Delivery Failed]
  DF -->|Receive accepted items: Return| STOCK[Stock restored]
  PAY[Refund Payment] -. no automatic stock effect .-> STOCK
```

## Producer và Product dependency

Producer phải được verify trước publish. Published Product phụ thuộc Producer public state; hide Producer có thể bị chặn khi còn published dependencies. Staff xử lý Product dependencies trước, không bypass bằng client flag.

## Media flow

Upload multipart → Pending asset → worker validates/scans/processes → Clean/Public hoặc Rejected/Failed → attach vào Product bằng stamp → set primary/order/caption → Product public reader chỉ trả usable media. Failed có thể retry; Rejected không attach; asset in-use không delete.

## Management reporting flow

Order/Payment/Shipment/Inventory facts được aggregate thành dashboard/analytics theo range. Analytics là read model; nó không điều khiển state và không thay thế transactional tables. Filter/paging trong UI không được dùng để tự tính KPI tổng hệ thống.
