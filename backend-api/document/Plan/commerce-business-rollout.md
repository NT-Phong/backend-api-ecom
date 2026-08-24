# Bộ nghiệp vụ triển khai Commerce Thanh Hoa

**Trạng thái:** Đề xuất triển khai, chưa phải hợp đồng API hay thay đổi schema đã được phê duyệt.

## 1. Mục tiêu và ranh giới

Đây là bộ nghiệp vụ tiếp theo cho nền tảng bán đặc sản Thanh Hóa. Nó tận dụng lõi đã có trong source: Catalog, ProductVariant/giá/tồn kho, Cart, checkout preview, Order, thanh toán SePay, shipment, quản trị tồn kho và địa chỉ khách hàng.

Mục tiêu là hoàn thành vòng đời sau mua và tạo niềm tin cho hàng địa phương:

```text
Khám phá sản phẩm → Mua hàng → Giao hàng → Hậu mãi
                                      ├→ Đánh giá / hỏi đáp
                                      ├→ Đổi trả / hoàn tiền
                                      └→ Truy xuất nguồn gốc
```

Không nằm trong đợt này: marketplace nhiều nhà bán, đa kho phức tạp, purchase order, stock transfer, CMS/SEO đầy đủ, recommendation, và dashboard phân tích nâng cao.

## 2. Các nguyên tắc chung

1. `ProductVariant` là đơn vị bán, giao, đổi trả và hoàn tồn; không thao tác theo Product chung chung.
2. Giá, khuyến mãi, số lượng đủ điều kiện và số tiền hoàn luôn được server tính lại từ snapshot đơn hàng; client không được quyết định các giá trị này.
3. Mọi thay đổi trạng thái phải đi qua hành động nghiệp vụ, tạo lịch sử và audit log; không có API cập nhật trực tiếp `status`.
4. Các thao tác có tiền, tồn kho hoặc gọi payment provider phải idempotent và có khoá/concurrency phù hợp.
5. Nhân viên chỉ được thao tác trong phạm vi permission; khách chỉ được thấy và thao tác đơn/yêu cầu do mình sở hữu.
6. Notification là hậu quả sau commit, không được làm hỏng transaction của nghiệp vụ chính.

## 3. Gói A — Đổi trả và hoàn tiền (P0 tiếp theo)

### 3.1. Mục đích

Cho khách tạo yêu cầu hậu mãi sau khi nhận hàng; cho vận hành kiểm tra hàng trả, quyết định hoàn tiền hoặc đổi hàng; đảm bảo tồn kho và thanh toán không bị cập nhật hai lần.

### 3.2. Phạm vi nghiệp vụ

- Khách tạo yêu cầu từ một đơn đã giao thành công, chọn từng `OrderItem`, số lượng, lý do, mô tả và tối đa số lượng ảnh bằng chứng.
- Khách chọn một trong hai hướng xử lý: `Refund` hoặc `Exchange`. Đợt đầu chỉ cho phép Exchange khi cùng variant còn hàng; không tự động đổi sang variant khác.
- Nhân viên xem bằng chứng, phê duyệt hoặc từ chối có lý do.
- Khi hàng hoàn về, nhân viên xác nhận nhận hàng, kiểm tra và quyết định có thể hoàn tồn hay phải loại bỏ.
- Hoàn tiền là tác vụ riêng, có tham chiếu Payment/transaction và kết quả đối soát; chỉ thành công một lần với cùng yêu cầu.

### 3.3. Vòng đời

```text
Requested → Approved → AwaitingReturn → Received → Inspecting
                                              ├→ RefundPending → Refunded → Completed
                                              └→ ExchangePending → Exchanged → Completed

Requested / Approved / AwaitingReturn → Rejected | Cancelled | Expired
Received / Inspecting → RejectedAfterInspection
```

`RejectedAfterInspection` phải lưu lý do và không tự động làm đơn hàng cũ biến mất. Nếu trả hàng hợp lệ, hàng chỉ được cộng lại tồn kho sau hành động nhận hàng/inspection đã quyết định `restockable`.

