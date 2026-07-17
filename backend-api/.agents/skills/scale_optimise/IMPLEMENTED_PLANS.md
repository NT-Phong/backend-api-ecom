# Scale Realtime Implemented Plans

Updated: 2026-07-10

This file records scale realtime plans already executed in this workspace. Read it before proposing new fixes so old wrong assumptions do not return.

## Plan 1 - Open Telemetry Logs For IoT Payload Inspection

Problem:
- Local logs did not show what IoT scale sent to backend.

Implemented:
- Added or enabled targeted logs around Event Hub, IoTConnectionManager, Code 503 payloads, DeviceHealth heartbeat, snapshot update, and SignalR broadcast groups.

Result:
- Confirmed IoT sent Code 503 payloads.
- Root cause moved from "IoT not sending" to backend context/subscription/freshness.

## Plan 2 - Split Display Freshness And Confirm Freshness

Problem:
- Mobile showed `Disconnected` too quickly and confirm safety was treated like display freshness.

Implemented:
- Display freshness is 15s.
- Confirm freshness remains stricter at 5s.
- Confirm is disabled when data is too old.

Result:
- UI can display recent values longer while confirm remains safe.

## Plan 3 - Add SignalR Group Diagnostics

Problem:
- It was hard to compare mobile subscription groups with backend broadcast groups.

Implemented:
- TelemetryHub logs subscription method, connection id, user id, and group.
- TelemetryNotification logs device, session, scale, cycle, duration, and group list.

Result:
- Logs can prove whether mobile subscribed to the same group backend broadcasts.

## Plan 4 - Remove DB From Normal Code 503 Hot Path

Problem:
- Code 503 processing could query DB repeatedly when realtime context was missing.

Implemented:
- Redis `ScaleSessionRealtimeContext` by `DeviceId` is the normal path.
- DB fallback is cache-miss/recovery only.
- Fallback warms a short-lived device context when safe.
- Slow context resolve logging exists.

Result:
- Healthy path resolves context from Redis.

## Plan 5 - Superseded: Add Realtime Context At Start Session

Initial idea:
- Write realtime context at start-session by requiring `ScaleId`.

Why superseded:
- Mobile starts a session before selecting a scale.

Current replacement:
- Start session creates only `ScaleRecordSessionContext`.
- Telemetry control creates `ScaleSessionRealtimeContext` after scale selection.

## Plan 6 - Enforce One Active Realtime Context Per Device

Problem:
- Several sessions could retain the same physical device id.

Implemented:
- `SetActiveScaleSessionContextAsync` became the primary writer.
- It removes previous device/session context and stale session device index before setting the new context.
- Disable/finish/discard cleanup removes contexts and indexes.

Result:
- One physical IoT device maps to one active realtime context.

## Plan 7 - Fix Start Session Without ScaleId

Problem:
- `/api/v1/scalerecord/start-session` was called before the UI had a scale.
- Old record-based active-session detection blocked new sessions after finish.

Implemented:
- `StartScaleSessionCommand.ScaleId` is optional.
- Validator requires `CycleId`; `ScaleId` is validated only if supplied.
- Added Redis active scale-record session APIs.
- Start session locks by `UserId + CycleId`, reuses active Redis session, or creates one.
- Old records alone are not active-session proof.

Result:
- Mobile can start a session with only `cycleId`.
- Finish then start again is no longer blocked by old records.

## Plan 8 - Bind Session To Scale In Telemetry Control

Problem:
- After start-session stopped requiring `ScaleId`, backend still needed a correct bind point.

Implemented:
- `ControlScaleLiveTelemetryCommandHandler` validates active session on `Enabled=true` when `SessionId` is supplied.
- It writes realtime context after scale/device resolution and successful IoT direct method response.

Result:
- Code 503 can resolve session by device after telemetry is enabled.

## Plan 9 - Cleanup Active Session On Finish/Discard

Problem:
- Sessions with no confirmed records still need to close cleanly.

Implemented:
- Finish/discard read active scale-record session context by session id.
- Both remove active scale-record session context on success.
- Both continue cleaning realtime session/device context and session device index.

Result:
- Finish/discard can close active sessions without confirmed records.
- Start after finish/discard can create a new session.

