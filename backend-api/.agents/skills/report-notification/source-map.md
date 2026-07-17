# Issue Report Phase 1/2 Source Map

This file summarizes the source that exists for Phase 1 and Phase 2 of Report Management / Issue Report. Use it to orient quickly before editing.

## Feature Shape

API boundary:

- `Presentation/Ecom.API/Controllers/V1/IssueReportController.cs`
- Base route is derived from the controller naming convention used by `BaseController`; actions expose:
  - `POST` create report
  - `GET my` current user's reports
  - `GET {id:guid}` detail
  - `GET` manage list
  - `PATCH {id:guid}/status` update status
  - `GET {id:guid}/histories` status history

Application boundary:

- `Core/Ecom.Application/Features/IssueReport/Commands/CreateIssueReport/`
- `Core/Ecom.Application/Features/IssueReport/Commands/UpdateIssueReportStatus/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetMyIssueReports/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetManageIssueReports/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetIssueReportDetail/`
- `Core/Ecom.Application/Features/IssueReport/Queries/GetIssueReportHistories/`

Domain boundary:

- `Core/Ecom.Domain/Entities/IssueReport.cs`
- `Core/Ecom.Domain/Entities/IssueReportStatusHistory.cs`
- `Core/Ecom.Domain/DTOs/IssueReport/IssueReportDto.cs`
- `Core/Ecom.Domain/DTOs/IssueReport/IssueReportStatusHistoryDto.cs`
- `Core/Ecom.Domain/Enums/IssueReportType.cs`
- `Core/Ecom.Domain/Enums/IssueReportStatus.cs`
- `Core/Ecom.Domain/Enums/IssueReportSeverity.cs`

Persistence boundary:

- `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/IssueReportConfiguration.cs`
- `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/IssueReportStatusHistoryConfiguration.cs`
- `Infrastructure/Ecom.Infrastructure/Migrations/20260609022857_AddIssueReportTables.cs`

Do not edit migration or snapshot files unless the user explicitly approves schema work.

## Domain Model

`IssueReport` stores common fields:

- `Code`
- `Type`
- `Title`
- `IssueDescription`
- `Severity`
- `CurrentStatus`
- `ScreenOrFeature`

Software report constructor:

- Sets `Type = IssueReportType.Software`.
- Requires code, title, description, severity, and `AppScreenEnum screenOrFeature`.
- Starts at `IssueReportStatus.New`.

IoT device report constructor:

- Sets `Type = IssueReportType.IoTDevice`.
- Requires pond/device identifiers and device context.
- Captures snapshots:
  - `PondNameSnapshot`
  - `DeviceCodeSnapshot`
  - `DeviceNameSnapshot`
  - `DeviceTypeSnapshot`
  - `HubNameSnapshot`
  - `InstallationStatusSnapshot`
  - `ConnectionStatusSnapshot`
  - `ReportedAt`

`IssueReportStatusHistory` records:

- `IssueReportId`
- `FromStatus`
- `ToStatus`
- optional `Comment`

## Create Flow

`CreateIssueReportCommand` accepts:

- `Type`
- `Title`
- `IssueDescription`
- `Severity`
- `ScreenOrFeature` for software reports
- `PondId`, `DeviceId`, `HubId` for IoT device reports
- `DocumentIds` for uploaded attachment documents

`CreateIssueReportCommandHandler`:

- Has `[EnableUnitOfWork]`.
- Validates attachment IDs before creating the report.
- Allows max 5 attachments.
- Allows `.jpg`, `.jpeg`, `.png`, `.mp4`, `.mov`.
- Enforces max 10 MB per file.
- Generates report code with `RP{yyMMddHHmmss}{millisecond-mod-100}`.
- For software report, requires `ScreenOrFeature`.
- For IoT device report:
  - requires `PondId`
  - requires `DeviceId`
  - loads `Pond`
  - loads `Device` with `DeviceHub`
  - rejects when `device.DeviceHub == null` or `device.DeviceHub.PondId != request.PondId`
  - snapshots pond/device/hub fields into the report
- Inserts the report and an initial status history.
- Saves once to get `report.No`.
- Links documents with `ParentId = report.No` and `ParentType = ParentEntityTypeEnum.IssueReport`.
- Saves again if attachments were linked.

## Query Flow

`GetMyIssueReportsQueryHandler`:

- Filters by `CreatedBy == currentUser.UserId`.
- Supports type, status, and severity filters.
- Returns paged `IssueReportDto` without attachment list.

`GetManageIssueReportsQueryHandler`:

- Allows admin/manager-like roles in handler code.
- Supports type, status, severity, and pond filters.
- Returns paged `IssueReportDto` without attachment list.

`GetIssueReportDetailQueryHandler`:

- Allows manager/admin-like roles or original reporter.
- Loads reporter name from `User`.
- Loads attachments from `Document` where `ParentId == report.No` and `ParentType == IssueReport`.
- Uses `IDocumentService.GetDocumentUrl` for file and thumbnail URLs.

`GetIssueReportHistoriesQueryHandler`:

- Allows manager/admin-like roles or original reporter.
- Joins history to `User` for `ChangedByName`.
- Orders histories by creation time.

## Status Flow

`UpdateIssueReportStatusCommandHandler`:

- Has `[EnableUnitOfWork]`.
- Loads report by ID.
- Allows only these transitions:
  - `New -> Received`
  - `New -> Rejected`
  - `Received -> InProgress`
  - `Received -> Rejected`
  - `InProgress -> Resolved`
  - `InProgress -> Rejected`
  - `Resolved -> Closed`
  - `Rejected -> Closed`
- Updates `CurrentStatus`.
- Inserts a status history row.
- Saves changes.

## Current Notification / Permission Notes

- `IssueReportController` has been updated in source with per-action `Permissions.IssueReport.*` policy attributes.
- `IssueReportCreatedEvent` and `IssueReportCreatedEventHandler` exist in source and publish report-created notifications through `INotificationService.NotifyBulkAsync`.
- Report-created notifications are categorized as `NotificationCategories.Issue` so frontend/mobile can query the separate IssueReport group with `category=issue`.
- Existing comments and some strings may display as mojibake in PowerShell. Do not bulk-normalize encoding in unrelated files during report work.

