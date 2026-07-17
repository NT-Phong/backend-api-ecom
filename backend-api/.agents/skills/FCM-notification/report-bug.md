# FCM Notification Bug Report

## Update - 2026-05-27 - Approval and rejection result notifications for warehouse forms

Task Type: Feature Implementation
Boundary/Module: import receipt, export receipt, and inventory-check workflow notifications
Status: Implemented

### Delta

- Import, export, and inventory-check approve/reject handlers now publish post-commit result notifications through `NotifyBulkAsync`, which also drives FCM delivery for active device tokens.
- Result messages use the approver name and open the corresponding receipt/check detail view.
- `SubmitInventoryCheckCommandHandler` now publishes the missing approval-request notification so explicit submit matches the existing auto-submit flow.
- Inventory-check approval still publishes its adjustment receipt and low-stock notifications in addition to the new approval-result notification.

### Recipient Policy

- Result and submit notifications continue to use warehouse-zone recipients with active roles: `ADMIN`, `MANAGER`, `EMPLOYEE_MANAGER`, and `EMPLOYEE_WAREHOUSE`.
- The actor is not excluded when they belong to the eligible role and zone.

## Update - 2026-05-27 - Low-stock notification for inventory-check shortage

Task Type: Bug Debugging
Boundary/Module: inventory-check approval notification ownership
Status: Implemented

### Problem

- Low-stock notifications were restored for automatic operational export and manual export approval, but not for the export receipt automatically created when an approved inventory check records a shortage.
- After the central warehouse event handler stopped owning notification delivery, inventory-check shortage could update stock below `AlertQty` and send the adjustment export notification without sending `Tồn kho`.

### Fix

- `ApproveInventoryCheckCommandHandler` now checks the warehouse items represented by its auto-created export receipt after the inventory-check transaction succeeds.
- For each exported material where `AlertQty > 0` and the post-adjustment stock is below the alert level, it publishes one `Tồn kho` notification through `ResolveForLowStockAlertAsync`.
- The notification remains post-commit and uses `NotificationTargetUrls.FarmMaterialsStock(zoneId)`.

### Evidence

- `Core/Ecom.Application/Features/InventoryCheck/Commands/ApproveInventoryCheck/ApproveInventoryCheckCommandHandler.cs`: low-stock dispatch is executed inside the existing post-commit `export != null` notification branch.

## Update - 2026-05-27 - Warehouse automatic receipt notification ownership

Task Type: Bug Debugging
Boundary/Module: warehouse receipt services, inventory-check approval, manual receipt submit
Status: Verified

### Delta

- Automatic operational import/export notifications are owned by `ImportReceiptService` and `ExportReceiptService` and are dispatched only after their explicit transaction succeeds.
- Inventory-check adjustment notifications remain owned by `ApproveInventoryCheckCommandHandler` and now use the dedicated receipt-level template with the inventory-check creator name.
- Generic central warehouse event handlers no longer publish receipt or low-stock notifications from `ReferenceType` alone.
- Manual import/export transitions to `Pending` now notify on both explicit submit and update-with-auto-submit paths.
- Low-stock notifications run after successful automatic export or manual export approval.

### Evidence

- `Infrastructure/Ecom.Infrastructure/Services/ImportReceiptService.cs`: sends item-level automatic import notifications after `ExecuteInTransactionAsync`.
- `Infrastructure/Ecom.Infrastructure/Services/ExportReceiptService.cs`: sends item-level automatic export and low-stock notifications after `ExecuteInTransactionAsync`.
- `Core/Ecom.Application/Features/InventoryCheck/Commands/ApproveInventoryCheck/ApproveInventoryCheckCommandHandler.cs`: sends inventory-check origin messages after commit.

### Verification

- `dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore`: passed with pre-existing warnings.
- `dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore`: passed with pre-existing warnings.

### Remaining Risks

- FCM delivery remains post-commit without an outbox/retry guarantee.
- Automatic receipt creation for harvest or stock transfer is not implemented by this change.

## Latest Update - 2026-05-18

Priority 0 changes were implemented for logout FCM cleanup and minimal FCM observability.

Changed files:

