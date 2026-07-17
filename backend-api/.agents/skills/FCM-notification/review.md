# FCM Push Notification - Source Review

## Current Delta - 2026-05-18

This review contains historical findings below. Current source has moved beyond several old assumptions.

Implemented Priority 0:

- `LogoutCommandHandler` resolves the target user from `currentUser` first, then from an active refresh token for anonymous logout.
- Anonymous logout can now deactivate device FCM tokens without changing the public API.
- `NotificationService.NotifyBulkAsync` logs `NOTIFICATION_BULK_PUBLISH`.
- `FcmNotificationEventHandler` uses minimal diagnostics instead of verbose per-platform/per-recipient logs.

Current FCM handler log keys:

```text
FCM_NO_ACTIVE_TOKEN
FCM_TOKEN_RESOLVED
FCM_SEND_RESULT
FCM_BATCH_EXCEPTION
FCM_TOKEN_STATE_UPDATED
```

Current source notes:

- `FcmNotificationEventHandler` no longer uses detached `Task.Run`.
- `IFcmService.SendMulticastAsync` returns `FcmSendResult`.
- Batch failures are not treated as token success.
- `FCM_EVENT_RECEIVED`, `FCM_SEND_ATTEMPT`, `PlatformCounts`, and `RecipientTokenCounts` are no longer required log keys.

Verification passed:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
```

**Role**: Ecom Backend Codeflow Analyst  
**Date**: 2026-05-07  
**Scope**: FCM backend flow, recipient resolving, token lifecycle, mobile/web delivery readiness  
**Rule**: Không chỉnh `appsettings.json` / `appsettings.Development.json` trong review này vì đang phục vụ local runtime.

---

## 1. Kết luận nhanh

Code FCM hiện tại đã tốt hơn bản đầu: có tách scope riêng cho UnitOfWork, có retry Firebase, có Android/APNS notification config, có grace period 7 ngày cho token fail, có API xem token cá nhân và API test FCM theo user hiện tại.

Tuy nhiên, source hiện tại **chưa đủ tin cậy để kết luận FCM production đã ổn**. Điểm nghẽn chính không còn là DB notification hay token active, mà là **delivery observability**: khi Firebase batch fail sau retry, `FirebaseFcmService` chỉ log rồi trả `failedTokens` rỗng. `FcmNotificationEventHandler` thấy danh sách failed rỗng sẽ cập nhật `LastUsedAt` như gửi thành công. Vì vậy backend có thể tạo ra trạng thái "thành công giả": DB có thông báo, token vẫn active, test endpoint có thể trả không có failed token, nhưng thiết bị thật không nhận OS push.

**Release gate hiện tại**:

| Hạng mục | Đánh giá |
|---|---|
| DB notification / app notification list | GO |
| Clean Architecture cơ bản | GO |
| Push FCM reliable cho mobile iOS/Android | NO-GO nếu chưa bổ sung delivery result/diagnostic |
| Web portal FCM token | NO-GO nếu web còn lỗi Firebase `projectId` |
| Durable push delivery | NO-GO, cần queue/outbox phase sau |

**Điểm tổng**: `7.2/10`  
**Điểm riêng cho push delivery observability**: `4/10`

---

## 2. Flow hiện tại

```text
Business handler
  -> INotificationService.NotifyAsync / NotifyBulkAsync
  -> MediatR Publish(NotificationEvent | BulkNotificationEvent)
      -> NotificationEventHandler / BulkNotificationEventHandler
           -> lưu DomainNotification vào DB
      -> FcmNotificationEventHandler
           -> Task.Run fire-and-forget
           -> tạo async scope riêng
           -> resolve IUnitOfWork + IFcmService từ scope mới
           -> query UserDeviceToken IsActive && !IsDeleted
           -> NotificationPayloadBuilder.Build(type, targetUrl)
           -> FirebaseFcmService.SendMulticastAsync(...)
           -> UpdateTokenDeliveryStateAsync(...)
```

Token lifecycle:

```text
Mobile/Web login hoặc token refresh
  -> SaveDeviceToken
  -> UserDeviceToken IsActive=true, LastUsedAt cập nhật

Push success
  -> UpdateLastUsed()

Push token-level unrecoverable fail
  -> nếu LastUsedAt còn trong 7 ngày: giữ active
  -> nếu quá 7 ngày hoặc null: Deactivate()

