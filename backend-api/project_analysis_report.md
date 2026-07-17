# Báo cáo Phân tích Chức năng & Tích hợp Hệ thống: MarketPro (Chợ Xanh)
> Tài liệu định hướng nghiệp vụ và tích hợp API phục vụ đội ngũ phát triển Backend (BE).

---

## 1. Tổng Quan Dự Án & Phạm Vi Nghiệp Vụ (Project Context & Scope)

Dự án **MarketPro** (phát triển với thương hiệu nội địa hóa là **Chợ Xanh**) là một nền tảng thương mại điện tử đa người bán (multi-vendor marketplace) tập trung vào nông sản sạch, thực phẩm tươi hữu cơ và nhu yếu phẩm gia đình hàng ngày.

### Công Nghệ & Luồng Giao Tiếp Frontend
* **Frontend Stack**: Next.js 15 (App Router), React 18, Bootstrap 5 & Sass (SCSS), `@phosphor-icons/web`. Giao diện được cấu trúc theo mô hình Server Components mặc định để tối ưu hóa SEO và Core Web Vitals.
* **Nguyên tắc giao tiếp API**: Frontend tách biệt hoàn toàn giao diện trực quan khỏi logic gọi API. Component gọi Feature Service/Hook -> Gọi `apiClient` -> Lấy thông tin từ Endpoint Registry (`apiEndpoints.js`) -> Gửi Request tới Backend.

### Ràng Buộc Phạm Vi Nghiệp Vụ Quan Trọng
1. **Chỉ tập trung vào phân hệ Nông sản/Thực phẩm sạch (Chợ Xanh)**: Toàn bộ các demo Electronics (Demo 2) và Fashion (Demo 3) có hậu tố `Two`, `Three` và các route chuyên biệt như `/index-two`, `/product-details-two`, `/vendor-two` đã bị loại bỏ. Backend chỉ xây dựng hệ thống API phục vụ nghiệp vụ bán hàng thực phẩm/nông sản sạch.
2. **Không thiết kế Database cố định**: Nhiệm vụ thiết kế database, chọn kiểu dữ liệu, viết DbContext và vẽ sơ đồ quan hệ (ERD) sẽ do đội ngũ Backend tự thực hiện dựa trên nghiệp vụ hệ thống.

---

## 2. Chi Tiết Các Chức Năng Hiện Có Trên Giao Diện Frontend (Included Features)

Dưới đây là chi tiết các chức năng đã được xây dựng sẵn giao diện tĩnh và luồng hành vi trên Next.js storefront, yêu cầu Backend xây dựng API tương thích.

### 2.1. Phân Hệ Khách Hàng & Giao Diện Trang Chủ (Homepage & Navigation)
* **Thanh Điều Hướng (Navigation Header)**:
  * Hiển thị danh mục sản phẩm đa cấp.
  * Ô tìm kiếm sản phẩm nhanh (Search bar).
  * Bộ đếm số lượng giỏ hàng (`Cart Counter`) và danh sách yêu thích (`Wishlist Counter`) hoạt động theo thời gian thực.
* **Banner Quảng Cáo & Khuyến Mại (Hero Banners & Promotions)**:
  * Hiển thị danh sách banner chính (lớn) kèm nút kêu gọi hành động (CTA).
  * Hiển thị các ô quảng cáo nhỏ (Promotional Tiles) phân bổ theo vị trí thiết lập sẵn trên trang chủ.
* **Phân Phối Sản Phẩm Theo Kệ (Product Shelves)**:
  * **Flash Sales**: Kệ sản phẩm giảm giá có đếm ngược thời gian (Countdown) và thanh tiến trình thể hiện số lượng còn lại/đã bán (Stock progress).
  * **Hot Deals**: Các sản phẩm deal hời đi kèm đếm ngược thời gian cụ thể cho từng deal.
  * **Best Sellers & New Arrivals**: Các kệ sản phẩm được lọc theo tiêu chí bán chạy nhất hoặc hàng mới nhập kho.
  * **Organic Products / Short Products**: Các kệ sản phẩm phân loại theo nhóm danh mục chính, hỗ trợ lọc nhanh qua các tab danh mục con trên trang chủ.
