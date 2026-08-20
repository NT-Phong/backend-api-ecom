# Commerce Cart, Checkout and Order V1 — FE API Guide

**Contract version:** current V1 source, updated 2026-08-20.
**Base URL:** `/api/v1`.
**Audience:** storefront FE, management FE, and API-client agents.

This is the source-backed contract for Cart, Checkout, Order, Payment, Shipment, management orders, and revenue analytics. Use deployed HTTPS API only. FE must never access PostgreSQL, guest-token hashes, provider webhooks, payment-provider references, proof assets, or storage keys.

## 1. Global contract

### Response envelope

Every route returns `ApiResponse<T>`.

```json
{
  "success": true,
  "data": {},
  "message": "Success",
  "errorCode": null,
  "validationErrors": null,
  "details": null,
  "timestamp": "2026-08-20T10:00:00Z"
}
```

Persisted timestamps are UTC ISO-8601. Amounts are server-calculated VND decimals: format them as VND, but never recalculate totals on the client.

### Headers, cookies, and CSRF

```ts
const api = (path: string, init: RequestInit = {}) =>
  fetch(`/api/v1${path}`, {
    credentials: "include",
    headers: { Accept: "application/json", "Content-Type": "application/json", ...init.headers },
    ...init
  });
```

- Storefront Cart/Checkout/Order accepts an authenticated user or an anonymous guest browser.
- Guest ownership is the Secure/HttpOnly `__Host-ecom_cart` cookie, created by the server on the first guest add-to-cart. FE cannot read or construct it.
- Before a state-changing request, call `GET /security/csrf` once per browser session, then send the returned token in `X-CSRF-TOKEN` on every write. Keep `credentials: "include"` on all calls.
- Management routes require `Authorization: Bearer <accessToken>` plus the policy shown later. Management writes also require CSRF.
- Never log recipient PII, idempotency keys, QR payment data, payment form fields, or secret/signature values.

### Error handling

| HTTP | FE behavior |
| --- | --- |
| `400` | Render `validationErrors`; do not retry automatically. |
| `401` | Login/refresh session or retain guest flow. |
| `403` | Hide/disable the action; do not retry. |
| `404` | Show not found. A non-owned order intentionally returns `404`. |
| `409` | Idempotency conflict/processing: see CreateOrder retry rule. |
| `422` | Render `message`, then refresh Cart, Preview, or Order detail as relevant. |
| `429` | Respect `Retry-After` if present; do not loop. |
| `500` | Show generic error; retain user input locally. |

## 2. Vocabulary and authoritative rules

| Field | Values |
| --- | --- |
| `order.status` | `Pending`, `Confirmed`, `Preparing`, `Shipping`, `Completed`, `Cancelled`, `DeliveryFailed` |
| `payment.method` | `COD`, `BankTransfer`, `SePay`, `SePayVietQr`; `Gateway` is unsupported |
| `payment.status` | `Pending`, `AwaitingConfirmation`, `Paid`, `Failed`, `Refunded`, `Cancelled` |
| `shipment.status` | `Pending`, `Ready`, `Shipping`, `Delivered`, `DeliveryFailed`, `Cancelled` |
| management `sort` | `PlacedAtDescending`, `PlacedAtAscending`, `GrandTotalDescending`, `GrandTotalAscending` |
| analytics `granularity` | `Day`, `Week`, `Month` |

Cart changes never reserve stock. The server validates current prices/availability in Preview and locks/reserves tracked stock only inside successful CreateOrder.

```text
Product variant id
  -> Cart mutation (no reservation)
  -> PreviewCheckout (server quote)
  -> CreateOrder + Idempotency-Key
     -> lock Cart + active lines
     -> revalidate quote and inventory
     -> create Order snapshots + Payment + reservation/movements
     -> clear selected Cart lines
  -> owner order detail / payment UI
  -> management fulfillment and reporting
```

## 3. Storefront Cart and Checkout

### `GET /cart`

Returns the current authenticated/guest Cart or a successful empty Cart.

```json
{
  "id": "uuid-or-null",
  "status": "Active",
  "items": [
    { "id": "cart-item-uuid", "productVariantId": "variant-uuid", "quantity": 2 }
  ]
}
```

The Cart contract deliberately contains no product title, image, price, stock, or total. Resolve display content through the public Product API; PreviewCheckout is the purchasable-price authority.

### Cart writes

All require CSRF.

