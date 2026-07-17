# Device Status Toggle Incident Report

Date: 2026-06-23
Scope: DeviceHub, child Device toggle, IoT Code 101, IoT telemetry Code 300, PaddleWheel mapping

## 1. Executive Summary

Qua cac lan kiem tra log va source, he thong da gap nhieu nhom su co khac nhau nhung deu bieu hien tren UI la "status nhay loan" hoac "bam thiet bi nay nhung thiet bi khac thay doi".

Ket luan hien tai:

- Toggle API khong tu y doi sang device khac. `ToggleDeviceCommandHandler` load device bang DB `Id`, sau do gui IoT `DeviceId = device.DeviceNumber`.
- Code 300 telemetry la nguon sync trang thai DB sau toggle. Neu Code 300 cu hoac mapping sai, UI co the thay status doi nguoc lai.
- Da them pending-state guard 30 giay de chan Code 300 stale ghi de trang thai toggle vua thanh cong.
- Mot so log moi cho thay pending guard hoat dong, vi co `Telemetry-Confirmed` va khong thay `Telemetry-Overwrite` trong case duoc doc.
- Rieng case PaddleWheel A8 moi nhat: FE goi dung DB Id cua "Quat nuoc 3 A8", DB device nay co `DeviceNumber = 13`, nen backend phai gui Code 101 `DeviceId = 13`. Neu quat vat ly so 4 bat, kha nang cao nam o mapping relay/firmware/wiring hoac DB name/deviceNumber khong phan anh dung vat ly.

## 2. Source Flow

### 2.1 Toggle Code 101

File:

- `Core/Ecom.Application/Features/DeviceControl/Commands/ToggleDevice/ToggleDeviceCommandHandler.cs`

Flow chinh:

1. FE goi `PATCH /api/v1/device/{deviceDbId}/toggle`.
2. Backend load `Device` bang DB `Id`.
3. Backend tinh target status `On <-> Off`.
4. Backend gui direct method `Control`, Code `101`.
5. Payload IoT dung `DeviceId = device.DeviceNumber`.
6. Neu IoT tra `response.IsSuccess == true`, backend update DB status cua dung device dang toggle.
7. Backend set pending state trong distributed cache de telemetry Code 300 khong ghi de stale trong 30 giay.

Source point quan trong:

```csharp
DeviceId = device.DeviceNumber
```

Dieu nay co nghia backend khong dung UI index, khong dung device name, va khong tu map `13 -> 14`.

### 2.2 Telemetry Code 300

File:

- `Infrastructure/Ecom.Infrastructure/IoT/DeviceStatusTelemetryHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/IoTConnectionManager.cs`

Flow chinh:

1. IoT gui Code `300` dinh ky khoang 10 giay/lần.
2. Backend tim DeviceHub bang `DeviceHub.DeviceId == deviceHubId` tu IoT.
3. Backend load child devices bang `DeviceHubId`.
4. Moi item trong payload duoc match bang:

```csharp
device.DeviceNumber == statusItem.DeviceId
```

5. Neu match, backend update connection/status theo payload.
6. Neu khong match, backend log `Telemetry-Unmatched` va bo qua item do.
7. Neu DB device khong xuat hien trong payload, backend co logic set missing device thanh `DisConnected + UnDefined`.

## 3. Incident Timeline va Findings

### 3.1 Initial Issue: status nhay loan sau toggle

Observed response:

```json
{
  "success": true,
  "data": {
    "success": true,
    "status": 200,
    "payload": "{\"Code\":101,\"TimeStamp\":1781859179,\"Message\":\"DeviceName: Máy cho ăn 2 A6, DeviceId: 32, Turn OFF\"}"
  }
}
```

Danh gia:

- `OPTIONS 200 OK` chi la CORS preflight, khong gui lenh IoT.
- `PATCH 200 OK` moi la request thuc su.
- Response Code 101 cho thay backend gui lenh cho dung mot device, `DeviceId = 32`.
- Toggle handler chi update device dang toggle, khong co doan nao update status cac may khac.

