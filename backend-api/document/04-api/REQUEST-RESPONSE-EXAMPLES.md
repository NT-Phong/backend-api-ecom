# Request/Response examples

Các JSON dưới đây minh họa field và ownership của contract. Tất cả response thực tế được bọc trong `ApiResponse<T>`; ví dụ chỉ hiển thị phần `data` khi cần ngắn gọn.

## Public Product list item

```json
{
  "id": "uuid",
  "slug": "mat-ong-rung",
  "name": "Mật ong rừng",
  "shortDescription": "Sản phẩm địa phương",
  "producer": { "id": "uuid", "code": "HTX-01", "name": "HTX A", "description": null, "websiteUrl": null },
  "primaryCategory": { "id": "uuid", "name": "Mật ong", "slug": "mat-ong", "isPrimary": true, "displayOrder": 0 },
  "primaryMedia": null,
  "fromPrice": 150000,
  "currencyCode": "VND",
  "hasEffectivePrice": true,
  "publishedAt": "2026-08-22T00:00:00Z"
}
```

Public detail thêm content/SEO, categories, media và variants. Variant response có `id`, `sku`, `name`, `price`, `currencyCode`, `priceType`, optional `weightGrams`, `options[]`.

## Catalog management list item

```json
{
  "id": "uuid",
  "producerId": "uuid",
  "name": "Mật ong rừng",
  "slug": "mat-ong-rung",
  "status": "Published",
  "createdAt": "2026-08-20T00:00:00Z",
  "updatedAt": "2026-08-22T00:00:00Z",
  "primaryCategory": null,
  "price": { "fromAmount": 150000, "currencyCode": "VND", "hasEffectivePrice": true },
  "inventory": { "stockedQuantity": 100, "reservedQuantity": 3, "availableQuantity": 97, "incomingQuantity": 0, "isTracked": true },
  "primaryMedia": null,
  "brandName": "Đặc sản Thanh Hóa"
}
```

Management detail thêm root descriptions, status dates, `concurrencyStamp`, categories, media metadata, variants và price periods.

## Cart

Request add:

```json
{ "productVariantId": "uuid", "quantity": 2 }
```

Response data:

```json
{
  "id": "cart-uuid",
  "status": "Active",
  "items": [{ "id": "cart-item-uuid", "productVariantId": "variant-uuid", "quantity": 2 }]
}
```

`items[].id`, không phải `productVariantId`, được gửi trong `cartItemIds` của preview/create order.

## Checkout preview

```json
{
  "lines": [{
    "cartItemId": "cart-item-uuid",
    "productVariantId": "variant-uuid",
    "productName": "Mật ong rừng",
    "variantName": "Hũ 500g",
    "sku": "HONEY-500G",
    "quantity": 2,
    "unitPrice": 150000,
    "lineTotal": 300000
  }],
  "subtotalAmount": 300000,
  "shippingAmount": 30000,
  "grandTotalAmount": 330000,
  "quoteFingerprint": "64-hex-characters",
  "quoteExpiresAt": "2026-08-22T01:00:00Z"
}
```

## Create Order success

```json
{
  "id": "order-uuid",
  "orderNumber": "ORD-20260822-...",
  "status": "Pending",
  "paymentStatus": "Pending",
  "grandTotalAmount": 330000,
  "placedAt": "2026-08-22T00:00:00Z"
}
```

Order detail giữ recipient và item snapshot:

```json
{
  "id": "order-uuid",
  "orderNumber": "ORD-...",
  "status": "Pending",
  "subtotalAmount": 300000,
  "discountAmount": 0,
  "shippingAmount": 30000,
  "grandTotalAmount": 330000,
  "currencyCode": "VND",
  "recipientName": "Nguyễn Văn A",
  "recipientPhone": "0900000000",
  "shippingAddress": "Địa chỉ",
  "items": [{ "id": "uuid", "productVariantId": "uuid", "productName": "Mật ong rừng", "variantName": "Hũ 500g", "sku": "HONEY-500G", "unitPrice": 150000, "quantity": 2, "discountAmount": 0, "lineTotalAmount": 300000 }],
  "payment": { "method": "SePayVietQr", "status": "Pending", "amount": 330000, "dueAt": null, "paidAt": null },
  "shipment": null,
  "timeline": []
}
```

## SePay Hosted Checkout

```json
{
  "orderId": "uuid",
  "actionUrl": "provider-url",
  "method": "POST",
  "fields": [{ "name": "field-name", "value": "provider-signed-value" }]
}
```

FE submit đúng method/action/fields; không sửa amount/signature.

## SePay VietQR

```json
{
  "orderId": "uuid",
  "qrImageUrl": "provider-or-storage-url",
  "bankCode": "BANK",
  "virtualAccountDisplay": "account",
  "accountHolder": "merchant",
  "amount": 330000,
  "currencyCode": "VND",
  "paymentCode": "order-payment-code",
  "expiresAt": "2026-08-22T01:00:00Z"
}
```

## Inventory level và movement

```json
{
  "inventoryItemId": "uuid",
  "productVariantId": "uuid",
  "sku": "HONEY-500G",
  "productName": "Mật ong rừng",
  "variantName": "Hũ 500g",
  "stockLocationId": "uuid",
  "stockLocationCode": "MAIN",
  "stockedQuantity": 100,
  "reservedQuantity": 3,
  "incomingQuantity": 0,
  "availableQuantity": 97
}
```

```json
{
  "id": "movement-uuid",
  "inventoryItemId": "uuid",
  "stockLocationId": "uuid",
  "orderItemId": null,
  "movementType": "Adjust",
  "quantityDelta": 25,
  "reason": "Kiểm kê đầu kỳ",
  "occurredAt": "2026-08-22T00:00:00Z"
}
```

## Producer management

```json
{
  "id": "uuid",
  "code": "HTX-01",
  "name": "HTX A",
  "legalName": null,
  "description": null,
  "websiteUrl": null,
  "publicStatus": "Published",
  "isVerified": true,
  "verifiedAt": "2026-08-22T00:00:00Z",
  "verifiedByUserId": "uuid",
  "concurrencyStamp": "uuid",
  "contacts": [],
  "facilities": []
}
```

## Validation failure

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errorCode": "BAD_REQUEST",
  "validationErrors": { "quantity": ["Quantity must be between 1 and 999."] },
  "details": null,
  "timestamp": "2026-08-22T00:00:00Z"
}
```
