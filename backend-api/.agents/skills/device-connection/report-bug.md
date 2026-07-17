# Bug Report: Device Remains Attached to Hub When Uninstalled

Agent Report Metadata:
- Task domain: Device connection, device hub assignment, uninstall behavior, and physical installation state.
- Canonical route: `.agents/context/task-router.md` -> Device hub connection / uninstall behavior.
- Update rule: append delta-first updates with `.agents/skills/backend-api-architecture/templates/skill-report-update.md` when device connection behavior, risks, or verification results change.
- Source of truth: source code and runtime/user verification outrank older report text.

## Vấn đề
Khi cập nhật thiết bị (API `PUT /api/v1/device/{id}`) với trạng thái `InstallationStatus = "Uninstalled"` hoặc `isInstalled = false`, thiết bị vẫn giữ nguyên `deviceHubId`, `pondName`, `deviceCode` và các thông tin vật lý liên kết với Hub ban đầu. Đáng lẽ ra các thông tin này phải được xóa (đặt về trạng thái chưa lắp đặt).

## Phân tích (Nguyên nhân lõi)
Trong `UpdateDeviceCommandHandler`, khi frontend gửi payload chứa `installationStatus: "Uninstalled"` nhưng vẫn kèm theo `deviceHubId: "8e5ac056-3a51-4cfd-85cc-3c3de30f4d33"`, logic hiện tại của backend sẽ không ưu tiên `installationStatus` để ngắt kết nối vật lý, mà vẫn đọc giá trị `deviceHubId` truyền lên.

Cụ thể:
1. `request.DeviceHubId.HasValue` vẫn trả về `true` vì frontend gửi kèm ID của Hub cũ.
2. Việc này làm đoạn check giữ lại Device Code (không chuyển thành mã `CLĐ`) hoạt động:
   ```csharp
   if (device.DeviceCode != null && !device.DeviceCode.Contains("CLĐ") && request.DeviceHubId.HasValue)
   {  
       deviceNo = -1; // Ngăn không cho sinh mã DeviceCode mới
   }
   ```
3. Sau đó, `device.Update` nhận toàn bộ các giá trị cũ này bao gồm `deviceHubId`, dẫn đến bản ghi trong database chỉ cập nhật `InstallationStatus = Uninstalled` nhưng vẫn giữ nguyên cắm vào Hub.

## Giải pháp đã áp dụng
Can thiệp xử lý ngay trước khi đọc thông tin Hub trong `UpdateDeviceCommandHandler.cs`:
Nếu `request.InstallationStatus == InstallationStatusEnum.Uninstalled`, chủ động đặt các thuộc tính về trạng thái ngắt kết nối:
- `request.DeviceHubId = null`
- `request.DeviceNumber = 0`
- `request.InstallationDate = null`
- `newConnectionStatus = ConnectionStatusEnum.UnDefined`
- `resolvedStatus = StatusDeviceEnum.UnDefined`

Khi `request.DeviceHubId = null`, logic sẽ tự động set `deviceNo = 0` thay vì `-1`, từ đó gọi hàm `GenerateDeviceCode` sinh mã thành `<TypeCode>-CLĐ` (Chưa Lắp Đặt) một cách chính xác theo business rule.
