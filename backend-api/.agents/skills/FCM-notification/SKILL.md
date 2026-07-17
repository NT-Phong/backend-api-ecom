---
name: fcm-notification
description: Use this skill when debugging, reviewing, planning, or changing Ecom FCM push notification delivery, notification category split, recipient resolution, warehouse notification recipients, user device token lifecycle, logout token cleanup, Firebase send results, and notification observability.
argument-hint: "Task focus: FCM delivery | notification category | notification recipients | token lifecycle | logout cleanup | Firebase result | warehouse notification"
user-invocable: true
---

# FCM Notification Skill

## Purpose

Guide work on backend notification persistence, notification category mapping, FCM push delivery, recipient resolution, and device-token lifecycle without changing established notification contracts accidentally.

Current notification contract:
- `type` is severity only: `SUCCESS`, `INFO`, `WARNING`, `ERROR`.
- `category` is the notification group used by frontend tabs and backend filtering.
- Valid category values are stable English keys:
  - `NotificationCategories.Notification = "notification"`
  - `NotificationCategories.Alert = "alert"`
  - `NotificationCategories.Issue = "issue"`
- Default category is `notification`.

## When To Use

Use this skill for:
- FCM send flow, Firebase errors, device-token cleanup, logout token behavior.
- Notification persistence, API query behavior, unread counts, read/read-all behavior.
- Mapping business events into `notification` or `alert`.
- Mapping IssueReport-created notifications into `issue`.
- Warehouse notification recipients for import receipt, export receipt, inventory check, and low-stock events.
- Camera, sensor, and Oxy notification handlers that publish `NotificationEvent`, `BulkNotificationEvent`, or call notification services.
- Auditing whether a new handler includes the correct `category` in DB, API response, and FCM payload.

## Required References

Open only the smallest relevant files first:
- `.agents/skills/FCM-notification/Doc_API.md` - current notification API and category contract.
- `.agents/skills/FCM-notification/task_2805.md` - business mapping between normal notifications and alerts.
- `.agents/skills/FCM-notification/report-bug.md` - prior FCM delivery and logging findings.
- `.agents/skills/FCM-notification/review.md` - prior review notes.
- `.agents/skills/FCM-notification/bug-fcm.md` - earlier bug context.
- `.agents/skills/FCM-notification/doc_fcm.md` - older FCM notes; verify against source before trusting.

Source code wins when docs disagree.

## Source Areas

Start searches here:
- `Core/Ecom.Domain/Constants/NotificationCategories.cs`
- `Core/Ecom.Domain/Entities/Notification.cs`
- `Core/Ecom.Application/Features/Notification`
- `Core/Ecom.Application/Common/Helpers/NotificationPayloadBuilder.cs`
- `Infrastructure/Ecom.Infrastructure/Event/FcmNotification`
- `Infrastructure/Ecom.Infrastructure/Services/ImportReceiptService.cs`
- `Infrastructure/Ecom.Infrastructure/Services/ExportReceiptService.cs`
- Warehouse import/export/inventory-check handlers under `Core` and `Infrastructure`.
- Camera, sensor, and Oxy handlers under `Core` and `Infrastructure`.

## Search Checklist

Use targeted search before opening large files:

```powershell
rg "NotifyBulkAsync\(|new BulkNotificationEvent|new NotificationEvent|NotificationPayloadBuilder" Core Infrastructure Presentation
rg "category: NotificationCategories\.|Category = NotificationCategories\." Core Infrastructure Presentation
rg "NotificationCategories\.Alert|NotificationCategories\.Notification" Core Infrastructure Presentation
rg "read-all|unread-count|category" Core/Ecom.Application/Features/Notification Presentation/Ecom.API
```

For warehouse-specific work:

```powershell
rg "ImportReceipt|ExportReceipt|InventoryCheck|LowStock|low stock|Tồn kho" Core Infrastructure
```

## Category Rules

Do not use `type` to decide frontend tabs. `type` remains severity.

Use `category = notification` for:
- Warehouse approval requests and approval/rejection results.
- Automatic import/export notifications from inventory check, process, or preparation flows.
- Camera recovery/active notifications.
- Camera hardware fault notifications when the event is informational per `task_2805.md`.
- Sensor normal/recovery and sensor reconnected notifications.
- Oxy initial farmer notification and Oxy escalation rows marked as normal notification in `task_2805.md`.

