# Bug Report - FCM khong gui tren Dev

## Latest Update - 2026-05-18

Priority 0 has been implemented:

- Anonymous logout can deactivate FCM tokens by resolving the user from an active refresh token.
- `NOTIFICATION_BULK_PUBLISH` is logged before publishing bulk notifications.
- `FcmNotificationEventHandler` now uses a minimal log set.

Current FCM log keys:

```text
FCM_NO_ACTIVE_TOKEN
FCM_TOKEN_RESOLVED
FCM_SEND_RESULT
FCM_BATCH_EXCEPTION
FCM_TOKEN_STATE_UPDATED
```

No longer expected from the current FCM handler:

```text
FCM_EVENT_RECEIVED
FCM_SEND_ATTEMPT
PlatformCounts
RecipientTokenCounts
```

Dev log reading update:

- `BulkNotification created` with no FCM logs: check deploy/runtime/log filter/handler registration.
- `FCM_NO_ACTIVE_TOKEN`: handler ran, but there were no active tokens for the recipient set.
- `FCM_TOKEN_RESOLVED`: handler resolved active tokens and proceeds to Firebase send.
- `FCM_SEND_RESULT`: Firebase send was attempted; if success but no OS push, investigate client/channel/foreground behavior.

Verification passed:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
```

Older sections below are historical and may reference removed verbose log keys.

Date: 2026-05-11
Status: Dang dieu tra dev runtime
Scope: Backend notification event -> FCM handler -> Firebase multicast.

---

## 1. Van de

Tren local, cung database va cung FCM credential, notification flow gui FCM thanh cong.

Tren dev, cung tac vu chi thay notification duoc tao trong database, nhung khong thay log FCM va thiet bi khong nhan push.

Trieu chung chinh:

```text
BulkNotification created
```

Nhung khong thay cac log sau tren dev:

```text
FCM_EVENT_RECEIVED
FCM_TOKEN_RESOLVED
FCM_SEND_ATTEMPT
FCM_SEND_RESULT
FCM_TOKEN_STATE_UPDATED
```

Ket luan tam thoi: loi hien tai nghieng ve dev runtime / handler khong duoc goi / deploy-slot-code khong dung hon la loi database, token, hay Firebase credential.

---

## 2. Bang chung local

Local da chay duoc end-to-end voi `POST /api/v1/importreceipt`.

Log local:

```text
RecipientResolver: Resolved 30 recipients by roles "MANAGER,EMPLOYEE_MANAGER,EMPLOYEE_WAREHOUSE,ADMIN"
BulkNotification created | Count=30 | Type="WARNING" | Title="Nhap kho"
FCM_SEND_RESULT: 14 success, 5 failure / 19
FCM_SEND_RESULT: SuccessCount=14, FailureCount=5, BatchFailureCount=0, TotalCount=19
FCM_TOKEN_STATE_UPDATED: MarkedSuccess=14; kept 5 unrecoverable failed tokens active.
```

Y nghia:

- Backend local tao notification DB thanh cong.
- FCM handler local co chay.
- Token duoc resolve tu database dung.
- Firebase multicast co thuc thi.
- Firebase tra ve success that su cho nhieu token.
- Thiet bi local da nhan push.

Vi local va dev dung chung database/connect/token set, kha nang loi do recipient/token database la thap.

---

## 3. Bang chung dev

Tren dev, cung tac vu chi thay:

```text
BulkNotification created
```

Khong thay:

```text
FCM_EVENT_RECEIVED
FCM_SEND_RESULT
```

Y nghia:

- Notification application event da tao DB row.
- Chua co bang chung `FcmNotificationEventHandler` duoc MediatR goi tren dev.
- Neu khong co `FCM_EVENT_RECEIVED`, loi nam truoc buoc resolve token va truoc buoc goi Firebase.

---

## 4. FCM credential va cau hinh

DevOps da setup:

```text
Fcm__CredentialsJson
Fcm__ProjectId
```

Doi chieu code hien tai:

- `Fcm__CredentialsJson` la key quan trong duoc dung de khoi tao Firebase credential.
- `Fcm__ProjectId` hien khong phai nguyen nhan chinh neu code dang khoi tao bang service account JSON.
- Credential dung da duoc test o local sau khi sua private key JSON; local co `FCM_SEND_RESULT` success.

Neu dev khong co bat ky log FCM handler nao, credential chua phai diem nghi ngo dau tien. Credential sai thuong se cho log loi o buoc Firebase init/send, khong phai mat hoan toan `FCM_EVENT_RECEIVED`.

---

## 5. Flow ky vong

```text
Business command
  -> NotificationService.CreateBulkNotificationAsync
  -> BulkNotificationEventHandler tao notification DB
  -> FcmNotificationEventHandler nhan BulkNotificationEvent
  -> Resolve UserDeviceToken active
  -> IFcmService.SendMulticastAsync
  -> Firebase Admin SDK
  -> FCM_SEND_RESULT
  -> Update LastUsed/token state
