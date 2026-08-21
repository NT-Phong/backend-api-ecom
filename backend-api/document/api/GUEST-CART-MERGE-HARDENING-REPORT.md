# Báo cáo nghiệp vụ và nâng cấp Guest Cart Merge

**Phạm vi:** Storefront Cart/Checkout backend  
**Cập nhật:** 2026-08-22  
**Trạng thái source:** Đã build; PostgreSQL integration tests chưa được chạy vì môi trường chưa cấu hình test database.

## 1. Mục tiêu nghiệp vụ

Khách chưa đăng nhập có thể thêm sản phẩm vào giỏ hàng bằng guest cart. Sau khi đăng nhập, hệ thống phải chuyển các dòng hàng của guest cart sang active cart của user để user tiếp tục checkout mà không mất sản phẩm hoặc bị cộng số lượng hai lần.

Mục tiêu của đợt nâng cấp:

1. Không vô hiệu hóa CSRF để đổi lấy việc hết lỗi `400`.
2. Không tạo nhiều active cart cho một user khi login/merge/add item chạy đồng thời.
3. Request merge bị gửi lại sau khi server đã commit vẫn phải thành công an toàn.
4. Cookie guest chỉ được xóa khi merge đã hoàn tất thành công.
5. FE/BFF có tín hiệu rõ ràng khi CSRF sai và khi cần truy vết lỗi server.

## 2. Các khái niệm và bất biến dữ liệu

| Khái niệm | Nguồn dữ liệu | Quy tắc |
| --- | --- | --- |
| Guest cart | `Tbl_Cart.GuestTokenHash` | Chỉ lưu SHA-256 hash của token cookie; không lưu token gốc. |
| User cart | `Tbl_Cart.UserId` | Một cart đang `Active` cho mỗi user. |
| Chủ sở hữu cart | `UserId XOR GuestTokenHash` | Cart thuộc đúng một trong hai loại chủ sở hữu. |
| Dòng cart | `Tbl_CartItem` | Một active line cho mỗi cặp `CartId + ProductVariantId`. |
| Merge thành công | `CartStatus.Active -> CartStatus.Converted` | Source guest cart bị chuyển trạng thái; item được cộng sang target cart. |
| Xóa cookie guest | HTTP response | Chỉ làm sau khi transaction merge trả thành công. |

Database đã có partial unique index bảo vệ một active cart cho user và một active cart cho guest token. Index là lớp bảo vệ cuối; code vẫn phải điều phối concurrent request để không biến unique violation thành `500`.

## 3. Luồng nghiệp vụ trước nâng cấp và nguyên nhân lỗi

```text
Guest thêm hàng
  -> Browser giữ __Host-ecom_cart
  -> Guest cart Active trong database
  -> User đăng nhập
  -> FE gọi POST /api/v1/cart/merge-guest
  -> Nếu merge không chạy hoặc bị lỗi
  -> Checkout dùng authenticated UserId nhưng không có active user cart
  -> checkout/preview trả NOT_FOUND: Active cart was not found
```

Các nguyên nhân đã xác định từ source/log:

1. `POST /api/v1/cart/merge-guest` bắt buộc CSRF. Thiếu cookie CSRF hoặc header `X-CSRF-TOKEN` làm request trả `400` trước khi vào handler merge.
2. Trước hardening, merge retry sau khi source cart đã `Converted` có thể trả lỗi thay vì xác nhận kết quả đã commit.
3. `MergeGuestCart` và `AddCartItem` đều có nhánh "không tìm thấy active cart thì tạo". Hai request đồng thời có thể cùng tạo cart cho một user, vi phạm partial unique index và sinh `500`.
4. Console browser chỉ chứa HTTP status/generic message không đủ để xác định lỗi database. Cần trace ID để nối với backend log.

Lỗi lazy-loading hình ảnh của browser không liên quan đến state cart, merge hoặc checkout.

## 4. Luồng sau nâng cấp

