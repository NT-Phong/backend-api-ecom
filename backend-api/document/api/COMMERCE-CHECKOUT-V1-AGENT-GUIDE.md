# Commerce Checkout V1 API

All responses use the existing `ApiResponse<T>` envelope. Currency is VND and all monetary values are server-calculated.

## Storefront flow

1. `POST /api/v1/cart/items` with `{ productVariantId, quantity }` creates a user or guest Cart. The guest token is an opaque `__Host-ecom_cart` HttpOnly cookie; clients never receive its database hash.
2. `POST /api/v1/checkout/preview` with selected `cartItemIds`, recipient snapshot, `shippingMethodCode` (`standard`) and `paymentMethod` returns line prices, availability, totals, and `quoteFingerprint`.
3. `POST /api/v1/orders` repeats the selection/snapshot/fingerprint and includes a unique `Idempotency-Key` header. The client never submits unit prices, discounts, inventory, or grand total.

The server recalculates price and availability before creating the order. A changed fingerprint, inactive variant, or insufficient stock is a business conflict; refetch preview and ask the customer to confirm again.

## Other routes

- `GET /api/v1/cart`, `PATCH /api/v1/cart/items/{cartItemId}`, `DELETE /api/v1/cart/items/{cartItemId}`.
- Authenticated users can merge the browser guest cart with `POST /api/v1/cart/merge-guest`.
- Authenticated address CRUD is available under `/api/v1/customer/addresses`.
- Staff routes are under `/api/v1/management/orders` and require Commerce policies; they confirm orders, verify manual transfer, and prepare/start/complete shipment.

## Idempotency

The same `Idempotency-Key` and identical request returns the original order. Reusing a key for a different request is a `409`. A request already executing is also retryable as `409`; clients must keep the same key when retrying a timeout.