## Plan 10 - Canonicalize Scale Skill Docs

Problem:
- Historical scale docs contained contradictory stale guidance.

Implemented:
- Created canonical docs:
  - `SKILL.md`
  - `ARCHITECTURE.md`
  - `scale-review.md`
  - `IMPLEMENTED_PLANS.md`
  - `PLAN.md`

Result:
- Future agents have one concise canonical path.

## Plan 11 - Fix 2026-05-18 Freshness, Rebind, And Hot Path Follow-Ups

Problem:
- `ScaleMeasurementSnapshot` stores derived `State`, `CanConfirm`, and `FreshnessSeconds`; cached snapshots could become stale when pushed later.
- `SubscribeToScale` pushed raw cached snapshots while `SubscribeToScaleSession` refreshed them.
- Rebinding a device to a new session could call public cleanup that acquires another session lock while current locks were held.
- Dropped scale packets were only debug-level visibility.

Implemented:
- `ScaleMeasurementTelemetryHandler` refreshes snapshot state before SignalR broadcast.
- `TelemetryHub.SubscribeToScale` refreshes initial snapshot before push.
- `ScaleCacheService` uses internal cleanup for previous active device/session context during rebind and removes previous session device index without calling public cleanup from inside current locks.
- `IoTConnectionManager` emits a warning every 100 dropped scale packets.

Result:
- Client push paths receive refreshed snapshot-derived fields.
- Scale detail/list subscriptions match session subscription behavior.
- Device rebind is safer under concurrent session/device contention.
- Operators can detect packet drop pressure without enabling debug logs.

## Verification Already Run

Historical builds previously passed after earlier scale changes:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

For Plan 11, only `git diff --check` was run by the agent; builds remain pending user-run verification.

## Current Acceptance Criteria

- Start session with only `cycleId` works.
- Start same user/cycle before finish reuses active session.
- Finish/discard removes active session context.
- Enabling telemetry with `sessionId + cycleId` binds realtime context.
- Code 503 context source is normally `RedisSessionContext`.
- Broadcast groups include `ScaleSession_`, `Scale_`, `Cycle_`, and `Device_`.
- REST live-weight does not show `Disconnected` while a fresh snapshot exists.
- Initial SignalR snapshot pushes refresh derived fields before sending.
- Confirm remains disabled when data is too old or net weight is below threshold.

## Plan 12 - Scale Control API For IoT Code 504

Problem:
- App/frontend needed a dedicated Scale API to trigger the three IoT-provided scale actions (ZERO, TARE, CLEAR_TARE) without sending generic TargetType, MethodName, or raw payload.

Implemented:
- Added `POST /api/v1/scale/{id:guid}/control` accepting `{ "action": "ZERO" | "TARE" | "CLEAR_TARE" }`.
- Added `ScaleControlProtocol` to map actions to IoT `MethodName` (`ZeroScale` / `TareScale`), payload `Code=504`.
- Added `ControlScaleCommandHandler` that calls `InvokeDirectMethodAsync` and updates scale connection status.
- Parsed root-level `Message` and `TareInfo` from device response.

Result:
- Frontend can explicitly trigger ZERO, TARE, CLEAR_TARE actions.

## Plan 13 - Chặn Lệnh Scale Control Khi Cân Chưa Stable

Problem:
- Sending control commands to a scale that is not stable or offline could cause errors or unexpected behavior.

Implemented:
- Added `IScaleCacheService` to `ControlScaleCommandHandler`.
- Loaded latest `ScaleMeasurementSnapshot` from Redis (`GetLatestSnapshotAsync`).
- Checked freshness (`ServerReceivedAtUtc` < 10s).
- Verified `Stable=true` and scale is not disconnected/error.
- Rejected request with `400 BAD_REQUEST` and friendly messages (`ScaleControlUnstable`, `ScaleControlNoFreshTelemetry`) if conditions are not met.

Result:
- Control commands are only sent to the IoT device if the scale is stable and telemetry is fresh, preventing invalid operations.

## Plan 14 - Latest-only stale packet drop for scale realtime

Problem:
- Scale packets could arrive from Event Hub already several seconds old and still be written to Redis and broadcast to mobile.

