# Hướng dẫn bàn giao & Kế hoạch triển khai Phase 3 & 4: Issue Report

Tài liệu này được biên soạn để bàn giao cho các thành viên trong đội phát triển tiếp tục thực hiện **Phase 3 (Permissions)** và **Phase 4 (FCM/Notifications)** cho chức năng Báo cáo lỗi (Report Management).

---

## 1. Tóm tắt những gì ĐÃ HOÀN THÀNH (Phase 1 & 2)

Hệ thống hiện tại đã biên dịch thành công (`Build succeeded`) và chạy hoàn chỉnh phần khung cốt lõi cùng cơ sở dữ liệu.

### A. Database Schema (Chỉ sửa DB một lần ở Phase 1)
Toàn bộ database schema đã được khởi tạo và chạy migration PostgreSQL local thành công. Không cần chỉnh sửa DB cho các phase sau.
* **`Tbl_IssueReport` (Thực thể: [IssueReport.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Domain/Entities/IssueReport.cs))**: Lưu trữ thông tin báo cáo lỗi.
* **`Tbl_IssueReportStatusHistory` (Thực thể: [IssueReportStatusHistory.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Domain/Entities/IssueReportStatusHistory.cs))**: Ghi vết lịch sử thay đổi trạng thái.
* **Đính kèm tài liệu (Phase 2)**: Sử dụng cấu trúc quan hệ đa hình có sẵn trên bảng `Tbl_Document` thông qua `ParentId` (lưu trường `No` tự tăng của `IssueReport`) và `ParentType` (lưu giá trị `10` của `ParentEntityTypeEnum.IssueReport`).

### B. Cấu trúc thư mục & DTOs
Hai file DTO đã được di chuyển sang Domain layer để tuân thủ kiến trúc Clean Architecture:
* [IssueReportDto.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Domain/DTOs/IssueReport/IssueReportDto.cs)
* [IssueReportStatusHistoryDto.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Domain/DTOs/IssueReport/IssueReportStatusHistoryDto.cs)
* Các project đã được khai báo `global using Ecom.Domain.DTOs.IssueReport;` để tự động import.

### C. Logic nghiệp vụ (CQRS & Validation)
* **Xác thực thiết bị thuộc ao qua tủ điều khiển**: Kiểm tra logic thiết bị IoT phải thuộc về ao chỉ định thông qua tủ điều khiển (`device.DeviceHub.PondId == request.PondId`).
* **Hệ thống hóa Error Messages**: Tất cả thông báo lỗi được định nghĩa dạng hằng số trong [ErrorCodes.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Domain/Constants/ErrorCodes.cs) thuộc lớp `MessageKey`. Tuyệt đối không viết cứng chuỗi String lỗi.
* **Đính kèm tệp ở Phase 2**:
  * Kiểm tra số lượng tối đa 5 files.
  * Định dạng cho phép: `.jpg, .jpeg, .png, .mp4, .mov` (không phân biệt chữ hoa/thường).
  * Dung lượng tối đa: `10MB / file`.
  * Gán liên kết đa hình sau khi lưu báo cáo (Save lần 1 sinh `No`, cập nhật thông tin cha vào `Document`, Save lần 2).

---

## 2. Kế hoạch triển khai chi tiết cho Phase 3: Permissions (Phân quyền)

Mục tiêu của Phase này là bảo vệ các API của báo cáo lỗi bằng hệ thống Policy có sẵn của Ecom.

### Bước 3.1: Khai báo Permission Constants
Mở file [Permissions.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Domain/Constants/Permissions.cs) và bổ sung các quyền cho `IssueReport`:
```csharp
public static class IssueReport
{
    public const string View = "Permissions.IssueReport.View";
    public const string Create = "Permissions.IssueReport.Create";
    public const string Manage = "Permissions.IssueReport.Manage";
    public const string UpdateStatus = "Permissions.IssueReport.UpdateStatus";
}
```
*Đừng quên đăng ký các hằng số này vào phương thức `GetAll()` của lớp `Permissions` để hệ thống tự động quét và seed vào database.*

### Bước 3.2: Áp dụng Policy cho API Controller
Cập nhật [IssueReportController.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Presentation/Ecom.API/Controllers/V1/IssueReportController.cs) bằng cách thay thế `[Authorize]` chung bằng các Policy cụ thể:
* **Tạo báo cáo (`POST /api/v1/issue-reports`)**:
  `[Authorize(Policy = Permissions.IssueReport.Create)]`
