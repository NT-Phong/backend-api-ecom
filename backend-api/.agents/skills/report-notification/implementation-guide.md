# Issue Report Implementation Guide

Use this guide for Phase 3 permissions and Phase 4 report-created notifications.

## Guardrails

Ask before changing:

- public routes or request/response DTO shapes
- permission constants and seeded policies beyond the requested IssueReport scope
- EF migrations, DbContext, model snapshots, or entity configuration
- notification category, target URL contract, or FCM payload contract
- recipient-scope semantics for Admin/Manager/zone filtering

Do not rename existing IssueReport files or types for style.

## Phase 3 Permissions

Expected permission model from `handoff_guide.md`:

```csharp
public static class IssueReport
{
    public const string View = "Permissions.IssueReport.View";
    public const string Create = "Permissions.IssueReport.Create";
    public const string Manage = "Permissions.IssueReport.Manage";
    public const string UpdateStatus = "Permissions.IssueReport.UpdateStatus";
}
```

Before implementation:

1. Inspect `Core/Ecom.Domain/Constants/Permissions.cs`.
2. Check how nearby modules declare nested permission classes.
3. Check `GetAll()` and `Infrastructure/Ecom.Infrastructure/Seeding/RoleSeeder.cs` behavior.
4. Confirm whether the user wants new permissions seeded in this task.

Controller policy mapping from the handoff plan:

- Create report: `Permissions.IssueReport.Create`
- Get my reports: `Permissions.IssueReport.View`
- Get detail: `Permissions.IssueReport.View`
- Manage list: `Permissions.IssueReport.Manage`
- Update status: `Permissions.IssueReport.UpdateStatus`
- Get histories: `Permissions.IssueReport.View`

Data access checks already exist in handlers, but do not treat controller policies and handler checks as interchangeable:

- Controller policies protect endpoint capability.
- Handler checks protect record-level ownership/management access.

If implementing Phase 3, preserve the handler-level owner checks unless the task explicitly changes the business rule.

## Phase 4 Notifications

Expected event shape from `handoff_guide.md`:

```csharp
public record IssueReportCreatedEvent(Guid IssueReportId) : INotification;
```

Prefer existing notification path:

- `Core/Ecom.Application/Common/Interfaces/INotificationService.cs`
- `Core/Ecom.Application/Common/Interfaces/INotificationRecipientResolver.cs`
- `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs`
- `Core/Ecom.Application/Features/Notification/Services/NotificationService.cs`
- FCM fanout is handled by existing notification event handlers under `Infrastructure/Ecom.Infrastructure/Event/FcmNotification/`.
- Use `NotificationCategories.Issue` for IssueReport-created notifications when they must appear in the separate IssueReport notification group.

Do not call Firebase directly from the IssueReport feature unless source shows that is the established pattern for comparable flows.

## Recipient Rules

Software report:

- Title: "Báo cáo lỗi phần mềm" unless product copy changes.
- Message shape: reporter name plus issue title.
- Recipients: Admin users.
- Source data: `IssueReport.CreatedBy`, `User.FullName`, `IssueReport.Title`.

IoT device report:

- Title: "Báo cáo lỗi thiết bị" unless product copy changes.
- Message shape: device name, pond name, issue title.
- Recipients: Admin plus managers for the relevant zone.
- Source data: report snapshots and pond/device relations.
- Derive zone from trusted server-side data:
  - Prefer `Pond.ZoneId` if available from `IssueReport.PondId`.
  - Do not trust a client-supplied zone field unless the source contract later adds and validates it.

Current `NotificationRecipientResolver.ResolveForZoneAsync` includes roles:

- `Manager`
- `EmployeeManager`
- `EmployeeWarehouse`
- `Admin`

If the BA says "Admin and Manager only", inspect resolver behavior and either use a narrower query or add a narrowly named resolver method with explicit approval.

## Target URL Guidance

Before choosing a target URL:

1. Inspect existing notification producers for `targetUrl` patterns.
2. Confirm FE route for report detail/manage screen if available.
3. Keep URL stable and include enough context for FE routing.

Likely candidates must be verified against FE contract, for example:

- `/issue-reports/{id}`
- `/issue-reports/manage/{id}`
- `/reports/issues/{id}`

Do not invent a final target URL in code without source/FE confirmation.

## Implementation Shape

Recommended narrow path for Phase 4:

1. Add `IssueReportCreatedEvent` under `Core/Ecom.Application/Features/IssueReport/Events/`.
2. Publish after the report and attachments are saved in `CreateIssueReportCommandHandler`.
3. Handle the event in an application-level handler.
4. Load the report by ID using no-tracking when possible.
5. Resolve recipient IDs according to report type.
6. Call `INotificationService.NotifyBulkAsync`.
7. If recipient list is empty, log and return without failing report creation unless BA requires failure.

Keep report creation durable even if notification delivery fails unless the user explicitly requests transactional notification behavior.

## Verification

For docs-only skill updates:

```powershell
git diff --check -- .agents/skills/report-notification
```

For code changes:

```powershell
dotnet build Core/Ecom.Application/Ecom.Application.csproj
dotnet build Presentation/Ecom.API/Ecom.API.csproj
git diff --check
```

If a skill report/doc is changed and `.agents/scripts/verify-skill-docs.ps1` exists, run it or explain why it was not run.

