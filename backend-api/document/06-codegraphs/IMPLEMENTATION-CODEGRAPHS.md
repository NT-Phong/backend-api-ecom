# Codegraph các luồng đã triển khai

Codegraph mô tả đường đi từ API đến dữ liệu và state change. Tên controller/request/handler là provenance để người sửa code định vị nhanh; phần diễn giải và contract đủ cho Agent không có source.

## Khung xử lý chung

```mermaid
flowchart LR
  FE[Browser / BFF / Provider] --> MW[Middleware: correlation, auth, error]
  MW --> CT[Thin Controller]
  CT --> MR[MediatR Request]
  MR --> AU[Authorization]
  AU --> VA[FluentValidation]
  VA --> TX{ITransactionalRequest?}
  TX -- no --> H[Query Handler]
  TX -- yes --> UOW[UnitOfWorkBehavior]
  UOW --> H2[Command Handler]
  H2 --> DM[Domain method / focused service]
  H --> EF[No-tracking read]
  DM --> EF2[Repository / EF tracking]
  EF --> PG[(PostgreSQL)]
  EF2 --> PG
  PG --> RES[TResult]
  RES --> ENV[ApiResponse]
```

Command mới chỉ có một commit point do UnitOfWork behavior sở hữu. Handler không tự commit và không gọi provider khi DB transaction đang mở.

## Public Product list/detail

```mermaid
flowchart LR
  R[GET /api/v1/products or /{slug}] --> PC[ProductsController]
  PC --> Q[GetProductList/GetProductDetail query]
  Q --> READ[Public catalog read model]
  READ --> JOIN[Product + Producer + Category + Active Variant + Effective Price + Usable Media]
  JOIN --> DTO[Product list/detail DTO]
  DTO --> API[ApiResponse with paging/detail]
```

Chỉ public Product thỏa điều kiện đọc mới xuất hiện. Filter/sort/paging chạy phía server; UI không tự suy ra price/availability từ Product root.

## Publish Product

```mermaid
flowchart TD
  R[POST /catalog/products/{id}/publish + concurrencyStamp] --> C[CatalogProductsController]
  C --> CMD[PublishProductCommand]
  CMD --> P[Load Product and dependencies]
  P --> ST{Stamp current?}
  ST -- no --> CON[409 concurrency conflict]
  ST -- yes --> G{Producer public; primary category/media; active variant; effective price?}
  G -- no --> BAD[Business validation error]
  G -- yes --> D[Product.Publish]
  D --> COMMIT[Single transaction commit]
  COMMIT --> OK[New status and concurrency stamp]
```

Tracked inventory setup là readiness để bán/fulfill, không phải điều kiện publish. Client không được bypass gate bằng cách ghi status trực tiếp.

## Add Cart item và merge guest

```mermaid
sequenceDiagram
  participant FE
  participant CART as CartController
  participant H as Cart Handler
  participant DB as PostgreSQL
  FE->>CART: POST /cart/items {productVariantId, quantity} + CSRF
  CART->>H: AddCartItemCommand
  H->>DB: resolve guest/user cart, active variant, effective price facts
  H->>DB: insert or combine line and commit
  H-->>FE: authoritative CartDto
  FE->>CART: POST /cart/merge-guest after login
  CART->>H: MergeGuestCartCommand
  H->>DB: lock guest and user carts; merge deterministically
  H->>DB: commit, then clear guest principal
  H-->>FE: merged CartDto
```

Server trả `CartItemDto.Id`; checkout phải dùng ID line này. Nếu merge rollback, guest cart vẫn còn để retry/refetch.

## Checkout preview và Create Order

```mermaid
sequenceDiagram
  participant FE
  participant PRE as CheckoutController
  participant ORD as OrdersController
  participant DB as PostgreSQL
  FE->>PRE: preview(cartItemIds, recipient/address, paymentMethod)
  PRE->>DB: read cart + current price + shipping setting + availability
  PRE-->>FE: lines, totals, quoteFingerprint, expiresAt
  FE->>ORD: POST /orders + same facts + Idempotency-Key
  ORD->>DB: lock idempotency scope and cart
  ORD->>DB: recalculate and compare fingerprint
  ORD->>DB: lock tracked levels; create order snapshots/reservations/payment
  ORD->>DB: convert selected cart lines and commit
  ORD-->>FE: existing or newly created OrderSummary
```