```

Sau khi them log moi, dev phai co chuoi log toi thieu:

```text
BulkNotification created
FCM_EVENT_RECEIVED: Mode=Bulk
FCM_TOKEN_RESOLVED: Mode=Bulk
FCM_SEND_ATTEMPT: Mode=Bulk
FCM_SEND_RESULT
```

---

## 6. Cach doc log moi

### Case A - Khong co `FCM_EVENT_RECEIVED`

Ket luan:

- FCM handler khong chay tren dev.

Huong kiem tra:

- Dev da deploy dung source moi chua.
- App Service slot dang test co dung artifact moi khong.
- Dependency registration co bi khac giua local va dev khong.
- Process dev co restart sau khi deploy khong.
- Log filter dev co dang che namespace `Ecom.Infrastructure.Event.FcmNotification` khong.

Day la case dang phu hop nhat voi hien trang.

### Case B - Co `FCM_EVENT_RECEIVED` nhung khong co `FCM_TOKEN_RESOLVED`

Ket luan:

- Handler da vao, nhung loi trong scope/DI/query truoc hoac trong query token.

Huong kiem tra:

- Tim log `FCM: Bulk push failed`.
- Kiem tra DI cua `IUnitOfWork`, `IFcmService`.
- Kiem tra exception query `UserDeviceToken`.

### Case C - Co `FCM_TOKEN_RESOLVED` voi `TokenCount=0`

Ket luan:

- Event co vao handler, nhung khong co token active cho recipient set.

Huong kiem tra:

- RecipientId trong notification co dung user dang login khong.
- Bang `UserDeviceToken` co `IsActive=true`, `IsDeleted=false` cho cac user do khong.
- Token co nam tren cung database dev dang ket noi khong.

### Case D - Co `FCM_SEND_ATTEMPT` nhung khong co `FCM_SEND_RESULT`

Ket luan:

- Handler da goi `IFcmService`, loi co the nam trong Firebase service hoac exception chua duoc quan sat dung.

Huong kiem tra:

- Tim `FCM_BATCH_EXCEPTION`.
- Tim `FCM: Bulk push failed`.
- Kiem tra Firebase credential tren Azure App Settings.

### Case E - Co `FCM_SEND_RESULT` success nhung thiet bi khong hien push

Ket luan:

- Backend da gui sang Firebase. Loi chuyen sang client/token/channel/foreground behavior.

Huong kiem tra:

- Android/iOS notification permission.
- Android notification channel.
- App foreground handler.
- Token tren thiet bi hien tai co trung token active trong DB khong.

---

## 7. File lien quan

### `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs`

Y nghia:

- Nhan `NotificationEvent` va `BulkNotificationEvent`.
- Resolve active FCM tokens tu `UserDeviceToken`.
- Goi `IFcmService.SendMulticastAsync`.
- Cap nhat `LastUsedAt` hoac deactivate token chet.
- Da them log:
  - `FCM_EVENT_RECEIVED`
  - `FCM_SKIP_NO_RECIPIENT`
  - `FCM_TOKEN_RESOLVED`
  - `FCM_NO_ACTIVE_TOKEN`
  - `FCM_SEND_ATTEMPT`

### `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs`

Y nghia:

- Khoi tao Firebase app bang `Fcm:CredentialsJson`.
- Gui multicast den Firebase.
- Log ket qua `FCM_SEND_RESULT`.
- Log batch exception neu Firebase Admin SDK loi.

### `Core/Ecom.Application/Features/Notification/Events/BulkNotificationEventHandler.cs`

Y nghia:

- Tao notification rows trong DB cho danh sach recipients.
- Neu file nay chay ma FCM handler khong chay, can kiem tra MediatR notification handler registration/runtime.

### `Core/Ecom.Application/Features/Notification/Services/NotificationService.cs`

Y nghia:

- Entry point publish notification event tu business command.
- Neu co `BulkNotification created`, tuc la flow tao notification DB da hoat dong.

### `Infrastructure/Ecom.Infrastructure/DependencyInjection.cs`

Y nghia:

- Noi dang ky MediatR notification handlers va FCM service implementation.
- Can doi chieu dev artifact co dung registration moi khong:

```csharp
services.AddTransient<INotificationHandler<NotificationEvent>, FcmNotificationEventHandler>();
services.AddTransient<INotificationHandler<BulkNotificationEvent>, FcmNotificationEventHandler>();
```

---

## 8. Danh gia nguyen nhan theo muc do kha nang

| Rank | Kha nang | Ly do |
| --- | --- | --- |
| 1 | Dev runtime khong chay dung code moi / slot sai / chua restart | Local cung DB va credential gui duoc, dev khong co log FCM handler |
| 2 | Log filter dev dang an namespace FCM | Neu handler co chay nhung log Information bi filter, can mo log namespace |
| 3 | MediatR handler registration tren dev khac local | Neu artifact/cau hinh DI tren dev khac, `BulkNotificationEventHandler` co the chay nhung `FcmNotificationEventHandler` khong chay |
| 4 | Exception trong handler truoc khi send | Can co `FCM: Bulk push failed`; neu khong co thi kha nang thap hon |
| 5 | Firebase credential sai | Local dung credential da gui duoc; credential sai thuong co exception Firebase, khong lam mat `FCM_EVENT_RECEIVED` |
| 6 | Token/recipient DB sai | Local/dev dung chung DB, local resolve va gui duoc |

---

## 9. Checklist yeu cau DevOps/Backend test dev

1. Deploy ban code co log `FCM_EVENT_RECEIVED`.
2. Restart App Service sau deploy.
3. Confirm slot/domain dang test dung artifact moi.
4. Mo log Information cho namespace:

```text
Ecom.Infrastructure.Event.FcmNotification
Ecom.Infrastructure.Services.Fcm
Ecom.Application.Features.Notification
```

5. Chay lai `POST /api/v1/importreceipt`.
6. Doi chieu log theo thu tu:

```text
BulkNotification created
FCM_EVENT_RECEIVED
FCM_TOKEN_RESOLVED
FCM_SEND_ATTEMPT
FCM_SEND_RESULT
FCM_TOKEN_STATE_UPDATED
```

7. Neu dung co `BulkNotification created` nhung khong co `FCM_EVENT_RECEIVED`, tap trung vao deploy/runtime/registration, khong tap trung vao Firebase credential.

---

## 10. Diem hien tai

| Tieu chi | Diem | Nhan xet |
| --- | ---: | --- |
| Local FCM flow | 9/10 | Da gui thanh cong va thiet bi nhan push |
| Dev FCM flow | 3/10 | Tao DB notification, chua thay FCM handler/send |
| Credential confidence | 8/10 | Credential dung khi local gui duoc |
| Token/DB confidence | 8/10 | Local/dev dung chung DB, local resolve duoc token |
| Observability sau khi them log | 8.5/10 | Du chia tach event/resolve/send/result |
| Durable retry | 4/10 | Chua co outbox/retry ben vung |

Overall: 6.5/10 cho dev readiness vi van thieu bang chung `FCM_EVENT_RECEIVED` tren dev.

---

## 11. Ket luan

Van de hien tai khong con la "FCM credential co dung khong" theo huong uu tien dau tien. Local da dung cung database va credential de gui FCM thanh cong.

Trong khi do dev chi tao notification DB ma khong thay `FCM_EVENT_RECEIVED`, nen diem can chung minh tiep theo la: `FcmNotificationEventHandler` co duoc goi trong dev runtime hay khong.

Neu sau deploy ban log moi ma dev van khong co `FCM_EVENT_RECEIVED`, ket luan manh la dev dang khong chay dung handler/code path. Khi do can kiem tra artifact, App Service slot, restart, log filter, va DI registration tren ban deploy.

