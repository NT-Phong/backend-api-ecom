# Scale Optimise Review

Updated: 2026-07-10

This is the canonical current review for Ecom scale realtime. Historical deep reports were folded into this file so future agents do not re-read stale "fixed" claims.

## Current State

Scale realtime now follows the mobile workflow:

```text
start session by cycle
-> select/add scale
-> enable live telemetry for scale
-> Code 503 telemetry
-> Redis context/snapshot
-> SignalR groups
-> mobile
```

Protected contracts remain unchanged:
- REST contracts are unchanged.
- Redis key formats are unchanged.
- SignalR groups remain `ScaleSession_{sessionId}`, `Scale_{scaleId}`, `Device_{deviceId}`, and `Cycle_{cycleId}`.
- IoT Code 503 payload handling is unchanged.

## Resolved Problems

### S1 - `start-session` must not require `ScaleId`

Problem:
- Mobile starts a scale-record session before selecting a scale.

Resolution:
- `StartScaleSessionCommand.ScaleId` is optional.
- Start session creates/reuses `ScaleRecordSessionContext` by `UserId + CycleId`.
- Realtime scale/device binding happens later in live telemetry control.

### S2 - Finished sessions blocked new sessions

Problem:
- Old logic inferred active sessions from historical `ScaleRecord` rows.

Resolution:
- Active session state lives in Redis active scale-record session context.
- Finish/discard remove active session context.
- Historical records alone are not active-session proof.

### S3 - One device could point to stale sessions

Problem:
- A physical scale device could keep stale session/device mappings.

Resolution:
- `SetActiveScaleSessionContextAsync` is the primary writer for realtime binding.
- It enforces one active realtime context per physical device.
- Rebind cleanup removes previous device/session context and previous session device index without calling public cleanup while holding current locks.

### S4 - Mobile showed `Disconnected` too easily

Problem:
- Display freshness and confirm freshness were mixed.
- Snapshot TTL was too short for IoT/Event Hub delay.

Resolution:
- Display freshness remains 15s.
- Confirm freshness remains stricter at 5s.
- Snapshot TTL is 30s.
- REST live-weight recomputes freshness from `ServerReceivedAtUtc`.

### S5 - `canConfirm=true` when net weight was 0

Problem:
- `EvaluateCanConfirm` allowed `net >= 0`.

Resolution:
- Confirm now requires `net >= DefaultNetThresholdKg` currently 0.5kg.
- Empty but stable scale must not be confirmable.

### S6 - Snapshot-derived fields could be stale

Problem:
- Redis snapshot stores derived fields: `State`, `CanConfirm`, and `FreshnessSeconds`.
- Raw cached values can become stale after read-back.

Resolution:
- SignalR telemetry broadcast refreshes snapshot state immediately before sending.
- `SubscribeToScaleSession` and `SubscribeToScale` refresh initial snapshots before sending.
- REST live-weight already recomputes state from `ServerReceivedAtUtc`.

Important rule:
- Treat `ServerReceivedAtUtc` as the source of truth for freshness.
- Do not use firmware `DeviceTimestampUtc` as freshness truth unless device clock trust is separately proven.

### S7 - Hot path delay and drop visibility

Problem:
- Scale packets could queue behind a per-device lock.
- Dropped packet visibility was too quiet.

Resolution:
- Code 503 path uses drop-if-busy because scale telemetry is stateless.
- The parser peeks telemetry code before locking.
- Every 100 dropped scale packets emits a warning.

### S8 - Unified Integer serialization matching Client Switch-Case

Problem:
- The Frontend and Mobile App are programmed to receive `state` exclusively as integers mapping to:
  `0` -> `Disconnected`, `1` -> `Empty`, `2` -> `Waiting`, `3` -> `Ready`.
- Previously, HTTP REST responses returned enums as strings (due to controllers JsonOptions), causing mixed types, state jitter, and wrong client mappings.

Resolution:
- Configured C# `ScaleMeasurementState` enum values to align perfectly with the client's expected integer indexes:
  `Disconnected = 0`, `Empty = 1`, `Waiting = 2`, `Ready = 3`.
- Changed `State` property type in `ScaleLiveWeightResponse` from `string` to `int` and casted `currentState` to `(int)currentState`.
- Removed `[JsonConverter]` to keep it numeric over SignalR, guaranteeing both REST and SignalR transmit uniform, correct integer indexes without modifying global setup.

