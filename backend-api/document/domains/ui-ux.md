# UI/UX contract cho Commerce

Tài liệu này mô tả trải nghiệm mà backend contract phải hỗ trợ; không áp đặt framework frontend. Mọi màn hình dùng dữ liệu server-authoritative và thể hiện rõ trạng thái chưa có dữ liệu, không đủ quyền, stale conflict và external dependency unavailable.

## Screen groups

- Storefront: product/category list/detail, variant selection, cart, checkout, order history/detail/payment status.
- Backoffice: dashboard; Product editor; Producer; Inventory; Order/Payment/Shipment; Media; Settings/Audit/Security.
- Auth: login/register/OTP/password/session; Demo QR phải hiển thị rõ demo-only.

## Storefront information architecture

| Screen | Dữ liệu chính | CTA/guard |
| --- | --- | --- |
| Home/discovery | category, published product, campaign khi có API | không giả trust badge |
| Product list | public list, query/filter/sort/paging | placeholder nếu no media/price |
| Product detail | content, producer, categories, media, variants/options | phải chọn active purchasable variant |
| Cart | CartDto và cartItem IDs | quantity 1–999, unavailable item error |
| Checkout | recipient/address, payment method, preview quote | preview trước create order |
| Payment | Hosted form hoặc VietQR intent | poll/read Order; redirect không mark Paid |
| Order history/detail | customer-owned snapshot/timeline | action theo current status |

## Backoffice information architecture

| Workspace | Screens |
| --- | --- |
| Dashboard | date range, KPIs, order/catalog/producer/inventory/user snapshot |
| Producer | list, detail, create/edit, verify/publish/hide, contacts/facility |
| Catalog | product list, multi-step editor, category tree, media/variant/price/lifecycle |
| Inventory | locations, levels, movement ledger, initialize/adjust modal |
| Orders | queue, detail/timeline, payment, shipment, internal note |
| System | shipping setting, audit, sessions, security events |

Product editor nên lưu theo mutation nhỏ tuần tự: root → categories → options/values → variants/mapping → prices → media → review/publish. Không có một request “save all Product”. Mỗi bước sử dụng latest concurrency stamp.

## UI state bắt buộc

Mỗi screen có loading, empty, success, validation error, authorization error, conflict/stale state, transient dependency error. Mutation disable double-submit nhưng vẫn dựa vào idempotency/concurrency server.

- Loading: skeleton/spinner không xóa data cũ cần so sánh.
- Empty: phân biệt chưa có record với filter không match.
- Validation: đặt message gần field từ `validationErrors`.
- Conflict: reload server version và để người dùng quyết định apply lại.
- Partial/dependency: giữ state nhập liệu, cho retry có chủ đích.
- Success: cập nhật ID/stamp/status từ response, không tự đoán.

## Mapping rules

- Public reads không tái dùng management DTO.
- Giá/tồn/tổng tiền/server status là read-only facts.
- Variant picker giữ `variant.id`; cart dùng `cartItem.id`; editor giữ latest `concurrencyStamp`; checkout giữ latest quote fingerprint.
- Payment UI poll/read Order; không đánh dấu Paid từ URL redirect hoặc QR display.
- Upload hiển thị Pending/Rejected/Failed/Clean; chỉ attach/show trusted URL khi server cho phép.

## Cache và refresh

- Public product/category cache tách khỏi management cache.
- Product publish/pause/media/price change invalidate management detail và affected public queries.
- Cart mutation thay CartDto bằng response hoặc refetch; guest merge luôn refetch.
- Inventory adjustment invalidate levels + movements.
- Order management action refetch order detail, list/analytics liên quan.
- Producer publish/hide invalidate Catalog producer picker.
- Không cache CSRF token, refresh token hoặc private URL trong public persistent storage.

## Error UX

401 → login/refresh theo contract; 403 → thiếu quyền, không giả là 404; 404 → resource cụ thể; 409 → reload và cho người dùng so sánh; 422 → business validation; 503 → dependency/config unavailable. Không tự sửa message server thành lỗi authentication chung nếu mã lỗi khác.

429 hiển thị countdown/Retry-After. Unknown 500 không tự resend mutation. Network timeout sau write phải read/reconcile trước. Với backend `CSRF_INVALID`, lấy token mới và retry đúng một lần; 400/403 khác không đi vào CSRF retry loop.

## BFF/cookie

Forward toàn bộ relevant `Set-Cookie`; giữ credentials, CSRF header/cookie pair; không log cookie/token. Kiểm browser Network từ FE → BFF → backend trước khi kết luận route backend sai.

## Accessibility và trust

Form có label/error focus, keyboard flow, mobile layout và confirm cho destructive/lifecycle action. Không hiển thị OCOP/VietGAP/chứng nhận/available stock nếu API không trả fact tương ứng. Money định dạng theo currency nhưng không dùng float client để tính total. Date hiển thị local timezone từ ISO UTC, giữ raw UTC cho request/filter.