- `Core/Ecom.Application/Features/Auth/Commands/Logout/LogoutCommandHandler.cs`
- `Core/Ecom.Application/Features/Notification/Services/NotificationService.cs`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs`

Current logout behavior:

- Authenticated logout still uses `currentUser.UserId`.
- Anonymous logout can resolve the target user from an active `JwtRefreshToken`.
- `LogoutAllDevices=true` deactivates all active FCM tokens for that user.
- `FcmToken` deactivates only that active token for that user.
- Invalid, expired, or already revoked refresh tokens do not deactivate FCM tokens.
- Raw refresh tokens and raw FCM tokens are not logged.

Current minimal FCM log keys:

```text
NOTIFICATION_BULK_PUBLISH
FCM_NO_ACTIVE_TOKEN
FCM_TOKEN_RESOLVED
FCM_SEND_RESULT
FCM_BATCH_EXCEPTION
FCM_TOKEN_STATE_UPDATED
```

No longer expected in the current FCM handler:

```text
FCM_EVENT_RECEIVED
FCM_SEND_ATTEMPT
PlatformCounts
RecipientTokenCounts
Context=... recipients
```

Verification:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
```

Both builds passed. Remaining warnings were pre-existing and unrelated.

Note: older sections below are historical and may reference previous verbose diagnostics.

Agent Report Metadata:
- Task domain: FCM notification delivery and notification recipient resolution.
- Canonical route: `.agents/context/task-router.md` -> FCM, notification delivery, warehouse notification recipients.
- Update rule: append delta-first updates with `.agents/skills/backend-api-architecture/templates/skill-report-update.md` when FCM behavior, risks, or verification results change.
- Source of truth: source code and runtime logs outrank older report text.

Date: 2026-05-08
Status: Updated after local runtime verification
Scope: Backend notification + FCM delivery flow for inventory check, inbound receipt, outbound receipt, and related warehouse notifications.

This report is the current source of truth for the FCM notification bug work. It replaces the older assumptions with the observed runtime logs and the fixes already applied in code.

---

## [0] Latest Update - 2026-05-08

### Current confirmed state

Local runtime verification for `POST http://localhost:5000/api/v1/importreceipt` confirmed the warehouse notification path is working end-to-end:

```text
BulkNotification created | Count=29 | Type="WARNING" | Title="Nhap kho"
FCM_EVENT_RECEIVED: Mode=Bulk, RecipientCount=29, ...
FCM_TOKEN_RESOLVED: Mode=Bulk, RecipientCount=29, TokenCount=17, PlatformCounts="Android:12,iOS:3,Web:2", RecipientTokenCounts="...48dc3396-041d-4297-82d2-0e1be20e9e56:10..."
FCM_SEND_RESULT: 13 success, 4 failure / 17
FCM_SEND_RESULT: SuccessCount=13, FailureCount=4, BatchFailureCount=0, TotalCount=17
FCM_TOKEN_STATE_UPDATED: MarkedSuccess=13; kept 4 unrecoverable failed tokens active.
```

Conclusion:

- Backend local creates DB notifications and executes FCM for import receipt.
- Current admin account `48dc3396-041d-4297-82d2-0e1be20e9e56` is included in recipients.
- Current admin account has 10 active FCM tokens in local data.
- Firebase returned real success for 13 tokens and failure for 4 tokens.
- If local device still does not show a push, the remaining local issue is client/token/channel/foreground behavior, not API notification publishing.

### Dev-specific symptom

On dev endpoint:

```text
POST https://mebi-mebione-d-backend-api-as-1.azurewebsites.net/api/v1/importreceipt
```

The portal shows the notification row, for example:

```text
Nhap kho
Admin Ecom tao phieu nhap #PNK-080526-08. Vui long phe duyet.
```

But no FCM execution is observed yet.

Current interpretation:

- Because local runs the full FCM path successfully, this is likely a dev deployment/runtime/logging mismatch, not import receipt business logic.
- If dev only logs `BulkNotification created` and never logs `FCM_EVENT_RECEIVED`, then the dev app is not running the same FCM handler version or the handler is not registered/executed in that runtime.
- If dev logs `FCM_EVENT_RECEIVED` but stops before `FCM_TOKEN_RESOLVED`, inspect DI/query exception in `FcmNotificationEventHandler`.
- If dev logs `FCM_NO_ACTIVE_TOKEN`, dev database lacks active token rows for the recipients.
- If dev logs `FCM_SEND_RESULT`, FCM did execute and remaining issue moves to token/client behavior.

