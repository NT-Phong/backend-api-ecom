---
name: device-system
description: Use this skill when debugging, reviewing, planning, or changing Ecom DeviceHub, Device, DeviceControl, IoT Code 300 telemetry, device toggle/control, hub installation, connection status, device status, warning hardware, feeder schedule, or device assignment flows.
argument-hint: "Task focus: DeviceHub | DeviceControl | Code 300 | toggle | connection status | installation | schedule"
user-invocable: true
---

# Device System Skill

**Upgrade plan:** For telemetry monitoring and device activity audit optimization, read `BASE-PLAN.md` in this skill folder.

**Domain:** DeviceHub · Device · DeviceControl · IoT Telemetry (Code 300)  
**Purpose:** Help AI Agent quickly understand context, locate files accurately, and avoid broad scans when working with Device and DeviceHub tasks.

---

## 1. Mental Model

### Physical / IoT layer

```
Zone
 └── Pond (legacy/default pond for one DeviceHub)
      └── DeviceHub (physical gateway, connects to Azure IoT Hub)
           └── Device[] (child devices: AirBlower, Feeder, PaddleWheel, Syphon, WaterPump...)
```

**Key rules:**
- `DeviceHub -> Device` is the physical/IoT layer.
- `DeviceHub.PondId` remains the legacy/default pond relation and the one-hub-per-pond rule is still enforced by DeviceHub create/update.
- DeviceHub must have `IsInstalled = true` to have `DeviceId` (Azure IoT Hub Device ID) and `HostName`.
- Device belongs to Hub via `DeviceHubId` + `DeviceNumber` (physical port number on Hub, used as IoT slot ID).
- `IsInHub = true` → device must have `DeviceHubId` before being set to `Connected`.
- Camera and Scale devices do **not** go through Hub (`IsInHub = false`).

### Business assignment layer

```
Pond[] <-- Tbl_PondDeviceAssignment --> Device[]
```

**Key rules:**
- A pond can actively use many hub devices.
- A hub device can actively serve many ponds.
- `Tbl_PondDeviceAssignment` is the pond-facing source for "devices serving this pond".
- Legacy migration backfill only creates assignments for `AirBlower` devices; other hub device types must be assigned explicitly through API/user action.
- Active uniqueness is `(PondId, DeviceId)` where `IsDeleted = false AND IsActive = true`.
- `UsageNo` is optional business metadata for roles such as Oxy #1/#2; it is not the IoT slot and must not replace `DeviceNumber`.
- `GET /api/v1/devices?pondIds=...` should prefer active assignments and keep `DeviceHub.PondId` fallback for transition/backfill safety.
- `GET /api/v1/pond/{pondId}/devices` reads active assignments for one pond and supports `deviceType`, `deviceHubId`, `usageNo`, and `searchText` filters.
- `GET /api/v1/devices/{deviceId}/ponds` is the read API for listing active ponds that share a device.
- `DELETE /api/v1/pond/{pondId}/devices/{deviceId}` removes only the business assignment by deactivating the row; it must not delete the Device, clear `DeviceHubId`, or change `DeviceNumber`.
- Once a pond-device pair has an assignment row, that assignment state should win over legacy fallback; inactive assignment must not be pulled back into device lists by `DeviceHub.PondId`.
- Do not use the assignment table to route Code 101/300 IoT commands. Toggle and telemetry must continue using `DeviceHubId` + `DeviceNumber`.
- Pond-device read filters are FE/search helpers only; they must not alter IoT routing, control payloads, or telemetry reconciliation.
- Active assignments are valid only while the device is still a hub device. Removing a device from hub or deleting it must deactivate active assignments, not delete assignment history.
- Sync conflicts on the active assignment unique index should return a friendly 409-style failure instead of a generic 500.
- If assignment data grows large, replace the `assignedDeviceIds.Contains(...)` list filter in device list queries with an `EXISTS`/join-based query.

---

## 2. Core Status Enums

### ConnectionStatusEnum
| Value | Meaning |
|---|---|
| `UnDefined` | Not determined / not installed |
| `Connected` | Currently connected |
| `DisConnected` | Connection lost |