### S9 - Latest-only stale packet handling

Problem:
- Scale packets could arrive from Event Hub already several seconds old and still be pushed to mobile.

Resolution:
- `DropStaleAzurePackets` is enabled with a 2s threshold.
- Code 503 stale packets are dropped in `IoTConnectionManager` before creating handler work or acquiring the device lock.
- `ScaleMeasurementTelemetryHandler` remains a second stale-drop guard.
- Event Hub low-latency options should be supplied by environment/runtime configuration when needed; do not hard-code them into project appsettings.

Result:
- Mobile should receive fresh/latest scale packets only; stale Event Hub packets are dropped before broadcast.

### S10 - Telemetry 403 With A Foreign Active Session

Problem:
- `POST /scale/{scaleId}/live-weight/telemetry` can return 403 `ScaleRecordConfirmUserNotFound` even though the Redis active session exists.
- The failure is not an IoT direct-method problem. It happens when `ControlScaleLiveTelemetryCommandHandler` finds `activeSession.UserId != currentUser.UserId`.
- This usually points to FE/session state reusing a `sessionId` created under another user/token.

Resolution:
- Keep the strict ownership rule: a user cannot enable telemetry with another user's active scale-record session.
- Do not create a replacement session inside telemetry control.
- Do not add data to the 403 failure response; `TResult` failures still return `data: null`.
- Add a warning log with `SessionId`, `ScaleId`, `CycleId`, and `CurrentUserId` when owner mismatch occurs.
- Extend only the success response with a narrow `scale` DTO so FE can confirm the bound scale without exposing sensitive device fields.

### S11 - Device Still Live But Session Live-Weight 404

Problem:
- A scale can remain live by `DeviceId` or `ScaleId` while `GET /scale/sessions/{sessionId}/live-weight` returns 404 `ScaleRecordSessionNotFound`.
- `finish-session` closes and cleans session state, but stopping telemetry on the physical device is best-effort.
- If direct-method stop fails, or `deviceIdsInSession` is empty, the scale can keep sending Code 503 after the session is removed.
- When Redis realtime context is missing, `ScaleMeasurementTelemetryHandler` DB fallback can warm a device context with `SessionId = Guid.Empty`, which must not be treated as a real session.

Current handling:
- Session cleanup behavior is intentional: finished/discarded sessions are no longer valid session-live targets.
- DB fallback is recovery for device/scale live data only; it must not create a fake session binding.

Follow-up plan:
- Normalize `Guid.Empty` session ids to `null` in live-weight responses.
- Return `400 BAD_REQUEST` if the session live-weight endpoint is called with `Guid.Empty`.
- Include real `sessionId` and `cycleId` in telemetry-control success when the request is session-bound.
- Make finish/discard telemetry shutdown more observable and more reliable by resolving device ids before cleanup from cached context, session device index, request scale ids, and current scale state.

### S12 - Vietnamese Scale Control Error Messages

Problem:
- `/api/v1/scale/{id}/control` parsed root-level IoT `Message` from Code 504 responses and returned it directly through `MessageKey.DeviceReportedError`.
- App users saw technical English messages such as `Zero failed: exceeds 2% full scale`.

Resolution:
- `ControlScaleCommandHandler` now translates known Code 504 failure messages with `ScaleControlErrorMessageTranslator` before formatting `Thiết bị báo lỗi: {0}`.
- Only failure messages are translated. Success `200` messages and `ControlScaleResult` payload shape remain unchanged.
- Unknown IoT messages fall back to the original device text so new device errors are not hidden.
- The translator normalizes whitespace/casing and supports the observed alias `Zero failed: exist 2% full scale` as the same too-heavy ZERO failure.

### S13 - Code 503 Hot Path No Longer Waits For DB Or SignalR

Problem:
- One Code 503 packet held the per-device lock while connection-status DB sync and SignalR broadcast completed.
- IoT packets arriving every 100ms were dropped while a 2-3s downstream operation held the lock, so FE updates inherited the downstream delay.