* **Xem danh sách của tôi (`GET /api/v1/issue-reports/my`)**:
  `[Authorize(Policy = Permissions.IssueReport.View)]`
* **Xem chi tiết lỗi (`GET /api/v1/issue-reports/{id}`)**:
  `[Authorize(Policy = Permissions.IssueReport.View)]`
* **Xem danh sách quản lý (`GET /api/v1/issue-reports`)**:
  `[Authorize(Policy = Permissions.IssueReport.Manage)]`
* **Cập nhật trạng thái (`PATCH /api/v1/issue-reports/{id}/status`)**:
  `[Authorize(Policy = Permissions.IssueReport.UpdateStatus)]`
* **Xem lịch sử lỗi (`GET /api/v1/issue-reports/{id}/histories`)**:
  `[Authorize(Policy = Permissions.IssueReport.View)]`

### Bước 3.3: Phân quyền dữ liệu trong Query Handlers
Cần kiểm soát để người dùng thường không xem được báo cáo lỗi của người khác:
* Trong [GetIssueReportDetailQueryHandler.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Application/Features/IssueReport/Queries/GetIssueReportDetail/GetIssueReportDetailQueryHandler.cs) và [GetIssueReportHistoriesQueryHandler.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Application/Features/IssueReport/Queries/GetIssueReportHistories/GetIssueReportHistoriesQueryHandler.cs):
  * Kiểm tra nếu người dùng hiện tại không có quyền `Permissions.IssueReport.Manage` và báo cáo lỗi không phải do họ tạo (`report.CreatedBy != currentUser.UserId`), thì trả về lỗi `Forbidden` thông qua `MessageKey.IssueReportAccessDenied`.

---

## 3. Kế hoạch triển khai chi tiết cho Phase 4: FCM & Notification (Gửi thông báo Push)

Mục tiêu là tự động gửi thông báo hệ thống và push qua Firebase (FCM) đến đúng nhóm người dùng khi có báo cáo lỗi mới.

### Bước 4.1: Định nghĩa Event
Tạo một Domain Event hoặc Application Event đại diện cho sự kiện báo cáo lỗi được tạo thành công:
`Core/Ecom.Application/Features/IssueReport/Events/IssueReportCreatedEvent.cs`
```csharp
public record IssueReportCreatedEvent(Guid IssueReportId) : INotification;
```

### Bước 4.2: Phát Event từ Handler
Trong [CreateIssueReportCommandHandler.cs](file:///d:/Mebisoft/Ecom/Project_Ecom/backend-api/Core/Ecom.Application/Features/IssueReport/Commands/CreateIssueReport/CreateIssueReportCommandHandler.cs):
* Inject `IMediator` hoặc publish thông qua EF Core Domain Event dispatcher (nếu thực thể kế thừa `BaseEntity`).
* Gọi phát sự kiện sau khi lưu DB thành công:
  ```csharp
  await _mediator.Publish(new IssueReportCreatedEvent(report.Id), cancellationToken);
  ```

### Bước 4.3: Viết Event Handler gửi Notification
Tạo Handler xử lý sự kiện:
`Core/Ecom.Application/Features/IssueReport/Events/IssueReportCreatedEventHandler.cs`
* Thực hiện gửi thông báo bằng cách sử dụng `INotificationService` hoặc `IFcmService` của hệ thống.
* **Xác định đối tượng nhận tin theo BA Define**:
  * **Lỗi phần mềm (Software Issue)**:
    * Tiêu đề: `Báo cáo lỗi phần mềm`
    * Nội dung: `[{UserName}] đã báo lỗi: {Title}`
    * Gửi tới: Toàn bộ người dùng thuộc nhóm vai trò `Admin`.
  * **Lỗi thiết bị IoT (IoT Device Issue)**:
    * Tiêu đề: `Báo cáo lỗi thiết bị`
    * Nội dung: `{DeviceName} tại {PondName} gặp lỗi: {Title}`
    * Gửi tới: Admin hệ thống và các `Manager` phụ trách vùng nuôi (`ZoneId`) xảy ra sự cố lỗi thiết bị.

