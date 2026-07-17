# Tài liệu API Điều Khiển Cân (Scale Control)

Dành cho Frontend/App khi cần thực hiện điều khiển cân trực tiếp.

## 1. Tổng quan

API gửi lệnh (`ZERO`, `TARE`, `CLEAR_TARE`) xuống thiết bị cân IoT. 
API có cơ chế Guard: Chỉ cho phép thao tác khi cân **đang trực tuyến (độ trễ < 15s)** và **đã đứng số (`Stable=true`)**. Trái điều kiện sẽ trả về lỗi HTTP 400 lập tức.

## 2. API Scale Control

### 2.1. Gửi lệnh điều khiển (Control)

```http
POST /api/v1/scale/{id}/control
Authorization: Bearer <JWT_TOKEN>
```

| Tham số | Kiểu | Bắt buộc | Vị trí | Mô tả |
|---|---:|---:|---|---|
| `id` | Guid | Có | Path | ID của cân |
| `action` | string | Có | Body | Hỗ trợ: `ZERO`, `TARE`, `CLEAR_TARE` |

Ví dụ Request:
```json
{
  "action": "TARE"
}
```

### 2.2. Chi tiết các Action

| Action | Chức năng |
|---|---|
| `ZERO` | Đưa giá trị cân về điểm 0. |
| `TARE` | Trừ bì dựa trên trọng lượng vật đang đặt trên cân. |
| `CLEAR_TARE` | Xóa trạng thái trừ bì, trở về cân Gross. |

### 2.3. Response Mẫu

**Thành công (200 OK):**
```json
{
  "success": true,
  "data": {
    "action": "TARE",
    "tareInfo": { "tareWeight": 1.25, "unit": "kg" }
  },
  "message": "Thực thi lệnh cân thành công."
}
```

**Thất bại (400 Bad Request):**
```json
{
  "success": false,
  "message": "Cân không ổn định, không thể thực hiện thao tác." 
}
```

## 3. Hướng dẫn Client

- **Trạng thái UI:** Nút bấm TARE/ZERO nên được Disable (Làm mờ) nếu màn hình cân đang **Mất kết nối** hoặc **Chưa đứng số**.
- **Hiển thị lỗi:** App bắt buộc phải xử lý lỗi HTTP 400 và hiển thị Popup/Toast nguyên văn trường `message` trả về từ API.
- **Dữ liệu:** Khi thực hiện lệnh `TARE` thành công, có thể đọc `data.tareInfo.tareWeight` để cập nhật khối lượng bì lên giao diện nếu cần.