Resolution:
- `ScaleMeasurementTelemetryHandler` now ends its hot path after context resolution, conditional Redis snapshot write, and non-blocking dispatcher enqueue.
- `ScaleRealtimeDispatcher` coalesces one latest pending snapshot per device and broadcasts at most every 250ms.
- Broadcasts remain serialized per device and are awaited by the dispatcher, so an old timed-out task cannot complete after a newer broadcast.
- Connection-status DB sync runs in an independent throttled worker loop and cannot block Redis ingestion or SignalR.
- Redis keys, snapshot TTL, Code 503 payload, `ReceiveScaleData`, and all SignalR group names remain unchanged.

## Current Files Of Interest

- `Core/Ecom.Application/Features/Scale/Common/ScaleMeasurementSnapshot.cs`
- `Core/Ecom.Application/Features/Scale/Common/ScaleControlErrorMessageTranslator.cs`
- `Core/Ecom.Application/Features/Scale/Common/ScaleControlProtocol.cs`
- `Core/Ecom.Application/Features/Scale/Commands/ControlScale/ControlScaleCommandHandler.cs`
- `Core/Ecom.Application/Features/Scale/Commands/ControlScaleLiveTelemetry/ControlScaleLiveTelemetryCommandHandler.cs`
- `Core/Ecom.Application/Features/Scale/Queries/GetScaleLiveWeight/GetScaleLiveWeightQuery.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/StartScaleSession/StartScaleSessionCommandHandler.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/FinishScaleSession/FinishScaleSessionCommandHandler.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/DiscardScaleSession/DiscardScaleSessionCommandHandler.cs`
- `Infrastructure/Ecom.Infrastructure/Caching/ScaleCacheService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/IoTConnectionManager.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/Handlers/ScaleMeasurementTelemetryHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/ScaleRealtimeDispatcher.cs`
- `Presentation/Ecom.API/Hubs/TelemetryHub.cs`
- `Presentation/Ecom.API/Services/TelemetryNotificationService.cs`

## Correctness Checklist

- Do not make `ScaleId` required in start-session.
- Do not bind realtime context before scale selection, except backward-compatible optional `ScaleId` handling.
- Do not infer active sessions from old records alone.
- Do not allow one device to stay bound to several active realtime session contexts.
- Do not change Redis key formats.
- Do not change SignalR group names.
- Keep `ScaleSession_{sessionId}` primary for weighing screens.
- Keep `Scale_{scaleId}` for scale list/detail screens.
- Refresh snapshot-derived fields before sending cached snapshots to clients.
- Keep display freshness and confirm freshness separate.
- Always serialize `state` as uniform integer index numbers matching the Client switch-case (`0 = Disconnected`, `1 = Empty`, `2 = Waiting`, `3 = Ready`) across both SignalR WebSocket and REST API.
- Drop stale Code 503 packets before handler work when `azure_to_backend_ms` exceeds the configured threshold; do not push delayed packets to mobile for UI continuity.
- Keep scale control Code 504 route/request/response contracts unchanged; translate only known device failure `Message` values for farmer-facing app display.
- Keep the Code 503 device lock limited to context resolution, Redis latest-snapshot acceptance, and dispatcher enqueue; DB and SignalR must remain outside it.
- Keep dispatcher semantics latest-only: one pending snapshot and one active broadcast per physical device.

## Known Follow-Ups

- Add automated tests for:
  - start-session without `ScaleId`;
  - start-session reuse before finish;
  - finish/discard cleanup of active session context;
- telemetry control rejects inactive/foreign sessions;
- telemetry control 403 owner mismatch remains strict and logs mismatch context;
- Code 503 cache-hit context resolution;
- live-weight responses never expose `Guid.Empty` as a real `sessionId`;
- session live-weight rejects zero GUID as invalid input;
- finish/discard stop-telemetry behavior is observable when the direct method fails or no device id can be resolved;
- SignalR initial snapshot refresh for `SubscribeToScaleSession` and `SubscribeToScale`;
- rebind of one device from session A to session B.
- Consider a diagnostic endpoint or script for current scale Redis keys during local testing.
- Review production behavior when Redis is unavailable; realtime should degrade clearly instead of silently using divergent per-instance state.
- Add focused tests for `ScaleControlErrorMessageTranslator` known messages, whitespace/casing normalization, `exist 2% full scale` alias, and unknown-message fallback.