* **Nhà Bán Hàng Nổi Bật (Top Vendors)**:
  * Hiển thị danh sách các cửa hàng/nhà bán hàng hàng đầu kèm logo, ảnh bìa sản phẩm tiêu biểu và đánh giá sao trung bình.
* **Đăng Ký Nhận Tin (Newsletter)**:
  * Form nhập email dưới chân trang để khách hàng đăng ký nhận khuyến mại.

### 2.2. Phân Hệ Cửa Hàng & Bộ Lọc Tìm Kiếm (Shop & Catalog)
* **Tìm kiếm & Phân trang**:
  * Hiển thị kết quả tìm kiếm sản phẩm theo tên, hỗ trợ phân trang danh sách kết quả (Pagination).
* **Bộ lọc Sản phẩm đa chiều (Product Filtering)**:
  * Lọc theo Danh mục (Categories) đa cấp.
  * Lọc theo Nhà bán hàng (Vendors).
  * Lọc theo Khoảng giá (Price Range Filter) sử dụng thanh trượt kéo (Slider).
  * Lọc theo Đánh giá (Rating Filter - lọc các sản phẩm từ 1 đến 5 sao).
* **Sắp xếp Sản phẩm (Product Sorting)**:
  * Sắp xếp theo giá (từ thấp đến cao và ngược lại).
  * Sắp xếp theo mức độ phổ biến, sản phẩm mới nhất hoặc đánh giá cao nhất.

### 2.3. Phân Hệ Chi Tiết Sản Phẩm (Product Details)
* **Thông tin Sản phẩm**:
  * Hiển thị tên, giá bán, so sánh giá (giá gạch đi), mô tả ngắn, mã SKU, thương hiệu/nhà bán hàng và trạng thái kho hàng (còn hàng/hết hàng).
* **Thư viện Ảnh (Image Gallery)**:
  * Trình chiếu ảnh sản phẩm lớn và danh sách ảnh con (Thumbnails), hỗ trợ zoom ảnh.
* **Đánh Giá & Nhận Xét (Product Reviews)**:
  * Đọc danh sách đánh giá từ người mua (số sao, tên người dùng, thời gian, nội dung nhận xét).
  * Cho phép người dùng đã mua hàng gửi đánh giá và chấm điểm sao mới cho sản phẩm.
* **Mã Giảm Giá Đi Kèm (Product Coupon Blocks)**:
  * Hiển thị danh sách các mã giảm giá (coupon) khả dụng cho sản phẩm này để người dùng có thể "Lưu nhanh" vào giỏ hàng.
* **Sản phẩm Liên quan (Related Products)**:
  * Đề xuất các sản phẩm cùng danh mục hoặc cùng nhà bán hàng ở cuối trang.

### 2.4. Phân Hệ Giỏ Hàng & Danh Sách Yêu Thích (Cart & Wishlist)
* **Quản lý Giỏ Hàng (Cart Management)**:
  * Hiển thị danh sách sản phẩm đã thêm vào giỏ: ảnh, tên, nhà bán hàng, đơn giá, số lượng, thành tiền từng dòng.
  * Tăng/Giảm số lượng sản phẩm tức thời trực tiếp trên dòng giỏ hàng (Quantity Control).
  * Xóa sản phẩm khỏi giỏ hàng.
  * Áp dụng mã giảm giá (Coupon Code): Hệ thống tính toán và hiển thị ngay số tiền giảm giá và tổng tiền thanh toán mới.
* **Danh sách Yêu thích (Wishlist)**:
  * Lưu trữ các sản phẩm khách hàng quan tâm.
  * Nút chuyển nhanh sản phẩm từ Wishlist sang giỏ hàng (Move to Cart) mà không làm mất sản phẩm trong Wishlist trừ khi người dùng chủ động xóa.

### 2.5. Phân Hệ Thanh Toán & Đơn Hàng (Checkout & Order Placement)
* **Form Thông Tin Giao Hàng (Billing & Shipping Form)**:
  * Nhập họ tên, email, số điện thoại, địa chỉ chi tiết của người nhận.
  * Ghi chú đơn hàng (Order notes).
* **Lựa chọn Phương thức Thanh toán**:
  * Thanh toán khi nhận hàng (COD).
  * Thanh toán qua cổng thẻ/chuyển khoản trực tuyến.