```text
1. FE/BFF lấy CSRF
   GET /api/v1/security/csrf
   <- token trong response + __Host-ecom_csrf cookie

2. User login thành công

3. FE/BFF merge guest cart
   POST /api/v1/cart/merge-guest
   Cookie: session, __Host-ecom_cart, __Host-ecom_csrf
   Header: X-CSRF-TOKEN

4. Backend
   Authorize -> validate CSRF -> UnitOfWork transaction
   -> PostgreSQL advisory locks theo user và guest-cart hash
   -> đọc guest/source cart và active user/target cart
   -> merge lines theo ProductVariantId
   -> source Active thành Converted
   -> commit

5. Controller chỉ sau commit mới xóa __Host-ecom_cart

6. FE thay cart state bằng response server, sau đó gọi checkout preview
```

## 5. Thay đổi source code

### 5.1 CSRF và lỗi có thể xử lý

`POST /api/v1/cart/merge-guest` dùng `ValidateCommerceAntiforgeryTokenAttribute` thay vì filter framework mặc định.

- CSRF hợp lệ: tiếp tục authorization và handler như bình thường.
- CSRF thiếu/sai: trả `400` với envelope `ApiResponse` và `errorCode = "CSRF_INVALID"`.
- Response lỗi có header `X-Trace-Id`.
- `ErrorHandlingMiddleware` cũng thêm `X-Trace-Id` cho exception không được xử lý.

Lý do chỉ áp dụng custom filter cho merge endpoint trong batch này: đây là endpoint lỗi thực tế, giảm phạm vi thay đổi public error contract của các mutation khác. Các endpoint cart/checkout khác vẫn giữ `ValidateAntiForgeryToken` hiện có.

### 5.2 Idempotent merge

Handler phân biệt trạng thái source cart:

| Source cart | Hành vi | HTTP kết quả |
| --- | --- | --- |
| Không có | Không tạo user cart, không xóa cookie | `404 NOT_FOUND` |
| `Active`, chưa hết hạn | Merge vào active user cart, chuyển source `Converted` | `200` |
| `Converted` | Không merge lại; trả active cart hiện tại của authenticated user, hoặc `CartDto.Empty` | `200` |
| Hết hạn hoặc không còn active | Không merge, không xóa cookie | `422 UNPROCESSABLE_ENTITY` |

Vì source `Converted` trả `200`, retry sau khi network mất response không làm quantity tăng lần hai.

### 5.3 PostgreSQL transaction advisory lock

Thêm `ICartMutationLock` ở Application và `CartMutationLock` ở Infrastructure.

- Lock là `pg_advisory_xact_lock`, gắn với transaction hiện tại của `UnitOfWorkBehavior`.
- Merge lấy lock theo cả user ID và guest-cart hash.
- Add item lấy lock theo principal hiện tại (user hoặc guest).
- Lock keys được sort trước khi acquire để tránh deadlock giữa các request có cùng scope.
- Commit/rollback tự giải phóng lock; handler không gọi `SaveChangesAsync` hay transaction trực tiếp.
- Không log token gốc, guest hash hoặc lock key.

Điều này serialize các trường hợp quan trọng:

| Request A | Request B | Kết quả mong đợi |
| --- | --- | --- |
| Merge cùng guest cart | Merge cùng guest cart | Một request merge data, request còn lại thấy `Converted`; không duplicate item. |
| Add item user | Merge vào user đó | Chỉ có một active user cart. |
| Add item guest | Merge guest cart đó | Không tạo/merge trùng guest cart. |

## 6. API contract cho FE/BFF

### 6.1 Bootstrap CSRF

```http
GET /api/v1/security/csrf
```

Response thành công chứa `data.token`; response đồng thời set cookie `__Host-ecom_csrf`.

### 6.2 Merge guest cart

```http
POST /api/v1/cart/merge-guest
Authorization: Bearer <authenticated session>
X-CSRF-TOKEN: <token từ endpoint csrf>
Cookie: __Host-ecom_cart=<guest token>; __Host-ecom_csrf=<csrf cookie>; ...
Content-Type: application/json
```

Request body hiện không có dữ liệu nghiệp vụ; FE không gửi giá, số lượng, cart ID hoặc guest token trong body.

Response thành công:

```json
{
  "success": true,
  "data": {
    "id": "cart-guid-or-null",
    "status": "Active",
    "items": [
      { "id": "cart-item-guid", "productVariantId": "variant-guid", "quantity": 2 }
    ]
  }
}
```

Các lỗi cần map:

