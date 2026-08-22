# Cart và Checkout

## Mục tiêu và ownership

Cart cho phép guest hoặc authenticated user lưu các ProductVariant muốn mua. Mỗi principal chỉ thao tác cart của mình; guest ownership dùng protected cookie/token hash, user ownership dùng UserId. Checkout không nhận giá/tổng từ client mà dựng quote mới từ cart, catalog, inventory và setting phí giao hàng.

## API

`GET /cart`; `POST /cart/items`; `PATCH|DELETE /cart/items/{cartItemId}`; authenticated `POST /cart/merge-guest`; `POST /checkout/preview`; authenticated customer-address CRUD tại `/customer/addresses`; `POST /orders` là ranh giới chuyển sang Order.

Mọi cart/checkout/order mutation browser dùng CSRF và rate limit. Cart read không cần body. API trả envelope chuẩn; `CartDto` gồm optional cart `id`, status và `items`.

## Nghiệp vụ

Cart item dùng `ProductVariantId`, quantity 1–999. Server yêu cầu Product Published, Variant Active và effective price. Guest cart có principal/cookie riêng; merge chỉ sau authentication và clear guest principal khi thành công.

### Add/change/remove

```json
{ "productVariantId": "uuid", "quantity": 2 }
```

Add cùng variant cộng/cập nhật item theo aggregate rule. Change quantity dùng `cartItemId` từ response, không dùng variant ID trên URL. Remove chỉ xóa item thuộc active cart hiện tại. Cart expired của guest có thể rotate principal và tạo cart mới; user cart expired trả business error thay vì âm thầm đổi owner.

### Guest merge

Sau login, `POST /cart/merge-guest` khóa/đọc cả guest và user cart, gộp item cùng variant theo rule quantity và giữ ownership user. Chỉ clear guest principal sau khi transaction thành công. Nếu gọi đồng thời, backend lock/idempotent state là authority; FE reload cart sau merge.

## Customer address

Authenticated customer có thể list/create/update/delete và set default. Address gồm `administrativeAreaId?`, `recipientName`, `phoneNumber`, `addressLine`, `postalCode?`, `label?`, `isDefault` khi create. Server đảm bảo ownership và default semantics; checkout vẫn nhận recipient snapshot để Order không phụ thuộc address bị sửa sau này.

Preview nhận cart-item IDs, recipient, optional administrative area/email, payment method và `ShippingMethodCode` hiện chỉ `standard`. Server trả quote và fingerprint; preview không reserve tồn.

Preview request:

```json
{
  "cartItemIds": ["cart-item-uuid"],
  "recipientName": "Nguyễn Văn A",
  "recipientPhone": "0900000000",
  "shippingAddress": "Địa chỉ giao hàng",
  "administrativeAreaId": null,
  "customerEmail": "a@example.com",
  "paymentMethod": "SePayVietQr",
  "shippingMethodCode": "standard"
}
```

Preview response data gồm `lines`, `subtotalAmount`, `shippingAmount`, `grandTotalAmount`, currency/payment facts và `quoteFingerprint`. Mỗi line có `cartItemId`, `productVariantId`, product/variant/SKU, unit price, quantity, line total và tracking/availability facts theo DTO. Shipping fee lấy typed setting `checkout.shipping.standardFeeVnd`; missing/invalid config có thể trả `SERVICE_UNAVAILABLE`.

Create order nhận cùng facts, thêm 64-char `QuoteFingerprint` và header `Idempotency-Key`. Handler khóa cart/tồn tracked, kiểm quote lại và tạo order/payment/reservation atomically.

```http
POST /api/v1/orders
Idempotency-Key: checkout-<stable-client-key>
X-CSRF-TOKEN: <token>
```

```json
{
  "cartItemIds": ["cart-item-uuid"],
  "recipientName": "Nguyễn Văn A",
  "recipientPhone": "0900000000",
  "shippingAddress": "Địa chỉ giao hàng",
  "administrativeAreaId": null,
  "customerEmail": "a@example.com",
  "paymentMethod": "SePayVietQr",
  "quoteFingerprint": "64-hex-characters",
  "shippingMethodCode": "standard"
}
```

CreateOrder: resolve principal → begin idempotency record → lock active cart → calculate quote lại → compare fingerprint → lock tracked inventory → create Order/OrderItems/history/Payment → reserve levels + movements → remove selected cart items/convert cart khi phù hợp → complete idempotency record → commit một lần.

## Failure semantics

| Trường hợp | Kết quả mong đợi |
| --- | --- |
| Variant không active/Product không published/no price | 422 unavailable |
| Quantity ngoài 1–999 | 400 validation |
| CartItem không thuộc owner | not found/forbidden theo contract |
| Shipping setting thiếu/sai | 503 service unavailable |
| Quote hoặc availability đổi | 409/conflict; preview lại |
| Idempotency key cùng payload đã complete | trả result Order trước đó |
| Key đang processing hoặc reuse payload khác | conflict; không tạo duplicate |
| Tracked inventory không đủ | 422; transaction rollback |

## FE behavior

Luôn dùng server `CartItemDto.Id`; không gửi ProductId/SKU thay Variant ID. Khi fingerprint đổi, reload cart + preview. Với `CSRF_INVALID`, refresh token và retry tối đa một lần; không blind retry conflict/unknown write.

UI phải giữ form recipient khi preview lỗi; disable double submit; dùng decimal-safe display; không optimistic-remove cart items trước create order success. Sau success điều hướng theo `orderId`, không dựa vào client-generated number.