**Domain guards:**
- `DeviceHub.UpdateConnectionStatus()`: if `!IsInstalled` → always set `UnDefined`; if input is `UnDefined` → set `DisConnected`.
- `Device.UpdateConnectionStatus()`: if `IsInHub && DeviceHubId == null && status == Connected` → throws `InvalidOperationException`.

### StatusDeviceEnum (on/off state)
| Value | Meaning |
|---|---|
| `UnDefined` | Unknown |
| `On` | Running |
| `Off` | Stopped |
| `Fault` | Error / failure |

---

## 3. Telemetry Code 300 Flow (Core of the System)

```
Azure IoT Hub / Event Hub
  → IoTConnectionManager.ProcessTelemetryMessageAsync()
      → IDeviceStatusTelemetryHandler.HandleDeviceStatusTelemetryAsync(deviceHubId, message)
          → Find DeviceHub where DeviceHub.DeviceId == deviceHubId (IoT ID, not DB Id)
          → IDeviceHealthMonitor.RecordDevicePing(deviceHubId, hub.Healthcheckbysecond)
          → Load all child Devices by DeviceHubId
          ┌─ For each device PRESENT in payload:
          │    device.MarkAsConnectedFromTelemetry()   ← does NOT reset Status
          │    device.UpdateDeviceStatus(ResolveStatus(item))
          │    → If AirBlower: evaluate OxyAlert (dispatch HandleOxyStatusChangeCommand)
          └─ For each device ABSENT from payload:
               device.UpdateConnectionStatus(DisConnected)
               device.UpdateDeviceStatus(UnDefined)
          → SaveChangesAsync (only if changes exist)
```

**Bug-prone points:**
- `MarkAsConnectedFromTelemetry()` ≠ `UpdateConnectionStatus(Connected)` — the former does NOT reset Status; the latter may throw if Hub is missing.
- Payload uses `DeviceNumber` (int, port number) to match DB Device — **not** DB `Id` (Guid).
- If Hub not found or `!IsInstalled` → skip entire message, return 0.
- `DbUpdateConcurrencyException` → caught separately, log warning, return 0 (not rethrown).

### Code 300 telemetry history side path

After an installed `DeviceHub` is found and `RecordDevicePing` is called, Code 300 also enqueues exactly one hub-level `DeviceTelemetryHistory` row through `IDeviceTelemetryQueue.TryEnqueue`.

Field rules:
- `DeviceHubId` = `DeviceHub.Id`.
- `DeviceId` = `DeviceHub.DeviceId` (IoT gateway id), not child `Device.Id`.
- `Data` = raw Code 300 JSON payload.
- `Timestamp` = payload `TimeStamp` converted to UTC.
- `ReceivedAt` = backend receive time in UTC.

Operational rules:
- Telemetry history is a secondary audit/monitoring path; it must not block realtime Code 300 status handling.
- `DeviceTelemetryQueue` is bounded by `DeviceTelemetryLoggingSettings.QueueCapacity`.
- If the queue is full, drop only the history row and keep processing device status.
- `DeviceTelemetryLoggingWorker` is the single consumer; it batch inserts, flushes by batch size/interval, drains pending rows on stop, and retries DB flush lightly.
- `DataMaintenanceWorker` hard-deletes old `DeviceTelemetryHistory` rows using `ReceivedAt < UtcNow - RetentionDays`; `RetentionDays <= 0` skips telemetry cleanup.

### DeviceTelemetryHistory read API

`GET /api/v1/device-hubs/telemetry-history` uses a dedicated page DTO, not the shared `PaginatedList` contract:

```text
GetDeviceTelemetryHistoryQuery
  -> IRequest<TResult<DeviceTelemetryHistoryPageDto>>
```

Request/query rules:
- `includeTotalCount=false` by default. Do not run `CountAsync` unless FE explicitly sends `includeTotalCount=true`.
- When `includeTotalCount=false`, response has `totalCount=null`, `totalPages=null`, and `isTotalCountExact=false`.
- Use `hasNextPage` / `hasPreviousPage` for fast paging. Page fetch uses `Take(pageSize + 1)` to detect next page without count.
- Clamp `Page < 1` to `1`, `PageSize <= 0` to `50`, and `PageSize > 50` to `50`.
- Reject `StartTime > EndTime`.
- Supported sort values: `timestamp asc`, `timestamp desc`, `receivedAt asc`, `receivedAt desc`; default is `receivedAt desc`.
- Filter `deviceHubId` directly on `DeviceTelemetryHistory.DeviceHubId`.
- For `zoneId` or `pondId`, resolve matching `DeviceHub.Id` values first, then query history using `DeviceHubId IN (...)`; do not join broadly from history to hub for the page query.
- Do not use `.Include(x => x.DeviceHub)` in the main history query. Fetch hub metadata separately for the page's distinct `DeviceHubId` values.