### 3.4. Quy tắc bắt buộc

- Chỉ `Delivered` mới đủ điều kiện; thời hạn mặc định lấy từ Commerce Settings, không hard-code ở FE.
- Tổng quantity của các ReturnItem đang mở/đã duyệt không vượt quantity đã giao của cùng OrderItem.
- Một ReturnRequest không trộn hai resolution khác nhau; tách yêu cầu nếu khách vừa đổi vừa hoàn tiền.
- Số tiền hoàn tối đa bằng phần khách thực trả phân bổ cho item, discount và shipping theo quy tắc server xác định; không dùng giá catalogue hiện hành.
- Hành động refund phải yêu cầu `Idempotency-Key`; retry trả lại kết quả đã có thay vì tạo giao dịch mới.
- Không tạo refund chỉ vì callback/redirect ở trình duyệt; kết quả payment provider phải được xác minh và đối soát.

### 3.5. Vai trò

| Vai trò | Quyền |
| --- | --- |
| Customer | tạo, xem, bổ sung bằng chứng, hủy trước khi được nhận hàng |
| Return staff | duyệt/từ chối, xác nhận nhận hàng, inspection |
| Finance | thực hiện/đánh dấu hoàn tiền theo policy |
| Inventory staff | quyết định nhập lại hoặc loại bỏ hàng trả |
| Admin | xem audit, can thiệp theo permission đặc biệt |

## 4. Gói B — Khuyến mãi và Coupon (P1)

### 4.1. Mục đích

Biến các entity Promotion/Coupon hiện có thành chương trình bán hàng áp dụng nhất quán ở product detail, cart và checkout.

### 4.2. Nghiệp vụ

- Promotion có các trạng thái `Draft → Scheduled → Active → Paused/Expired`.
- Coupon có mã, thời gian hiệu lực, giới hạn tổng lượt dùng, giới hạn mỗi khách và lịch sử redemption.
- Điều kiện P1: tổng đơn tối thiểu, sản phẩm, category, customer eligibility và thời gian. Không triển khai stack rule phức tạp trước khi có pricing test cho tổ hợp đó.
- Khi thêm coupon hoặc preview checkout, server trả quote mới với danh sách adjustment, lý do coupon bị từ chối và `QuoteFingerprint`.
- CreateOrder chỉ chấp nhận fingerprint của quote còn hợp lệ; hết hạn/đổi giá/hết tồn phải buộc preview lại.

### 4.3. Quy tắc

- Coupon không bị “giữ chỗ” chỉ vì được nhập vào cart.
- Lượt dùng được chốt cùng transaction tạo Order thành công; cancel/expiry/rejected payment chỉ giải phóng theo chính sách rõ ràng.
- Không dùng discount của campaign hiện tại để hoàn tiền cho đơn cũ; Return/Refund dùng snapshot trên Order.

## 5. Gói C — Tương tác sau mua: Wishlist, Review và Q&A (P1)

### 5.1. Wishlist

Khách lưu ProductVariant hoặc Product; source cần quyết định một chuẩn duy nhất. Đề xuất lưu ProductVariant để trạng thái giá/tồn kho và “add to cart” không mơ hồ. Đây không phải reservation và không giữ giá/tồn.

### 5.2. Review

- Chỉ buyer có OrderItem `Delivered` mới tạo review, tối đa một review cho một OrderItem.
- Review gồm rating, nội dung, media; ban đầu `Pending`, chỉ public sau moderation.
- Sửa/xóa phải được audit; product rating chỉ tính review `Published`.
- Admin có `Approve`, `Hide`, `Reject`, `Reply`; lý do ẩn/từ chối không được lộ dữ liệu nội bộ cho public.

### 5.3. Q&A

Khách có thể hỏi trên product; staff/producer được phân quyền trả lời. Nội dung spam/vi phạm bị ẩn thay vì xóa mất lịch sử moderation.

## 6. Gói D — Truy xuất nguồn gốc và chứng nhận (P1)

### 6.1. Mục đích

