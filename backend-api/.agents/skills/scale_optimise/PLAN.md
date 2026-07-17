# Scale Realtime Current Plan

Updated: 2026-07-10

## Summary

The accepted scale realtime flow is:

```text
Start session by cycle -> select/add scale -> enable live telemetry -> Code 503 telemetry -> Redis snapshot/context -> SignalR -> mobile
```

The key contract is unchanged: `/api/v1/scalerecord/start-session` must support only `cycleId`. Realtime scale/device binding belongs to `/api/v1/scale/{scaleId}/live-weight/telemetry`, where the route `scaleId` and body `sessionId + cycleId` are available.

## Current Implementation Plan

### P0 - Session Contract

- `StartScaleSessionCommand.ScaleId` stays optional.
- Start session validates current user and `CycleId`.
- Active session state is Redis `ScaleRecordSessionContext` by `UserId + CycleId`.
- Old `ScaleRecord` rows alone must not block new sessions.

### P1 - Realtime Binding

- `ControlScaleLiveTelemetryCommandHandler` is the normal bind point.
- On `Enabled=true` with `SessionId`, validate:
  - active session exists;
  - active session belongs to current user;
  - supplied `CycleId`, if any, matches active session;
  - selected scale is not occupied by another user/cycle.
- Write `ScaleSessionRealtimeContext` only after successful IoT direct method response.
- `SetActiveScaleSessionContextAsync` enforces one active realtime context per device.
- If active session ownership mismatches the current user, keep returning 403. Log `SessionId`, `ScaleId`, `CycleId`, and `CurrentUserId`, but do not add payload data to the failure response.
- Success responses from telemetry control may include a narrow bound-scale DTO. Do not expose `AuthDevice`, host names, tokens, secrets, or raw IoT payload internals.

### P2 - Snapshot Freshness

- `ServerReceivedAtUtc` is the source of truth for display/confirm freshness.
- Redis snapshot may keep derived fields for compatibility, but cached snapshots must be refreshed before client push.
- REST live-weight recomputes freshness at read time.
- SignalR telemetry broadcast refreshes snapshot state before sending.
- `SubscribeToScaleSession` and `SubscribeToScale` refresh initial snapshots before sending.
- Display freshness is 15s.
- Confirm freshness is stricter, currently 5s.
- Enforce consistent integer enum serialization across both SignalR Websocket and HTTP REST API (`0 = Disconnected`, `1 = Empty`, `2 = Waiting`, `3 = Ready`) by aligning the C# enum order, removing `[JsonConverter]`, and changing the `ScaleLiveWeightResponse.State` property type to `int` to match Client expectations.

### P3 - Cleanup Lifecycle

- Finish/discard remove:
  - active scale-record session context;
  - realtime session context;
  - realtime device context;
  - session device index.
- Disable telemetry removes latest snapshot and realtime context for the selected device/session.
- Rebinding a physical device to a different session removes previous context/index before setting the new active context.
- Finish/discard currently treats stop-telemetry as best-effort. If the device keeps sending Code 503 after finish, the session is still closed; diagnose whether direct-method stop failed or whether no device id was resolved before cleanup.
- Do not treat DB fallback `SessionId = Guid.Empty` as a valid session binding. Device/scale live data may continue without a session-bound snapshot.

### P5 - Empty Session Guard Follow-Up

- `ScaleLiveWeightResponse.From` should normalize `Guid.Empty` to `null` for `SessionId`.
- `GetScaleSessionLiveWeightQueryHandler` should reject `Guid.Empty` with `BAD_REQUEST` rather than returning misleading 404.
- `ControlScaleLiveTelemetryResult` should include real `sessionId` and `cycleId` on success when telemetry was bound to an active session.
- FE must not convert `sessionId: null` into `00000000-0000-0000-0000-000000000000`.

### P4 - Hot Path Observability

Keep these logs during scale testing:

- `[ScaleRecordSession] active session created`
- `[ScaleRecordSession] active session reused`
- `[ScaleRecordSession] active session removed`
- `[ScaleSessionRealtime] active context set`
- `[ControlScaleLiveTelemetry] Scale=..., Device=..., Enabled=...`
- `[ScaleMeasurementTelemetry] Context resolved ... Source="RedisSessionContext"`
- `[ScaleTelemetryStaleDropped] ...`
- `[ScaleCacheService] Snapshot updated`
- `[TelemetryNotification] Broadcast scale data ... Groups="..."`
- `[IoTConnectionManager] Dropped ... scale packets ...`

Scale realtime is latest-only: stale Code 503 packets older than the configured `ScaleTelemetry.StaleAzurePacketThresholdSeconds` should be dropped instead of being broadcast to mobile.

Current hot-path rule:
- the per-device lock may cover context resolution, Redis latest-snapshot acceptance, and dispatcher enqueue only;
- SignalR and connection-status DB sync run outside the ingestion lock;
- the dispatcher keeps one pending latest snapshot and one active broadcast per device;
- default SignalR cadence is at most 4Hz (250ms minimum interval), even when IoT sends every 100ms.

## Correct Mobile/API Flow

1. Start scale-record session:

```json
POST /api/v1/scalerecord/start-session
{
  "cycleId": "{cycleId}"
}
```

Expected:
- returns `sessionId`;
- no scale/device realtime context is required yet.

2. Enable selected scale telemetry:

```json
POST /api/v1/scale/{scaleId}/live-weight/telemetry
{
  "enabled": true,
  "sessionId": "{sessionId}",
  "cycleId": "{cycleId}"
}
```

Expected:
- IoT direct method succeeds;
- active realtime context is set for `sessionId + scaleId + deviceId + cycleId`;
- later Code 503 telemetry resolves context from Redis.

3. Subscribe SignalR:

```text
SubscribeToScaleSession(sessionId)
SubscribeToScale(scaleId)
```

Expected:
- active weighing screen uses `ScaleSession_{sessionId}`;
- scale list/detail can use `Scale_{scaleId}`;
- initial cached snapshots are refreshed before push.

4. Confirm records:

Confirm uses:
- `sessionId`;
- `cycleId`;
- `scaleId`;
- `weight`;
- `deviceTimestamp`.

Confirm freshness stays stricter than display freshness.

5. Finish or discard:

Expected:
- active scale-record session is removed;
- realtime context is removed;
- session device index is removed;
- scale can be used by a new session.

## Test Plan

1. Start session with only `cycleId`.
2. Start again with same user/cycle before finish; active session should be reused/conflict with same session id.
3. Enable telemetry with `sessionId + cycleId`; Redis realtime context should be written.
4. Enable telemetry with a session owned by another user; API should still return 403 and no realtime context should be bound.
5. Send Code 503 telemetry; context source should be `RedisSessionContext`.
6. Send stale Code 503 telemetry; packet should be logged as stale-dropped and not broadcast.
7. Subscribe via `SubscribeToScaleSession` and `SubscribeToScale`; initial snapshots should have refreshed `FreshnessSeconds`, `State`, and `CanConfirm`.
8. Verify `net=0`, stable, no error returns `CanConfirm=false`.
9. Verify `net>=0.5`, stable, fresh, no error can return `CanConfirm=true`.
10. Rebind a device from session A to session B; latest device context should point only to session B.
11. Finish/discard; starting a new session for same user/cycle should succeed.
12. If scale still sends Code 503 after finish, scale-level live data may continue but session-live endpoint must not accept a fake or zero session id.

## Verification Commands

Builds are user-run by default:

```powershell
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

Guidance docs verification:

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-guidance.ps1
powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-skill-docs.ps1
```

## Remaining Risks

- If Redis is flushed or unavailable, active session context can be lost.
- Existing local Redis keys from older builds can confuse manual testing; clear old scale keys before verifying a fresh flow.
- No dedicated automated test project is currently visible in the solution.
- The empty-session guard follow-up is not fully implemented until `Guid.Empty` is normalized to `null`, zero-GUID session requests return 400, and telemetry-control success includes real session binding fields.