Required dev log keys for every warehouse notification test:

```text
NOTIFICATION_BULK_PUBLISH
BulkNotification created
FCM_EVENT_RECEIVED
FCM_TOKEN_RESOLVED
FCM_NO_ACTIVE_TOKEN
FCM_SEND_RESULT
FCM: Bulk push failed
```

### Role matrix update for warehouse approvals

Confirmed roles that must receive/view approval-related warehouse notifications:

| Role code | Name | Scope |
| --- | --- | --- |
| `EMPLOYEE_WAREHOUSE` | Nhan vien Thu kho | Quan ly vat tu |
| `ADMIN` | Admin | Quan tri he thong |
| `EMPLOYEE_MANAGER` | Nhan vien Ky thuat | Quan ly trai nuoi |
| `MANAGER` | Chu trai | Quan ly dieu hanh |

Code update:

- `NotificationRecipientResolver.ResolveManagersAsync(...)` now includes `EMPLOYEE_MANAGER`.
- `NotificationRecipientResolver.ResolveManagersAndEmployeesAsync(...)` now includes `EMPLOYEE_MANAGER`.
- This affects import receipt, export receipt, inventory check, and low-stock alert flows because they all use the shared resolver.

Files:

- `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs`

### Oxy notification update

Oxy alert flow was also corrected during this investigation:

- New Oxy state no longer sets `LastFcmpushed = now` before a real notification publish.
- `FarmerNotificationSentAt` and `LastFcmpushed` are only updated after `BulkNotificationEvent` is actually published.
- Initial Oxy dedup key now includes scope `InitialFarmer` to avoid being blocked by unrelated critical/escalation dedup keys.
- Oxy escalation no longer falls back from `FarmerNotificationSentAt` to `LastFcmpushed`; if farmer initial notification was never truly published, escalation is skipped.
- Oxy escalation recipients include `ADMIN`, `MANAGER`, and `EMPLOYEE_MANAGER`.

Files:

- `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangeCommandHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertReminderService.cs`

### Newly added diagnostics

Added/confirmed logs:

```text
NOTIFICATION_BULK_PUBLISH
FCM_EVENT_RECEIVED
FCM_TOKEN_RESOLVED
FCM_NO_ACTIVE_TOKEN
FCM_SEND_RESULT
OXY_INITIAL_NOTIFICATION_STATE_UPDATED
```

`FCM_TOKEN_RESOLVED` for bulk now includes `RecipientTokenCounts`, for example:

```text
RecipientTokenCounts="48dc3396-041d-4297-82d2-0e1be20e9e56:10"
```

This proves whether a specific user is both in the notification recipient set and has active FCM tokens, without logging raw FCM tokens.

---

## [1] Problem

The original symptom was:

- `POST /api/v1/notification/test-fcm` could send FCM successfully to the current user's Android tokens.
- Creating an inventory check created notification rows, but the current admin creator did not receive a push notification on dev/local for the equivalent business action.
- The notification list could show data, and the same FCM token worked through the test endpoint.

The confirmed root cause was not Firebase token validity. The main business-flow issue was recipient resolution:

- Actor user `692a3959-2855-42e0-a072-55f29683218d` was an admin and had active FCM tokens.
- Inventory check recipient resolver was called with `ExcludeActor=True`.
- Because the actor was excluded, their FCM tokens were never selected for the business notification.
- After changing warehouse notification flows to include the actor when the actor belongs to the target role group, the actor appeared in recipients and FCM push was delivered.

---

## [2] Runtime Evidence

### Test FCM endpoint

Endpoint:

```text
POST /api/v1/notification/test-fcm
```

Observed result:

```text
activeTokenCount=2
successCount=2
failureCount=0
failedTokenCount=0
batchFailureCount=0
platforms=Android
platformCounts=Android:2
```

Observed logs:

```text
FCM_SEND_RESULT: 2 success, 0 failure / 2
FCM_SEND_RESULT: Test FCM sent for UserId=692a3959-2855-42e0-a072-55f29683218d. ActiveTokenCount=2, SuccessCount=2, FailureCount=0, BatchFailureCount=0
```

Conclusion:

- Firebase credentials and the two Android tokens were valid.
- The FCM sending layer was able to deliver to Firebase.