Nghi van ban dau:

- Code 300 telemetry tu tu dien co the ghi de trang thai vua toggle.
- Neu Code 300 la partial payload nhung backend xu ly nhu full snapshot, cac device thieu trong payload se bi set `DisConnected + UnDefined`.

### 3.2 Fix Attempt: pending-state guard cho stale Code 300

Da them model/service pending state:

- `DeviceTogglePendingState`
- `IDeviceTogglePendingStateService`
- `DeviceTogglePendingStateService`

Cache key:

```text
device:toggle_pending:{hubIotId}:{deviceNumber}
```

TTL:

```text
30 seconds
```

Behavior:

- Khi toggle Code 101 thanh cong, backend set pending target status theo device.
- Khi Code 300 den:
  - Neu telemetry status bang target status: clear pending va xu ly binh thuong.
  - Neu telemetry status khac target status trong thoi gian pending: skip update status de tranh stale overwrite.

Log quan sat da them:

```text
[DeviceStatusRace][Toggle-Start]
[DeviceStatusRace][Toggle-Response]
[DeviceStatusRace][Toggle-PendingSet]
[DeviceStatusRace][Toggle-DbPending]
[DeviceStatusRace][Toggle-Saved]
[DeviceStatusRace][Telemetry-Start]
[DeviceStatusRace][Telemetry-DbSnapshot]
[DeviceStatusRace][Telemetry-PendingSnapshot]
[DeviceStatusRace][Telemetry-Unmatched]
[DeviceStatusRace][Telemetry-Match]
[DeviceStatusRace][Telemetry-Confirmed]
[DeviceStatusRace][Telemetry-StaleSkipped]
[DeviceStatusRace][Telemetry-Overwrite]
[DeviceStatusRace][Telemetry-MissingConnection]
[DeviceStatusRace][Telemetry-MissingStatusClear]
[DeviceStatusRace][Telemetry-EventHub]
```

### 3.3 Oxy toggle constraint

Su co rieng:

- Can cho phep tat may oxy so 1, khong chan luong bat/tat may oxy nua.

Thay doi da thuc hien trong session:

- Go bo cac blocker Oxy-specific trong `ToggleDeviceCommandHandler`.
- Go bo force-on Oxy #1 trong `UpdateDeviceCommandHandler`.
- Go bo helper khong con dung trong `DeviceControlDtos.cs`.

Note:

- Day la thay doi business rule rieng, khong phai root cause cua PaddleWheel status jumping.
- Oxy alert telemetry van co the danh gia Oxy Off la failure theo rule canh bao.

### 3.4 Case log `test-devices-2`: missing `DeviceNumber = 12`

Log da doc:

```text
HubIotId="test-devices-2"
DbDeviceNumbers="11,13,14,21,22,31,41"
PayloadDeviceNumbers="11,12,13,14,21,22,31,32,41,42"
```

Repeated unmatched:

```text
[DeviceStatusRace][Telemetry-Unmatched]
PayloadDeviceId=12
PayloadDeviceName="PaddleWheel_2"
PayloadStatus="OFF"
DbDeviceNumbers="11,13,14,21,22,31,41"
```

Danh gia tai thoi diem do:

- IoT bao co `PaddleWheel_2 / DeviceId=12`.
- DB hub khong co child device `DeviceNumber=12`.
- Backend bo qua telemetry cua slot 12.
- Neu FE render danh sach theo index thay vi `DeviceNumber`, nut "Quat 2/3/4" co the bi lech cam giac.

Sau do user cung cap DB moi cho A8 da co du:

```text
11 = Quat nuoc 1 A8
12 = Quat nuoc 2 A8
13 = Quat nuoc 3 A8
14 = Quat nuoc 4 A8
```

Nen ket luan "thieu DeviceNumber=12" chi dung cho log cu / hub cu, khong con du de giai thich case A8 moi nhat.

### 3.5 Case FE sent wrong ID

User cung cap request:

