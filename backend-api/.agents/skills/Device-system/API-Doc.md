# Hướng Dẫn Tích Hợp API Lớp Nghiệp Vụ Pond <-> Device

Tài liệu này hướng dẫn cách gọi và map dữ liệu cho các API mới thuộc luồng nghiệp vụ **PondDeviceAssignment**, giúp tách biệt việc gán thiết bị theo nghiệp vụ ra khỏi cấu trúc vật lý của DeviceHub.

## 1. Lấy danh sách thiết bị theo Ao (Pond)

Sử dụng API này để hiển thị danh sách thiết bị đang hoạt động tại một ao cụ thể.

**Endpoint:** 
`GET /api/v1/pond/{pondId}/devices`

**Query Parameters (Tùy chọn - Dùng để filter):**
*   `deviceType` (enum/int): Loại thiết bị (VD: `AirBlower`, `PaddleWheel`,...)
*   `deviceHubId` (Guid): Lọc theo tủ điều khiển (Hub) cụ thể.
*   `usageNo` (int): Lọc theo số thứ tự sử dụng (Oxy #1, Oxy #2).
*   `searchText` (string): Tìm kiếm theo tên hoặc mã thiết bị.

**Response:**
Trả về mảng các `PondDeviceAssignmentDto`.
```json
{
  "isSuccess": true,
  "data": [
    {
      "pondId": "guid",
      "pondName": "Ao 1",
      "deviceId": "guid",
      "deviceName": "Quạt nước 1",
      "deviceCode": "QN01",
      "deviceType": "PaddleWheel",
      "deviceHubId": "guid",
      "deviceHubName": "Tủ điều khiển A",
      "deviceNumber": 1,
      "usageNo": 1,
      "note": "Ghi chú...",
      "isActive": true,
      "assignedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

## 2. Đồng bộ thiết bị cho Ao (Thêm/Cập nhật)

Sử dụng API này khi người dùng gán thiết bị mới vào ao hoặc cập nhật metadata (như `usageNo` hay `note`). Cơ chế hoạt động là "Upsert": nếu chưa có sẽ thêm mới, nếu đã có sẽ cập nhật (hoặc kích hoạt lại nếu đã bị xóa mềm).

**Endpoint:** 
`PUT /api/v1/pond/{pondId}/devices`

**Lưu ý quan trọng cho FE/APP:**
*   `pondId` được lấy từ URL. KHÔNG cần (và không được phép) truyền `pondId` vào trong body (đã được cấu hình `JsonIgnore`).
*   Truyền danh sách các thiết bị muốn đồng bộ trong trường `devices`.

**Request Body:**
```json
{
  "devices": [
    {
      "deviceId": "guid-thiet-bi",
      "usageNo": 1,
      "note": "Ghi chú tùy chọn"
    }
  ]
}
```

**Response:** 
Trả về đối tượng `SyncPondDevicesResult` bao gồm danh sách thiết bị vừa được đồng bộ.

---

## 3. Gỡ thiết bị khỏi Ao

Sử dụng API này khi muốn xóa bỏ sự liên kết nghiệp vụ giữa một thiết bị và một ao (Chỉ Deactivate assignment, KHÔNG xóa Device khỏi Hub vật lý).

**Endpoint:** 
`DELETE /api/v1/pond/{pondId}/devices/{deviceId}`

**Lưu ý:**
Truyền cả `pondId` và `deviceId` trên Route. Body trống.

**Response:**
Trả về `isSuccess: true` nếu gỡ thành công.

---

## 4. Xem danh sách Ao đang sử dụng một Thiết bị

Sử dụng API này ở màn hình Quản lý thiết bị (hoặc chi tiết thiết bị) để xem thiết bị này đang được gán cho những ao nào.

**Endpoint:** 
`GET /api/v1/devices/{deviceId}/ponds`

**Response:**
Trả về danh sách các assignment liên kết với `deviceId` (có chứa thông tin `PondName`).

---

## 5. Xử lý Lỗi (Error Handling) Đặc Biệt

*   **Lỗi Trùng Lặp Gán Ghép (Conflict):** 
    Nếu gọi `PUT` mà gặp lỗi 400 Bad Request kèm `MessageKey.PondDeviceAssignmentSyncConflict`, điều đó có nghĩa là dữ liệu gửi lên bị mâu thuẫn (Ví dụ: truyền nhiều thiết bị cùng 1 ID trong cùng 1 request, hoặc trạng thái db bị conflict). FE cần show thông báo lỗi chuẩn thay vì báo lỗi hệ thống.