Response mapping rules:
- `data` must be returned as JSON object/array, not an escaped JSON string.
- `delayMilliseconds = ReceivedAt - Timestamp`.
- If hub metadata cannot be loaded, still return the telemetry row and set:
  - `hasWarning=true`
  - `warningCode="DEVICE_HUB_METADATA_UNAVAILABLE"`
  - `warningMessage` explaining that telemetry data is still available but hub metadata is missing.
- FE should display telemetry data even when `hasWarning=true` and fall back to `hubIotDeviceId || deviceId` when `deviceHubName` is empty.

Read indexes:
- Keep `(DeviceHubId, ReceivedAt DESC)` for fast "latest 50 by hub" queries.
- Keep `(ReceivedAt DESC)` for latest global/zone/pond fallback reads.
- Existing `(DeviceHubId, Timestamp)`, `(DeviceId, ReceivedAt)`, and `Timestamp` indexes still support time and IoT-gateway filters.

---

## 4. Toggle Device Flow (Code 101)

```
HTTP POST /api/v1/devices/{id}/toggle
  → ToggleDeviceCommand
  → ToggleDeviceCommandHandler
      1. Acquire distributed lock: "Ecom:DeviceControl:Toggle:{deviceId}" (timeout 500ms)
         → Lock unavailable → Failure(DeviceToggleInProgress)
      2. Load Device + include DeviceHub + Pond.PondCategory
      3. Guard: device != null, device.DeviceHub != null
      4. Guard: DeviceHub.ConnectionStatus == Connected
         → Not connected → Failure(DeviceHubNotConnected)
      5. OxyDeviceControlRules.CanToggle() — AirBlower always ON, cannot be turned off
      6. OxyDeviceControlRules.IsFaultedConnectedNurseryBackupOxy() — faulted backup oxy
      7. Compute new deviceStatus: On ↔ Off
      8. Build payload: { Code: 101, TimeStamp, Data: { DeviceName, DeviceId(=DeviceNumber), Value(0/1), Message } }
      9. IoTHubService.InvokeDirectMethodAsync(hub.DeviceId, "Control", payload, 30s)
     10. If response.IsSuccess → device.UpdateStatus(deviceStatus) + UpdateAsync
     11. Write ActivityLogs (always, even on failure)
     12. SaveChangesAsync
     13. Handle error 404103 → Failure(DeviceHubNotConnected)
```

**Bug-prone points:**
- `device.UpdateStatus()` (not `UpdateDeviceStatus()`) — separate method on Device entity.
- Lock key uses `deviceId` (DB Guid), not IoT Device ID.
- Payload `DeviceId` = `device.DeviceNumber` (int), not DB `device.Id` (Guid).
- Error 404103 is checked in both `response.ErrorMessage` and `catch (Exception)`.
- `PondCategory` resolved from `device.DeviceHub.Pond` or `device.Pond` (fallback).
- **Unlike `InvokeDeviceMethodHandler`**, this handler does NOT update DB ConnectionStatus to DisConnected on 404103.

---

## 5. Test DeviceHub Connection Flow

```
HTTP POST /api/v1/device-hubs/{id}/test-connection
  → TestDeviceHubConnectionCommand
  → TestDeviceHubConnectionCommandHandler [EnableUnitOfWork]
      1. Load DeviceHub by Id
      2. Guard: IsInstalled == true, HostName != null, DeviceId != null
      3. IoTHubService.TestDeviceConnectionAsync(hub.DeviceId, hub.HostName)
      ┌─ isConnected = true:
      │    hub.UpdateConnectionStatus(Connected)
      │    IoTHubService.GetDeviceStatusListAsync() → List<int> of IoT slot IDs
      │    For each child Device:
      │      - No Twin list → set Connected (fallback, only Installed devices)
      │      - DeviceNumber in Twin list → Connected
      │      - DeviceNumber NOT in Twin list → UnDefined
      └─ isConnected = false:
           hub.UpdateConnectionStatus(DisConnected)
           Cascade: all child Devices → DisConnected
      → SaveChangesAsync
```