Use `category = alert` for:
- Low stock.
- AI person detection.
- Camera disconnected/offline notifications.
- Sensor threshold violation, repeated threshold violation, prolonged threshold violation.
- Sensor disconnected/offline and repeated/prolonged disconnected states.
- Oxy reminders and Oxy off-too-long alerts.

If a notification is not listed in `task_2805.md`, do not guess silently. Classify conservatively, explain the reason, and add it to a recommendation table for user review when the task is planning/review.

## API Contract

Current endpoints keep existing routes and add optional category behavior:
- `GET /api/v1/notification?category=alert|notification|issue` filters list results when supplied.
- `GET /api/v1/notification/unread-count?category=alert|notification|issue` filters unread count when supplied.
- `PATCH /api/v1/notification/read-all` reads optional JSON body `{ "zoneId": "...", "category": "alert|notification|issue" }` and marks only that category/zone as read when supplied.
- `PATCH /api/v1/notification/{id}/read` reads a specific notification id and does not need category.

Responses include `category`. FCM data payload also includes `category`.

## Implementation Rules

- Default every notification to `NotificationCategories.Notification` unless the business mapping requires `Alert`.
- Use `NotificationCategories.Issue` for IssueReport-created notifications.
- When using `NotifyBulkAsync`, pass the category argument explicitly.
- When publishing `BulkNotificationEvent` directly, set `Category = NotificationCategories.Alert` or `NotificationCategories.Notification` in the object initializer.
- When publishing `NotificationEvent` directly, include the correct category.
- Keep `Notification.Category` non-null with default `notification`.
- Do not backfill historical rows into alerts unless a separate task explicitly requests historical reclassification.
- Do not change routes, auth policies, current-user behavior, recipient resolution, FCM token handling, Firebase config, or logout semantics during category work.
- Do not edit migrations, model snapshots, or `ApplicationDbContext` unless the user explicitly asks for migration/model changes.
- Never log raw FCM tokens, JWTs, refresh tokens, connection strings, or secrets.

## Observability Rules

Keep useful logs focused on:
- notification id or event id,
- recipient/user id count,
- category,
- severity type,
- Firebase success/failure counts,
- recoverable error reason.

Do not reintroduce noisy per-token logs. Batch-level Firebase failures do not prove every token succeeded or failed.

## Verification

For code changes, prefer narrow checks:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

Static category audit:

```powershell
rg "NotifyBulkAsync\(" Core Infrastructure Presentation
rg "new BulkNotificationEvent" Core Infrastructure Presentation
rg "new NotificationEvent" Core Infrastructure Presentation
```

Expected runtime checks:
- Low-stock notification persists and pushes `category=alert`.
- Warehouse approval request persists and pushes `category=notification`.
- Sensor threshold/disconnection persists and pushes `category=alert`.
- Sensor/camera recovery persists and pushes `category=notification`.
- `GET /notification?category=alert` returns only alerts.
- `GET /notification?category=notification` returns only normal notifications.
- `GET /notification?category=issue` returns only IssueReport notifications.
- No category filter returns both.
- `unread-count` and `read-all` respect category when supplied and preserve old behavior when omitted.

## Migration Notes

The notification category migration adds a non-null `Category` column with default `notification` and an index for category/read/time queries.

If `dotnet ef database update` fails on an older Expense/Expensive migration, verify migration history and physical table state before blaming the notification migration. A known failure mode is a database that already has `Tbl_Expense` and `Tbl_ExpenseCategory` while EF still tries to apply an old migration against `Tbl_Expensive`.

Useful DB check:

```sql
SELECT column_name, data_type, column_default, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'Tbl_Notification'
  AND column_name = 'Category';
```

Expected result: `Category`, `character varying`, default `'notification'::character varying`, `NO`.

## Final Response Expectations

When finishing notification work, report:
- Summary of behavior changed.
- Files changed.
- Verification run or why it was skipped.
- Remaining risks or follow-ups, especially unclassified notification types or migration/database state.

