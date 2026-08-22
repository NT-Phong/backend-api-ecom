# Order, Payment và Shipment

## Aggregate và authority

Order là snapshot thương mại của checkout. Payment quản lý nghĩa vụ thanh toán; Shipment quản lý fulfillment. Ba state machine liên quan nhưng không đồng nhất: Paid không tự nghĩa là Delivered, Refund không tự restock, redirect success không tự Paid.

## Customer API

`GET /orders`, `GET /orders/{id}`, `POST /orders`, `POST /orders/{id}/cancel`, `POST /orders/{id}/payments/sepay/checkout`, `POST /orders/{id}/payments/sepay-vietqr`.

Customer list/detail chỉ trả order thuộc authenticated user hoặc guest principal phù hợp. Summary gồm `id`, order number, order/payment status, total và placed time. Detail gồm recipient snapshot, immutable items, payment current state, shipment current state và timeline customer-safe.

Cancel request `{ "reason": "..." }`; domain chặn hủy ở state không cho phép và release active reservation khi hủy thành công.

## Provider API

`POST /payments/sepay/ipn` nhận JSON + `X-Secret-Key`. `POST /payments/sepay-bank/webhook` xác thực timestamp/signature trên raw body. Provider notification phải idempotent/auditable; redirect success/cancel không phải payment authority.

### SePay Hosted Checkout

`POST /orders/{id}/payments/sepay/checkout` trả `orderId`, provider `actionUrl`, HTTP method và danh sách form fields đã ký. FE dựng form đúng như response, không sửa amount/currency/invoice/signature. Action có thể tạo hoặc reuse attempt hợp lệ; timeout cần đọc lại Order/reconcile trước khi tạo attempt khác.

IPN chứa order/invoice và transaction facts. Backend kiểm secret, provider reference, amount/currency/order ownership/status và duplicate notification trước khi mark Paid. Notification không khớp hoặc ambiguous đi `NeedsReconciliation`, không được ép Paid.

### SePay VietQR/bank webhook

`POST /orders/{id}/payments/sepay-vietqr` trả `orderId`, `qrImageUrl`, bank code, virtual account display, amount/content và expiry theo DTO. QR display chỉ là payment intent. Bank webhook phải xác thực timestamp và signature trên **raw body**; proxy không được parse/re-serialize trước validation.

## Payment methods

- `COD`: payment thường Pending/AwaitingConfirmation theo order flow, hoàn tất theo business operation tương ứng.
- `BankTransfer`: staff xác minh bằng provider reference qua management API.
- `SePay`: Hosted Checkout + IPN.
- `SePayVietQr`: in-page QR + signed bank webhook.
- `Gateway`: enum tồn tại nhưng create-order validator hiện không chấp nhận generic Gateway.

## Management API

`/management/orders`: list/detail/analytics, confirm/cancel, verify bank transfer/refund, shipment prepare/start/complete/delivery-failed/receive-return, internal note. `/management/payments/sepay/reconciliation` cho `Payments.Verify`.

Management list filter gồm search order number/recipient, order status, payment status, UTC range và paging. Detail có item snapshots, payment/shipment, notes và timeline. Internal note request chỉ có `content` tối đa theo validator; backend force note internal/non-customer-visible.

| Action | Body chính | Permission |
| --- | --- | --- |
| Confirm | none | `orders.manage` |
| Cancel | `reason` | `orders.manage` |
| Verify bank transfer | `providerReference` | `payments.verify` |
| Refund | `providerReference`, `reason` | `payments.refund` |
| Prepare shipment | none | `shipments.manage` |
| Start shipment | optional `carrierName`, `trackingCode` | `shipments.manage` |
| Complete shipment | none | `shipments.manage` |
| Delivery failed | `reason` | `shipments.manage` |
| Receive returned items | item IDs/quantities + reason | `inventory.adjust` |

## State machines

- Order: Pending → Confirmed → Preparing → Shipping → Completed; nhánh Cancelled/DeliveryFailed.
- Payment: Pending/AwaitingConfirmation → Paid; nhánh Failed/Cancelled/Refunded.
- Shipment: Pending → Ready → Shipping → Delivered; nhánh DeliveryFailed/Cancelled.

```mermaid
flowchart TD
  OP[Order Pending] --> OC[Confirmed]
  OC --> PR[Preparing]
  PR --> SH[Shipping]
  SH --> DONE[Completed]
  OP --> CAN[Cancelled]
  OC --> CAN
  SH --> FAIL[DeliveryFailed]
  PAYP[Payment Pending/Awaiting] --> PAID[Paid]
  PAID --> REF[Refunded]
  RES[Reservation Active] --> CONS[Consumed on Ship]
  RES --> REL[Released on Cancel/Expiry]
  FAIL --> RET[Receive Return movement]
```

## Inventory effects

- Checkout preview: không reserve.
- Create Order: `Allocate`, tăng reserved, tạo reservation.
- Cancel/reservation expiry: `Release`, giảm reserved.
- Ship: `Ship`, giảm stocked và reserved, consume reservation.
- Receive returned items: `Return`, tăng stocked theo accepted quantity.
- Refund: chỉ payment transaction/state; không tự tạo Return movement.

## Idempotency và reconciliation

Provider notification có unique/idempotency facts. Duplicate hợp lệ không tạo second capture. Amount/reference/order mismatch hoặc crash boundary cần reconciliation. Không retry mù IPN/webhook từ FE; provider/ops dùng contract riêng. Management reconciliation đọc attempts/notifications cần xử lý, không phải công cụ sửa trực tiếp bằng client amount.

State chuyển bằng domain command, không PATCH enum trực tiếp. Unknown payment/write result cần reconcile/read authoritative state trước khi thử lại.

## Error UX

409 dùng cho duplicate/conflict/ambiguous state; 422 cho invalid transition, insufficient/released reservation hoặc business guard; 401/403 theo ownership/policy. Sau mọi management action, refetch detail/list thay vì optimistic state vì một action có thể đồng thời đổi Order, Payment, Shipment và Inventory.