**Bug-prone points:**
- `GetDeviceStatusListAsync` returns `List<int>` (slot IDs), matched against `device.DeviceNumber`.
- Empty/null Twin list → fallback sets Connected for all `Installed` devices only.
- Returns `TResult<bool>` — disconnect path returns `Failure(...)`, not `Success(false)`.

---

## 6. InvokeDeviceMethod Flow (General Dispatcher)

```
HTTP POST /api/v1/device-control/invoke
  → InvokeDeviceMethodCommand { TargetId, TargetType, MethodName, Payload(JsonElement) }
  → InvokeDeviceMethodHandler [EnableUnitOfWork]
      1. Parse Code from Payload.Code (int)
      2. Resolve targetIotDeviceId by TargetType:
         - DeviceHub       → hub.DeviceId
         - WarningDeviceHub → warningHub.DeviceId
         - Scale           → scale.DeviceId
         - SensorDevice    → sensor.SensorTTSId
      3. If Code == 501 (OTA Update):
         - Find Document where FileName == Version
         - Generate SAS URL (15 min) from Azure Blob
         - Build payload { Code, TimeStamp, Data: { Version, Url } }
      4. Other codes: forward Data from FE as-is
      5. IoTHubService.InvokeDirectMethodAsync(targetIotDeviceId, MethodName, payload, 30s)
      6. Handle response:
         - IsSuccess → UpdateTargetConnectionStatus(Connected)
         - Status 404 / "404103" / "isn't online" → DisConnected + Failure(DeviceIotOffline)
         - Status 504 / "504101" → DisConnected + Failure(DeviceIotTimeout)
         - "unknown command code" → Failure(InvalidCode)
         - Parse JSON Message from device → Failure(DeviceReportedError)
      7. Scale: MapScaleSuccessResponse() by Code (503, 504, 502, 501)
```

**Bug-prone points:**
- `UpdateTargetConnectionStatusAsync` is a local function — only updates if status actually changed.
- Scale uses `scale.DeviceId` (string), not `DeviceNumber`.
- SensorDevice uses `sensor.SensorTTSId`, not `sensor.DeviceId`.
- 404103 checked via both `response.Status == 404` and string contains.

---

## 7. Background Health Sweep (IoTBackgroundService)

```
Timer (interval = DeviceHealthSettings.CheckIntervalSeconds)
  → IDeviceHealthMonitor.GetUnhealthyDevices()
  → DB Sweep: SensorDevices Connected in DB but missing from in-memory ping store
  → OnDeviceUnhealthyAsync(deviceId):
      - Find DeviceHub where DeviceHub.DeviceId == deviceId (IoT string ID)
      - If Hub found:
          hub.UpdateConnectionStatus(DisConnected)
          Cascade: all child Devices → DisConnected + Status = UnDefined
      - If Hub not found → try parse Guid → find SensorDevice
          sensor.UpdateConnectionStatus(DisConnected)
          or dispatch HandleSensorConnectionChangeCommand
```

**Bug-prone points:**
- `deviceId` in health monitor is **IoT string ID** for DeviceHub, but **DB Guid string** for SensorDevice.
- Child device cascade uses `DeviceHubId == hub.Id` (DB Guid), not IoT ID.
- Each `OnDeviceUnhealthyAsync` creates its own scope — does not share UnitOfWork with the main loop.

---

## 8. DeviceHealthMonitor (In-Memory Ping Tracking)

```csharp
// Singleton — thread-safe ConcurrentDictionary
RecordDevicePing(deviceId, timeoutSeconds?)
  → Stores (LastPingTime, TimeoutSeconds) keyed by deviceId (IoT string ID)

GetUnhealthyDevices()
  → Returns deviceIds where LastPingTime + TimeoutSeconds < UtcNow

IsDeviceHealthy(deviceId) → bool

GetAllDevicesHealth() → IEnumerable<DeviceHealthInfo>
```