Tạo bằng chứng tin cậy cho sản phẩm địa phương qua QR công khai, không yêu cầu đăng nhập.

### 6.2. Mô hình công khai

```text
Mã truy xuất → TraceLot → Producer / Facility
                         → Production / Processing / Packaging events
                         → Certification evidence
```

Chỉ xuất bản dữ liệu đã được duyệt. Không trả địa chỉ chi tiết, tài liệu nội bộ, thông tin liên hệ riêng hoặc bằng chứng chưa kiểm duyệt.

### 6.3. Quy tắc

- Mã QR là public identifier không đoán được và có thể bị revoke.
- Trace event phải có thời điểm, loại sự kiện, actor/nguồn xác thực và evidence tùy chọn.
- Certification có ngày hiệu lực/hết hạn; badge public phải phản ánh hiệu lực tại thời điểm đọc.
- Không khẳng định "đã xác thực" chỉ vì producer tự tải giấy tờ lên; cần trạng thái staff verification.

## 7. Gói E — Vận hành tối thiểu hỗ trợ các gói trên

- Notification template/log cho: return requested, approved/rejected, refund completed, review published/hidden, coupon lifecycle.
- Management order detail phải hiển thị link đến return, payment/refund, shipment và audit timeline thay vì các màn hình rời rạc.
- Dashboard bổ sung: return đang chờ, refund đang chờ, coupon usage bất thường, chứng nhận sắp hết hạn.
- System settings bổ sung: return window, evidence size/count, refund approval threshold, review moderation policy; secret tích hợp không được trả về UI/API.

## 8. Thứ tự triển khai và acceptance gate

| Batch | Kết quả cần có | Gate trước khi sang batch sau |
| --- | --- | --- |
| 0 | Chuẩn hoá quote/fingerprint và order snapshot contract | PostgreSQL test cho idempotency, giá, tồn và rollback |
| A1 | ReturnRequest, ReturnItem, state machine, ownership và audit | Test transition, quantity race, quyền customer/staff |
| A2 | Receive/inspect/restock/refund orchestration | PostgreSQL test double-submit refund và inventory rollback |
| B | Promotion/Coupon pricing và redemption | Test thời gian, usage limit, concurrent redemption, order snapshot |
| C | Wishlist/Review/Q&A | Test delivered-only review, uniqueness, moderation authorization |
| D | Traceability/Certification public + admin | Test public-data redaction, expired certificate, QR revoke |
| E | Notifications/dashboard/settings | Test outbox/post-commit and permission checks |

Mỗi batch chỉ được coi là hoàn thành khi có: domain invariants, request validation, authorization, migration đã được phê duyệt, PostgreSQL integration tests và API smoke test. Build thành công không thay thế các gate này.

## 9. Hợp đồng FE–BE dự kiến

Các route dưới đây chỉ là contract đề xuất, cần approval trước khi public API được thay đổi:

```text
POST /api/v1/orders/{orderId}/returns
GET  /api/v1/returns/{returnId}
POST /api/v1/returns/{returnId}/cancel

GET  /api/v1/management/returns
POST /api/v1/management/returns/{returnId}/approve
POST /api/v1/management/returns/{returnId}/receive
POST /api/v1/management/returns/{returnId}/inspect
POST /api/v1/management/returns/{returnId}/refund

POST /api/v1/cart/coupons
DELETE /api/v1/cart/coupons/{code}
GET  /api/v1/trace/{code}
```

Mọi mutation có CSRF protection; các mutation đổi tiền/tồn kho phải có idempotency và response cần trả state/timeline mới để FE không tự suy diễn trạng thái.

## 10. Tiêu chí hoàn thành product-level

Hệ thống được xem là đủ cho một vòng vận hành Commerce đáng tin cậy khi khách có thể mua, theo dõi, yêu cầu hậu mãi, nhận kết quả hoàn tiền và xác minh nguồn gốc; còn staff có thể xử lý từng bước với audit, không làm sai payment hay inventory. CMS, warehouse nâng cao và analytics sâu chỉ nên mở sau khi các luồng này có bằng chứng PostgreSQL và runtime.
