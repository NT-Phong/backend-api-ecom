# Phân tích Luồng Thông báo FCM: ImportReceipt, ExportReceipt, InventoryCheck

Dựa trên yêu cầu và hướng dẫn từ kĩ năng `fcm-notification`, tôi đã tiến hành phân tích tác vụ liên quan đến **ImportReceipt**, **ExportReceipt**, và **InventoryCheck**, sau đó đối chiếu chéo với mã nguồn thực tế của dự án theo quy định trong `AGENTS.md`.

## 1. Mục tiêu và bối cảnh (Context)

Theo lịch sử trong `.agents/skills/FCM-notification/report-bug.md`, đã có các bản vá (fix) quan trọng được đưa vào luồng tạo phiếu và duyệt phiếu kho nhằm giải quyết các lỗi:
- **Người thực hiện (Actor) bị loại trừ** khỏi danh sách nhận thông báo đẩy (FCM).
- **Thiếu cảnh báo tồn kho (Low-stock)** khi tự động tạo phiếu xuất từ hệ quả của kiểm kê kho (shortage).
- **Quyền sở hữu luồng thông báo** (Ownership) bị phân tán (trước đây do các handler sự kiện chung của Central Warehouse đảm nhận, nay được dời về các Service chuyên biệt để đảm bảo sau khi Transaction DB commit thành công mới gửi).

## 2. Đối chiếu mã nguồn (Source Code Cross-Reference)

### 2.1. Cấu hình độ phân giải người nhận (Recipient Resolver)
**Tập tin**: `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs`

*   **Báo cáo**: Các hàm xử lý đối tượng nhận thông báo kho sẽ không còn loại bỏ actor nếu actor đó cũng thuộc nhóm quyền (roles) nhận thông báo. 
*   **Mã nguồn**: 
    - Các phương thức `ResolveForImportReceiptAsync`, `ResolveForExportReceiptAsync`, `ResolveForInventoryCheckAsync` và `ResolveForLowStockAlertAsync` đều được cấu hình cứng tham số `excludeActor = false`.
    - Ma trận quyền (Role matrix) dành cho Zone/Warehouse nay bao gồm `Manager`, `EmployeeManager`, `EmployeeWarehouse`, `Admin`.

> [!NOTE]
> **Nhận xét**: Mã nguồn hoàn toàn khớp với tài liệu `report-bug.md`. Người dùng tạo phiếu hoặc duyệt phiếu vẫn sẽ nhận được push notification trên thiết bị của họ.

### 2.2. Luồng Nhập kho tự động (Import Receipt)
**Tập tin**: `Infrastructure/Ecom.Infrastructure/Services/ImportReceiptService.cs`

*   **Báo cáo**: Notification cho phiếu nhập tự động được xử lý ngay trong `ImportReceiptService` sau khi transaction lưu thành công.
*   **Mã nguồn**:
    - Phương thức `CreateAutoImportReceiptAsync` chạy qua `_unitOfWork.ExecuteInTransactionAsync`.
    - Sau khi ra khỏi khối transaction thành công, gọi tiếp `SendAutoImportNotificationsAsync(...)`.
    - Hàm gửi thông báo dùng `ResolveManagersAndEmployeesAsync(..., excludeActor: false)`.
    - Gửi thông báo thông qua `NotifyBulkAsync` với title `"Nhập kho"` đi kèm tên `actorName`.

### 2.3. Luồng Xuất kho tự động & Cảnh báo tồn kho thấp (Export Receipt)
**Tập tin**: `Infrastructure/Ecom.Infrastructure/Services/ExportReceiptService.cs`

*   **Báo cáo**: Notification cho xuất kho và cảnh báo tồn kho do `ExportReceiptService` đảm nhận.
*   **Mã nguồn**:
    - Tương tự phiếu nhập, `SendAutoExportNotificationsAsync` được gọi **sau** khi transaction hoàn tất.
    - Duyệt qua từng `warehouseItems` có trong phiếu xuất, kiểm tra điều kiện `wi.AlertQty > 0 && wi.IsBelowAlertLevel()`.
    - Nếu vật tư dưới mức cảnh báo, tự động gửi tiếp notification thứ hai với title `"Tồn kho"` qua tập người nhận từ `ResolveForLowStockAlertAsync`.

> [!TIP]
> **Nhận xét**: Việc bóc tách gửi thông báo ra khỏi CentralWarehouseEventHandler đã ngăn ngừa rủi ro bắn FCM push thành công nhưng sau đó Transaction DB có thể bị rollback.

### 2.4. Phê duyệt kiểm kê kho (Inventory Check Approval)
**Tập tin**: `Core/Ecom.Application/Features/InventoryCheck/Commands/ApproveInventoryCheck/ApproveInventoryCheckCommandHandler.cs`

*   **Báo cáo**: Hành động duyệt phiếu kiểm kê sẽ bắn thông báo duyệt phiếu (approval result), kèm theo thông báo nhập/xuất điều chỉnh kho, và thông báo tồn kho thấp nếu thiếu hụt.
*   **Mã nguồn**:
    - Transaction block `_unitOfWork.ExecuteInTransactionAsync` chịu trách nhiệm duyệt phiếu, điều chỉnh các lô hàng (Refill/Consume), và tạo ImportReceipt (hàng thừa) / ExportReceipt (hàng thiếu).
    - Sau khi transaction commit, phương thức `SendNotificationsAsync` được gọi:
        1.  Gửi thông báo duyệt phiếu `"Kiểm kê kho"` với tên của người duyệt (`approverName`).
        2.  Nếu có hàng thừa (Import), gửi thông báo `"Nhập kho"` đi kèm mã phiếu kiểm kê và tên người tạo (`creatorName`).
        3.  Nếu có hàng thiếu (Export), gửi thông báo `"Xuất kho"`.
        4.  Lọc lại danh sách vật tư thiếu bị rớt xuống dưới mức alert (Low stock). Nếu có, gửi cảnh báo `"Tồn kho"`.

## 3. Tổng kết & Đánh giá

- **Sự đồng nhất**: Hiện trạng source code đang chạy hoàn toàn phản ánh chính xác các nội dung đã báo cáo trong `report-bug.md`. 
- **Quy tắc an toàn**: Không có việc phơi bày (log) chuỗi FCM Tokens gốc hoặc Refresh Tokens. Dữ liệu debug hiện tại như `FCM_SEND_RESULT` và `NOTIFICATION_BULK_PUBLISH` cung cấp đủ evidence cho các bước kiểm chứng FCM.
- **Tính khả thi**: Việc thực thi `NotifyBulkAsync` sau transaction DB là cách tiếp cận an toàn tạm thời. Tuy nhiên, như `report-bug.md` (BUG-H2) có chỉ ra: FCM task đang được chạy bất đồng bộ nhưng thiếu hàng đợi (Outbox/Retry). Nếu backend crash ngay sau DB Commit nhưng trước khi FCM thực sự push tới Firebase, thì thông báo FCM vẫn có thể bị thất lạc vĩnh viễn.

> [!WARNING]
> Nếu hệ thống đòi hỏi **guaranteed delivery** cho Push Notification, cần xem xét việc triển khai Outbox pattern (lưu intent của việc gửi thông báo kèm transaction của nghiệp vụ) và một worker xử lý Retry. Cấu trúc mã hiện tại rất dễ refactor sang hướng này vì đã chuyển các hàm dispatch ra khỏi tầng Core/EventHandlers.

