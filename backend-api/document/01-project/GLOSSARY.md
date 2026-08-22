# Thuật ngữ nghiệp vụ

| Thuật ngữ | Nghĩa chính xác trong dự án |
| --- | --- |
| Producer | Đơn vị sản xuất/HTX/doanh nghiệp sở hữu thông tin Product; phải verified + published để dùng trong catalog picker công khai |
| Product | Nội dung thương mại cấp sản phẩm: tên, slug, mô tả, category, media, producer và lifecycle |
| ProductVariant | Quy cách thực tế có thể mua; có SKU, option values, inventory mode và price periods |
| VariantPrice | Giá theo variant, loại giá, currency, min quantity và khoảng hiệu lực; không phải field trực tiếp của Product |
| Effective price | Giá server chọn tại một thời điểm theo rule hiệu lực; storefront/cart/checkout dùng giá này |
| InventoryItem | Định danh theo dõi tồn cho một tracked ProductVariant |
| InventoryLevel | Balance của InventoryItem tại StockLocation: stocked, reserved, incoming và available server-derived |
| InventoryMovement | Ledger append-only giải thích thay đổi: Receive, Allocate, Release, Adjust, Ship, Return |
| InventoryReservation | Phần tồn bị giữ cho OrderItem; Active rồi Consumed/Released/Expired |
| Cart | Giỏ Active của guest hoặc user; chứa CartItem tham chiếu ProductVariant |
| Quote | Kết quả server tính lại line price, subtotal, shipping, total và availability |
| Quote fingerprint | SHA-256 fingerprint 64 ký tự khóa facts của preview; create order phải gửi lại |
| Idempotency-Key | Header định danh một yêu cầu create order; retry cùng request dùng cùng key, payload khác phải dùng key khác |
| Order snapshot | Dữ liệu Product/Variant/SKU/price/quantity được đóng băng trong OrderItem |
| Payment attempt | Lần tạo Hosted Checkout hoặc VietQR; không đồng nghĩa đã thanh toán |
| IPN/Webhook | Callback provider đã xác thực, dùng làm nguồn thay đổi payment state chứ không dựa vào redirect client |
| ConcurrencyStamp | UUID optimistic-concurrency cho Catalog/Producer/Location/Settings; mutation stale trả conflict |
| MediaAsset | File metadata và scan lifecycle độc lập; upload xong chưa tự gắn Product |
| ProductMedia | Liên kết MediaAsset vào Product với primary/order/caption |
| Public DTO | Dữ liệu an toàn cho anonymous storefront, không chứa stamp/private metadata |
| Management DTO | Dữ liệu vận hành cần auth/policy, có state/stamp/private facts phù hợp |
| BFF | Lớp proxy frontend; phải giữ cookie/CSRF và không tự đổi nghĩa backend error |
| Source proof | Có code/contract trong repository; chưa đồng nghĩa runtime/staging/production proof |
