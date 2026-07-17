# Tài liệu tích hợp Notification & FCM

Dành cho frontend và mobile khi hiển thị in-app notification, push notification, badge unread count, và tách tab `notification` / `alert` / `issue`.

## 1. Tổng quan

Backend lưu lịch sử notification trong bảng `Tbl_Notification` và phát push qua Firebase Cloud Messaging (FCM) đến các device token đang active của user.

Notification hiện có 2 trường phân loại:

| Trường | Giá trị | Mục đích |
|---|---|---|
| `type` | `SUCCESS`, `INFO`, `WARNING`, `ERROR` | Severity/trạng thái hiển thị như cũ. |
| `category` | `notification`, `alert`, `issue` | Nhóm hiển thị: thông báo thường, cảnh báo, hoặc thông báo sự cố IssueReport. |

Frontend dùng `category` để map tab/loại thông báo. Không dùng `type` để tách tab.

## 2. Quy ước chung

Base URL hiện tại trong controller là:

```text
/api/v1/notification
```

Giá trị hợp lệ của `category`:

| Giá trị | Mô tả |
|---|---|
| `notification` | Thông báo thường: workflow, kết quả duyệt/từ chối, trạng thái phục hồi, nhập/xuất/kiểm kê. |
| `alert` | Cảnh báo cần ưu tiên: tồn kho thấp, AI phát hiện người, mất kết nối, vượt ngưỡng, Oxy cần xử lý. |
| `issue` | Thông báo sự cố sinh từ IssueReport: lỗi phần mềm hoặc lỗi thiết bị IoT. |

Nếu không truyền `category`, API giữ hành vi cũ và trả/đối tượng trên tất cả nhóm.

## 3. API Notification

### 3.1. Lấy danh sách notification

```http
GET /api/v1/notification
Authorization: Bearer <JWT_TOKEN>
```

Query parameters:

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---|---:|---:|---|
| `isRead` | bool? | Không | Lọc theo trạng thái đọc. Bỏ trống để lấy tất cả. |
| `zoneId` | Guid? | Không | Lọc notification theo zone hiện tại và notification global. |
| `category` | string? | Không | `notification`, `alert` hoặc `issue`. Bỏ trống để lấy tất cả. |
| `page` | int | Không | Mặc định 1. |
| `pageSize` | int | Không | Mặc định 10, tối đa 50. |

Ví dụ:

```http
GET /api/v1/notification?category=alert&page=1&pageSize=20
```

Response mẫu:

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "89543881-ad6d-4af4-a53b-3a39a84b8807",
        "title": "Tồn kho",
        "message": "Vật tư thức ăn đang có số lượng tồn kho dưới mức cảnh báo.",
        "type": "WARNING",
        "category": "alert",
        "isRead": false,
        "targetUrl": "/material-management/farm-materials/stock?zoneId=3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "createdAt": "2026-05-28T03:30:05Z",
        "payload": {
          "notificationType": "WARNING",
          "category": "alert",
          "targetUrl": "/material-management/farm-materials/stock?zoneId=3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "createdAt": "2026-05-28 10:30:05.000 +0700",
          "screen": "InventoryList",
          "type": "InventoryList",
          "zoneId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        }
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1
  },
  "message": "Success",
  "errors": null
}
```

Nếu `category` không hợp lệ, backend trả về lỗi validation/business:

```json
{
  "success": false,
  "message": "Category chỉ chấp nhận notification, alert hoặc issue."
}
```

### 3.2. Lấy số lượng unread

```http
GET /api/v1/notification/unread-count
Authorization: Bearer <JWT_TOKEN>
```

Query parameters:

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---|---:|---:|---|
| `zoneId` | Guid? | Không | Lọc theo zone hiện tại. |
| `category` | string? | Không | `notification`, `alert` hoặc `issue`. Bỏ trống để đếm tất cả. |

Ví dụ:

```http
GET /api/v1/notification/unread-count?category=alert
```

Response:

```json
{
  "success": true,
  "data": 3,
  "message": "Success",
  "errors": null
}
```

### 3.3. Đánh dấu một notification đã đọc

```http
PATCH /api/v1/notification/{id}/read
Authorization: Bearer <JWT_TOKEN>
```

Hành vi:

- Chỉ mark notification có đúng `{id}` thành `isRead = true`.
- Không cần và không nhận filter `category`.
- Áp dụng cho cả `notification` và `alert`.

Response thành công:

```json
{
  "success": true,
  "data": null,
  "message": "Success",
  "errors": null
}
```

### 3.4. Đánh dấu tất cả đã đọc

```http
PATCH /api/v1/notification/read-all
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

Request body:

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---|---:|---:|---|
| `zoneId` | Guid? | Không | Chỉ mark read notification thuộc zone tương ứng. |
| `category` | string? | Không | Chỉ mark read theo nhóm `notification`, `alert` hoặc `issue`. Bỏ trống để mark tất cả nhóm như hành vi cũ. |

