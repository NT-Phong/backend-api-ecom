---
name: camera-media
description: Use this skill when debugging, reviewing, planning, or changing Ecom camera/media behavior, including CameraController, MediaMTX stream URLs, viewer-limit confirmation, liveUrl list behavior, HLS/WebRTC/RTSP, camera workers, and camera notification targets.
argument-hint: "Task focus: camera stream | viewer limit | liveUrl | MediaMTX | HLS/WebRTC/RTSP | camera notification"
user-invocable: true
---

# Camera Media Skill

## Purpose

Guide camera and media work without accidentally bypassing viewer limits, changing stream protocols, or weakening camera/media security assumptions.

## When To Use

Use this skill for:
- `GET /api/v1/camera/{sn}/stream`.
- Camera list `liveUrl` behavior.
- MediaMTX stream URL generation and viewer counts.
- HLS, WebRTC, RTSP, and HD `_hd` stream path questions.
- Camera disconnected/recovery/person-detected notification routing.
- Camera workers, snapshots, playback, Dahua/MediaMTX integration, Basic Auth, or TLS handling.

For notification-category-only work, also use `FCM-notification`. For general CQRS/controller review, use `backend-api-architecture`.

## Current Source Flow

```text
CameraController.GetStream
-> GetCameraStreamQuery / GetCameraStreamHandler
-> IMediaMtxService.GetStreamUrlAsync
-> MediaMtxService
-> CameraStreamDto
-> ApiResponse
```

List flow:

```text
CameraController.GetCameras
-> GetCamerasQuery / GetCamerasHandler
-> CameraResponseDto.liveUrl
```

Notification flow:

```text
camera event/stream warning
-> NotificationTargetUrls.CameraDetail(deviceCode, zoneId)
-> NotificationRecipientResolver zone recipients
-> NotificationService.NotifyBulkAsync or notification event pipeline
```

## Current Rules

- `MediaMtxService` currently enforces a `MaxStreamViewers` value of `3`.
- Viewer-limit handling is a warning/confirmation flow, not a normal hard validation failure.
- `CameraController.GetStream` can return HTTP `429 Too Many Requests` while still wrapping the body with `ApiResponse.Ok(...)` when `RequiresConfirmation == true`.
- `ConfirmOverLimit` means the user accepted the warning; do not remove or bypass it without product approval.
- `liveUrl` is a real HLS viewing path unless product explicitly says it is preview-only.
- HD stream selection uses the stream name suffix `_hd`; protocol selection is separate.
- The response protocol should match the requested protocol; do not hardcode WebRTC when HLS or RTSP was requested.
- Treat MediaMTX Basic Auth, TLS validation, and stream URLs as security-sensitive.

## Source Areas

Start with targeted search:

```powershell
rg "GetCameraStream|CameraStreamDto|ConfirmOverLimit|RequiresConfirmation|liveUrl|GetCamerasHandler" Core Presentation Infrastructure
rg "MaxStreamViewers|GetStreamUrlAsync|GetStreamUrlIfExistsAsync|BuildStreamUrl" Infrastructure/Ecom.Infrastructure/Services/Camera Core/Ecom.Domain/Interfaces/Services
rg "CameraDetail|CameraList|NotificationTargetUrls|CameraDevice" Core Infrastructure Presentation
```

Likely files:
- `Presentation/Ecom.API/Controllers/V1/CameraController.cs`
- `Core/Ecom.Application/Features/Device/Queries/GetCameraStream`
- `Core/Ecom.Application/Features/Device/Queries/GetCameras`
- `Core/Ecom.Domain/Interfaces/Services/IMediaMtxService.cs`
- `Infrastructure/Ecom.Infrastructure/Services/Camera/MediaMtxService.cs`
- `Core/Ecom.Application/Common/Constants/NotificationTargetUrls.cs`
- `Core/Ecom.Application/Common/Helpers/NotificationPayloadBuilder.cs`
- `Infrastructure/Ecom.Infrastructure/Services/NotificationRecipientResolver.cs`

## Protected Contracts

Do not change these without explicit approval:
- Camera REST route shapes and response DTO fields.
- Viewer-limit semantics or threshold.
- `ConfirmOverLimit` behavior.
- `liveUrl` meaning.
- MediaMTX stream path naming, including `_hd`.
- HLS/WebRTC/RTSP URL protocol mapping.
- Camera notification target URL shape.
- Basic Auth, TLS, credentials, or runtime camera/media config.

## Verification

For guidance-only changes, run `.agents` verification scripts.

For code changes, recommend the narrowest affected build:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

Runtime/manual checks when camera stream behavior changes:
- `/camera/{sn}/stream` returns requested protocol and correct HD/main stream path.
- Viewer count at or above threshold returns warning-shaped confirmation data.
- Confirmed over-limit request returns a usable stream URL.
- Camera list `liveUrl` behavior matches the product decision and does not silently bypass the intended viewing limit.
- Camera notification target opens the camera detail/list screen expected by frontend/mobile.

