# Plan: IoT Device Report Notification Target

## Summary

Use this plan when implementing the next Issue Report notification adjustment for IoT device reports. The requirement is now:

- Web frontend expects notification `targetUrl` to be `/overview/home`.
- Mobile app expects FCM payload `screen` to be `ControlDetail`.
- `zoneId` will be handled in a later task, so do not add or depend on `zoneId` in this task.
- `Software` report behavior stays unchanged.

## Current Source Context

Current implementation areas:

- `Core/Ecom.Application/Features/IssueReport/Events/IssueReportCreatedEventHandler.cs`
- `Core/Ecom.Application/Common/Constants/NotificationTargetUrls.cs`
- `Core/Ecom.Application/Common/Helpers/NotificationPayloadBuilder.cs`
- `Core/Ecom.Application/Common/Constants/MobileScreens.cs`
- `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs`

Current IoT flow already loads `Pond` from `IssueReport.PondId` and uses `Pond.ZoneId` for recipient resolution and URL. For this task, recipient resolution can still use pond zone if needed, but notification URL/payload must not expose or rely on `zoneId` until the later zoneId task.

## Implementation Plan

Keep `Software` report flow as-is.

For `IoTDevice` report notification:

- Keep loading `Pond` from `report.PondId` to derive business context and recipients.
- Keep resolving recipients by pond zone according to the agreed recipient rule at implementation time.
- Change web `targetUrl` to `/overview/home`.
- Add mobile screen support so FCM payload returns:
  - `targetUrl = "/overview/home"`
  - `screen = "ControlDetail"`
  - `type = "ControlDetail"`
- Do not append `zoneId` to `targetUrl` in this task.
- Do not change Notification API zone filtering in this task.

Recommended source changes:

- Add `MobileScreens.ControlDetail = "ControlDetail"`.
- Add `NotificationTargetUrls.OverviewHome()` returning `/overview/home`.
- Update `NotificationPayloadBuilder` mapping so path `overview/home` maps to `ControlDetail`.
- Update `IssueReportCreatedEventHandler.NotifyDeviceIssueAsync` to use `NotificationTargetUrls.OverviewHome()` for IoT device report target URL.

## Verification Plan

Build:

```powershell
dotnet build Core/Ecom.Application/Ecom.Application.csproj --no-restore
dotnet build Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj --no-restore
```

Static audit:

```powershell
rg -n "OverviewHome|ControlDetail|IssueReportDetail|NotifyDeviceIssueAsync" Core Infrastructure
```

Runtime scenario:

- Create an IoT device report with `type = IoTDevice` and a valid `pondId`.
- Confirm DB notification has `TargetUrl = /overview/home`.
- Confirm FCM data payload has `screen = ControlDetail` and `type = ControlDetail`.
- Confirm no `zoneId` is required for the web URL in this task.
- Create a Software report and confirm its existing notification behavior is unchanged.

## Explicit Non-Goals

- Do not change request/response DTOs.
- Do not add migrations.
- Do not change `GET /api/v1/notification` zone filtering.
- Do not add `zoneId` to IssueReport notification URL until the later task requests it.