**Notes:**
- `timeoutSeconds` comes from `DeviceHub.Healthcheckbysecond` (nullable int).
- If null → uses `DeviceHealthSettings.DefaultTimeoutSeconds`.
- Tracks DeviceHub (IoT string ID) and SensorDevice (DB Guid string) only.
- Does **not** track child Devices directly — their state is derived from Code 300 telemetry.

---

## 9. Key Domain Methods — Device Entity

| Method | Description | When to use |
|---|---|---|
| `MarkAsConnectedFromTelemetry()` | Set Connected, does NOT reset Status | Code 300 — device appears in payload |
| `UpdateConnectionStatus(status)` | Set ConnectionStatus with guard | Test connection, background sweep |
| `UpdateConnectionStatusAndClearDeviceStatus(status)` | Set ConnectionStatus + reset Status = UnDefined | Device physically unplugged |
| `UpdateDeviceStatus(status)` | Set Status (On/Off/Fault/UnDefined) | Telemetry, background sweep |
| `UpdateStatus(status)` | Set Status (no guard) | Toggle command after IoT confirms |
| `ConnectToHub(hubId, deviceNumber)` | Assign DeviceHubId + DeviceNumber | AddDeviceToHub command |
| `DisconnectFromHub()` | Clear hub, reset all fields | RemoveDeviceFromHub command |

---

## 10. File Map by Task

### Debug: toggle on/off failure
```
1. ToggleDeviceCommandHandler.cs          ← main logic
2. Device.cs (entity)                     ← UpdateStatus(), guards
3. DeviceHub.cs (entity)                  ← ConnectionStatus guard
4. IoTHubService.cs                       ← InvokeDirectMethodAsync
5. OxyDeviceControlRules (search with rg) ← CanToggle, IsFaultedConnectedNurseryBackupOxy
```

### Debug: wrong connection status after telemetry
```
1. DeviceStatusTelemetryHandler.cs        ← Code 300 handler
2. IoTConnectionManager.cs                ← entry point, telemetry routing
3. Device.cs (entity)                     ← MarkAsConnectedFromTelemetry vs UpdateConnectionStatus
4. DeviceHealthMonitor.cs                 ← ping tracking
```

### Debug: missing or delayed DeviceTelemetryHistory rows
```
1. DeviceStatusTelemetryHandler.cs        <- creates one hub-level history row after installed hub is found
2. IDeviceTelemetryQueue.cs               <- non-blocking queue contract
3. DeviceTelemetryQueue.cs                <- bounded queue, QueueCapacity, drop logging
4. DeviceTelemetryLoggingWorker.cs        <- batch insert, flush interval, stop-time drain, retry
5. DataMaintenanceWorker.cs               <- retention cleanup by ReceivedAt
6. DeviceTelemetryHistoryConfiguration.cs <- jsonb, FK, indexes
```

### Debug: test connection sets wrong child device status
```
1. TestDeviceHubConnectionCommandHandler.cs  ← main logic
2. IoTHubService.cs                          ← TestDeviceConnectionAsync, GetDeviceStatusListAsync
3. DeviceHub.cs (entity)                     ← UpdateConnectionStatus guard
4. Device.cs (entity)                        ← UpdateConnectionStatus
```

### Debug: background sweep sets wrong status
```
1. IoTBackgroundService.cs                ← OnDeviceUnhealthyAsync
2. DeviceHealthMonitor.cs                 ← GetUnhealthyDevices, IsDeviceHealthy
3. DeviceHub.cs (entity)                  ← UpdateConnectionStatus
4. Device.cs (entity)                     ← UpdateConnectionStatus, UpdateDeviceStatus
```

### Debug: InvokeDeviceMethod / OTA firmware
```
1. InvokeDeviceMethodHandler.cs           ← general dispatcher
2. IoTHubService.cs                       ← InvokeDirectMethodAsync
3. DeviceHub.cs / WarningDeviceHub.cs     ← UpdateConnectionStatus
```

---

## 11. Full File Index