Implemented:
- Enabled scale stale-drop defaults (`DropStaleAzurePackets=true`, `StaleAzurePacketThresholdSeconds=2`).
- Added early stale packet drop in `IoTConnectionManager` before handler task creation and before device lock/context resolution.
- Added concise debug-level stale-drop logging for production diagnosis.
- Left Event Hub low-latency options to environment/runtime configuration instead of hard-coding project appsettings.

Result:
- Delayed Code 503 packets are dropped instead of broadcast; fresh packets continue through Redis and SignalR unchanged.

## Plan 15 - Telemetry Success Response Includes Bound Scale

Problem:
- FE hit 403 `ScaleRecordConfirmUserNotFound` from `POST /scale/{scaleId}/live-weight/telemetry` while the `sessionId` existed in Redis.
- Source showed the 403 was caused by `activeSession.UserId != currentUser.UserId`, meaning the caller was trying to enable telemetry with a session owned by another user/token.
- FE also needed the success response to include the selected scale that was bound after telemetry control succeeded.

Implemented:
- Kept strict session ownership validation in `ControlScaleLiveTelemetryCommandHandler`.
- Added warning log on owner mismatch with `SessionId`, `ScaleId`, `CycleId`, and `CurrentUserId`.
- Extended `ControlScaleLiveTelemetryResult` with a narrow `ControlScaleLiveTelemetryScaleResult`.
- Returned only non-sensitive scale fields: `id`, `code`, `name`, `deviceId`, `zoneId`, `usageStatus`, `connectionStatus`, `currentCycleId`, `occupiedByUserId`, and `isOccupiedByCurrentUser`.
- Did not change request body, route, Redis keys, SignalR groups, Code 503 flow, or failure response shape.

Result:
- Success response tells FE which scale was bound.
- Foreign-session telemetry attempts still fail with 403 and `data: null`.
- The failure remains a session-state/ownership issue, not an IoT transport issue.

## Plan 16 - Farmer-Friendly Scale Control Error Messages

Problem:
- Code 504 scale control failures from IoT returned technical English messages through `MessageKey.DeviceReportedError`.
- App users saw messages such as `Thiết bị báo lỗi: Zero failed: exceeds 2% full scale`, which are not farmer-friendly.

Implemented:
- Added `ScaleControlErrorMessageTranslator` under `Core/Ecom.Application/Features/Scale/Common`.
- `ControlScaleCommandHandler` translates known failure `Message` values before formatting `Thiết bị báo lỗi: {0}`.
- Covered ZERO, TARE, and CLEAR_TARE failure messages from the BA-provided definition.
- Added normalized matching for whitespace/casing and the observed `Zero failed: exist 2% full scale` alias.
- Left success `200` messages, `ControlScaleResult`, route, request body, IoT Code 504 payload, Redis keys, SignalR groups, and live-weight flow unchanged.

Result:
- Scale control failures shown in the app use Vietnamese farmer-facing wording while unknown IoT messages still fall back to the original device text.

Verification:
- `dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore` passed.
- `dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore` passed.

## Plan 17 - Decouple Code 503 Ingestion From DB And SignalR

Problem:
- The per-device Code 503 lock covered Redis, connection-status DB sync, and SignalR broadcast.
- A 2-3s downstream delay caused all 100ms packets arriving during that interval to be dropped, making FE appear to update every few seconds.

Implemented:
- Added internal `IScaleRealtimeDispatcher` and singleton `ScaleRealtimeDispatcher` background service.
- Kept handler hot path limited to validation, Redis context resolution, latest snapshot write, and non-blocking enqueue.
- Added one latest pending snapshot per device, 250ms minimum broadcast cadence, bounded scheduling capacity, and eight broadcast workers.
- Kept one active broadcast per device and removed detached timeout broadcasts to preserve ordering.
- Moved throttled connection-status DB sync to an independent worker loop.
- Added aggregate ingress/dispatch counters and separate hot-path, dispatch-wait, SignalR, and DB-sync timings.

Result:
- Slow DB or SignalR no longer holds the Code 503 device lock.
- Burst traffic is coalesced instead of queued, while FE receives the newest accepted snapshot.
- Public REST, Redis, SignalR, and IoT contracts are unchanged.

