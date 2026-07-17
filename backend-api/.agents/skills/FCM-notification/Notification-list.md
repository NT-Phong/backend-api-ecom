# Danh Sách Tổng Hợp Thông Báo (FCM Notifications)

Tài liệu này tổng hợp toàn bộ các loại thông báo đẩy (Push Notification) hiện đang được hệ thống Ecom gửi đến người dùng thông qua Firebase Cloud Messaging (FCM).

---

## 1. Thông Tin Cơ Bản

### 1.1 Các dạng thông báo (Notification Types)
Hệ thống hiện tại phân loại thông báo thành 4 mức độ (dựa trên `NotificationTypes` enum):
- 🟢 **SUCCESS:** Thông báo thành công, phục hồi trạng thái bình thường (VD: Thiết bị kết nối lại, duyệt phiếu thành công).
- 🟡 **WARNING:** Cảnh báo cần chú ý, chờ duyệt (VD: Mất kết nối, chỉ số vượt ngưỡng, nhắc nhở xử lý, có phiếu cần duyệt).
- 🔴 **ERROR:** Lỗi nghiêm trọng cần xử lý khẩn cấp (VD: Thiết bị gặp sự cố phần cứng, sự cố Oxy kéo dài).
- 🔵 **INFO:** Thông tin hệ thống thông thường.

### 1.2 Các lĩnh vực và Quyền nhận thông báo
Các thông báo được phân chia theo từng lĩnh vực (Domain) cụ thể. Hệ thống sử dụng `NotificationRecipientResolver` hoặc logic điều phối riêng để gửi đúng người.

| Lĩnh vực (Domain) | Đối tượng nhận (Recipients) |
| --- | --- |
| **Thiết bị Cảm biến (Sensor)** | `FARMER` quản lý Zone chứa thiết bị đó. Nếu sự cố kéo dài (leo thang), sẽ gửi thêm cho `ADMIN`. |
| **Camera (AI / Trạng thái)** | `FARMER` quản lý Zone có Camera, hoặc `ADMIN`. |
| **Kho (Warehouse / Receipt)** | Những user có Role là Quản lý (`MANAGER`) hoặc Nhân viên Kho (`EMPLOYEE`) thuộc cùng Warehouse đó. |
| **Sự cố Oxy (Pond Alert)** | `FARMER` chịu trách nhiệm Ao đó. Nhắc nhở leo thang gửi cho `MANAGER`/`ADMIN`. |

---

## 2. Chi Tiết Các Thông Báo Hiện Có

Dưới đây là danh sách chi tiết tất cả các thông báo, được hệ thống tự động sinh ra trong quá trình vận hành.

### 2.1 Lĩnh Vực Camera & AI

**2.1.1 Phát Hiện Người (AI)**
- **Loại (Type):** `WARNING`
- **Mô tả:** Camera AI phát hiện có người xâm nhập vào khu vực.
- **Tiêu đề:** `Phát Hiện Người (AI)`
- **Nội dung:** `Camera [Tên Camera] (Kênh [ID Kênh]) lúc [Thời gian].`
- **Ví dụ:** Camera Ao Nuôi 03 Farm 5 KG (Kênh 3) lúc 17:15 26/05/2026.

**2.1.2 Camera Hoạt Động Trở Lại**
- **Loại (Type):** `SUCCESS`
- **Tiêu đề:** `Camera Hoạt Động`
- **Nội dung:** `Camera [Tên Camera] đã quay trở lại trạng thái hoạt động.`

**2.1.3 Camera Mất Kết Nối**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Camera Không Hoạt Động`
- **Nội dung:** `Camera [Tên Camera] đã ngừng hoạt động.`

**2.1.4 Camera Gặp Lỗi Phần Cứng**
- **Loại (Type):** `ERROR`
- **Tiêu đề:** `Camera Gặp Lỗi`
- **Nội dung:** `Camera [Tên Camera] đang gặp lỗi.`

---

### 2.2 Lĩnh Vực Cảm Biến Nước (Sensor Device)

**2.2.1 Cảm biến vượt ngưỡng**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Cảm biến vượt ngưỡng`
- **Nội dung:** `Cảm biến [Mã CB] ([Tên Zone]) phát hiện chỉ số bất thường: [Danh sách chỉ số]. Vui lòng kiểm tra ngay.`

**2.2.2 Cảm biến vượt ngưỡng kéo dài (Leo thang)**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Cảm biến vượt ngưỡng kéo dài`
- **Nội dung:** `Cảm biến [Mã CB] ([Tên Zone]) tiếp tục vi phạm ngưỡng sau [Thời gian] ([Chỉ số]). Cần xử lý khẩn cấp.`

**2.2.3 Cảm biến trở về bình thường**
- **Loại (Type):** `SUCCESS`
- **Tiêu đề:** `Cảm biến trở về bình thường`
- **Nội dung:** `Cảm biến [Mã CB] ([Tên Zone]) đã trở về mức bình thường.`

**2.2.4 Cảm biến mất kết nối / Kết nối lại**
- **Loại (Type):** `WARNING` / `SUCCESS`
- **Tiêu đề:** `Cảm biến mất kết nối` / `Cảm biến hoạt động`
- **Nội dung:** `Cảm biến [Mã CB] đã mất kết nối. Vui lòng kiểm tra thiết bị.` hoặc `Cảm biến [Mã CB] đã hoạt động trở lại.`

---

### 2.3 Lĩnh Vực Kho (Warehouse & Receipts)

**2.3.1 Yêu cầu duyệt Phiếu Nhập Kho**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Nhập kho`
- **Nội dung:** `[Tên NV] tạo phiếu nhập #[Mã Phiếu]. Vui lòng phê duyệt.`
- **Ví dụ:** Nguyễn Văn A tạo phiếu nhập #NK-2024-001. Vui lòng phê duyệt.