| Method and path | Body | Result |
| --- | --- | --- |
| `POST /cart/items` | `{ "productVariantId": "uuid", "quantity": 2 }` | Creates/uses owner Cart; duplicate variant is merged. Quantity per request is `1..999`. Product must be Published, Variant Active, and have an effective price. |
| `PATCH /cart/items/{cartItemId}` | `{ "quantity": 3 }` | Changes an owned active line. Non-owned/missing line is `404`. |
| `DELETE /cart/items/{cartItemId}` | none | Removes an owned active line. |
| `POST /cart/merge-guest` | none | Authenticated only. Merge guest cookie Cart into user Cart, then clear guest cookie. Call once after login. |

All successful Cart mutations return the Cart shape above. Do not show stock as reserved after adding/changing a Cart line.

### `POST /checkout/preview`

```json
{
  "cartItemIds": ["cart-item-uuid"],
  "recipientName": "Nguyễn An",
  "recipientPhone": "0900000000",
  "shippingAddress": "123 Đường A, Phường B",
  "administrativeAreaId": "uuid-or-null",
  "customerEmail": "an@example.com",
  "paymentMethod": "COD",
  "shippingMethodCode": "standard"
}
```

- `cartItemIds` must be non-empty active lines owned by the current user/guest.
- `shippingMethodCode` currently only supports `standard`.
- `Gateway` is rejected.
- Product/Variant eligibility, effective price, tracked availability, and shipping fee are all server-owned.

Success `data`:

```json
{
  "lines": [
    {
      "cartItemId": "uuid",
      "productVariantId": "uuid",
      "productName": "Mật ong rừng",
      "variantName": "Hũ 500g",
      "sku": "HONEY-500",
      "quantity": 2,
      "unitPrice": 120000,
      "lineTotal": 240000
    }
  ],
  "subtotalAmount": 240000,
  "shippingAmount": 30000,
  "grandTotalAmount": 270000,
  "quoteFingerprint": "64-char-sha256-hex",
  "quoteExpiresAt": "2026-08-20T10:05:00Z"
}
```

`quoteExpiresAt` is a refresh hint, not a guarantee. CreateOrder recalculates the quote regardless.

### `POST /orders`

Required header:

```http
Idempotency-Key: <new-uuid-or-opaque-unique-key>
```

```json
{
  "cartItemIds": ["cart-item-uuid"],
  "recipientName": "Nguyễn An",
  "recipientPhone": "0900000000",
  "shippingAddress": "123 Đường A, Phường B",
  "administrativeAreaId": "uuid-or-null",
  "customerEmail": "an@example.com",
  "paymentMethod": "SePayVietQr",
  "shippingMethodCode": "standard",
  "quoteFingerprint": "64-char-sha256-hex"
}
```

Success `data`:

```json
{
  "id": "order-uuid",
  "orderNumber": "DH-...",
  "status": "Pending",
  "paymentStatus": "Pending",
  "grandTotalAmount": 270000,
  "placedAt": "2026-08-20T10:00:00Z"
}
```

FE retry rule:

1. Generate the key once when customer confirms.
2. Disable submit while in flight.
3. On an unknown network outcome, retry the **identical body with the same key**.
4. On changed price/availability (`409`/`422`), fetch a new preview, show it, and use a **new key** only after customer confirms again.
5. Never send unit price, discounts, shipping fee, stock, payment status, or grand total.

## 4. Customer Order and Payment

### `GET /orders`

Returns all orders owned by authenticated user or guest cookie, newest first. Current V1 response is not paginated.

```json
[
  {
    "id": "uuid",
    "orderNumber": "DH-...",
    "status": "Pending",
    "paymentStatus": "AwaitingConfirmation",
    "grandTotalAmount": 270000,
    "placedAt": "2026-08-20T10:00:00Z"
  }
]
```

### `GET /orders/{orderId}`

Returns only an owned order; another user's/guest's UUID returns `404`.

```json
{
  "id": "uuid",
  "orderNumber": "DH-...",
  "status": "Shipping",
  "subtotalAmount": 240000,
  "discountAmount": 0,
  "shippingAmount": 30000,
  "grandTotalAmount": 270000,
  "currencyCode": "VND",
  "placedAt": "2026-08-20T10:00:00Z",
  "recipientName": "Nguyễn An",
  "recipientPhone": "0900000000",
  "shippingAddress": "123 Đường A",
  "items": [
    {
      "id": "order-item-uuid",
      "productVariantId": "uuid-or-null",
      "productName": "Mật ong rừng",
      "variantName": "Hũ 500g",
      "sku": "HONEY-500",
      "unitPrice": 120000,
      "quantity": 2,
      "discountAmount": 0,
      "lineTotalAmount": 240000
    }
  ],
  "payment": { "method": "COD", "status": "Pending", "amount": 270000, "dueAt": "2026-08-20T10:30:00Z", "paidAt": null },
  "shipment": null,
  "timeline": [
    { "type": "order", "status": "Pending", "occurredAt": "2026-08-20T10:00:00Z" }
  ]
}
```