```text
PATCH /api/v1/device/64d2e37d-298f-4d16-a0a1-126bc1a76da5/toggle
```

DB data:

```text
64d2e37d-298f-4d16-a0a1-126bc1a76da5
DeviceNumber = 14
Name = Quat nuoc 4 A8
```

Ket luan:

- Request nay la toggle "Quat nuoc 4 A8", khong phai "Quat nuoc 3 A8".
- Neu UI hien thi dang bam "Quat nuoc 3 A8" nhung request gui Id cua quat 4, root cause nam o FE binding/render mapping.
- Backend xu ly dung request nhan duoc.

### 3.6 Case FE sent correct ID but physical fan 4 turns on

User cung cap request moi:

```text
PATCH /api/v1/device/dcf8a6b7-4ddb-4cd4-b749-9f7eec373fa8/toggle
```

DB data:

```text
dcf8a6b7-4ddb-4cd4-b749-9f7eec373fa8
DeviceNumber = 13
Name = Quat nuoc 3 A8
Status = On
```

Theo source backend:

- Backend load device bang Id `dcf8a6b7-4ddb-4cd4-b749-9f7eec373fa8`.
- Backend gui Code 101 voi `DeviceId = 13`.
- Backend khong co logic nao doi `DeviceNumber 13` thanh `14`.

Neu thuc te "Quat nuoc 4 A8" bat/tat:

1. Neu response Code 101 payload la `DeviceId: 13`, backend da gui dung contract.
2. Neu vat ly quat 4 bat, kha nang cao:
   - firmware/relay mapping `DeviceId=13` dang dieu khien relay vat ly cua quat 4;
   - wiring tu dien cam nham relay;
   - DB name/deviceNumber khong phan anh dung mapping vat ly;
   - hoac telemetry Code 300 tra `DeviceId=14 ON` sau khi nhan command `13`, gay UI hien quat 4 ON.

## 4. Evidence Checklist Can Lay Cho Moi Lan Test

Khi test toggle mot thiet bi, can luu du 4 nhom evidence:

### 4.1 FE request

```text
Request URL
Request Method
Status Code
Response payload
```

Can xac dinh:

- URL `{deviceDbId}` la Id cua device nao trong DB.
- Response payload Code 101 co `DeviceId` nao.

### 4.2 Backend toggle logs

Can lay:

```text
[DeviceStatusRace][Toggle-Start]
[DeviceStatusRace][Toggle-Response]
[DeviceStatusRace][Toggle-PendingSet]
[DeviceStatusRace][Toggle-Saved]
```

Can xac dinh:

- `DbDeviceId`
- `DeviceNumber`
- `DeviceName`
- `PreviousStatus`
- `TargetStatus`
- `ResponsePayload`

### 4.3 IoT telemetry logs

Can lay Code 300 ngay truoc va sau toggle:

```text
[IoTConnectionManager] Device Status Payload (300)
[DeviceStatusRace][Telemetry-Match]
[DeviceStatusRace][Telemetry-Confirmed]
[DeviceStatusRace][Telemetry-StaleSkipped]
[DeviceStatusRace][Telemetry-Overwrite]
[DeviceStatusRace][Telemetry-Unmatched]
```

Can xac dinh:

- Sau khi gui `DeviceId=13`, Code 300 bao `13=ON/OFF` hay `14=ON/OFF`.
- Co `Telemetry-Overwrite` khong.
- Co `Telemetry-StaleSkipped` khong.
- Co unmatched slot hop le khong.

### 4.4 Physical observation

Can ghi:

```text
User clicked: Quat nuoc X
FE request deviceDbId: ...
BE sent DeviceId: ...
Physical relay/fan changed: Quat nuoc Y
Next Code 300 reported changed slot: DeviceId Z
```

Bang nay la cach nhanh nhat de tach loi BE, FE, IoT telemetry, firmware, va wiring.

## 5. Current Root Cause Matrix