### Inventory check before recipient fix

Observed logs:

```text
RecipientResolver: Resolved 4 recipients by roles "MANAGER,EMPLOYEE_WAREHOUSE,ADMIN" | WarehouseId=3d2fa19e-4141-4ba1-babe-aadeb9da4d12 | ExcludeActor=True
NOTIFICATION_INVENTORY_CHECK_RECIPIENTS: InventoryCheckId=000ddd81-b468-4b08-b8e7-10311b09e94d, WarehouseId=3d2fa19e-4141-4ba1-babe-aadeb9da4d12, ActorUserId=692a3959-2855-42e0-a072-55f29683218d, RecipientCount=4, RecipientIds="5f1761df-39b8-4715-8b65-80e4dbcca759,65c66a6c-115a-4932-9370-400d33141ead,84713d29-8c22-4d93-867d-6e1653383eb3,8ded1bf0-a971-4db0-beee-62de4499b332"
NOTIFICATION_BULK_CREATED: Count=4, RecipientIds="5f1761df-39b8-4715-8b65-80e4dbcca759,65c66a6c-115a-4932-9370-400d33141ead,84713d29-8c22-4d93-867d-6e1653383eb3,8ded1bf0-a971-4db0-beee-62de4499b332", Type="WARNING", Title="Kiem ke kho", TargetUrl="/material-management/form?type=counting&mode=approve&id=000ddd81-b468-4b08-b8e7-10311b09e94d"
```

Conclusion:

- Notification rows were created for 4 recipients.
- Actor `692a3959-2855-42e0-a072-55f29683218d` was not in `RecipientIds`.
- The actor's FCM tokens were not part of this business notification flow.

### Inventory check after recipient fix

Observed logs:

```text
RecipientResolver: Resolved 5 recipients by roles "MANAGER,EMPLOYEE_WAREHOUSE,ADMIN" | WarehouseId=3d2fa19e-4141-4ba1-babe-aadeb9da4d12 | ExcludeActor=False
NOTIFICATION_INVENTORY_CHECK_RECIPIENTS: InventoryCheckId=eb54d8f1-00dc-4c5e-8ec4-302bd356c80d, WarehouseId=3d2fa19e-4141-4ba1-babe-aadeb9da4d12, ActorUserId=692a3959-2855-42e0-a072-55f29683218d, RecipientCount=5, RecipientIds="5f1761df-39b8-4715-8b65-80e4dbcca759,65c66a6c-115a-4932-9370-400d33141ead,692a3959-2855-42e0-a072-55f29683218d,84713d29-8c22-4d93-867d-6e1653383eb3,8ded1bf0-a971-4db0-beee-62de4499b332"
NOTIFICATION_BULK_PUBLISH: RecipientCount=5, RecipientIds="5f1761df-39b8-4715-8b65-80e4dbcca759,65c66a6c-115a-4932-9370-400d33141ead,692a3959-2855-42e0-a072-55f29683218d,84713d29-8c22-4d93-867d-6e1653383eb3,8ded1bf0-a971-4db0-beee-62de4499b332", Type="WARNING", Title="Kiem ke kho", TargetUrl="/material-management/form?type=counting&mode=approve&id=eb54d8f1-00dc-4c5e-8ec4-302bd356c80d"
NOTIFICATION_BULK_CREATED: Count=5, RecipientIds="5f1761df-39b8-4715-8b65-80e4dbcca759,65c66a6c-115a-4932-9370-400d33141ead,692a3959-2855-42e0-a072-55f29683218d,84713d29-8c22-4d93-867d-6e1653383eb3,8ded1bf0-a971-4db0-beee-62de4499b332", Type="WARNING", Title="Kiem ke kho", TargetUrl="/material-management/form?type=counting&mode=approve&id=eb54d8f1-00dc-4c5e-8ec4-302bd356c80d"
FCM_TOKEN_RESOLVED: RecipientCount=5, RecipientIds="5f1761df-39b8-4715-8b65-80e4dbcca759,65c66a6c-115a-4932-9370-400d33141ead,692a3959-2855-42e0-a072-55f29683218d,84713d29-8c22-4d93-867d-6e1653383eb3,8ded1bf0-a971-4db0-beee-62de4499b332", TokenCount=2, PlatformCounts="Android:2", Type="WARNING", Title="Kiem ke kho"
FCM_SEND_ATTEMPT: RecipientCount=5, TokenCount=2, Title="Kiem ke kho"
FCM_SEND_RESULT: 2 success, 0 failure / 2
FCM_TOKEN_STATE_UPDATED: Marked 2 tokens as successful. Context="5 recipients"
```