Provider references, payment proof, guest-token hashes, internal notes, and staff identities are intentionally absent.

### `POST /orders/{orderId}/cancel`

```json
{ "reason": "Customer changed mind" }
```

Available to owner or `orders.manage` staff while the state allows cancellation and payment is not `Paid`. A paid order must use management refund. Active reservations are released server-side when applicable.

### SePay Hosted Checkout

#### `POST /orders/{orderId}/payments/sepay/checkout`

No body. Owner-only; requires `SePay`, `Pending`, and unexpired payment.

```json
{
  "orderId": "uuid",
  "actionUrl": "https://provider.example/...",
  "method": "POST",
  "fields": [{ "name": "invoice", "value": "..." }]
}
```

Build and submit a native form with the returned fields. Do not modify/sign fields and do not treat provider redirect as success. Redirect to a neutral order page and poll detail until `payment.status === "Paid"`.

### SePay VietQR

#### `POST /orders/{orderId}/payments/sepay-vietqr`

No body. Owner-only; requires `SePayVietQr`, `Pending`, and unexpired payment.

```json
{
  "orderId": "uuid",
  "qrImageUrl": "https://...",
  "bankCode": "...",
  "virtualAccountDisplay": "...",
  "accountHolder": "...",
  "amount": 270000,
  "currencyCode": "VND",
  "paymentCode": "DH...",
  "expiresAt": "2026-08-20T10:30:00Z"
}
```

Render server data only. Never generate QR payload or call provider webhooks. Poll order detail; displaying a QR never means paid.

## 5. Management Order APIs

All use `/management/orders`, Bearer auth, and the policy below.

| Capability | Policy |
| --- | --- |
| List, detail, analytics | `orders.read` |
| Confirm, cancel, note | `orders.manage` |
| Verify BankTransfer | `payments.verify` |
| Refund non-SePay payment | `payments.refund` |
| Fulfillment actions | `shipments.manage` |
| Receive stock return | `inventory.adjust` |

### `GET /management/orders`

Pagination query parameters:

| Name | Meaning |
| --- | --- |
| `q` | order number, recipient name, recipient phone |
| `status`, `paymentStatus`, `paymentMethod`, `shipmentStatus` | corresponding enum |
| `fromUtc`, `toUtc` | inclusive UTC ISO-8601 bounds |
| `sort` | default `PlacedAtDescending` |
| `page`, `pageSize` | 1-based, `pageSize` 1..100, default 20 |

```http
GET /api/v1/management/orders?status=Pending&paymentMethod=COD&fromUtc=2026-08-01T00:00:00Z&page=1&pageSize=20
```

`data` has `items`, `pageNumber`, `totalPages`, `totalCount`, `pageSize`, `hasPreviousPage`, `hasNextPage`. An item contains:

```json
{
  "id": "uuid", "orderNumber": "DH-...", "status": "Pending",
  "paymentStatus": "Pending", "paymentMethod": "COD", "shipmentStatus": null,
  "recipientName": "Nguyễn An", "recipientPhone": "0900000000", "itemCount": 2,
  "subtotalAmount": 240000, "discountAmount": 0, "shippingAmount": 30000,
  "grandTotalAmount": 270000, "currencyCode": "VND", "placedAt": "2026-08-20T10:00:00Z"
}
```

### `GET /management/orders/{orderId}`

Operational detail contains recipient/contact snapshot, immutable item snapshots, payment/shipment summaries, internal notes, and merged Order/Shipment timeline. It excludes provider references, payment proof, and guest-token hashes.

### Lifecycle actions

All routes below are `POST`, require CSRF, and return a success envelope with no data unless stated otherwise. Refetch detail after every success; never optimistically advance a status.

| Suffix | Policy | Body | Effect |
| --- | --- | --- | --- |
| `/{id}/confirm` | `orders.manage` | none | Pending → Confirmed; prepayment must be Paid; creates Shipment/confirmed hold. |
| `/{id}/cancel` | `orders.manage` | `{ "reason": "..." }` | Releases active reservation; paid order needs refund. |
| `/{id}/payment/verify-bank-transfer` | `payments.verify` | `{ "providerReference": "..." }` | BankTransfer only; marks exact server amount Paid. |
| `/{id}/payment/refund` | `payments.refund` | `{ "providerReference": "...", "reason": "..." }` | Full manual non-SePay refund, then cancels when allowed. |
| `/{id}/shipment/prepare` | `shipments.manage` | none | Confirmed/Pending shipment → Preparing/Ready. |
| `/{id}/shipment/start` | `shipments.manage` | `{ "carrierName": "...", "trackingCode": "..." }` | Starts delivery and consumes reservation/stock. |
| `/{id}/shipment/complete` | `shipments.manage` | none | Shipping → Completed/Delivered; pending COD becomes Paid. |
| `/{id}/shipment/delivery-failed` | `shipments.manage` | `{ "reason": "..." }` | Shipping → DeliveryFailed; no automatic refund/stock return. |
| `/{id}/shipment/receive-return` | `inventory.adjust` | see below | Physically receives failed-delivery stock. |
| `/{id}/notes` | `orders.manage` | `{ "content": "..." }` | Creates internal note; returns note DTO. |