**2.3.2 Yêu cầu duyệt Phiếu Xuất Kho**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Xuất kho`
- **Nội dung:** `[Tên NV] tạo phiếu xuất #[Mã Phiếu]. Vui lòng phê duyệt.`

**2.3.3 Yêu cầu duyệt Phiếu Kiểm Kê**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Kiểm kê kho`
- **Nội dung:** `[Tên NV] tạo phiếu Kiểm kê kho [Mã Phiếu]. Vui lòng phê duyệt.`
- **Ví dụ:** Hoàng Văn D tạo phiếu kiểm kê kho #KK-2024-047. Vui lòng phê duyệt.

**2.3.4 Nhập kho tự động (Từ Kiểm Kê Kho)**
- **Loại (Type):** `SUCCESS`
- **Tiêu đề:** `Nhập kho`
- **Nội dung:** `Hệ thống tự tạo phiếu nhập [Mã phiếu] từ kết quả kiểm kê [Mã phiếu KK] | [Tên người tạo phiếu KK].`
- **Ví dụ:** Hệ thống tự tạo phiếu nhập #NK-2024-102 từ kết quả kiểm kê KK-2024-012 | Nguyễn Thị E

**2.3.5 Xuất kho tự động (Từ Kiểm Kê Kho)**
- **Loại (Type):** `SUCCESS`
- **Tiêu đề:** `Xuất kho`
- **Nội dung:** `Hệ thống tự tạo phiếu xuất [Mã phiếu] từ kết quả kiểm kê [Mã phiếu KK] | [Tên người tạo phiếu KK]`
- **Ví dụ:** Hệ thống tự tạo phiếu xuất #XK-2024-055 từ kết quả kiểm kê KK-2024-012 | Nguyễn Thị E

**2.3.6 Cảnh báo Tồn Kho Thấp**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Tồn kho`
- **Nội dung:** `Vật tư [Tên Vật Tư] đang có số lượng tồn kho dưới mức cảnh báo.`

**2.3.7 Nhập kho tự động (Quy trình tự động - Chuẩn bị thực hiện)**
- **Loại (Type):** `SUCCESS`
- **Tiêu đề:** `Nhập kho`
- **Nội dung:** `Hệ thống tự tạo phiếu nhập [Mã phiếu] từ quy trình tự động nhập kho | [Tên người thực hiện].`
- **Ghi chú:** Tính năng chuẩn bị thực hiện (VD: Hoạt động tự động sinh hàng trả về,...).

**2.3.8 Xuất kho tự động (Quy trình tự động - Chuẩn bị thực hiện)**
- **Loại (Type):** `SUCCESS`
- **Tiêu đề:** `Xuất kho`
- **Nội dung:** `Hệ thống tự tạo phiếu xuất [Mã phiếu] từ quy trình tự động xuất kho | [Tên người thực hiện].`
- **Ghi chú:** Tính năng chuẩn bị thực hiện (VD: Cho tôm ăn, Thay nước, Sử dụng thuốc/hóa chất,...).

---

### 2.4 Lĩnh Vực Sự Cố Ao Nuôi (Oxy / Pond Alert)

**2.4.1 Cảnh Báo Sự Cố Oxy**
- **Loại (Type):** `WARNING` / `ERROR`
- **Tiêu đề:** `Cảnh Báo: Sự Cố Thiết Bị Oxy` hoặc `Sự Cố Thiết Bị Nghiêm Trọng`
- **Nội dung:** Nôi dung động báo thiết bị nào mất kết nối tại ao nào. VD: `Thiết bị máy sục khí Oxy tại Ao 1 bị mất kết nối.`

**2.4.2 Nhắc Nhở Xử Lý Sự Cố Oxy**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Nhắc Nhở: Sự Cố Oxy Chưa Xử Lý`
- **Nội dung:** `[Nhắc nhở] Sự cố Oxy vẫn tiếp diễn. Hệ thống vẫn chưa ghi nhận phục hồi.`

**2.4.3 Báo Động Máy Oxy Tắt Quá Lâu**
- **Loại (Type):** `WARNING`
- **Tiêu đề:** `Cảnh Báo: Máy Oxy Tắt Quá Lâu`
- **Nội dung:** `Máy Oxy số [X] - Ao [Tên Ao] đã tắt quá [N] phút, vui lòng kiểm tra.`

**2.4.4 Leo Thang Sự Cố Trầm Trọng**
- **Loại (Type):** `ERROR`
- **Tiêu đề:** `Sự cố Oxy chưa được xử lý`
- **Nội dung:** `Sự cố Oxy tại [Tên Ao] chưa được xử lý sau [N] phút.`