### Application Layer
| File | Task |
|---|---|
| `Core/.../Features/DeviceControl/Commands/ToggleDevice/ToggleDeviceCommandHandler.cs` | Toggle on/off (Code 101) |
| `Core/.../Features/DeviceControl/Commands/ToggleDevice/ToggleDeviceCommand.cs` | Command model |
| `Core/.../Features/DeviceControl/Commands/ToggleDevice/ToggleDeviceValidator.cs` | Validation |
| `Core/.../Features/DeviceControl/Commands/InvokeDeviceMethod/InvokeDeviceMethodHandler.cs` | General dispatcher |
| `Core/.../Features/DeviceControl/Commands/InvokeDeviceMethod/InvokeDeviceMethodCommand.cs` | Command model |
| `Core/.../Features/DeviceControl/Commands/InvokeDeviceMethod/InvokeDeviceMethodValidator.cs` | Validation |
| `Core/.../Features/DeviceControl/Queries/GetDeviceHealth/GetDeviceHealthQueryHandler.cs` | Query single device health |
| `Core/.../Features/DeviceControl/Queries/GetIoTHealth/GetIoTHealthQueryHandler.cs` | Query full IoT health |
| `Core/.../Features/DeviceControl/Queries/GetSchedule/GetScheduleQueryHandler.cs` | Query schedule |
| `Core/.../Features/DeviceControl/Commands/ScheduleDevice/ScheduleDeviceCommandHandler.cs` | Create schedule |
| `Core/.../Features/DeviceControl/Commands/DeleteSchedule/DeleteScheduleCommandHandler.cs` | Delete schedule |
| `Core/.../Features/DeviceControl/Commands/AddDeviceToHub/AddDeviceToHubCommandHandler.cs` | Add device to hub |
| `Core/.../Features/DeviceControl/Commands/AssignDeviceToHub/AssignDeviceToHubCommandHandler.cs` | Assign device to hub |
| `Core/.../Features/DeviceControl/Commands/RemoveDeviceFromHub/RemoveDeviceFromHubCommandHandler.cs` | Remove device from hub |
| `Core/.../Features/DeviceControl/Commands/UpdateFirmware/UpdateFirmwareCommandHandler.cs` | Update firmware |
| `Core/.../Features/DeviceControl/Commands/UploadFirmware/UploadFirmwareCommandHandler.cs` | Upload firmware |
| `Core/.../Features/DeviceControl/DeviceControlDtos.cs` | Shared DTOs |
| `Core/.../Features/DeviceHub/Commands/TestDeviceHubConnection/TestDeviceHubConnectionCommandHandler.cs` | Test hub connection |
| `Core/.../Features/DeviceHub/Commands/CreateDevicehub/CreateDevicehubCommandHandler.cs` | Create hub |
| `Core/.../Features/DeviceHub/Commands/UpdateDeviceHub/UpdateDeviceHubCommandHandler.cs` | Update hub |
| `Core/.../Features/DeviceHub/Commands/DeleteDevicehub/DeleteDevicehubCommandHandler.cs` | Delete hub |
| `Core/.../Features/DeviceHub/Queries/GetDeviceHubDetail/GetDeviceHubDetailQueryHandler.cs` | Hub detail |
| `Core/.../Features/DeviceHub/Queries/GetDevicehubs/GetDevicehubsQueryHandler.cs` | Hub list |
| `Core/.../Features/Device/Commands/CreateDevice/CreateDeviceCommandHandler.cs` | Create device |
| `Core/.../Features/Device/Commands/UpdateDevice/UpdateDeviceCommandHandler.cs` | Update device |
| `Core/.../Features/Device/Commands/DeleteDevice/DeleteDeviceCommandHandler.cs` | Delete device |
| `Core/.../Features/Device/Queries/GetDeviceDetail/GetDeviceDetailQueryHandler.cs` | Device detail |
| `Core/.../Features/Device/Queries/GetDevices/GetDevicesHandler.cs` | Device list |
| `Core/.../Features/Pond/Devices/Commands/*` | Pond-facing device assignment mutations |
| `Core/.../Features/Pond/Devices/Queries/*` | Pond-facing device assignment reads |
| `Core/.../Features/Pond/Devices/Dtos/*` | Pond-device assignment DTOs |
| `Core/.../Common/Interfaces/IIoTServices.cs` | IIoTHubService, IIoTConnectionManager, IDeviceHealthMonitor |
| `Core/.../Common/Interfaces/IDeviceStatusTelemetryHandler.cs` | Code 300 handler interface |
| `Core/.../Common/Interfaces/IDeviceTelemetryQueue.cs` | DeviceTelemetryHistory non-blocking queue contract |
| `Core/.../Common/Configuration/DeviceTelemetryLoggingSettings.cs` | DeviceTelemetryHistory queue, batch, retry, retention settings |
| `Core/.../Features/DeviceHub/Queries/GetDeviceTelemetryHistory/*` | Telemetry history query API models and handler |