Ví dụ:

```http
PATCH /api/v1/notification/read-all
Content-Type: application/json
```

```json
{
  "zoneId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "category": "alert"
}
```

Hành vi:

- `category=alert`: chỉ mark alert unread thành read.
- `category=notification`: chỉ mark thông báo thường unread thành read.
- `category=issue`: chỉ mark IssueReport unread thành read.
- Không truyền `category`: mark tất cả unread visible của user.

Response `data` là số row đã cập nhật:

```json
{
  "success": true,
  "data": 5,
  "message": "Success",
  "errors": null
}
```

## 4. API Device Token

### 4.1. Đăng ký FCM device token

```http
POST /api/v1/notification/device-token
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

Request body:

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---|---:|---:|---|
| `fcmToken` | string | Có | Token do Firebase SDK cấp. |
| `platform` | string | Có | `Web`, `Android`, hoặc `iOS`. |

Response thành công:

```json
{
  "success": true,
  "data": null,
  "message": "Success",
  "errors": null
}
```

### 4.2. Logout và hủy token

```http
POST /api/v1/auth/logout
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

Request body:

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---|---:|---:|---|
| `refreshToken` | string | Có | Token dùng để vô hiệu hóa phiên làm việc. |
| `logoutAllDevices` | bool | Không | `true` để logout tất cả thiết bị; `false` để logout thiết bị hiện tại. |
| `fcmToken` | string | Không | FCM token của thiết bị hiện tại để backend deactivate token này. |

Không log raw refresh token hoặc raw FCM token.

## 5. FCM payload data

Push notification FCM có `notification` và `data`. Frontend/mobile nên đọc `data.category` để map tab và hành vi hiển thị.

Payload data chuẩn:

```json
{
  "notificationType": "WARNING",
  "category": "alert",
  "targetUrl": "/material-management/farm-materials/stock?zoneId=3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2026-05-28 10:30:05.000 +0700",
  "screen": "InventoryList",
  "type": "InventoryList",
  "zoneId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

Lưu ý:

- `notificationType` là severity gốc (`SUCCESS`, `INFO`, `WARNING`, `ERROR`).
- `category` là nhóm hiển thị (`notification`, `alert`, `issue`).
- `targetUrl` dùng cho deep-link.
- `screen`, `type`, `id`, `zoneId`, `deviceCode` có thể được parse từ `targetUrl` nếu có.
- Các key reserved trong payload builder gồm `notificationType`, `category`, `targetUrl`, `createdAt`, `screen`; query param trùng key reserved sẽ không override các key này.

## 6. Mapping category hiện tại

| Nhóm nghiệp vụ | Trường hợp | `category` |
|---|---|---|
| Warehouse | Yêu cầu duyệt phiếu nhập/xuất/kiểm kê | `notification` |
| Warehouse | Duyệt/từ chối phiếu nhập/xuất/kiểm kê | `notification` |
| Warehouse | Nhập/xuất tự động từ quy trình hoặc từ kiểm kê | `notification` |
| Warehouse | Tồn kho thấp | `alert` |
| Camera/AI | AI phát hiện người | `alert` |
| Camera/AI | Camera không hoạt động/mất kết nối | `alert` |
| Camera/AI | Camera hoạt động lại | `notification` |
| Camera/AI | Camera gặp lỗi phần cứng | `notification` |
| Sensor | Cảm biến vượt ngưỡng/vượt ngưỡng kéo dài | `alert` |
| Sensor | Cảm biến mất kết nối/mất kết nối kéo dài | `alert` |
| Sensor | Cảm biến trở về bình thường | `notification` |
| Sensor | Cảm biến kết nối/hoạt động lại | `notification` |
| Oxy | Nhắc xử lý sự cố Oxy | `alert` |
| Oxy | Máy Oxy tắt quá lâu | `alert` |
| Oxy | Initial incident và escalation theo workflow hiện tại | `notification` |
| IssueReport | Báo cáo lỗi phần mềm | `issue` |
| IssueReport | Báo cáo lỗi thiết bị IoT | `issue` |

## 7. Hướng dẫn client

- Tab thông báo thường gọi `GET /notification?category=notification`.
- Tab cảnh báo gọi `GET /notification?category=alert`.
- Tab sự cố IssueReport gọi `GET /notification?category=issue`.
- Badge riêng cho alert gọi `GET /notification/unread-count?category=alert`.
- Badge riêng cho IssueReport gọi `GET /notification/unread-count?category=issue`.
- Nút "mark all read" trong tab alert gọi `PATCH /notification/read-all` với body `{ "zoneId": "<zoneId>", "category": "alert" }`.
- Khi user click một item cụ thể, gọi `PATCH /notification/{id}/read`; không cần truyền `category`.
- Nếu app đang mở theo zone, tiếp tục truyền `zoneId` vào list/count query string và truyền `zoneId` trong body của read-all để giảm dữ liệu load.