Fingerprint/giá/tồn đổi làm toàn bộ create thất bại; FE phải preview lại. Retry cùng idempotency key và cùng payload trả kết quả đã tạo; cùng key khác payload là conflict.

## SePay Hosted Checkout và VietQR

```mermaid
flowchart TD
  START[Customer creates payment intent] --> LOCK[Lock Order/Payment]
  LOCK --> ATT[Create or reuse provider attempt]
  ATT --> OUT[Return hosted form/QR facts]
  OUT --> CB[Provider IPN or bank webhook]
  CB --> AUTH{Secret/signature/timestamp valid?}
  AUTH -- no --> REJ[Reject without payment transition]
  AUTH -- yes --> DEDUP{Notification already processed?}
  DEDUP -- yes --> ACK[Idempotent acknowledgement]
  DEDUP -- no --> MATCH{Order, reference, amount, currency match?}
  MATCH -- no --> REC[Persist NeedsReconciliation]
  MATCH -- yes --> PAID[Apply Payment domain transition]
  PAID --> LOG[Persist transaction/notification and commit]
```

Management có reconciliation view/manual verification theo quyền. Redirect của browser không bao giờ là nguồn sự thật cho `Paid`.

## Inventory adjust, reserve, ship và return

```mermaid
flowchart LR
  INIT[Initialize zero InventoryLevel] --> ADJ[Management Adjust + stamp]
  ADJ --> MOV1[Adjustment movement]
  MOV1 --> LEVEL[InventoryLevel]
  LEVEL -->|Create Order| ALLOC[Active reservation / Allocate]
  ALLOC -->|Cancel or expiry| REL[Release movement]
  ALLOC -->|Ship| SHIP[Ship movement + reservation consumed]
  SHIP -->|Accepted return| RET[Return movement restores stock]
```

Mỗi operation kiểm quantity và concurrency. Không sửa balance hoặc movement cũ để “sửa tồn”; refund không nối tự động sang Return.

## Management Order và Shipment

```mermaid
flowchart TD
  LIST[Management list/detail/analytics] --> CONF[Confirm Order]
  CONF --> PREP[Prepare Shipment]
  PREP --> START[Start Shipment]
  START --> DONE[Complete Shipment]
  START --> FAIL[Mark Delivery Failed]
  FAIL --> RECEIVE[Receive accepted returned items]
  RECEIVE --> STOCK[Return inventory movement]
  LIST --> CANCEL[Cancel when invariant allows]
  LIST --> NOTE[Add internal note]
```

Order, Payment và Shipment có state machine riêng. Không cung cấp generic status patch; action chỉ hợp lệ khi current state và permission cho phép.

## Media upload đến Product

```mermaid
flowchart LR
  U[POST /api/v1/media multipart] --> P[MediaAsset Pending]
  P --> W[Scanner/processor worker]
  W --> C{Clean and processed?}
  C -- no --> F[Rejected/Failed; retry only if allowed]
  C -- yes --> PUB[Asset usable/public]
  PUB --> A[Attach ProductMedia + Product stamp]
  A --> M[Set primary/order/caption]
  M --> READ[Public Product reader]
```

Upload không đồng nghĩa public hoặc attached. Asset đang được tham chiếu không được xóa tùy ý.

## Password V2 và session

```mermaid
flowchart TD
  REG[Register] --> VERIFY[Email verification]
  VERIFY --> LOGIN[Password login]
  LOGIN --> SESSION[Access token + refresh session]
  SESSION --> REFRESH[Rotate refresh]
  REFRESH --> SESSION
  SESSION --> OWN[GET me / own session revoke]
  SESSION --> LOGOUT[Logout current or all]
  RECOVER[Forgot password] --> RESET[Protected reset token]
  RESET --> LOGIN
  SESSION --> CHANGE[Change/setup password + Bearer + CSRF]
```

Refresh rotation và revoke là server-side session facts. Không log token; Password V2 còn phụ thuộc effective feature/config và email delivery của môi trường.