* **Tóm Tắt Đơn Hàng (Order Summary)**:
  * Hiển thị tổng tiền hàng, phí giao hàng, giảm giá từ coupon, và tổng số tiền cuối cùng phải thanh toán.

### 2.6. Phân Hệ Nhà Bán Hàng & Đăng Ký Cửa Hàng (Vendor & Become Seller)
* **Danh sách Nhà Bán Hàng (Vendors List)**:
  * Trang tìm kiếm và liệt kê toàn bộ các đối tác bán hàng trên hệ thống kèm bộ lọc tìm kiếm tên cửa hàng.
* **Chi tiết Cửa Hàng (Vendor Details)**:
  * Giao diện riêng của từng cửa hàng hiển thị ảnh bìa, ảnh đại diện, mô tả cửa hàng, đánh giá trung bình và bộ lọc sản phẩm dành riêng cho cửa hàng đó.
* **Đăng ký Trở thành Người Bán (Become Seller)**:
  * Trang giới thiệu các bước tham gia, chính sách bán hàng.
  * Form đăng ký thông tin đối tác gửi yêu cầu phê duyệt cửa hàng (Seller Application) bao gồm: tên cửa hàng, người đại diện, thông tin liên lạc và mô tả mô hình kinh doanh.

### 2.7. Phân Hệ Tin Tức & Hỗ Trợ (Blog & Support)
* **Tin Tức / Blog**:
  * Trang danh sách bài viết tin tức nông sản sạch, kinh nghiệm nấu ăn.
  * Hộp danh mục blog, tag bài viết bài bên thanh bên (Sidebar).
  * Trang chi tiết bài viết hỗ trợ viết bình luận bài viết (Blog comments).
* **Trang Liên Hệ (Contact)**:
  * Form gửi lời nhắn liên hệ của khách hàng đến ban quản trị hệ thống.

---

## 3. Các Đề Xuất Nghiệp Vụ Cần Có Từ Backend (Proposed BE Features)

Để hệ thống vận hành trơn tru và chuyên nghiệp dưới góc độ một sàn thương mại điện tử đa người bán thực thụ, đội ngũ Backend cần bổ sung các chức năng nghiệp vụ sau:

### 3.1. Phân Hệ Xác Thực OTP & Đăng Ký Qua Số Điện Thoại
* **Mô tả**: Tích hợp với **VietGuys API** để gửi SMS OTP.
* **Luồng nghiệp vụ đề xuất**:
  * Cho phép người dùng đăng ký/đăng nhập nhanh bằng Số điện thoại + OTP.
  * Xác thực OTP tại thời điểm đặt hàng đối với các đơn hàng COD có giá trị cao để giảm thiểu tỷ lệ đơn hàng ảo.

### 3.2. Quản Lý Đơn Hàng Nhiều Nhà Bán (Split Order System)
* **Mô tả**: Do đây là mô hình Multi-vendor, một giỏ hàng của người dùng có thể chứa sản phẩm từ nhiều Vendor khác nhau.
* **Nghiệp vụ đề xuất**:
  * Khi người dùng thực hiện thanh toán, hệ thống Backend cần có logic tự động tách đơn hàng lớn thành các đơn hàng con (`Sub-orders`) tương ứng với từng nhà bán hàng để phục vụ việc chuẩn bị hàng, theo dõi giao vận và đối soát dòng tiền riêng biệt sau này.

### 3.3. Snapshot Thông Tin Đơn Hàng (Order Snapshot)
* **Mô tả**: Bảo vệ lịch sử giao dịch khỏi các thay đổi trong danh mục sản phẩm.
* **Nghiệp vụ đề xuất**:
  * Khi lưu chi tiết đơn hàng (`order_items`), Backend bắt buộc chụp lại (snapshot) tên sản phẩm, ảnh đại diện, mã SKU và giá bán thực tế tại thời điểm mua. Đảm bảo nếu Vendor cập nhật giá sản phẩm hoặc xóa sản phẩm trong tương lai, hóa đơn cũ của khách hàng vẫn hiển thị thông tin chính xác.