| Symptom | Evidence | Most likely owner |
|---|---|---|
| Bam Quat 3 nhung FE request Id cua Quat 4 | Request URL chua DB Id cua Quat 4 | FE binding/render |
| Backend gui DeviceId 13, physical quat 4 bat | Toggle response payload `DeviceId: 13`, vat ly quat 4 chay | IoT firmware/relay/wiring or DB physical mapping |
| Backend gui DeviceId 13, Code 300 sau do bao 14 ON | Toggle response `13`, telemetry changed `14` | IoT telemetry mapping |
| Backend gui DeviceId 13, Code 300 cu bao 13 OFF va overwrite DB | Co `Telemetry-Overwrite` sau toggle, khong co pending skip | BE stale guard issue |
| Payload co DeviceId hop le nhung DB khong co DeviceNumber do | `Telemetry-Unmatched`, DB snapshot thieu slot | DB config/install mapping |
| UI nhay nut do thu tu list | API detail khong sort, FE dung index | FE + API response ordering hardening |

## 6. Recommended Fix Plan

### Phase 1: Confirm mapping for A8

Voi A8, can test tung slot:

```text
11 -> Quat nuoc 1 A8
12 -> Quat nuoc 2 A8
13 -> Quat nuoc 3 A8
14 -> Quat nuoc 4 A8
```

Cho moi slot, ghi:

```text
PATCH deviceDbId
Toggle-Start DeviceNumber
Toggle-Response payload DeviceId
Physical fan changed
Next Code 300 status changed
```

Neu `DeviceId=13` lam physical quat 4 chay, khong sua BE truoc. Can sua mapping firmware/relay/wiring hoac swap DB `DeviceNumber` theo mapping thuc te sau khi IoT confirm.

### Phase 2: Harden API detail ordering

Trong `GetDeviceHubDetailQueryHandler`, nen sort:

```csharp
deviceHub.Devices
    .OrderBy(d => d.DeviceNumber)
    .Select(...)
```

Muc tieu:

- FE render thiet bi theo slot vat ly on dinh.
- Giam rui ro FE map label/action theo index sai.

### Phase 3: Add mapping diagnostics endpoint or report query

Can mot cach kiem tra nhanh mismatch:

- DB child device numbers cua hub.
- Latest Code 300 payload device numbers.
- Unmatched payload slots.
- DB devices missing from payload.

Co the bat dau bang log/report noi bo, chua can public API neu chua co yeu cau.

### Phase 4: Keep pending guard for stale Code 300

Pending guard van can giu vi Code 300 gui moi 10 giay va co the den sau command.

Can tiep tuc quan sat:

```text
Telemetry-StaleSkipped
Telemetry-Confirmed
Telemetry-Overwrite
```

Neu sau khi mapping dung ma van co nhay status, log nay se giup chot stale telemetry.

## 7. SQL / Data Checks Suggested

Kiem tra child devices trong mot hub:

```sql
select "Id",
       "DeviceHubId",
       "PondId",
       "DeviceNumber",
       "Name",
       "DeviceCode",
       "DeviceType",
       "InstallationStatus",
       "ConnectionStatus",
       "Status",
       "IsDeleted",
       "UpdatedAt"
from "Devices"
where "DeviceHubId" = '<hub-db-id>'
order by "DeviceNumber";
```

Kiem tra duplicate device number trong cung hub:

```sql
select "DeviceHubId", "DeviceNumber", count(*)
from "Devices"
where "IsDeleted" = false
group by "DeviceHubId", "DeviceNumber"
having count(*) > 1;
```

Kiem tra PaddleWheel A8:

```sql
select "Id", "DeviceNumber", "Name", "DeviceCode", "Status", "ConnectionStatus", "UpdatedAt"
from "Devices"
where "DeviceHubId" = '463e1749-2cad-4277-adf4-b87feebc88ec'
  and "DeviceType" = 'PaddleWheel'
  and "IsDeleted" = false
order by "DeviceNumber";
```

## 8. Current Open Questions