Batch-level Firebase/network fail
  -> hiện tại chỉ log trong FirebaseFcmService
  -> caller không biết fail
  -> có nguy cơ bị tính nhầm là success
```

---

## 3. Công nghệ đã có sẵn trong dự án

| Nhóm | Thành phần sẵn có | Ý nghĩa với FCM |
|---|---|---|
| Application flow | MediatR event, CQRS handlers | Đã dùng đúng cho notification event và DB notification |
| Transaction | EF Core + `IUnitOfWork` | Đã dùng để query/update `UserDeviceToken`; scope riêng giúp tránh conflict với request UoW |
| Integration | Firebase Admin SDK | Gửi Android/iOS/Web FCM multicast |
| Resilience | Polly | Đã retry `FirebaseException` và `HttpRequestException` |
| Background runtime | Hosted service patterns, ví dụ Oxy alert background worker | Có thể tái dùng để nâng FCM sang queue/outbox worker |
| Cache/infra | Redis / distributed cache services | Có thể dùng làm queue tạm hoặc lock, nhưng durable delivery nên ưu tiên DB/outbox |
| Observability | ILogger, Serilog/OpenTelemetry trong project | Có nền tảng log, nhưng FCM hiện thiếu structured event đủ truy vết |
| Domain | `UserDeviceToken`, `DomainNotification` | Đã có nơi lưu token và thông báo |
| Payload | `NotificationPayloadBuilder`, `NotificationTargetUrls` | Đã chuẩn hóa payload/screen mapping cho app |

Điểm đáng chú ý: dự án đã có đủ nền để làm FCM tốt hơn mà không cần đưa thêm framework lớn ngay. Fix cần nhất ở phase hiện tại là **structured send result + diagnostic endpoint/log**, không phải đổi toàn bộ kiến trúc.

---

## 4. Những điểm đã hoàn thành

### Done-1 - Tách FCM khỏi request scope chính

`FcmNotificationEventHandler` đã dùng `IServiceScopeFactory.CreateAsyncScope()` và resolve `IUnitOfWork`, `IFcmService` trong scope mới.

**Evidence**:
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:49-51`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:88-90`

**Đánh giá**: Đúng hướng với Clean Architecture và tránh DbContext/UoW conflict với transaction nghiệp vụ chính.

### Done-2 - API nghiệp vụ không còn chờ Firebase đồng bộ

Handler đang fire-and-forget qua `Task.Run`.

**Evidence**:
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:30`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:38`

**Đánh giá**: Giảm latency API, nhưng chưa phải durable delivery. Nếu Azure App Service recycle ngay sau khi publish event, task có thể mất.

### Done-3 - Có retry Firebase transient failure

`FirebaseFcmService` dùng Polly retry 3 lần cho `FirebaseException` và `HttpRequestException`.

**Evidence**:
- `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs:84-94`

**Đánh giá**: Tốt cho lỗi ngắn hạn. Chưa đủ nếu Firebase/network fail lâu hơn retry window.

### Done-4 - Có notification config cho Android/APNS

Message có `Notification`, Android priority high, sound default, channel id default, APNS priority 10.

**Evidence**:
- `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs:52-77`

**Đánh giá**: Đúng hướng để OS tray hiện thông báo. Vẫn cần app Android tạo channel `default` tương ứng.

### Done-5 - Grace period cho failed token

Token fail không bị deactivate ngay nếu `LastUsedAt` còn trong 7 ngày.

**Evidence**:
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:22`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:156-188`

**Đánh giá**: Giảm nguy cơ "lúc nhận lúc không" do backend loại token quá sớm. Vì chưa thay schema nên đây là giải pháp pragmatic.

### Done-6 - Payload builder đã bền hơn

`NotificationPayloadBuilder` đã bắt nhiều loại lỗi format thay vì chỉ `UriFormatException`.

**Đánh giá**: Giảm nguy cơ một `targetUrl` lỗi làm rơi cả push pipeline.

### Done-7 - Có API kiểm tra token cá nhân và test FCM

Đã có endpoint lấy `UserDeviceToken` của user đang đăng nhập và endpoint test FCM theo user hiện tại. Đây là đúng hướng bảo mật vì user chỉ xem token của mình, token được mask.

**Đánh giá**: Hữu ích cho debug, nhưng test endpoint hiện vẫn bị giới hạn bởi lỗi "thành công giả" nếu `IFcmService` chỉ trả failed token mà không trả trạng thái batch exception.

---

## 5. Findings cần sửa

### H1 - Batch-level FCM failure bị nuốt, gây trạng thái gửi thành công giả

**Severity**: High  
**File**: `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs:120-125`  
**File**: `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:144-153`

`FirebaseFcmService.SendMulticastAsync` catch exception sau retry, log lỗi rồi tiếp tục và trả `failedTokens`. Khi lỗi là batch-level exception, không có token nào được thêm vào `failedTokens`. Vì vậy caller nhận danh sách rỗng.

`FcmNotificationEventHandler.UpdateTokenDeliveryStateAsync` hiểu `failedSet.Count == 0` là toàn bộ token gửi thành công và gọi `UpdateLastUsed()` cho tất cả token.

**Impact**:
- API test FCM có thể trả kết quả đẹp nhưng thiết bị không nhận.
- `LastUsedAt` bị cập nhật sai, làm token trông còn khỏe.
- Không phân biệt được "Firebase gửi thành công" với "Firebase batch fail nhưng bị nuốt".
- Khi lên dev mà không xem được log, gần như không xác định được lỗi nằm ở Firebase credentials, network, sender id, Android channel hay app foreground handler.

**Adjustment**:
- Đổi `IFcmService.SendMulticastAsync` từ `IReadOnlyList<string>` sang structured result:
  - `SuccessCount`
  - `FailureCount`
  - `BatchFailureCount`
  - `FailedTokens`
  - `TokenErrors`
  - `BatchExceptions`
- Nếu chưa đổi contract ngay, tối thiểu khi catch batch exception phải trả trạng thái batch fail để handler không update `LastUsedAt` như success.

### H2 - Fire-and-forget bằng `Task.Run` chưa đảm bảo delivery trên Azure dev/prod

**Severity**: High  
**File**: `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:30`  
**File**: `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs:38`

`Task.Run` giúp API không bị block nhưng không bền. App Service recycle, process crash, scale-out instance stop hoặc unhandled runtime cancellation đều có thể làm mất push sau khi DB notification đã lưu.

**Impact**:
- App notification list có thông báo vì DB handler đã chạy.
- OS push có thể không có vì background task bị mất hoặc batch fail sau đó.
- Đây khớp với hiện tượng user thấy: trong app có item mới, nhưng thanh notification của Android/iOS không hiện.

**Adjustment**:
- Phase gần: thêm structured log/result để chứng minh task có chạy và Firebase trả gì.
- Phase sau: đưa FCM vào outbox/background worker dùng DB hoặc queue. Dự án đã có hosted service pattern nên có thể triển khai theo cách quen thuộc.

### H3 - Không có observability đủ dùng khi không xem được log server

**Severity**: High  
**Files**:
- `FirebaseFcmService.cs`
- `FcmNotificationEventHandler.cs`
- `NotificationController` / API diagnostic hiện có

Hiện tại debug phụ thuộc quá nhiều vào server log. Khi không xem được log Azure dev, các endpoint hiện tại chưa trả đủ bằng chứng:
- Có resolve ra đúng recipient không?
- Có bao nhiêu token active?
- Đã gọi Firebase chưa?
- Firebase trả success/failure từng token ra sao?
- Có batch exception không?
- Có token nào bị SenderIdMismatch/Unregistered/InvalidArgument không?

**Adjustment**:
- Thêm diagnostic cho admin/dev:
  - `notificationId`
  - `recipientCount`
  - `tokenCount`
  - `platformCounts`
  - `sendAttempted`
  - `successCount`
  - `failureCount`
  - `batchFailureCount`
  - `errorCodeCounts`
  - `tokenMasked`
- Log theo event name cố định:
  - `FCM_EVENT_RECEIVED`
  - `FCM_TOKEN_RESOLVED`
  - `FCM_SEND_ATTEMPT`
  - `FCM_SEND_RESULT`
  - `FCM_TOKEN_FAILURE`
  - `FCM_BATCH_EXCEPTION`

### M1 - Role resolver vẫn đang là test-only, chưa đúng production matrix

**Severity**: Medium  
**File**: `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs:38-53`

`ResolveManagersAsync` đang dùng ADMIN tạm thời. Comment ghi rõ production cần đổi lại MANAGER. Với rule user cung cấp:

| Role | Ý nghĩa |
|---|---|
| FARMER | Nông dân, tác vụ mobile |
| EMPLOYEE_WAREHOUSE | Nhân viên thủ kho, quản lý vật tư |
| MANAGER | Chủ trại, quản lý điều hành |
| EMPLOYEE_MANAGER | Nhân viên kỹ thuật, quản lý trại nuôi |
| ADMIN | Quản trị hệ thống |
| USER | Người dùng mới |
| EMPLOYEE | Xem/tạo, không duyệt |

**Đánh giá**:
- Với account ADMIN hiện tại, role không phải nguyên nhân trực tiếp vì resolver đang gửi cho ADMIN.
- Nhưng cho production, import/export/inventory approval không nên phụ thuộc ADMIN test-only.

**Adjustment đề xuất**:
- Phiếu nhập/xuất/kiểm kê cần duyệt: `MANAGER`, có thể thêm `EMPLOYEE_WAREHOUSE` nếu nghiệp vụ cho phép duyệt/xử lý kho, `ADMIN` chỉ nên là support/system.
- Low stock/material alerts: `MANAGER`, `EMPLOYEE_WAREHOUSE`, optional `ADMIN`.
- Oxy/sự cố ao: `FARMER`, `MANAGER`, `EMPLOYEE_MANAGER`, optional `ADMIN`.
- `EMPLOYEE` chỉ xem/tạo thì không nhận notification yêu cầu duyệt nếu không có quyền duyệt.

### M2 - Web portal Firebase config lỗi `projectId`

**Severity**: Medium  
**Area**: Frontend/web portal config

Console web portal báo:

```text
FirebaseError: Installations: Missing App configuration value: "projectId"
```

Đây không phải lỗi backend FCM Android trực tiếp. Nhưng nó làm Web FCM token không đáng tin và có thể gây nhiễu khi cùng account có cả token Web/iOS/Android.

**Adjustment**:
- Fix frontend Firebase config/env trên dev portal: `apiKey`, `authDomain`, `projectId`, `messagingSenderId`, `appId`, `vapidKey`.
- Backend không nên sửa `appsettings` local để giải quyết lỗi frontend này.

### M3 - Android channel id `default` cần khớp với app

**Severity**: Medium  
**File**: `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs:60-64`

Backend gửi `ChannelId = "default"`. Android 8+ yêu cầu app tạo notification channel trước. Nếu mobile app không tạo channel `default`, push có thể không hiện đúng OS tray dù FCM nhận message.

**Adjustment**:
- Confirm app tạo channel `default` với importance high/default.
- Hoặc thống nhất channel id rõ hơn, ví dụ `Ecom_default`, rồi backend và app cùng dùng.

### M4 - App foreground/local notification handling chưa được chứng minh

**Severity**: Medium  
**Area**: Mobile app

FCM notification message thường hiện OS tray khi app background/killed. Khi app foreground, app phải tự hiển thị local notification hoặc cập nhật UI. User thấy app list có thông báo nhưng OS tray không hiện có thể do test lúc app đang foreground hoặc local notification handler chưa bật.

**Adjustment**:
- Test 3 trạng thái riêng: foreground, background, killed.
- Mobile cần log:
  - token received
  - token refresh
  - onMessage foreground
  - onBackgroundMessage
  - local notification show result

### L1 - `GoogleCredential.FromJson` có cảnh báo obsolete

**Severity**: Low  
**File**: `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs:32`

Không phải nguyên nhân push không hiện ngay, nhưng nên xử lý sau để tránh technical debt.

---

## 6. Đánh giá Clean Architecture / Clean Code / Pattern

| Tiêu chí | Điểm | Đánh giá |
|---|---:|---|
| Clean Architecture | 8/10 | FCM nằm Infrastructure, controller mỏng, event từ Application. Đúng hướng. |
| UoW/DbContext scope | 8/10 | Scope riêng đã ổn. Còn rủi ro do fire-and-forget không durable. |
| Token lifecycle | 7/10 | Grace period tốt khi chưa đổi schema. Chưa có FailCount/LastFailedAt. |
| Firebase integration | 7/10 | Có retry và platform config. Thiếu structured result. |
| Observability | 4/10 | Log có nhưng không đủ khi không xem được server log; API test có thể false-positive. |
| Role/business mapping | 6/10 | Đang test-only ADMIN, chưa production-ready. |
| Mobile delivery readiness | 6/10 | Backend có notification payload, nhưng chưa chứng minh Android channel/foreground handling. |
| Durable delivery | 4/10 | Chưa có outbox/queue/dead-letter. |

**Điểm tổng**: `7.2/10`.

---

## 7. Vấn đề khớp với triệu chứng hiện tại

Triệu chứng user mô tả:

1. Trên dev, account ADMIN có Android token active.
2. Tạo phiếu xuất trên portal.
3. App notification list có item mới.
4. OS notification tray không có thông báo.
5. Web portal có lỗi Firebase `projectId`.
6. Có máy team app từng hiện oxy alert.

Phân tích:

- DB notification flow đang chạy.
- Resolver với ADMIN hiện tại có thể resolve đúng admin.
- FCM không chết toàn hệ thống vì oxy alert từng có thiết bị nhận.
- Lỗi web `projectId` không giải thích trực tiếp Android app không nhận, nhưng chứng minh môi trường FCM dev đang có cấu hình frontend chưa đúng.
- Điểm backend đáng nghi nhất là `FirebaseFcmService` có thể batch fail nhưng handler vẫn update token như success.
- Điểm mobile đáng nghi là Android channel/foreground local notification chưa được xác nhận.
- Điểm business đáng nghi cho production là role resolver còn test-only, nhưng riêng ADMIN hiện tại thì không phải nguyên nhân chính.

---

## 8. Kế hoạch sửa ưu tiên

### P0 - Bổ sung result thật cho FCM send

Đổi contract `IFcmService` để trả structured result. Đây là việc cần sửa ngay nhất trước khi tiếp tục đoán.

Kết quả mong muốn:

```text
FcmSendResult
  TotalTokens
  SuccessCount
  FailureCount
  BatchFailureCount
  FailedTokens
  TokenErrors
  BatchErrors