### Domain Layer
| File | Task |
|---|---|
| `Core/Ecom.Domain/Entities/IOT/DeviceHub.cs` | DeviceHub entity + domain methods |
| `Core/Ecom.Domain/Entities/IOT/Device.cs` | Device entity + domain methods |
| `Core/Ecom.Domain/Entities/IOT/PondDeviceAssignment.cs` | Business mapping Pond <-> Device |
| `Core/Ecom.Domain/Entities/IOT/DeviceHubSession.cs` | Hub connection session |

### Infrastructure Layer
| File | Task |
|---|---|
| `Infrastructure/.../IoT/DeviceStatusTelemetryHandler.cs` | **Code 300** — device status telemetry |
| `Infrastructure/.../IoT/DeviceTelemetryQueue.cs` | Bounded non-blocking queue for DeviceTelemetryHistory |
| `Infrastructure/.../IoT/DeviceTelemetryLoggingWorker.cs` | Batch insert worker for DeviceTelemetryHistory |
| `Infrastructure/.../IoT/DataMaintenanceWorker.cs` | Retention cleanup for SensorDataLog and DeviceTelemetryHistory |
| `Infrastructure/.../IoT/IoTConnectionManager.cs` | Event Hub entry point, telemetry routing |
| `Infrastructure/.../IoT/IoTBackgroundService.cs` | Background health sweep |
| `Infrastructure/.../IoT/DeviceHealthMonitor.cs` | In-memory ping tracking |
| `Infrastructure/.../IoT/IoTHubService.cs` | Azure IoT Hub client |

### Presentation Layer
| File | Task |
|---|---|
| `Presentation/.../Controllers/V1/DevicehubController.cs` | HTTP endpoints for DeviceHub |
| `Presentation/.../Controllers/V1/DeviceController.cs` | HTTP endpoints for Device + DeviceControl |

### EF Configurations
| File | Entity |
|---|---|
| `Infrastructure/.../Configurations/IOT/DeviceHubConfiguration.cs` | DeviceHub (Healthcheckbysecond, ConnectionStatus) |
| `Infrastructure/.../Configurations/IOT/DeviceConfiguration.cs` | Device (ConnectionStatus, Status) |
| `Infrastructure/.../Configurations/IOT/PondDeviceAssignmentConfiguration.cs` | Pond <-> Device assignment |
| `Infrastructure/.../Configurations/IOT/DeviceHubSessionConfiguration.cs` | DeviceHubSession (IsConnected) |

---

## 12. Common Bugs & How to Find Them

### Device still Connected after Hub goes offline
- Check `IoTBackgroundService.OnDeviceUnhealthyAsync` — did cascade run?
- Check `DeviceHealthMonitor.RecordDevicePing` — called from `DeviceStatusTelemetryHandler`?
- Check `DeviceHub.Healthcheckbysecond` — null uses default timeout.
- Search: `rg "OnDeviceUnhealthyAsync|RecordDevicePing" Infrastructure`

### Toggle fails with 404103
- Hub `ConnectionStatus` in DB may be stale (Connected in DB but offline in reality).
- `ToggleDeviceCommandHandler` only checks DB status, does not ping.
- On 404103, handler returns `Failure(DeviceHubNotConnected)` but does **not** update DB ConnectionStatus.
- `InvokeDeviceMethodHandler` does update DisConnected on 404103 — key difference.

### Device status out of sync after telemetry
- Check `DeviceNumber` in payload matches DB `Device.DeviceNumber`.
- `MarkAsConnectedFromTelemetry()` does not reset Status — if device was `Fault`, it stays `Fault` until `ResolveStatus` returns a new value.
- `DbUpdateConcurrencyException` → handler catches and returns 0, no rethrow — status may not be saved.

### Test connection sets wrong child device status
- Check what `GetDeviceStatusListAsync` returns — null/empty triggers fallback (set all Installed → Connected).
- `DeviceNumber` must match IoT slot ID (int) in Twin list.
- Devices with `InstallationStatus != Installed` are skipped in fallback path.