| HTTP | `errorCode` | FE/BFF xử lý |
| --- | --- | --- |
| `400` | `CSRF_INVALID` | Gọi lại CSRF, thay cookie/token, retry **một lần** merge. |
| `401` | `401` | Không gọi merge tiếp; yêu cầu login. |
| `404` | `404` | Guest cart không còn tồn tại; clear local cart UI sau khi user xác nhận/refetch server cart. |
| `422` | `422` | Guest cart hết hạn/không active; thông báo và refetch cart. |
| `429` | `429` | Tôn trọng `Retry-After`; không retry ngay. |
| `500` | `500` | Không retry mù; gửi `X-Trace-Id` cho backend để điều tra. |

`200` khi source cart đã `Converted` là thành công idempotent, không phải dấu hiệu cart bị merge lần nữa.

### 6.3 BFF/local HTTPS

BFF phải forward đầy đủ cookie và header CSRF giữa browser và backend, đồng thời chuyển tiếp `Set-Cookie` khi backend xóa guest cart cookie. Không dùng HTTP local cho cookie `__Host-*`; API phải chạy HTTPS và development certificate phải được trust.

## 7. Truy vết vận hành

Khi người dùng báo lỗi:

1. Xác định request đầu tiên lỗi trong Network: `merge-guest` hay `checkout/preview`.
2. Với `400`, kiểm tra `errorCode` có phải `CSRF_INVALID`, cookie CSRF và header `X-CSRF-TOKEN` có cùng session không.
3. Với `500`, lấy `X-Trace-Id` từ response, tìm log backend có cùng TraceId.
4. Kiểm tra database theo ID nội bộ (không xuất guest token):
   - source guest cart có `Active`, `Converted` hay `Expired`;
   - user có đúng một cart `Active`;
   - mỗi variant trong target cart có đúng một cart item và quantity mong đợi.
5. Chỉ sau khi merge trả `200` mới đánh giá lỗi `checkout/preview`. Nếu user cart thật sự rỗng, `Active cart was not found` là hậu quả đúng của merge thất bại/không có data, không phải lỗi giá hay payment.

Log không được ghi guest token, guest hash, email, phone, address, payment credential hay provider reference.

## 8. Kiểm thử đã bổ sung

`Tests/Ecom.IntegrationTests/Cart/CartMergeApiTests.cs` bao gồm:

- anonymous merge trả `401`;
- CSRF thiếu trả `400/CSRF_INVALID`, không xóa guest cookie;
- guest cart không tồn tại trả `404` và không tạo user cart;
- merge tạo user cart và chỉ sau thành công mới xóa cookie;
- merge gộp variant trùng với cart đã có;
- retry sau commit trả `200` nhưng không duplicate quantity;
- hai merge đồng thời tạo đúng một active user cart;
- guest cart hết hạn trả `422` và giữ cookie.

Lệnh build đã chạy:

```powershell
dotnet build Tests/Ecom.IntegrationTests/Ecom.IntegrationTests.csproj --no-restore
```

Kết quả: build thành công, không warning/error ở lần build xác nhận.

Lệnh test hẹp đã chạy nhưng toàn bộ test PostgreSQL bị skip vì thiếu `ECOM_TEST_POSTGRES` và `ECOM_TEST_ALLOW_RESET=true`. Đây không phải bằng chứng runtime/PostgreSQL pass.

## 9. Checklist trước khi đóng lỗi

- [ ] Backend mới được build và restart trên HTTPS.
- [ ] BFF forward CSRF cookie, guest cookie, auth session, `X-CSRF-TOKEN`, `Set-Cookie` và `X-Trace-Id`.
- [ ] Test manual: guest add item -> login -> merge `200` -> reload cart -> checkout preview `200`.
- [ ] Test CSRF invalid: nhận `CSRF_INVALID`, refresh token, retry duy nhất một lần.
- [ ] Chạy `CartMergeApiTests` với PostgreSQL test database thật; không còn test skip.
- [ ] Theo dõi log/latency lock sau khi có concurrent traffic thực tế.

## 10. Giới hạn của batch này

- Không migration, không thay partial unique index, không thay permission.
- Không thay đổi logic giá, tồn kho, reservation, quote fingerprint hoặc payment.
- Không thêm retry tự động cho `500`.
- Không mở rộng custom CSRF envelope sang toàn bộ API mutation; việc đó cần batch/approval riêng để quản lý public error contract.