```

Handler chỉ gọi `UpdateLastUsed()` cho token có success thật. Nếu batch exception, không cập nhật token như success.

### P1 - Nâng API test FCM thành diagnostic endpoint

Endpoint test cần trả:

```text
tokenCount
platformCounts
activeTokenCount
successCount
failureCount
batchFailureCount
errorCodeCounts
tokensMasked
```

Không trả full token thật.

### P2 - Thêm structured logs

Log bắt buộc:

```text
FCM_EVENT_RECEIVED
FCM_TOKEN_RESOLVED
FCM_SEND_ATTEMPT
FCM_SEND_RESULT
FCM_TOKEN_FAILURE
FCM_BATCH_EXCEPTION
FCM_TOKEN_STATE_UPDATED
```

### P3 - Sửa role matrix production

Thay comment/test-only role bằng rule nghiệp vụ đã xác nhận. Nếu vẫn cần ADMIN nhận trong dev, thêm config/flag hoặc giữ ADMIN như support role có chủ đích, không để comment "test only".

### P4 - Xác nhận mobile integration

Checklist app:

- Android đã xin quyền notification.
- Android tạo channel id trùng backend.
- Foreground handler gọi local notification.
- Background/killed handler nhận message.
- Token refresh gọi SaveDeviceToken.
- App dev/prod dùng đúng Firebase project với backend service account.

### P5 - Phase durable delivery

Khi cần đảm bảo không mất push:

- Lưu FCM send job vào DB/outbox khi DB notification được tạo.
- Background worker đọc outbox và gửi Firebase.
- Có retry policy, next retry time, attempt count, dead-letter reason.
- Có admin endpoint xem trạng thái push theo notification.

---

## 9. Next actions

1. Sửa `IFcmService` trả structured result để chặn false-success.
2. Cập nhật `FcmNotificationEventHandler` chỉ update token thành công khi Firebase xác nhận success.
3. Cập nhật test endpoint để trả batch failure/error code.
4. Kiểm tra mobile Android channel `default` và foreground handler.
5. Fix web portal Firebase `projectId` ở frontend env.
6. Chốt role matrix production cho import/export/inventory/oxy.