### 3.4. Hệ Thống Quản Lý Mã Giảm Giá Cấp Cao (Coupon Engine)
* **Mô tả**: Xử lý các quy tắc áp dụng mã giảm giá động.
* **Nghiệp vụ đề xuất**:
  * Quản lý điều kiện áp dụng: Giá trị đơn hàng tối thiểu (`min_order_amount`), Số tiền giảm tối đa đối với coupon giảm giá theo phần trăm (`max_discount_amount`).
  * Quản lý giới hạn: Giới hạn tổng số lần sử dụng của coupon, giới hạn số lần sử dụng trên mỗi tài khoản khách hàng.

### 3.5. Kiểm Tra Tồn Kho Tức Thời (Real-time Inventory & Concurrency Control)
* **Mô tả**: Tránh tình trạng quá bán (Over-selling) khi nhiều khách hàng cùng thanh toán một sản phẩm hữu hạn.
* **Nghiệp vụ đề xuất**:
  * Sử dụng cơ chế khóa lạc quan (Optimistic Concurrency) thông qua `concurrency_stamp` trên thực thể sản phẩm khi thực hiện trừ kho hàng.
  * Tự động kiểm tra lượng tồn kho khả dụng tại thời điểm khách hàng nhấn "Đặt hàng" (Checkout), từ chối thanh toán nếu sản phẩm đã hết hàng trong quá trình khách hàng cân nhắc ở trang giỏ hàng.

### 3.6. Quản Lý Phân Quyền Theo Hành Động (Policy-based Access Control)
* **Mô tả**: Quản lý nhiều đối tượng vận hành hệ thống (Admin hệ thống, Quản trị viên nội dung, Nhân viên nhà bán hàng, Chủ cửa hàng).
* **Nghiệp vụ đề xuất**:
  * Không phân quyền cứng theo vai trò (Role-based), hãy sử dụng phân quyền theo hành động (Policy-based). Mỗi tài khoản sẽ được cấp danh sách các quyền hạn cụ thể (ví dụ: `products.create`, `orders.update_status`, `cms.manage_banners`).

---

## 4. Quy Tắc Tích Hợp Kỹ Thuật (Integration & Technical Rules)

### 4.1. Định Dạng Kết Nối API Chuẩn Hóa
Tất cả các API gửi về frontend cần được chuẩn hóa cấu trúc để tầng xử lý kết quả (`apiResult.js`) hoạt động đồng bộ:
* **Mã trạng thái HTTP**: Sử dụng đúng mã trạng thái (ví dụ: `200 OK`, `201 Created`, `400 Bad Request` cho lỗi nhập liệu, `401 Unauthorized` cho lỗi phiên làm việc, `403 Forbidden` khi thiếu quyền).
* **FluentValidation Error Mapping**: Lỗi xác thực nhập liệu từ các form cần được trả về dưới dạng object chi tiết theo key-value như định nghĩa ở mục 4.1 của báo cáo trước để frontend tự động hiển thị lỗi màu đỏ dưới từng ô nhập liệu tương ứng.

### 4.2. Luồng Tải Lên Tập Tin Qua Azure Blob Storage
Để bảo mật tài nguyên lưu trữ của doanh nghiệp, frontend không giữ kết nối trực tiếp đến Azure Blob. Backend thực hiện nghiệp vụ sinh chữ ký SAS (Shared Access Signature):
1. Frontend gửi yêu cầu tải file lên Backend (kèm tên file, định dạng).
2. Backend kiểm tra quyền của người dùng, sinh đường dẫn SAS tạm thời có giới hạn thời gian (ví dụ: hiệu lực trong 10 phút) và trả về cho frontend kèm link public xem ảnh.
3. Frontend thực hiện đẩy file trực tiếp từ trình duyệt lên Azure Blob thông qua SAS Url vừa nhận.
4. Giao diện lưu thông tin đối tượng (sản phẩm, banner) kèm link public đã được sinh ra.

### 4.3. Quản Lý Phiên Làm Việc (Auth Session & Token)
* Phiên làm việc sử dụng cặp Access Token (JWT) ngắn hạn và Refresh Token dài hạn.
* Khuyến nghị Backend cấu hình Refresh Token trả về thông qua **HttpOnly Secure Cookie** để phòng ngừa tối đa nguy cơ bị đánh cắp token qua các script độc hại (XSS) ở phía Client.

---
*Báo cáo được chuẩn hóa để định hướng phát triển tính năng hệ thống thương mại điện tử MarketPro (Chợ Xanh).*
