# Scale Realtime Architecture

Updated: 2026-07-10

## Goal

Scale realtime must let mobile show stable live weight without flickering `Disconnected`, while keeping confirm safe and preventing one physical scale from being attached to several active sessions.

The current architecture separates two concepts:

```text
Scale-record session: business session opened by user for a cycle.
Scale realtime context: runtime binding between session, scale, and IoT device.
```

This separation is important because mobile opens the session before it selects a scale.
One scale-record session may bind multiple scales/devices over its lifetime, but each
physical scale/device must have only one active realtime session binding at a time.

## Layer Structure

### Presentation Layer

Main responsibilities:
- expose REST endpoints;
- expose SignalR subscription methods;
- broadcast telemetry to client groups.

Important files:
- `Presentation/Ecom.API/Controllers/V1/ScaleRecordController.cs`
- `Presentation/Ecom.API/Controllers/V1/ScaleController.cs`
- `Presentation/Ecom.API/Hubs/TelemetryHub.cs`
- `Presentation/Ecom.API/Services/TelemetryNotificationService.cs`

Important endpoints:
- `POST /api/v1/scalerecord/start-session`
- `POST /api/v1/scalerecord/finish-session`
- `POST /api/v1/scalerecord/discard`
- `POST /api/v1/scalerecord/confirm`
- `GET /api/v1/scale/{scaleId}/live-weight`
- `POST /api/v1/scale/{scaleId}/live-weight/telemetry`

Important SignalR methods:
- `SubscribeToScaleSession(string sessionId)`
- `SubscribeToScale(string scaleId)`
- `SubscribeToCycle(string cycleId)`
- `SubscribeToDevice(string deviceId)`

### Application Layer

Main responsibilities:
- validate command intent;
- enforce business rules;
- coordinate UnitOfWork and cache interfaces;
- keep controllers/hubs thin.

Important files:
- `Core/Ecom.Application/Common/Interfaces/IScaleCacheService.cs`
- `Core/Ecom.Application/Common/Interfaces/SignalR/ITelemetryNotificationService.cs`
- `Core/Ecom.Application/Features/Scale/Common/ScaleMeasurementSnapshot.cs`
- `Core/Ecom.Application/Features/Scale/Common/ScaleRecordSessionContext.cs`
- `Core/Ecom.Application/Features/Scale/Common/ScaleSessionRealtimeContext.cs`
- `Core/Ecom.Application/Features/Scale/Commands/ControlScaleLiveTelemetry/ControlScaleLiveTelemetryCommandHandler.cs`
- `Core/Ecom.Application/Features/Scale/Queries/GetScaleLiveWeight/GetScaleLiveWeightQuery.cs`
- `Core/Ecom.Application/Features/Scale/Queries/GetScales/GetScalesQueryHandler.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/StartScaleSession/StartScaleSessionCommandHandler.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/ConfirmScaleRecord/ConfirmScaleRecordCommandHandler.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/FinishScaleSession/FinishScaleSessionCommandHandler.cs`
- `Core/Ecom.Application/Features/ScaleRecord/Commands/DiscardScaleSession/DiscardScaleSessionCommandHandler.cs`

### Infrastructure Layer

Main responsibilities:
- Redis cache implementation;
- IoT/EventHub ingestion;
- direct method control;
- telemetry parsing and hot path processing.

Important files:
- `Infrastructure/Ecom.Infrastructure/Caching/ScaleCacheService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/EventHubConsumerService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/IoTConnectionManager.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/Handlers/ScaleMeasurementTelemetryHandler.cs`

## Current Domain Flow

### 1. Start Business Session

Request:

```json
POST /api/v1/scalerecord/start-session
{
  "cycleId": "{cycleId}"
}
```

Backend behavior:
- validates current user and cycle;
- checks Redis active scale-record session by `UserId + CycleId`;
- reuses active session if present;
- otherwise creates a new `SessionId`;
- writes `ScaleRecordSessionContext`;
- does not resolve scale;
- does not resolve IoT device;
- does not write `ScaleSessionRealtimeContext`.

Why:
- at this moment mobile has not selected a scale.

### 2. Select Scale And Enable Realtime

Request:

```json
POST /api/v1/scale/{scaleId}/live-weight/telemetry
{
  "enabled": true,
  "sessionId": "{sessionId}",
  "cycleId": "{cycleId}"
}
```

Backend behavior:
- resolves route `scaleId`;
- validates device config;
- validates scale is not occupied by another user/cycle;
- validates supplied session is active for current user/cycle;
- invokes IoT direct method `GetWeight`;
- writes `ScaleSessionRealtimeContext`;
- uses device-level active context replacement to prevent stale session bindings.
- registers the device in the session device index so one session can track multiple scales.

### 3. Receive IoT Code 503 Telemetry

Telemetry shape seen in logs:

```json
{
  "payload": {
    "HostName": "dev-iot-hub-1.azure-devices.net",
    "DeviceId": "dev-weight-scale-01",
    "Code": 503,
    "TimeStamp": 1778748808,
    "Weight": {
      "Gross": 5,
      "Net": 5,
      "Unit": "kg",
      "Stable": true,
      "Overload": false,
      "Underload": false,
      "TareEntered": false,
      "Error": false
    }
  }
}
```