#### `POST /management/orders/{orderId}/shipment/receive-return`

```json
{
  "items": [{ "orderItemId": "uuid", "quantity": 1 }],
  "reason": "Carrier returned parcel"
}
```

Only tracked lines with consumed reservations can be received. Order must be `DeliveryFailed` or `Cancelled`, while Shipment stays `DeliveryFailed`. The server locks inventory and prevents cumulative returned quantity exceeding shipped quantity.

Success `data` is an array:

```json
[
  {
    "id": "movement-uuid", "inventoryItemId": "uuid", "stockLocationId": "uuid",
    "orderItemId": "uuid", "movementType": "Return", "quantityDelta": 1,
    "reason": "Carrier returned parcel", "occurredAt": "2026-08-20T10:00:00Z"
  }
]
```

Refund and physical return are separate. Refund never increases stock; expose them as distinct staff-confirmed UI actions.

## 6. Revenue dashboard

### `GET /management/orders/analytics/overview`

Requires `orders.read`.

| Query | Meaning |
| --- | --- |
| `from`, `to` | inclusive local date `YYYY-MM-DD` in `Asia/Ho_Chi_Minh` |
| `granularity` | `Day`, `Week`, `Month`; default `Day` |
| `topLimit` | 1..50; default 10 |

Default range is the latest 30 local days; max requested span is 366 days. Do not send UTC timestamps for `from`/`to`.

```json
{
  "currencyCode": "VND",
  "from": "2026-07-22",
  "to": "2026-08-20",
  "kpis": {
    "ordersPlaced": 12, "completedOrderCount": 8,
    "collectedGross": 3200000, "refundAmount": 200000,
    "netCollected": 3000000, "completedSales": 2800000
  },
  "series": [{
    "period": "2026-08-20", "ordersPlaced": 1, "completedOrderCount": 1,
    "collectedGross": 270000, "refundAmount": 0, "netCollected": 270000, "completedSales": 270000
  }],
  "statusBreakdown": [{ "status": "Completed", "orderCount": 8, "grandTotalAmount": 2800000 }],
  "paymentMethodBreakdown": [{ "paymentMethod": "COD", "collectedGross": 1000000, "refundAmount": 0, "netCollected": 1000000 }],
  "topProducts": [{
    "productVariantId": "uuid-or-null", "productName": "Mật ong rừng", "variantName": "Hũ 500g",
    "sku": "HONEY-500", "quantitySold": 10, "salesAmount": 1200000
  }]
}
```

Metric rules:

- `ordersPlaced`: Order `PlacedAt` in selected local interval.
- `collectedGross`: Paid capture/verify transaction by occurrence time.
- `refundAmount`: Refunded transaction by occurrence time.
- `netCollected = collectedGross - refundAmount`.
- `completedSales`: Order grand total whose status changed to Completed in interval. It is fulfillment revenue, not cash collection.
- `topProducts`: immutable completed `OrderItem` snapshots, never mutable Catalog data.

This is cash/sales reporting only: no profit, COGS, commission, operating expense, or producer revenue.

## 7. FE workflow rules and unsupported scope

1. After CreateOrder, navigate/refetch `GET /orders/{id}`.
2. For SePay/VietQR, poll owned detail on a bounded interval while payment is `Pending`/`AwaitingConfirmation`; stop on Paid, terminal status, expiry, or page leave.
3. After management mutation, refetch detail and visible list/analytics queries.
4. Do not mutate local stock/payment state from DeliveryFailed, refund, or receive-return before server success.
5. Never blindly retry `409`. Reuse an idempotency key only for the identical CreateOrder request.

Not currently available: customer self-service refund/return, partial payment/refund, coupon/promotion, multi-vendor checkout, producer settlement, customer order-list pagination/filtering, and FE access to provider webhook/reconciliation APIs.

## 8. Readiness boundary

This guide reflects current source and DTOs. Core build evidence exists, but PostgreSQL concurrency and API runtime acceptance remain pending. FE must retain CSRF/cookie, idempotency, quote-refresh, and post-action-refetch behavior even if local calls appear successful.
