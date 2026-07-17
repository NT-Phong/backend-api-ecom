# 📱 Danh sách Notification & Payload Mapping

Tài liệu này tổng hợp toàn bộ các loại thông báo (Push Notification) đang có trong hệ thống. Tài liệu đóng vai trò là **API Dictionary** để đội ngũ Frontend Web và Mobile App dễ dàng tra cứu và xử lý điều hướng (Deep-link).

> 💡 **Lưu ý cho Mobile App**: 
> Khi nhận được thông báo FCM, Mobile App cần parse data payload và dựa vào các khóa chính như `screen`, `type`, `mode`, `id` để quyết định mở đúng màn hình tương ứng.

---

## 📦 1. Nhóm Quản Lý Kho (Nhập / Xuất / Kiểm Kê)

### Yêu cầu duyệt Phiếu Nhập Kho
*Thông báo khi Admin tạo phiếu thủ công hoặc import từ Excel.*
* **Loại (Type)**: `WARNING`
* **TargetUrl (Web)**: `/material-management/form?type=inbound&mode=approve&id={id}&zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **ImportApprove**
  * `type`: inbound
  * `mode`: approve
  * `id`: {id}
  * `zoneId`: {zoneId}

### Yêu cầu duyệt Phiếu Xuất Kho
*Thông báo khi Admin tạo phiếu xuất kho cần xác nhận.*
* **Loại (Type)**: `WARNING`
* **TargetUrl (Web)**: `/material-management/form?type=outbound&mode=approve&id={id}&zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **ExportApprove**
  * `type`: outbound
  * `mode`: approve
  * `id`: {id}
  * `zoneId`: {zoneId}

### Yêu cầu duyệt Kiểm Kê Kho
*Thông báo khi có phiếu kiểm kê mới cần xác nhận.*
* **Loại (Type)**: `WARNING`
* **TargetUrl (Web)**: `/material-management/form?type=counting&mode=approve&id={id}&zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **InventoryApprove**
  * `type`: counting
  * `mode`: approve
  * `id`: {id}
  * `zoneId`: {zoneId}

### Hệ thống tự động Nhập / Xuất Kho
*Phát sinh tự động sau khi hoàn thành quy trình kiểm kê.*
* **Loại (Type)**: `SUCCESS`
* **TargetUrl (Web)**: `/material-management/form?type={inbound/outbound}&mode=view&id={id}&zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **ImportApprove** *(hoặc ExportApprove)*
  * `type`: inbound *(hoặc outbound)*
  * `mode`: view
  * `id`: {id}
  * `zoneId`: {zoneId}

### Cập nhật trạng thái Phiếu Nhập / Xuất Kho
*Khi phiếu thay đổi trạng thái (đã duyệt, từ chối...).*
* **Loại (Type)**: `INFO`
* **TargetUrl (Web)**: `/material-management/form?type={inbound/outbound}&mode=view&id={id}`
* **Payload Data (Mobile App)**:
  * `screen`: **ImportApprove** *(hoặc ExportApprove)*
  * `type`: inbound *(hoặc outbound)*
  * `mode`: view
  * `id`: {id}

### Cảnh báo Sắp Hết Vật Tư (Low Stock Alert)
*Cảnh báo khi số lượng vật tư trong kho xuống dưới mức tối thiểu.*
* **Loại (Type)**: `WARNING`
* **TargetUrl (Web)**: `/material-management/farm-materials/stock?zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **InventoryList**
  * `type`: InventoryList
  * `zoneId`: {zoneId}

---

## 📡 2. Nhóm Thiết Bị & Cảm Biến

### Trạng thái Cảm Biến
*Khi thiết bị cảm biến mất kết nối hoặc có mạng trở lại.*
* **Loại (Type)**: `SUCCESS` *(Có mạng)* / `WARNING` *(Mất kết nối)*
* **TargetUrl (Web)**: `/sensor-devices?mode=view&id={id}`
* **Payload Data (Mobile App)**:
  * `screen`: **SensorDeviceList**
  * `type`: SensorDeviceList
  * `mode`: view
  * `id`: {id}

### Sự cố Cảnh Báo Oxy (Oxy Incident)
*Hệ thống phát hiện nồng độ Oxy nguy hiểm hoặc gửi nhắc nhở.*
* **Loại (Type)**: `ERROR`
* **TargetUrl (Web)**: *(Không có URL, hệ thống dùng Deep-link nội bộ)*
* **Payload Data (Mobile App)**:
  * `targetUrl`: `""` *(Chuỗi rỗng)*

---

## 📷 3. Nhóm Camera An Ninh

### Trạng thái Camera
*Khi Camera bị offline, gặp lỗi, hoặc online trở lại.*
* **Loại (Type)**: `SUCCESS` / `WARNING` / `ERROR`
* **TargetUrl (Web)**: `/camera-management/report?zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **camera-management/report**
  * `zoneId`: {zoneId}

### Camera Cảnh Báo
*Phát hiện có người xâm nhập.*
* **Loại (Type)**: `WARNING`
* **TargetUrl (Web)**: `/camera-management/live/{sn}?zoneId={zoneId}`
* **Payload Data (Mobile App)**:
  * `screen`: **camera-management/live/{sn}**
  * `zoneId`: {zoneId}

---

## ⚙️ Quy Tắc Mapping (Dành cho Backend)
*(Được xử lý tự động trong `NotificationPayloadBuilder.cs`)*

1. **Nhóm Phê duyệt / Chi tiết Kho**: 
   * Nhận diện qua `mode = approve/view` và `type = inbound/outbound/counting`.
   * **Kết quả**: Gán `screen = ImportApprove | ExportApprove | InventoryApprove`.
2. **Nhóm Danh sách Kho**: 
   * Nhận diện qua URL gốc (`/import-receipts`, `/export-receipts`, `/inventory-checks`).
   * **Kết quả**: Gán `screen = ImportReceiptList | ExportWarehouseList | InventoryList`.
3. **Nhóm Thiết bị**: 
   * Trả về màn hình danh sách với `screen = SensorDeviceList`.
4. **Nhóm Camera**: 
   * Trực tiếp dùng đường dẫn gốc làm tên màn hình (VD: `screen = camera-management/report`) do Mobile App hiện chưa định nghĩa hằng số màn hình riêng cho module Camera.

---

## Backend FCM lifecycle note - 2026-05-18

Device token lifecycle:

- Client registers or refreshes a push token through `POST /api/v1/notification/device-token`.
- `SaveDeviceTokenCommandHandler` stores an active `UserDeviceToken` for the authenticated user.
- Logout now cleans up FCM tokens even when `/auth/logout` is called anonymously with a valid active refresh token:
  - `LogoutAllDevices=true` deactivates all active FCM tokens for the refresh-token owner.
  - `FcmToken` deactivates that device token for the refresh-token owner.
  - Without `FcmToken` and without `LogoutAllDevices`, logout only revokes the refresh token.
- Invalid, expired, or already revoked refresh tokens do not deactivate FCM tokens.

Current backend diagnostic log keys:

```text
NOTIFICATION_BULK_PUBLISH
FCM_NO_ACTIVE_TOKEN
FCM_TOKEN_RESOLVED
FCM_SEND_RESULT
FCM_BATCH_EXCEPTION
FCM_TOKEN_STATE_UPDATED
```

The FCM handler intentionally avoids verbose logs such as raw token values, per-platform token counts, per-recipient token counts, notification titles, and generic event-received logs.