Conclusion:

- Actor is now included in the recipient set.
- The FCM handler resolved 2 Android tokens.
- Firebase send succeeded for both tokens.
- User confirmed that both FCM tokens received the notification.

---

## [3] Fixed Issues

### BUG-H1 - Fixed: FCM result did not distinguish success, token failure, and batch failure

Old behavior:

- `IFcmService.SendMulticastAsync` returned only failed token strings.
- The caller could not know whether tokens were successfully delivered.
- Batch exceptions could be interpreted incorrectly by higher-level code.

Current behavior:

- `IFcmService.SendMulticastAsync` returns `FcmSendResult`.
- Result includes:
  - `TotalTokens`
  - `SuccessCount`
  - `FailureCount`
  - `BatchFailureCount`
  - `SuccessfulTokens`
  - `TokenFailures`
  - `BatchErrors`
- Token failures include `ErrorCode` and `IsUnrecoverable`.

Files:

- `Core/Ecom.Application/Common/Interfaces/IFcmService.cs`
- `Infrastructure/Ecom.Infrastructure/Services/Fcm/FirebaseFcmService.cs`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs`

### BUG-M0 - Fixed: Actor was excluded from warehouse notification recipients

Old behavior:

- Inventory check, inbound receipt, and outbound receipt recipient resolution excluded the actor.
- If the actor was also an admin/manager/warehouse employee and should receive the notification, their token was skipped.

Current behavior:

- Warehouse notification flows include the actor when the actor belongs to the resolved role group.
- This was applied consistently for:
  - Inventory check
  - Import receipt
  - Export receipt
  - Low stock alert already used actor inclusion behavior

Files:

- `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs`

### BUG-H3 - Partially fixed: Missing diagnostic evidence across notification and FCM flow

Added diagnostics:

- `NOTIFICATION_EVENT_PUBLISH`
- `NOTIFICATION_BULK_SKIP`
- `NOTIFICATION_BULK_PUBLISH`
- `NOTIFICATION_BULK_CREATED`
- `NOTIFICATION_INVENTORY_CHECK_RECIPIENTS`
- `FCM_TOKEN_RESOLVED`
- `FCM_SEND_ATTEMPT`
- `FCM_SEND_RESULT`
- `FCM_BATCH_EXCEPTION`
- `FCM_TOKEN_STATE_UPDATED`

Development logging was also adjusted so unrelated logs are quieter and notification/FCM logs remain visible.

Files:

- `Core/Ecom.Application/Features/Notification/Services/NotificationService.cs`
- `Core/Ecom.Application/Features/Notification/Events/BulkNotificationEventHandler.cs`
- `Core/Ecom.Application/Features/InventoryCheck/Commands/CreateInventoryCheck/CreateInventoryCheckCommandHandler.cs`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs`
- `Presentation/Ecom.API/appsettings.Development.json`

### BUG-T1 - Fixed: Test FCM endpoint returned weak diagnostics

Old behavior:

- Test endpoint returned active token count and failed token count only.
- It was easy to confuse "no failed tokens" with "confirmed delivered".

Current behavior:

- Test endpoint returns:
  - `activeTokenCount`
  - `successCount`
  - `failureCount`
  - `failedTokenCount`
  - `batchFailureCount`
  - `platforms`
  - `platformCounts`
  - `errorCodeCounts`
  - masked tokens
  - per-token platform/status/errorCode

Files:

- `Core/Ecom.Application/Features/Notification/Commands/TestCurrentUserFcm/TestCurrentUserFcmCommand.cs`
- `Core/Ecom.Application/Features/Notification/Commands/TestCurrentUserFcm/TestCurrentUserFcmCommandHandler.cs`

### BUG-H2A - Mitigated: Fire-and-forget FCM task could disappear after DB notification was created

New evidence from Oxy alerts:

