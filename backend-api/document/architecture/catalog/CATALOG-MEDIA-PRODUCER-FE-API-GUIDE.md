# Catalog Media & Producer Picker — FE API Guide

**Phạm vi:** backoffice Product Wizard, ProductImage upload/scan/attach, và Producer picker.

**Nguồn sự thật:** API V1 hiện tại. Đây không phải contract cho Producer self-service, Azure Blob trực tiếp, stock, promotion, hoặc media scan webhook/SSE.

## 1. Quy ước chung

- Base route: `/api/v1`.
- Tất cả response dùng envelope:

```json
{
  "success": true,
  "data": {},
  "message": "Success",
  "errorCode": null,
  "validationErrors": null,
  "details": null,
  "timestamp": "2026-08-09T00:00:00Z"
}
```

- Enum được trả dưới dạng string; thời gian UTC ISO-8601.
- Với `401`, đưa người dùng tới authentication. Với `403`, ẩn/khóa action theo quyền. Với `429`, tôn trọng header `Retry-After` nếu có.
- Không tạo hoặc đoán Blob URL, storage key, scan lease, retry job hay `failureCode` ngoài dữ liệu API trả về.

## 2. Quyền FE cần có

| Màn hình / action | HTTP policy |
|---|---|
| Upload ProductImage | `media.upload` |
| Xem metadata và retry scan | `media.read` |
| Retry media người khác | `media.read` **và** `media.manage` |
| Attach media vào Product | `catalog.products.update` + quyền owner media hoặc `media.manage` |
| Producer picker / tạo Product | `catalog.products.create` |
| Đọc management Product để lấy stamp mới | `catalog.products.read` |

Owner nghĩa là user đã upload media đó. Server là authority; FE không suy luận owner từ UI state.

## 3. ProductImage flow

### 3.1 Chọn Producer trước khi tạo Product

`GET /api/v1/catalog/producers?q={text}&page=1&pageSize=20`

Chỉ trả Producer `Published` và `isVerified=true`. `q` tìm theo code hoặc name, tối đa 250 ký tự; `page >= 1`, `pageSize` từ 1 đến 100.

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "b4db3d0b-9c91-4be6-b1ef-0c9f0f6d17d6",
        "code": "HTX-MOC-CHAU",
        "name": "HTX Mộc Châu",
        "publicStatus": "Published",
        "isVerified": true
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1,
    "pageSize": 20,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

Khi form đang giữ một ID, có thể revalidate bằng `GET /api/v1/catalog/producers/{producerId}`. `404` nghĩa là ID không còn eligible và FE phải yêu cầu chọn lại; không gửi ID này vào wizard như lựa chọn hợp lệ.

Tạo Product vẫn dùng API Product hiện có. Giữ `data.id` và `data.concurrencyStamp` từ response tạo Product; mọi mutation Product sau đó phải gửi stamp mới nhất.

### 3.2 Upload

`POST /api/v1/media` — `multipart/form-data`

| Field | Giá trị |
|---|---|
| `file` | file ảnh, bắt buộc; giới hạn request 10 MiB |
| `intent` | `ProductImage` |
| `altText` | tùy chọn, tối đa 500 ký tự |

Response thành công là `201`:

```json
{
  "success": true,
  "data": {
    "id": "c49aab87-dce4-4efe-9bc7-02609b85f571",
    "originalFileName": "tra-xanh.jpg",
    "contentType": "image/jpeg",
    "sizeBytes": 248991,
    "mediaType": "Image",
    "visibility": "Restricted",
    "scanStatus": "Pending",
    "targetVisibility": "Public"
  }
}
```

Lưu `mediaAssetId`; không attach ngay sau upload.

### 3.3 Poll metadata và render state

`GET /api/v1/media/{mediaAssetId}`

```json
{
  "success": true,
  "data": {
    "id": "c49aab87-dce4-4efe-9bc7-02609b85f571",
    "originalFileName": "tra-xanh.jpg",
    "contentType": "image/jpeg",
    "sizeBytes": 248991,
    "mediaType": "Image",
    "uploadIntent": "ProductImage",
    "visibility": "Public",
    "targetVisibility": "Public",
    "scanStatus": "Clean",
    "altText": "Gói trà xanh",
    "scanFailureCode": null,
    "scanFailureReason": null,
    "canRetryScan": false,
    "nextScanAttemptAt": null,
    "createdAt": "2026-08-09T00:00:00Z"
  }
}
```

| `scanStatus` / visibility | FE rendering | Action tiếp theo |
|---|---|---|
| `Pending` | “Đang kiểm tra ảnh”; disable attach | Poll tiếp. Nếu `nextScanAttemptAt` có giá trị, không hiện retry manual. |
| `Clean` + `Public` | Có thể attach | Gọi attach với Product stamp mới nhất. |
| `Clean` + không `Public` | Không attach | Hiện trạng thái unavailable; không tự đổi visibility. |
| `Failed` | Hiện `scanFailureReason`; bật Retry chỉ khi `canRetryScan=true` | Gọi retry scan. |
| `Rejected` | Hiện reason; không có retry | Yêu cầu chọn/upload ảnh khác. |

