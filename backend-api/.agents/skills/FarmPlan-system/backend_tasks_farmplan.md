# Phân rã tác vụ Backend - Module Kế Hoạch (Farm Plan)

Tài liệu này định nghĩa các tác vụ kỹ thuật dành cho team Backend để triển khai Module Kế Hoạch theo tài liệu đặc tả nghiệp vụ `Define_KeHoach_Ecom_v1.1.docx`.

## 1. Thiết kế cơ sở dữ liệu (Database Schema & Entities)
Tạo mới các entity trong thư mục `Core/Ecom.Domain/Entities/FarmPlan` và cấu hình EF Core.

### 1.1. Bảng `FarmPlans` (Kế hoạch)
Thiết kế dùng chung một bảng với cột phân loại `PlanType` (Construction / Farming) do các trường khác biệt không quá nhiều.

**Trường chung:**
- `Id` (Guid, PK)
- `Name` (string, bắt buộc): Tên kế hoạch
- `ScopeId` (Guid, bắt buộc): ID của trại hoặc ao
- `ScopeType` (Enum: Farm/Pond, bắt buộc): Loại phạm vi (Trại hoặc Ao)
- `PlanType` (Enum: Construction/Farming, bắt buộc): Kế hoạch xây dựng hay Vụ nuôi
- `Status` (Enum: Draft/InProgress/Completed, bắt buộc): Trạng thái (Mặc định: Draft)
- `Description` (string, max: tự do): Mô tả chung / Ghi chú
- `StartDate` (DateTime, nullable): Ngày bắt đầu / Ngày thả giống
- `ExpectedEndDate` (DateTime, nullable): Ngày hoàn thành / thu hoạch dự kiến
- Các trường Audit tiêu chuẩn: `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, `IsDeleted` (Soft delete)

**Trường riêng cho Vụ nuôi (Farming Plan) - Nullable:**
- `ShrimpType` (string): Loại tôm thả giống
- `ExpectedCost` (decimal): Chi phí dự kiến
- `ExpectedYield` (decimal): Dự kiến sản lượng (tấn)
- `FCR` (decimal): Hệ số chuyển đổi thức ăn (Feed Conversion Ratio)
- `ExpectedPrice` (decimal): Giá bán dự kiến (đ/kg)
- `ActualYield` (decimal): Sản lượng thực tế (Nhập sau khi hoàn thành)
- `ActualCost` (decimal): Tổng chi phí thực tế (Nhập sau khi hoàn thành)
- `ActualRevenue` (decimal): Doanh thu thực tế (Nhập sau khi hoàn thành)

*(Ghi chú: Thức ăn cần = Sản lượng * FCR, Doanh thu = Sản lượng * Giá bán, Lợi nhuận = Doanh thu - Chi phí. Backend sẽ tính toán trả về qua DTO, không cần lưu vào DB nếu có thể tính toán, hoặc lưu cache).*

### 1.2. Bảng `FarmPlanAttachments` (Tệp đính kèm)
- `Id` (Guid, PK)
- `FarmPlanId` (Guid, FK)
- `FileName` (string)
- `FileUrl` (string)
- `FileSize` (long)
- `FileType` (string)

### 1.3. Bảng `FarmPlanHistories` (Lịch sử chỉnh sửa)
- `Id` (Guid, PK)
- `FarmPlanId` (Guid, FK)
- `Action` (Enum: Created, Updated, Deleted)
- `ChangedBy` (string/Guid): Người thực hiện
- `ChangedAt` (DateTime): Thời gian thực hiện
- `Changes` (string/JSON, nullable): Lưu chi tiết thay đổi (nếu cần)

---

## 2. Lớp Domain & Application (CQRS & Validation)

### 2.1. Định nghĩa Enums & Constants
- `PlanType`: `Construction` (Xây dựng), `Farming` (Vụ nuôi)
- `PlanStatus`: `Draft` (Bản nháp), `InProgress` (Đang thực hiện), `Completed` (Hoàn thành)

### 2.2. Commands (Thay đổi trạng thái hệ thống)
- **`CreateFarmPlanCommand`**: 
  - Validate: `Name` và `ScopeId` là bắt buộc.
  - Xử lý tệp đính kèm (upload, max 20MB).
  - Tự động ghi nhận `CreatedBy` và tạo bản ghi `FarmPlanHistory`.
- **`UpdateFarmPlanCommand`**:
  - Validate: Không cho sửa `PlanType`.
  - Tự động tính toán các trường `ExpectedRevenue`, `ExpectedProfit` (nếu trả về trực tiếp, hoặc xử lý trên view).
  - Tự động ghi nhận `UpdatedBy`, thời gian sửa và tạo bản ghi `FarmPlanHistory`.
- **`DeleteFarmPlanCommand`**:
  - Thực hiện Soft Delete.
  - Ghi nhận lịch sử xóa.
- **`ChangeFarmPlanStatusCommand`**:
  - Cập nhật thủ công `Status`.

### 2.3. Queries (Truy xuất dữ liệu)
- **`GetFarmPlansQuery`**:
  - Input: Keyword (Name), Status (List), ScopeId (List), PlanType.
  - Output: Pagination kết hợp thông tin `Scope` (tên trại/ao). Trả về DTO chứa thông tin tổng hợp cho danh sách. Sắp xếp: Cập nhật cuối mới nhất.
- **`GetFarmPlanDetailQuery`**:
  - Output: Chi tiết Kế hoạch bao gồm danh sách tệp đính kèm (`FarmPlanAttachments`) và chi phí/sản lượng (đối với vụ nuôi).
  - Tự tính toán: Doanh thu dự kiến, Lợi nhuận dự kiến.
- **`GetFarmPlanHistoriesQuery`**:
  - Output: Danh sách lịch sử cập nhật của một kế hoạch. Sắp xếp: Mới nhất lên đầu.
- **`GetFarmPlanComparisonQuery`**:
  - So sánh Kế hoạch vs Thực tế (Chỉ hợp lệ khi trạng thái `Completed`).
  - So sánh giữa các vụ: Lấy danh sách các kế hoạch `Farming` đã `Completed` của cùng một `ScopeId` (Trại/Ao).

### 2.4. Business Rules & Validation (FluentValidation)
- Yêu cầu bắt buộc `Name` (không để trống) và `ScopeId` hợp lệ.
- File đính kèm phải tuân thủ dung lượng (Max 20MB) và định dạng cho phép (PDF, PNG, JPG, DOCX, DWG, XLSX).
- Validation dữ liệu thực tế chỉ được nhập/cập nhật khi `Status = Completed`.

---

## 3. Cấu hình API Controllers (`Presentation/Ecom.API`)

Tạo `FarmPlansController` bao gồm các endpoints:
- `GET /api/farm-plans`: Lấy danh sách (Có filter và phân trang)
- `GET /api/farm-plans/{id}`: Xem chi tiết (Tab Thông tin)
- `POST /api/farm-plans`: Tạo mới (Nhận form-data để xử lý upload file đính kèm)
- `PUT /api/farm-plans/{id}`: Cập nhật
- `DELETE /api/farm-plans/{id}`: Xóa (Soft delete)
- `PATCH /api/farm-plans/{id}/status`: Cập nhật trạng thái
- `GET /api/farm-plans/{id}/histories`: Lấy lịch sử cập nhật (Tab Lịch sử)
- `GET /api/farm-plans/{id}/comparisons`: So sánh Kế hoạch vs Thực tế
- `GET /api/farm-plans/scope/{scopeId}/comparisons`: So sánh liên vụ của cùng một ao/trại

---

## 4. Tác vụ Hạ tầng (Infrastructure)

- **Upload Service**: 
  - Tích hợp / Mở rộng thư viện upload file hiện tại để hỗ trợ upload và lưu trữ các tệp đính kèm của Kế hoạch (có kiểm tra giới hạn 20MB).
- **Phân quyền (Permissions)**:
  - Cập nhật danh sách Permission cho module kế hoạch (VD: `Permissions.FarmPlans.View`, `Permissions.FarmPlans.Create`, `Permissions.FarmPlans.Edit`, `Permissions.FarmPlans.Delete`).
  - Phân quyền theo Role hoặc Scope (Chủ trại, Quản lý, Kỹ thuật viên).

---

## 5. Các bước triển khai gợi ý
1. **Migrations**: Viết code Entities, DbContext và chạy `add-migration AddFarmPlanModule`. Cập nhật database.
2. **CQRS & Validation**: Triển khai Commands, Queries, Validators trong `Ecom.Application`.
3. **API Controllers**: Gắn kết Endpoints trong `Ecom.API` và phân quyền `[Authorize]`.
4. **Unit Tests / Manual Tests**: Viết test hoặc xác minh các nghiệp vụ tính toán Tự động (Doanh thu, Lợi nhuận) và chặn chuyển trạng thái, chặn so sánh khi chưa `Completed`.