- App notification list showed a new item at 10:45.
- No matching FCM logs were observed at 10:45.
- This matches the known unstable pattern where `BulkNotificationEventHandler` creates DB rows, while `FcmNotificationEventHandler` starts FCM through `Task.Run` and returns immediately.

Current mitigation:

- `FcmNotificationEventHandler` no longer starts FCM through detached `Task.Run`.
- FCM send is now returned as the handler task, so MediatR awaits the FCM path for the same notification event.
- Added `FCM_EVENT_RECEIVED` log before token resolution so a missing FCM path can be separated from token lookup/send failure.

Files:

- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/FcmNotificationEventHandler.cs`

### BUG-OXY1 - Fixed observability gap: Oxy notifications did not expose publish/dedup/reminder decisions

Oxy alerts have their own Redis state, dedup, and reminder scheduler. Without Oxy-specific logs, it was hard to know whether a notification came from the initial alert or a worker reminder.

Current behavior:

- Initial Oxy alert logs `OXY_NOTIFICATION_PUBLISH` when it publishes a notification.
- Initial Oxy alert logs `OXY_NOTIFICATION_DEDUP_SKIP` when the dedup key blocks publish.
- Normal and critical reminders log `OXY_NOTIFICATION_PUBLISH`.
- Reminder state updates log `OXY_NOTIFICATION_STATE_UPDATED`.
- Development logging now opens `Ecom.Application.Features.PondAlert`, `Ecom.Infrastructure.IoT.OxyAlert`, and `Ecom.Infrastructure.IoT.OxyAlertBackgroundWorker` at `Information`.

Files:

- `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangeCommandHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertReminderService.cs`
- `Presentation/Ecom.API/appsettings.Development.json`

### BUG-OXY2 - Fixed: Oxy alert recipient roles did not match the real role matrix

Confirmed role matrix:

- `FARMER`
- `EMPLOYEE_WAREHOUSE`
- `MANAGER`
- `EMPLOYEE_MANAGER`
- `ADMIN`
- `USER`
- `EMPLOYEE`

There is no active `SUPER_ADMIN` role in the current system.

Current Oxy notification rule:

- When an Oxy incident starts, send notification only to `FARMER`.
- If the incident remains active and is not acknowledged for 10 minutes after the farmer notification, escalate once to `ADMIN` and `MANAGER`.
- Farmer reminders still go to `FARMER`.
- Escalation state is tracked separately from `LastFcmpushed` so cooldown reminders do not corrupt the 10-minute escalation rule.

Files:

- `Core/Ecom.Domain/Models/PondAlert/OxyAlertSessionState.cs`
- `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangeCommandHandler.cs`
- `Core/Ecom.Application/Common/Interfaces/IOxyAlertReminderService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertConstants.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertSchedulerService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertReminderService.cs`

---

## [4] Open Risks

### BUG-H2 - Open: Fire-and-forget FCM delivery is still not durable

The immediate detached `Task.Run` risk has been mitigated, but durable delivery is still not solved.

Risk:

- If the process stops after DB notification creation but before the notification event fully completes, push may still be lost.
- There is still no durable retry queue/outbox for FCM delivery.

Recommended solution:

- Add an outbox or notification delivery table.
- Persist delivery intent in the same transaction as notification creation.
- Process pending deliveries with a background worker.
- Track delivery status, retry count, last error, and last attempt time.

### BUG-M2 - Partially fixed: Recipient resolver still relies mainly on role + warehouse, not full business scope

Current fixed behavior:

- Actor inclusion is corrected.
- Warehouse approval notification role group now includes:
  - `MANAGER`
  - `EMPLOYEE_MANAGER`
  - `EMPLOYEE_WAREHOUSE`
  - `ADMIN`

Remaining risk:

- More granular farm/site/tenant/business-unit scope rules may still be needed.
- This should be validated against production authorization rules before widening notifications further.

Recommended solution:

- Define expected recipients per notification type.
- Add tests for warehouse/farm scope and actor inclusion.
- Keep admin behavior explicit instead of accidental.

### BUG-M3 - Open: Web FCM projectId fallback still needs frontend/environment confirmation

Backend can send to valid tokens, but web push token generation still depends on the frontend Firebase configuration.

Recommended check:

- Confirm frontend uses the same Firebase project as backend credentials.
- Confirm service worker and VAPID key are configured for web push.
- Confirm dev/staging/prod Firebase project boundaries.

### BUG-M4 - Open: Mobile foreground/channel behavior still needs client confirmation

Backend has proven Firebase delivery for Android tokens.

Client-side items still need validation:

- Android notification channel exists and is enabled.
- Foreground message handler displays local notification if required.
- App permissions are granted.
- Battery/background restrictions do not suppress display.

---

## [5] Current Notification Flow

```text
Business command
  -> Resolve recipients by role/scope
  -> Publish notification event
  -> Create notification rows
  -> Publish FCM event
  -> Resolve active FCM tokens
  -> Send multicast to Firebase
  -> Update token delivery state only from confirmed send result
