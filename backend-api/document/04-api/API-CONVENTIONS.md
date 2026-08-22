# API conventions và contract chung

## Transport

- Prefix: `/api/v1` cho API version 1.
- Response dùng `ApiResponse<T>`; expected failure đi qua `TResult`/`ErrorCodes`.
- Public DTO và management DTO là hai boundary khác nhau.
- Route ID được controller chép vào command/query, không tin body ID mâu thuẫn.

Envelope chuẩn:

```json
{
  "success": true,
  "data": {},
  "message": "Success",
  "errorCode": null,
  "validationErrors": null,
  "details": null,
  "timestamp": "2026-08-22T00:00:00Z"
}
```

Failure có `success=false`, `data=null`, stable `errorCode`, optional field `validationErrors`. `details` không được chứa exception/secret. Enum là string đúng spelling; time là ISO-8601 UTC; money là JSON decimal/number nhưng client phải dùng decimal-safe handling.

Pagination:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## Security

Route có antiforgery lấy token tại `GET /api/v1/security/csrf` (hoặc `/api/v2/auth/csrf` cho authenticated V2 flow), giữ cookie `__Host-ecom_csrf`, gửi header `X-CSRF-TOKEN`. Nhóm này gồm Cart/Address/Checkout/Order, Management mutations, Password V2 change/setup và media retry. Catalog mutations hiện dùng Bearer + policy + concurrency nhưng không có antiforgery attribute; Agent không được tự mô tả ngược lại. SePay IPN/webhook dùng secret/signature riêng và không dùng CSRF.

Không log Authorization, access/refresh/guest token, CSRF cookie/header, payment signature/secret, password/OTP, raw upload hoặc private customer data. Public endpoint chỉ trả public DTO; management endpoint không được gọi anonymous.

## Concurrency, idempotency, retry

- Management/Catalog mutation có concurrency stamp ở contract tương ứng; 409 cần reload, không replay mù.
- `POST /orders` bắt buộc `Idempotency-Key`; cùng key chỉ dùng lại cho cùng payload.
- Không tự retry unknown write, 409, 500, webhook hoặc CSRF-unrelated 400/403.

## HTTP/error map

| Status | Nghĩa | Client action |
| --- | --- | --- |
| 200/201 | success/read hoặc created | dùng `data`, stamp/ID/status trả về |
| 400 | bind/validation/bad request | map `validationErrors`, không retry |
| 401 | unauthenticated/expired | refresh đúng flow một lần hoặc login |
| 403 | thiếu policy/ownership | ẩn CTA, không retry |
| 404 | route/resource/child scope không tồn tại | refetch/quay list |
| 409 | duplicate, stale stamp, fingerprint/idempotency conflict | reload/reconcile, không replay mù |
| 422 | business invariant/state/availability | giữ form, hiển thị message, refetch detail |
| 429 | rate limit | dùng Retry-After/countdown |
| 500 | unexpected | không tự resend mutation |
| 502/503 | external/config dependency unavailable | retry có chủ đích, giữ state |

## Checklist khi document endpoint

Route/method; auth/policy; CSRF/rate limit; request + validator; response DTO/envelope; ownership; state transition; transaction; error codes; concurrency/idempotency; external effect; tests và runtime gate.
