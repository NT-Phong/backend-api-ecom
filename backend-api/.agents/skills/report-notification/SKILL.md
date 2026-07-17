---
name: report-notification
description: Use this skill when analyzing, implementing, or reviewing Issue Report / Report Management work, especially software issue reports, IoT device issue reports, report permissions, attachments, and report-created notifications.
argument-hint: "Task focus: phase 3 permissions | phase 4 notifications | software report | device report | attachments | access control"
user-invocable: true
---

# Report Notification Skill

## Purpose

Help agents work on the Ecom Issue Report feature without rereading the whole backend. This feature covers two business flows:

- Software report: a user reports a software screen/feature issue.
- IoT device report: a user reports an IoT device issue tied to a pond and device.

Use this skill with `AGENTS.md`, `.agents/context/task-router.md`, and the backend architecture rules. Source code remains the current truth.

## Read Order

1. `handoff_guide.md` for the original Phase 1/2 completion notes and Phase 3/4 plan.
2. `source-map.md` for the current source files and behavior already added.
3. `implementation-guide.md` for permission and notification work.
4. Only then inspect source with targeted `rg`.

Useful searches:

```powershell
rg -n "IssueReport|issue-reports|IssueReportType|IssueReportStatus" Core Presentation Infrastructure
rg -n "NotifyBulkAsync|INotificationRecipientResolver|ResolveForZoneAsync|NotificationCategories" Core Infrastructure
rg -n "Permissions\.IssueReport|IssueReportAccessDenied|ManageIssueReportAccessDenied" Core Presentation Infrastructure
```

## Current Source Boundary

Primary files:

- `Presentation/Ecom.API/Controllers/V1/IssueReportController.cs`
- `Core/Ecom.Application/Features/IssueReport/Commands/CreateIssueReport/`
- `Core/Ecom.Application/Features/IssueReport/Commands/UpdateIssueReportStatus/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetMyIssueReports/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetManageIssueReports/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetIssueReportDetail/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetIssueReportHistories/`
- `Core/Ecom.Domain/Entities/IssueReport.cs`
- `Core/Ecom.Domain/Entities/IssueReportStatusHistory.cs`
- `Core/Ecom.Domain/DTOs/IssueReport/`
- `Core/Ecom.Domain/Enums/IssueReportType.cs`
- `Core/Ecom.Domain/Enums/IssueReportStatus.cs`
- `Core/Ecom.Domain/Enums/IssueReportSeverity.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/IssueReportConfiguration.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/IssueReportStatusHistoryConfiguration.cs`

Do not edit migrations, model snapshot, permissions, role seeding, public routes, or notification contracts unless the user explicitly asks for that phase.

## Business Rules To Preserve

- `IssueReportType.Software = 1`: requires `ScreenOrFeature`; does not require pond/device.
- `IssueReportType.IoTDevice = 2`: requires `PondId` and `DeviceId`.
- Device reports validate ownership through `device.DeviceHub.PondId == request.PondId`.
- Device reports store snapshots for pond, device, hub, installation status, connection status, and `ReportedAt`.
- New reports start at `IssueReportStatus.New`.
- Status transitions are intentionally restricted in `UpdateIssueReportStatusCommandHandler`.
- Attachments are linked through `Document.ParentId = report.No` and `Document.ParentType = ParentEntityTypeEnum.IssueReport`.
- Attachment limits: max 5 files, extensions `.jpg`, `.jpeg`, `.png`, `.mp4`, `.mov`, max 10 MB per file.
- Detail and history handlers allow managers/admin-like roles or the original reporter.

## Phase Awareness

Phase 1/2 are source-present:

- Domain schema and EF configuration.
- DTOs and enums.
- Create, update status, list, detail, history flows.
- Attachment linking through existing `Document`.

Phase 3/4 source has evolved; re-check current source because permissions and notifications are contract-sensitive:

- `IssueReportController` may already have per-action `Permissions.IssueReport.*` policy attributes.
- `IssueReportCreatedEvent` and `IssueReportCreatedEventHandler` may already be present.
- Report-created notifications use the existing notification pipeline and should be categorized with `NotificationCategories.Issue` when the separate IssueReport notification group is required.

## Notification Direction

For report-created notifications:

- Software report should notify Admin users.
- IoT device report should notify Admin plus managers responsible for the report zone.
- Derive zone for IoT reports from the pond/device context in source, not from client-provided unverified fields.
- Prefer existing notification contracts: `INotificationService.NotifyBulkAsync`, `INotificationRecipientResolver`, existing `NotificationTypes`, existing `NotificationCategories`, and local target URL patterns.
- Use `NotificationCategories.Issue` for report-created notifications when separating IssueReport from normal notifications.
- Do not invent new Firebase send paths if the internal notification service already publishes the bulk notification event used by FCM handlers.

## Output Expectations

For analysis or review:

- Identify exact source files inspected.
- Separate current implemented behavior from handoff plan.
- Call out permission, recipient-scope, target-url, and attachment risks.

For implementation:

- Keep changes in the smallest feature boundary.
- Follow nearby IssueReport and notification patterns.
- Run the narrowest relevant build or explain why not run.
- If this skill's durable knowledge changes, update `source-map.md` or `implementation-guide.md`.