```

For inventory check creation:

```text
CreateInventoryCheckCommandHandler
  -> NotificationRecipientResolver.ResolveForInventoryCheckAsync(...)
  -> NotificationService.CreateBulkNotificationAsync(...)
  -> BulkNotificationEventHandler
  -> FcmNotificationEventHandler
  -> FirebaseFcmService.SendMulticastAsync(...)
```

---

## [6] Done Table

| ID | Status | Summary |
| --- | --- | --- |
| DONE-1 | Done | Added structured FCM send result instead of returning only failed token strings. |
| DONE-2 | Done | Added successful token tracking. |
| DONE-3 | Done | Added token failure error code and unrecoverable classification. |
| DONE-4 | Done | Prevented batch failure from being treated as token success. |
| DONE-5 | Done | Updated token `LastUsedAt` only for confirmed successful tokens. |
| DONE-6 | Done | Added richer test FCM response with platform/error/status counts. |
| DONE-7 | Done | Added notification/FCM logs for recipient, publish, create, token resolve, send attempt, send result, and token state update. |
| DONE-8 | Done | Focused development logging on notification and FCM while hiding unrelated noisy logs. |
| DONE-9 | Done | Included actor in inventory check, import receipt, and export receipt notification recipients when actor is part of the target role group. |
| DONE-10 | Done | Verified runtime flow: recipient includes actor, two Android tokens resolved, two Firebase sends succeeded, and device received push. |
| DONE-11 | Done | Added `NOTIFICATION_BULK_PUBLISH` log before publishing bulk notification events. |
| DONE-12 | Done | Added bulk `RecipientTokenCounts` to prove per-user active token counts without exposing token values. |
| DONE-13 | Done | Added `EMPLOYEE_MANAGER` to warehouse approval notification recipient role matrix. |
| DONE-14 | Done | Fixed Oxy initial notification timestamps so `FarmerNotificationSentAt` and `LastFcmpushed` are updated only after a real publish. |
| DONE-15 | Done | Scoped Oxy initial dedup key with `InitialFarmer` and removed escalation fallback to fake `LastFcmpushed`. |

---

## [7] Score

Current score after fixes:

- FCM delivery evidence: 8.5/10
- Token state correctness: 8/10
- Runtime observability: 8/10
- Business recipient correctness: 8.5/10
- Durable delivery guarantee: 4/10
- Overall: 8.2/10

The main remaining architecture gap is durable delivery/retry. The main remaining business gap is formal recipient-scope testing across farm/site/warehouse boundaries.

---

## [8] Next Actions

Priority 1:

- Add tests for actor inclusion across:
  - Inventory check
  - Import receipt
  - Export receipt
  - Low stock alert

Priority 2:

- Add tests for `FcmNotificationEventHandler` token-state updates:
  - all success
  - partial token failure
  - unrecoverable token failure
  - batch exception
  - no tokens

Priority 3:

- Design notification outbox/background worker for durable FCM retry.

Priority 4:

- Confirm frontend/mobile environment behavior:
  - Firebase project ID
  - VAPID key for web
  - Android notification channel
  - foreground display behavior

---

## [9] Final Conclusion

The confirmed dev issue was recipient selection, not FCM token validity.

The admin creator did not receive the inventory-check push because the actor was excluded from recipient resolution. After changing warehouse notification flows to include the actor, the logs showed the actor in the recipient list, two Android tokens resolved, Firebase send success for both tokens, and the device received the notification.

The FCM path is now observable enough to compare notification list creation, recipient resolution, token resolution, Firebase send result, and token state updates in one trace. The next serious backend improvement should be a durable outbox/retry mechanism.

