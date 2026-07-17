---
name: scale_optimise
description: Use this skill when debugging, reviewing, planning, or changing Ecom scale realtime flows and scale control flows, including scale sessions, IoT Code 503 telemetry, IoT Code 504 ZERO/TARE/CLEAR_TARE, Redis snapshots/context, SignalR groups, live-weight APIs, and session lifecycle cleanup.
argument-hint: "Task focus: scale realtime | live weight | Code 503 | Code 504 control | Redis context | SignalR group | session lifecycle"
user-invocable: true
---

# Scale Realtime Optimise Skill

## Purpose
Guide agents working on scale live-weight and scale-record session flows without reintroducing old assumptions.

## Load Order
Use this skill only when the task touches scale realtime, live weight, Code 503 telemetry, Code 504 scale control, scale session lifecycle, Redis scale context, or TelemetryHub scale groups.

Read only what the task needs:
1. `.agents/skills/scale_optimise/scale-review.md` for current problem set, architecture summary, file list, checklist, and known follow-ups.
2. `.agents/skills/scale_optimise/PLAN.md` for the current accepted mobile/API flow and verification plan.
3. `.agents/skills/scale_optimise/IMPLEMENTED_PLANS.md` before proposing new fixes, so superseded ideas do not return.
4. `.agents/skills/scale_optimise/ARCHITECTURE.md` only when the task needs detailed Redis key, SignalR group, or end-to-end flow context.

Historical deep-review files were folded into the canonical docs. Do not recreate date-stamped review files unless the user explicitly asks for an archival report.

## Current Rules
- `/api/v1/scalerecord/start-session` must support starting with only `cycleId`; the UI may not have selected a scale yet.
- Scale realtime binding belongs to `/api/v1/scale/{scaleId}/live-weight/telemetry`, where route `scaleId` and body `sessionId + cycleId` are available.
- `ScaleSession_{sessionId}` is the primary SignalR group for active weighing screens.
- `Scale_{scaleId}`, `Device_{iotDeviceId}`, and `Cycle_{cycleId}` are secondary or compatibility groups.
- Code 503 telemetry should resolve context from Redis in the normal hot path, not hit the database on every packet.
- Code 503 device locks must end after latest-snapshot Redis acceptance and dispatcher enqueue; SignalR and connection-status DB sync stay outside the ingestion lock.
- `ScaleRealtimeDispatcher` keeps one latest pending snapshot and one active broadcast per device, with a default 250ms minimum broadcast interval.
- One physical IoT scale device should map to one active realtime context.
- Finish/discard must clean scale-record session context, realtime context, and session-device indexes.
- Display freshness may be more lenient than confirm freshness; do not relax confirm safety for UI convenience.
- Treat `ServerReceivedAtUtc` as the freshness source of truth; firmware `DeviceTimestampUtc` is not trusted for display/confirm freshness.
- Refresh cached `ScaleMeasurementSnapshot` derived fields before sending snapshots to clients.
- When rebinding one physical device to another session, remove previous context/index without changing Redis key formats or SignalR group names.
- For `/api/v1/scale/{id}/control` Code 504 failures, keep the public API shape unchanged and translate known IoT device `Message` values through `ScaleControlErrorMessageTranslator` before wrapping with `MessageKey.DeviceReportedError`.

## Protected Contracts
Do not change these without explicit approval:
- Scale REST request/response contracts.
- Redis key formats and context indexes.
- SignalR group names.
- IoT Code 503 payload handling.
- IoT Code 504 request payload and action contract (`ZERO`, `TARE`, `CLEAR_TARE`).
- Scale session lifecycle semantics.

## Source Areas
Start with targeted search, then inspect only the affected files:
- Scale controllers and live-weight endpoints.
- `Core/Ecom.Application/Features/Scale`
- `Core/Ecom.Application/Features/Scale/Common/ScaleControlProtocol.cs`
- `Core/Ecom.Application/Features/Scale/Common/ScaleControlErrorMessageTranslator.cs`
- `Core/Ecom.Application/Features/ScaleRecord`
- Scale telemetry handlers in Infrastructure.
- `ScaleRealtimeDispatcher` for latest-only SignalR delivery and connection-status sync.
- `ScaleCacheService` and scale cache interfaces.
- `TelemetryNotificationService`
- `TelemetryHub`

## Verification
For guidance-only changes, run `.agents` verification scripts.

For code changes, recommend narrow builds from `PLAN.md` and `.agents/context/commands.md`. Runtime scale verification should check:
- start session with only `cycleId`;
- enable telemetry with `sessionId + cycleId`;
- Code 503 context source is Redis;
- broadcasts include expected groups;
- SignalR initial pushes refresh cached snapshot state;
- finish/discard cleanup allows a new session.

