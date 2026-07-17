# Device System Base Plan

Use this plan when analyzing, planning, or implementing telemetry monitoring and device activity audit upgrades for DeviceHub, Device, Code 300, DeviceTelemetryHistory, or DeviceLogs.

## Goals

- Separate packet telemetry history from device state-change audit.
- Keep Code 300 realtime status handling fast and predictable.
- Avoid writing high-volume telemetry through entity patterns that add unnecessary audit, concurrency, and soft-delete overhead.
- Preserve IoT routing contracts: `DeviceHub.DeviceId` identifies the physical hub in Azure IoT, and `Device.DeviceNumber` identifies the child device slot in Code 300 payloads.

## Implemented Status

### DeviceTelemetryHistory

Current implemented shape:

```csharp
public class DeviceTelemetryHistory
{
    public Guid Id { get; private set; }
    public Guid DeviceHubId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string Data { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    public DeviceHub DeviceHub { get; private set; } = null!;
}
```

Implemented rules:

- Does not inherit `BaseEntity`.
- `DeviceId` is intentionally the IoT gateway identity from `DeviceHub.DeviceId`, not the child device DB id.
- `DeviceHubId` is the DB FK to `DeviceHub`.
- `Data` stores the raw Code 300 packet JSON as `jsonb`.
- `Timestamp` stores payload `TimeStamp` converted to UTC.
- `ReceivedAt` stores backend receive time in UTC.
- Code 300 creates one history row per valid installed hub packet, not one row per child device.
- If the hub is unknown, uninstalled, deleted, or has no IoT `DeviceId`, no telemetry history row is written.

Legacy limitations this solved:

- The old `DeviceTelemetryHistory.DeviceId` pointed to child `Device.Id`, but Code 300 is a hub-level packet containing many child device rows.
- The old entity inherited `BaseEntity`, adding `No`, `ConcurrencyStamp`, soft-delete fields, global query filter, and default indexes that are not needed for append-only high-volume telemetry.
- The old shape did not store the telemetry timestamp from payload `TimeStamp`.
- The old shape did not store backend receive time, so UI could not measure packet delay or backend/Event Hub backlog.

### DeviceLogs

Current shape:

```csharp
public class DeviceLogs : BaseEntity
{
    public Guid DeviceId { get; private set; }
    public DeviceActionEnum? Action { get; private set; }
    public LogSourceEnum? Source { get; private set; }
    public string? Value { get; private set; }
    public Device? Device { get; private set; }
}
```

Limitations:

- Has no explicit old/new value fields, so UI cannot query state transitions cleanly.
- `Value` is too ambiguous for audit state changes.
- It is not currently the Code 300 state-change audit path.

## Target Model

### DeviceTelemetryHistory

Use as high-volume packet history for Code 300. Prefer hub-level rows, not one row per child device.

```csharp
public class DeviceTelemetryHistory
{
    public Guid Id { get; private set; }
    public Guid DeviceHubId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string Data { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    public DeviceHub DeviceHub { get; private set; } = null!;
}
```

Field rules:

- `DeviceHubId`: DB FK for the physical cabinet/hub that sent the Code 300 packet.
- `DeviceId`: IoT gateway id copied from `DeviceHub.DeviceId`; keep the name for API/schema continuity, but do not treat it as child `Device.Id`.
- `Data`: raw or normalized packet JSON, stored as `jsonb`.
- `Timestamp`: UTC time converted from payload `TimeStamp`.
- `ReceivedAt`: UTC backend receive time. Keep this to measure telemetry delay with `ReceivedAt - Timestamp`.
- Do not inherit `BaseEntity` unless there is a hard product requirement for soft delete or created-by auditing on each telemetry row.

Recommended indexes:

- `(DeviceHubId, Timestamp DESC)` for history by cabinet/hub.
- `(DeviceId, ReceivedAt)` for IoT gateway history.
- `Timestamp` for payload-time query.
- `ReceivedAt` for retention cleanup.
- `(DeviceHubId, ReceivedAt DESC)` for fast "latest rows by cabinet/hub" reads.
- `(ReceivedAt DESC)` for fast newest-first reads when hub scope is not provided.

Retention:

- Use hard delete by time window, similar to `SensorDataLog`.
- Prefer `ReceivedAt < UtcNow - RetentionDays` for cleanup because device payload time can be stale or wrong.
- If `RetentionDays <= 0`, skip telemetry cleanup.
- Do not soft delete telemetry history rows.

### DeviceLogs

Use as low-volume audit for state changes and control events. Keep `BaseEntity`.

```csharp
public class DeviceLogs : BaseEntity
{
    public Guid DeviceId { get; private set; }
    public DeviceActionEnum Action { get; private set; }
    public LogSourceEnum Source { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? Value { get; private set; }
    public DateTime? OccurredAt { get; private set; }

    public Device Device { get; private set; } = null!;
}
```

Field rules:

- `DeviceId`: child device being audited.
- `Action`: transition/event type, for example status changed, connection changed, toggle requested.
- `Source`: origin, for example user, device, system, scheduler, API.
- `OldValue` and `NewValue`: queryable state transition values such as `Off -> On` or `Connected -> DisConnected`.
- `Value`: optional message or compact metadata. Use `jsonb` only if UI needs structured metadata.
- `OccurredAt`: optional event time. Use it when the true event time differs from backend `CreatedAt`; otherwise `CreatedAt` is enough.

Keep `BaseEntity` because `DeviceLogs` is low-volume audit data, not raw telemetry. `CreatedAt`, `CreatedBy`, `No`, and soft-delete behavior can remain useful for audit/list UI and local conventions.

## Write Strategy

### Code 300 packet monitoring

Do not insert telemetry history directly in the hot status update path unless volume is proven low. Prefer:

```text
IoTConnectionManager / DeviceStatusTelemetryHandler
  -> update current DeviceHub/Device state
  -> enqueue DeviceTelemetryHistory through IDeviceTelemetryQueue.TryEnqueue
  -> if the telemetry queue is full, drop only the history row and log at interval
  -> DeviceTelemetryLoggingWorker batch inserts rows
  -> DataMaintenanceWorker deletes old rows by retention
```

Use one `DeviceTelemetryHistory` row per hub packet. Do not create one telemetry history row per child device unless a UI/API requirement needs direct child-device packet search at scale.

### Queue and backpressure

Use a dedicated telemetry queue abstraction instead of exposing `Channel<DeviceTelemetryHistory>` through shared DI:

```text
IDeviceTelemetryQueue
  -> DeviceTelemetryQueue
      -> bounded Channel<DeviceTelemetryHistory>
      -> QueueCapacity default 10000
      -> TryEnqueue only, no blocking in Code 300
      -> drop and interval-log when full
```

Settings:

```csharp
public bool Enabled { get; set; } = true;
public int BatchSize { get; set; } = 150;
public int FlushIntervalSeconds { get; set; } = 5;
public int QueueCapacity { get; set; } = 10000;
public int DropLogInterval { get; set; } = 100;
public int FlushRetryCount { get; set; } = 2;
public int FlushRetryDelayMilliseconds { get; set; } = 500;
public int RetentionDays { get; set; } = 30;
```

Worker rules:

- Use one consumer loop and one batch list.
- Flush when `BatchSize` is reached.
- Flush when `FlushIntervalSeconds` elapses.
- On service stop, drain all remaining queued rows across as many batches as needed.
- Retry DB flush lightly, then log and drop that batch if retries still fail.
- Never let history flush failures crash the worker or slow Code 300 status handling.

### Device state audit

Write `DeviceLogs` only when:

- a child device `Status` changes from telemetry,
- a child device `ConnectionStatus` changes,
- a user/API/scheduler command is sent or confirmed,
- background health sweep marks a hub/device disconnected.

Do not write `DeviceLogs` on every unchanged Code 300 packet.

## UI Mapping

### Telemetry monitor

Use `DeviceTelemetryHistory`.

Expected filters:

- hub/cabinet,
- IoT gateway id (`DeviceTelemetryHistory.DeviceId` / `DeviceHub.DeviceId`),
- pond/zone through joins from `DeviceHub`,
- time range using `Timestamp` or `ReceivedAt`,
- late packet detection using `ReceivedAt - Timestamp`.

Expected display:

- hub name,
- IoT `DeviceHub.DeviceId`,
- `Timestamp`,
- `ReceivedAt`,
- delay,
- payload detail.

API guardrails:

- `GET /api/v1/device-hubs/telemetry-history`.
- Uses `GetDeviceTelemetryHistoryQuery : IRequest<TResult<DeviceTelemetryHistoryPageDto>>`; do not assume the shared `PaginatedList<T>` contract for this endpoint.
- `includeTotalCount=false` is the default fast path. In this mode, do not run `CountAsync`; return `totalCount=null`, `totalPages=null`, `isTotalCountExact=false`, and rely on `hasNextPage`.
- If FE needs the exact total, it must send `includeTotalCount=true`; only then run the count query and return numeric `totalCount` / `totalPages`.
- Clamp `Page < 1` to `1`.
- Clamp `PageSize <= 0` to default `50`.
- Clamp `PageSize > 50` to `50`.
- Reject `StartTime > EndTime`.
- Supported sort values: `timestamp asc`, `timestamp desc`, `receivedAt asc`, `receivedAt desc`; default is `receivedAt desc`.
- Return `Data` as JSON object/array, not as an escaped JSON string.
- `DelayMilliseconds = ReceivedAt - Timestamp`.
- For `deviceHubId`, query history directly by `DeviceTelemetryHistory.DeviceHubId`.
- For `pondId` / `zoneId`, resolve matching `DeviceHub.Id` values first, then query history by `DeviceHubId IN (...)`; avoid a broad `Include(DeviceHub)` or navigation join on the main page query.
- Fetch hub metadata separately for the page items. If metadata is missing because of soft delete/global filter/FK drift, return the row with `hasWarning=true`, `warningCode="DEVICE_HUB_METADATA_UNAVAILABLE"`, and a warning message instead of failing the API.
- FE should use `hasNextPage` / `hasPreviousPage` for default paging and only request `includeTotalCount=true` when an exact total is required.

### Device activity monitor

Use `DeviceLogs`.

Expected filters:

- device,
- source,
- action,
- old/new value,
- time range.

Expected display:

- device name,
- action,
- source,
- old value,
- new value,
- occurred/created time,
- optional detail value.

## Non-Goals

- Do not route Code 101/300 using pond-device assignment data.
- Do not use `PondDeviceAssignment.UsageNo` as `DeviceNumber`.
- Do not store every unchanged child-device state as a device log.
- Do not use `DeviceLogs` as the raw Code 300 packet store.
- Do not block Code 300 on telemetry history persistence.
- Do not expose `Channel<DeviceTelemetryHistory>` directly as a shared DI primitive; use `IDeviceTelemetryQueue`.
- Do not add Redis key, SignalR group, or IoT protocol changes for this plan unless explicitly requested.

## Migration and deployment checks

Before applying `UpdateDeviceTelemetryHistoryForCode300` to an existing database:

- Count existing `Tbl_DeviceTelemetryHistory` rows.
- Count rows that map from old child `DeviceId` to installed `DeviceHub`.
- Count rows that will be deleted because they cannot map to a hub or because the hub has no IoT `DeviceId`.
- Backup/export old telemetry history if those rows still have business value.
- Generate and review the migration script before `database update`.