1. Response payload cua request `dcf8a6b7-4ddb-4cd4-b749-9f7eec373fa8/toggle` co hien `DeviceId: 13` hay khong?
2. IoT direct method log phia firmware co nhan `DeviceId=13` hay `14`?
3. Code 300 ngay sau request do bao slot nao thay doi: `13` hay `14`?
4. Neu BE gui `13` va Code 300 bao `13`, nhung vat ly quat 4 chay, mapping relay/wiring trong tu dien dang sai.
5. Neu BE gui `13` nhung Code 300 bao `14`, firmware telemetry mapping dang sai.
6. Neu FE lai request Id cua quat 4, FE binding van con sai o mot flow khac.

## 9. Final Assessment

Hien tai co hai nhom loi can tach rieng:

1. Consistency race BE vs Code 300:
   - Da co pending-state guard 30 giay.
   - Log moi co the xac nhan `Telemetry-Confirmed`, `Telemetry-StaleSkipped`, `Telemetry-Overwrite`.

2. Mapping device:
   - Day la nhom loi dang giai thich tot nhat cho case "bam Quat 3 nhung Quat 4 bat".
   - Backend toggle theo DB `DeviceNumber`.
   - Neu request dung DB Id cua Quat 3 va response payload dung `DeviceId=13`, backend da lam dung.
   - Loi con lai nam o FE binding, IoT telemetry mapping, firmware relay mapping, wiring, hoac DB name/deviceNumber khong dung vat ly.

Khuyen nghi khong sua logic toggle backend nua truoc khi co bang chung `Toggle-Response`, firmware receive log, va Code 300 sau toggle cho dung case A8.

## 10. Mapping Hardening Implemented

Date: 2026-06-24

Scope:

- Giu nguyen API, Code 101, Code 300, Redis pending key, SignalR, va DB schema.
- Khong auto tao/sua/xoa device tu telemetry.
- Chi tang kha nang phat hien slot lech/thieu/trung va lam on dinh thu tu devices tra ve FE.

Thay doi chinh:

- `DeviceStatusTelemetryHandler` them audit tong hop moi lan nhan Code 300:

```text
[DeviceMappingCheck][Telemetry-MappingAudit]
[DeviceMappingCheck][Telemetry-PayloadSlotsMissingInDb]
[DeviceMappingCheck][Telemetry-DuplicateSlotDetected]
[DeviceMappingCheck][Telemetry-DbSlotsMissingInPayload]
```

- `CreateDeviceCommandHandler` them log slot auto-assign:

```text
[DeviceMappingCheck][CreateDevice-AssignSlot]
```

- `GetDeviceHubDetailQueryHandler` canh bao duplicate `DeviceNumber` va tiep tuc tra danh sach device sort theo `DeviceNumber`:

```text
[DeviceMappingCheck][HubDetail-DuplicateSlotDetected]
```

Y nghia log:

- `Telemetry-PayloadSlotsMissingInDb`: IoT gui slot nhung DB khong co device tuong ung. Backend skip slot nay, khong update nham device khac.
- `Telemetry-DbSlotsMissingInPayload`: DB co device nhung Code 300 khong gui slot do. Theo logic hien tai device nay co the bi set `DisConnected + UnDefined`.
- `Telemetry-DuplicateSlotDetected`: DB hoac payload co trung slot, can sua data/config ngay.
- `CreateDevice-AssignSlot`: flow tao device tu dong lay slot thap nhat con trong trong range loai thiet bi. Neu van hanh muon Feeder slot `32`, can update mapping ro rang thay vi de auto lay `31`.

Acceptance sau khi sua data:

```text
FE PATCH dung DbDeviceId
Toggle-LoadedDevice DeviceNumber dung slot vat ly
Toggle-Outbound101 DeviceId dung slot vat ly
Code 300 sau toggle match cung DeviceId
Khong con Telemetry-PayloadSlotsMissingInDb cho slot dang ton tai vat ly
Khong con Telemetry-DbSlotsMissingInPayload cho device dang khai bao trong DB
Khong con HubDetail-DuplicateSlotDetected
```

