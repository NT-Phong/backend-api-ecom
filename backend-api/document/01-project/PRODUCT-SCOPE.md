# Dự án làm gì và phạm vi sản phẩm

Thanh Hoa Commerce là nền tảng thương mại điện tử dành cho sản phẩm địa phương Thanh Hóa. Hệ thống vừa phục vụ bán hàng trực tuyến, vừa giới thiệu đơn vị sản xuất, câu chuyện sản phẩm, hình ảnh và dữ liệu tạo niềm tin. Mô hình hiện tại là một storefront do đội vận hành quản trị tập trung; chưa phải marketplace nơi mỗi nhà cung cấp tự vận hành gian hàng độc lập.

## Giá trị sản phẩm

- Khách hàng tìm, so sánh, hiểu và mua đúng quy cách sản phẩm địa phương.
- Đơn vị vận hành quản lý producer, catalog, giá, ảnh, tồn kho, đơn, thanh toán và giao hàng.
- Dữ liệu giao dịch giữ nguyên snapshot để thay đổi catalog sau này không làm sai đơn cũ.
- Nền entity chuẩn bị cho chứng nhận, truy xuất, nội dung, B2B, promotion và analytics nhưng không tuyên bố các API này đã hoàn chỉnh.

## Người dùng

- Khách vãng lai hoặc khách có tài khoản: duyệt sản phẩm, cart, checkout, order.
- Nhân viên catalog/producer/inventory/order/payment/shipment: thao tác qua policy cụ thể.
- Quản trị: settings, audit/security reads, session revoke, role/policy.
- Provider ngoài: SePay IPN/bank webhook; storage/scanner tùy effective configuration.

## Hành trình chính

1. Staff tạo và xác minh Producer.
2. Staff tạo Product Draft, category, option, variant, price, media và inventory.
3. Product qua review rồi Published để xuất hiện trên storefront.
4. Khách chọn variant, thêm Cart, nhập người nhận và tạo checkout preview.
5. Server khóa lại giá/tồn và tạo Order từ quote fingerprint.
6. Khách dùng COD, chuyển khoản, SePay Hosted Checkout hoặc SePay VietQR.
7. Staff xác nhận, chuẩn bị, giao và hoàn tất đơn; hệ thống cập nhật reservation/movement tương ứng.

## Nguyên tắc kinh doanh

- Tính xác thực: không hiển thị chứng nhận/truy xuất nếu chưa có dữ liệu được xác minh.
- Tính nhất quán: cùng một price/status phải có cùng nghĩa ở storefront, backoffice và báo cáo.
- Bảo toàn lịch sử: OrderItem lưu snapshot tên, SKU, variant, giá và quantity.
- Server authority: client không quyết định price, stock, discount, total hoặc payment success.
- Truy vết: lifecycle quan trọng cần history/attempt/notification/audit phù hợp.
- Khả năng mở rộng: phân biệt Product, Variant, Price và Inventory dù UI ban đầu đơn giản.

## Mục tiêu kiến trúc

Pragmatic Clean Architecture với CQRS/MediatR, domain invariants, PostgreSQL và contract API versioned. Thiết kế ưu tiên bảo toàn lịch sử giao dịch, server authority, optimistic concurrency, idempotency và auditability.

## Ngoài phạm vi được chứng minh hiện tại

Entity Trust, CMS, Promotion, B2B, Notification và Analytics có trong model nhưng không phải mọi module đều có API hoàn chỉnh. Không tạo UI hoặc claim workflow chỉ từ sự tồn tại của entity. Aquaculture/IoT là legacy, không phải hướng phát triển Commerce.

## Capability map

| Capability | Trạng thái | Nội dung |
| --- | --- | --- |
| Authentication/authorization | Implemented | OTP V1, Password V2, refresh/session, role-policy, management security reads |
| Public catalog | Implemented | Product/category browse, search/filter/sort, public detail |
| Catalog backoffice | Implemented | Product/category/options/variants/prices/media lifecycle và concurrency |
| Producer management | Implemented | create/update, contacts/facility, verify/publish/hide |
| Cart/checkout/order | Implemented | guest/user cart, merge, address, preview, create/read/cancel order |
| Payment | Implemented, external proof separate | COD/bank transfer, SePay hosted, VietQR, IPN/webhook, reconciliation |
| Inventory/fulfillment | Implemented core | location, level, adjustment, reservation, ship/return flows |
| Media | Implemented ProductImage slice | multipart upload, scan lifecycle, metadata, attach Product |
| CMS/Trust/B2B/Promotion | Domain foundation/roadmap | entity model có, API/UI chưa đầy đủ |