Backend không cung cấp SSE/webhook hay polling interval. FE nên dùng backoff có giới hạn, dừng khi state terminal (`Clean`, `Failed`, `Rejected`) hoặc khi user rời wizard; luôn fetch metadata mới trước khi quyết định attach/retry.

Các `scanFailureCode` hiện có: `FILE_REJECTED`, `SCANNER_UNAVAILABLE`, `THUMBNAIL_GENERATION_FAILED`, `STORAGE_PROCESSING_FAILED`, `MEDIA_PROCESSING_FAILED`. Chỉ dùng để phân loại UX/telemetry; `scanFailureReason` là nội dung an toàn để hiển thị.

### 3.4 Retry failed scan

`POST /api/v1/media/{mediaAssetId}/retry-scan`

- Không có request body.
- Browser flow phải lấy CSRF token trước bằng `GET /api/v1/security/csrf`, lưu cookie phản hồi, và gửi token trong header `X-CSRF-TOKEN` cho POST. Khi dùng cross-origin browser request, bật credentials để cookie CSRF được gửi.
- API bị rate-limit theo policy management mutation; xử lý `429` bằng `Retry-After`.

Response `200` là cùng shape metadata. Semantics:

- `Failed` → reset thành `Pending`, xoá reason/code và reset internal retry budget.
- `Pending` → `200` metadata hiện tại; không tạo scan job trùng.
- `Clean` hoặc `Rejected` → `400`, `message="MEDIA_SCAN_RETRY_INVALID"`, `errorCode="BAD_REQUEST"`.
- Asset không tồn tại → `404`; asset không thuộc quyền owner/manager → `403`.

Sau `200`, thay object state trong store bằng `data` response và tiếp tục poll; không optimistic-attach.

### 3.5 Attach media vào Product

`POST /api/v1/catalog/products/{productId}/media`

```json
{
  "concurrencyStamp": "4a5b05bb-bcea-4bbd-a3d3-a9b5833d0870",
  "mediaAssetId": "c49aab87-dce4-4efe-9bc7-02609b85f571",
  "displayOrder": 0,
  "makePrimary": true,
  "caption": "Gói trà xanh"
}
```

- Route `productId` là authoritative; FE vẫn phải gửi body hợp lệ.
- `concurrencyStamp` là bắt buộc và phải là giá trị mới nhất từ create/detail/mutation Product.
- `displayOrder >= 0`; `caption` tối đa 500 ký tự.
- Server kiểm tra media ownership trước readiness, nên FE không được diễn giải `403` là scan failure.

Response thành công:

```json
{
  "success": true,
  "data": {
    "id": "a0998f32-4c33-448e-9763-4a426c480dca",
    "slug": "tra-xanh-huu-co",
    "status": "Draft",
    "concurrencyStamp": "0b7fc12e-85ce-49df-a534-3c7593ce76a2"
  }
}
```

Ngay khi thành công, ghi đè Product stamp trong store bằng stamp response. Khi `409` (`errorCode="ALREADY_EXISTS"`), refetch `GET /api/v1/catalog/products/{productId}`, hiển thị state mới và yêu cầu user xác nhận áp dụng lại thay đổi; không replay body cũ tự động.

Media chưa `Clean + Public` trả tương thích `400`, `message="MEDIA_NOT_READY"`, `errorCode="BAD_REQUEST"`. Với `403`, dừng flow và không hiển thị scan state của asset đó.

## 4. Store và UI state tối thiểu

```text
producerSelection: CatalogProducerPickerDto | null
product: { id, concurrencyStamp, ... } | null
mediaById[mediaAssetId]: MediaMetadataResult
activePolls: Set<mediaAssetId>
```

1. Picker chọn Producer eligible → tạo hoặc cập nhật wizard state.
2. Upload → lưu `mediaAssetId` ở `Pending` → bắt đầu poll.
3. `Clean + Public` → mở action Attach.
4. Attach success → cập nhật stamp và refetch Product detail nếu cần hydrate media list.
5. `Failed` → retry theo user action; `Rejected` → remove khỏi UI selection và yêu cầu file mới.

## 5. Capability không có trong API hiện tại

- Không có endpoint để FE đọc Producer Draft/unverified hoặc quản trị Producer.
- Không có endpoint đổi visibility, ép scan-clean, cancel scan, trigger scan ngay, hay nhận tiến trình scan push.
- Không có direct Azure Blob read/write, raw storage key/SAS URL hay client-side storage promotion.
- Không suy ra stock, promotion, review, shipping, preview image URL hoặc Product public visibility chỉ từ metadata media.