### Hub not found in telemetry
- `DeviceStatusTelemetryHandler` finds Hub by `DeviceHub.DeviceId == deviceHubId` (IoT string ID).
- Hub skipped if `!IsInstalled` or `IsDeleted`.
- Search: `rg "DeviceId.*deviceHubId\|deviceHubId.*DeviceId" Infrastructure/Ecom.Infrastructure/IoT`

---

### DeviceTelemetryHistory grows too fast or is missing rows
- Check `DeviceTelemetryLoggingSettings.QueueCapacity`; queue full drops history but keeps Code 300 realtime status handling.
- Check `DeviceTelemetryQueue` drop logs; logging is interval-based via `DropLogInterval`.
- Check `DeviceTelemetryLoggingWorker` flush logs and retry settings (`FlushRetryCount`, `FlushRetryDelayMilliseconds`).
- Check `DataMaintenanceWorker` retention cleanup; telemetry history cleanup uses `ReceivedAt`, not payload `Timestamp`.
- `RetentionDays <= 0` disables telemetry history cleanup.

---

## 13. Search Recipes

```powershell
# All places that set ConnectionStatus for DeviceHub
rg "UpdateConnectionStatus" Core/Ecom.Application/Features/DeviceHub Infrastructure/Ecom.Infrastructure/IoT

# All places that set ConnectionStatus for Device
rg "UpdateConnectionStatus|MarkAsConnectedFromTelemetry" Core/Ecom.Domain/Entities/IOT/Device.cs Infrastructure/Ecom.Infrastructure/IoT

# Code 300 telemetry flow
rg "Code 300|DeviceStatusTelemetry|HandleDeviceStatusTelemetry" Infrastructure/Ecom.Infrastructure

# DeviceTelemetryHistory queue, worker, retention, API
rg "DeviceTelemetryHistory|DeviceTelemetryQueue|DeviceTelemetryLoggingWorker|DeviceTelemetryLoggingSettings|GetDeviceTelemetryHistory" Core Infrastructure Presentation

# Distributed lock for toggle
rg "DeviceControl:Toggle|ToggleLock" Core/Ecom.Application/Features/DeviceControl

# OxyDeviceControlRules
rg "OxyDeviceControlRules|CanToggle|IsFaultedConnected" Core/Ecom.Application

# Cascade disconnect
rg "DeviceHubId.*hub.Id\|childDevice.*DisConnected" Infrastructure/Ecom.Infrastructure/IoT

# Health ping
rg "RecordDevicePing|Healthcheckbysecond" Infrastructure/Ecom.Infrastructure/IoT

# 404103 error handling
rg "404103" Core/Ecom.Application/Features/DeviceControl Infrastructure/Ecom.Infrastructure/IoT
```

---

## 14. Risk Areas — Do Not Change Without Explicit Request

- `DeviceHub.DeviceId` (IoT string ID) — used as key in health monitor and telemetry routing.
- `Device.DeviceNumber` (int) — used as IoT slot ID, matched against telemetry payload.
- `PondDeviceAssignment.UsageNo` — business display/role metadata only; never use it as `DeviceNumber`.
- `DeviceHub.PondId` unique/default behavior — keep for legacy fallback unless a schema migration explicitly replaces it.
- `ConnectionStatusEnum` values — contract with frontend and IoT devices.
- Distributed lock key format: `"Ecom:DeviceControl:Toggle:{deviceId}"`.
- `OxyDeviceControlRules` — business rules protecting oxygen devices, do not alter logic.
- `MarkAsConnectedFromTelemetry()` vs `UpdateConnectionStatus()` — different semantics, not interchangeable.
### DeviceTelemetryHistory risk notes
- `DeviceTelemetryHistory.DeviceId` is the IoT gateway id from `DeviceHub.DeviceId`, not child `Device.Id`.
- `IDeviceTelemetryQueue.TryEnqueue` must remain non-blocking for Code 300; queue-full behavior is drop history, not delay realtime status handling.
- Do not expose `Channel<DeviceTelemetryHistory>` directly as a shared DI primitive; keep queue behavior inside `DeviceTelemetryQueue`.
- Do not change telemetry retention back to payload `Timestamp` unless business explicitly wants device-time retention instead of backend-received-time retention.