Backend hot path:

```text
EventHubConsumerService
-> IoTConnectionManager
-> ScaleMeasurementTelemetryHandler
-> ScaleCacheService.TrySetLatestSnapshotAsync
-> ScaleRealtimeDispatcher.TryEnqueueSnapshot
```

Downstream realtime delivery is intentionally outside the per-device ingestion lock:

```text
ScaleRealtimeDispatcher (latest pending snapshot per device)
-> refresh derived snapshot state
-> TelemetryNotificationService.BroadcastScaleDataAsync
-> SignalR groups
```

The dispatcher sends at most once every 250ms per device, awaits one broadcast at a time per device, and replaces pending values with the newest accepted snapshot. Connection-status DB sync runs in a separate 30s-throttled loop.

Normal context source must be:

```text
Source="RedisSessionContext"
```

### 4. Broadcast To SignalR

Broadcast groups:
- `ScaleSession_{sessionId}`
- `Scale_{scaleId}`
- `Cycle_{cycleId}`
- `Device_{deviceId}`

Client event:

```text
ReceiveScaleData(string deviceId, ScaleMeasurementSnapshot payload)
```

Session screen should prefer:

```text
SubscribeToScaleSession(sessionId)
```

Scale list/detail can use:

```text
SubscribeToScale(scaleId)
```

### 5. Read REST Live Weight

Request:

```text
GET /api/v1/scale/{scaleId}/live-weight
```

Behavior:
- resolves `scaleId -> deviceId`;
- reads latest Redis snapshot by device;
- overlays active realtime context if available;
- display freshness uses 15s;
- if no fresh snapshot exists, response can be `Disconnected`.

Confirm safety is separate and stricter than display freshness.
`ServerReceivedAtUtc` is the source of truth for freshness; do not use firmware device time as the primary freshness clock unless firmware clock trust has been separately proven.

### 6. Confirm Batch

Confirm uses:
- `sessionId`
- `cycleId`
- `scaleId`
- `weight`
- `deviceTimestamp`

Confirm handler can refresh realtime context for the confirmed scale/device, but it is not the first-choice bind point. The first-choice bind point is live telemetry control.

### 7. Finish Or Discard

Finish/discard cleanup must remove:
- active scale-record session context;
- realtime session context;
- realtime device context;
- session device index;
- latest snapshot when telemetry is stopped.

This allows the same cycle/user to start a new session after finish without false `ALREADY_EXISTS`.

## Redis Keys

```text
Ecom:Scale:Live:{deviceId}
Ecom:ScaleRecord:ActiveSession:User:{userId}:Cycle:{cycleId}
Ecom:ScaleRecord:Session:{sessionId}:Context
Ecom:Scale:Realtime:Session:{sessionId}:Context
Ecom:Scale:Realtime:Device:{deviceId}:Context
Ecom:Scale:Session:{sessionId}:Devices
Ecom:Scale:Mapping:{scaleId}
```

TTL expectations:
- latest snapshot: 30s;
- active session/context/session index: roughly one active weighing work period, currently 8h;
- scale mapping: short mapping cache, currently around 30min.

## Core Data Types

### `ScaleRecordSessionContext`

Business active-session context:
- `SessionId`
- `CycleId`
- `UserId`
- `StartedAtUtc`

Used before scale selection.

### `ScaleSessionRealtimeContext`

Runtime realtime context:
- `SessionId`
- `ScaleId`
- `DeviceId`
- `CycleId`
- `UserId`
- `StartedAtUtc`

Used after scale selection and telemetry enable.

### `ScaleMeasurementSnapshot`

Latest displayed telemetry:
- device/session/cycle/scale identity;
- gross/net/unit/stability flags;
- server/device timestamps;
- state and confirm/display freshness.

`State`, `CanConfirm`, and `FreshnessSeconds` are derived fields. Cached snapshots must be refreshed before SignalR initial pushes and telemetry broadcasts.

## Debug Flow

Expected healthy logs:

```text
[ScaleRecordSession] active session created/reused
[ControlScaleLiveTelemetry] Scale=..., Device=..., Enabled=True
[ScaleSessionRealtime] active context set
[EventHubConsumer] Message received
[IoTConnectionManager] Scale Live Payload (503)
[ScaleMeasurementTelemetry] Context resolved ... Source="RedisSessionContext"
[ScaleCacheService] Snapshot updated
[TelemetryNotification] Broadcast scale data ... Groups="ScaleSession_..., Device_..., Scale_..., Cycle_..."
[IoTConnectionManager] Dropped ... scale packets ...
```

If mobile still shows disconnected:
1. Check whether Code 503 telemetry is arriving.
2. Check whether context source is `RedisSessionContext`.
3. Check whether snapshot is updated within 30s and whether pushed snapshots refresh derived fields.
4. Check whether broadcast groups include the group mobile subscribed to.
5. Check whether mobile subscribed after context was bound.
6. Check REST `/live-weight` only as fallback evidence, not as proof SignalR is working.

