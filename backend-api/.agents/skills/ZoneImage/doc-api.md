# Hướng dẫn tích hợp API Ảnh Sơ đồ 3D (Zone 3D Images)

Tài liệu hướng dẫn Front-End (FE) và QA tích hợp đúng các API quản lý ảnh sơ đồ 3D của Zone (Trại/Vùng nuôi).

---

## 1. Lưu ý quan trọng về Dữ liệu

| Trường dữ liệu | Kiểu dữ liệu | Ý nghĩa | Lưu ý quan trọng |
| :--- | :--- | :--- | :--- |
| **`id`** | Guid | ID của bản ghi ảnh 3D (`imageId`) | **Bắt buộc dùng ID này** làm `{imageId}` trên path khi PATCH/DELETE. |
| **`zoneId`** | Guid | ID của Zone | Gắn vào path `{zoneId}` của tất cả API ảnh 3D. Không gửi trong body. |
| **`documentId`** | Guid | ID của Document liên kết | Chỉ dùng để tham chiếu file gốc. **Không** dùng làm `{imageId}` để PATCH/DELETE. |
| **`imageUrl`** | String | URL hiển thị ảnh | Lấy trực tiếp từ API. Không tự cache lâu vì URL CDN có thể hết hạn. |
| **`sortOrder`** | Int | Thứ tự hiển thị | Dùng để hiển thị theo đúng thứ tự (0-based). |

### Ví dụ Response Object:
```json
{
  "id": "a695a8c8-7d6a-4c34-98a7-829bb7bcd194",
  "zoneId": "8d2cd504-f7b5-4019-84c4-2ed938c14d80",
  "documentId": "b3f50eb4-c838-48b8-8d5f-8f0600c14959",
  "fileName": "Farm 5 Kien Giang.jpg",
  "imageUrl": "https://...",
  "thumbnailUrl": "https://...",
  "sortOrder": 0
}
```

---

## 2. Chi tiết các API

### 2.1. Lấy danh sách ảnh (GET)
* **Endpoint:** `GET /api/v1/zone/{zoneId}/3d-images?excludePdf=false`
* **Headers:** `Authorization: Bearer <JWT_TOKEN>`
* **Tham số Path (URL Parameter):**
  * `zoneId` (Guid, Bắt buộc): ID của Zone.
* **Tham số Query:**
  * `excludePdf` (Boolean, Không bắt buộc): Truyền `true` để ẩn các file sơ đồ dạng PDF khỏi danh sách trả về (Dành cho Mobile App). Mặc định `false` sẽ trả về đầy đủ ảnh và PDF.
* **Xử lý FE:** BE đã tự động sắp xếp danh sách trả về theo thứ tự `sortOrder` tăng dần và `createdAt`. FE chỉ cần nhận và hiển thị trực tiếp.
* **Phản hồi mẫu (Response Body - `200 OK`):**
  ```json
  {
    "success": true,
    "data": [
      {
        "id": "a695a8c8-7d6a-4c34-98a7-829bb7bcd194",
        "zoneId": "8d2cd504-f7b5-4019-84c4-2ed938c14d80",
        "documentId": "b3f50eb4-c838-48b8-8d5f-8f0600c14959",
        "fileName": "Farm 5 Kien Giang.jpg",
        "imageUrl": "https://...",
        "thumbnailUrl": "https://...",
        "sortOrder": 0
      }
    ]
  }
  ```

### 2.2. Thêm mới ảnh (POST)
* **Endpoint:** `POST /api/v1/zone/{zoneId}/3d-images`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>`
  * `Content-Type: multipart/form-data`
* **Tham số Path (URL Parameter):**
  * `zoneId` (Guid, Bắt buộc): ID của Zone.
* **Tham số Body (FormData Request):**
  * `Files` (Kiểu `file[]`, Bắt buộc): Chọn 1 hoặc nhiều file ảnh.
* **Lưu ý:**
  * Key trong FormData phải chính xác là `Files`.
  * Không gửi kèm `SortOrder`; BE sẽ tự động append vào cuối danh sách (tăng dần `sortOrder` bắt đầu từ `0`).
* **Phản hồi mẫu (Response Body - `200 OK`):**
  ```json
  {
    "success": true,
    "data": [
      {
        "id": "e91dc457-3a1b-4cd3-a8c4-123456789abc",
        "zoneId": "8d2cd504-f7b5-4019-84c4-2ed938c14d80",
        "documentId": "b3f50eb4-c838-48b8-8d5f-8f0600c14959",
        "fileName": "new-farm-map.jpg",
        "imageUrl": "https://...",
        "thumbnailUrl": "https://...",
        "sortOrder": 1
      }
    ]
  }
  ```

### 2.3. Cập nhật ảnh hoặc thứ tự (PATCH)
* **Endpoint:** `PATCH /api/v1/zone/{zoneId}/3d-images/{imageId}`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>`
  * `Content-Type: multipart/form-data`
* **Tham số Path (URL Parameter):**
  * `zoneId` (Guid, Bắt buộc): ID của Zone.
  * `imageId` (Guid, Bắt buộc): ID bản ghi ảnh 3D (`id` của `Zone3DImage`, không phải `documentId`).
* **Tham số Body (FormData Request - Phải gửi ít nhất 1 trong 2 trường):**
  * `Image` (Kiểu `file`, Không bắt buộc): File ảnh mới nếu muốn thay đổi hình ảnh.
  * `SortOrder` (Kiểu `int`, Không bắt buộc): Giá trị thứ tự mới (>= 0).
* **Phản hồi mẫu (Response Body - `200 OK`):**
  ```json
  {
    "success": true,
    "data": {
      "id": "a695a8c8-7d6a-4c34-98a7-829bb7bcd194",
      "zoneId": "8d2cd504-f7b5-4019-84c4-2ed938c14d80",
      "documentId": "c4d50eb4-d939-59c9-9e5f-9f0600c14959",
      "fileName": "updated-farm-map.jpg",
      "imageUrl": "https://...",
      "thumbnailUrl": "https://...",
      "sortOrder": 2
    }
  }
  ```

### 2.4. Xóa ảnh (DELETE)
* **Endpoint:** `DELETE /api/v1/zone/{zoneId}/3d-images/{imageId}`
* **Headers:** `Authorization: Bearer <JWT_TOKEN>`
* **Tham số Path (URL Parameter):**
  * `zoneId` (Guid, Bắt buộc): ID của Zone.
  * `imageId` (Guid, Bắt buộc): ID bản ghi ảnh 3D (`id` của `Zone3DImage`, không phải `documentId`).
* **Lưu ý:** Sau khi xóa thành công, BE sẽ tự động cập nhật lại thứ tự (`sortOrder`) của các ảnh còn lại để đảm bảo tính liên tục từ `0` đến `n-1`.
* **Phản hồi mẫu (Response Body - `200 OK`):**
  ```json
  {
    "success": true,
    "data": null
  }
  ```

---

## 3. Quy định validate & Giới hạn
* **Giới hạn dung lượng:**
  * Mỗi file tối đa **50 MB**.
  * Tổng request multipart tối đa **250 MB**.
* **Định dạng:** Chấp nhận các định dạng file ảnh hợp lệ và định dạng PDF. Nếu có bất kỳ file nào không hợp lệ trong request tải lên nhiều file, toàn bộ request sẽ bị từ chối. (Lưu ý: Hệ thống không tạo thumbnail tự động cho file PDF).
* **Quy tắc map API:** Luôn gọi lại API GET danh sách sau khi thực hiện thêm, sửa hoặc xóa để cập nhật chính xác URL và thứ tự hiển thị mới nhất từ server.
