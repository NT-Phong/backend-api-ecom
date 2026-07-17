# Oxy Incident Worker Flow

This is the merged skill-level workflow for the former `flow-woker.md` and `flow-worker.md` drafts.

Use it when an agent is asked to analyze, debug, or change the Oxy incident worker flow. For the full canonical documentation, read:

- `docs/oxy/oxy-incident-workflow.md`
- `docs/oxy/oxy-implementation-review.md`
- `docs/oxy/oxy-bug-fix-report.md`
- `docs/oxy/oxy-open-issues.md`

Source code and tests are the source of truth. If this file conflicts with code, trust code and update this file.

## 1. Scope

This workflow covers:

- Code `300` device status telemetry.
- Oxy AirBlower failure/recovery detection.
- Pond type resolution for `Ao veo` and `Ao nuoi`.
- Normal/Critical severity evaluation.
- Redis alert state and active pond index.
- Background worker queue reconciliation.
- Bell/Speaker scheduler behavior.
- Backend exclusion from backup Oxy and PaddleWheel auto-control.
- DeviceNumber integrity for installed hub devices.
- Notification, reminder, escalation, acknowledge, and recovery.

## 2. Terminology Map

| BA/PM term | Code term | Notes |
|---|---|---|
| Oxy device | `DeviceTypeEnum.AirBlower` | Only AirBlower devices participate in Oxy alert rules. |
| Ao veo | `OxyAlertPondKind.Veo` | Resolved from `PondCategoryEnum.CreateCycle`, or fallback when total Oxy count <= 1. |
| Ao nuoi | `OxyAlertPondKind.Nursery` | Resolved from `PondCategoryEnum.GenerateCycle`, or fallback when total Oxy count > 1. |
| THUONG / Normal | `OxyAlertSeverityEnum.Normal` | Speaker-only warning path. |
| GAP / URGENT / Critical | `OxyAlertSeverityEnum.Critical` | Bell + Speaker warning path. |
| Dung canh bao / Acknowledge | `AcknowledgeOxyAlertCommand` | User accepts the incident and warning hardware is silenced. |

## 3. Source Files To Inspect First

Read these files before modifying Oxy flow:

1. `Infrastructure/Ecom.Infrastructure/IoT/IoTConnectionManager.cs`
2. `Infrastructure/Ecom.Infrastructure/IoT/DeviceStatusTelemetryHandler.cs`
3. `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangeCommandHandler.cs`
4. `Core/Ecom.Application/Features/PondAlert/Commands/AcknowledgeOxyAlert/AcknowledgeOxyAlertCommandHandler.cs`
5. `Core/Ecom.Application/Features/PondAlert/Services/OxyAlertRuleEvaluator.cs`
6. `Core/Ecom.Application/Features/PondAlert/Services/OxyAlertMessageBuilder.cs`
7. `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertQueueService.cs`
8. `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertSchedulerService.cs`
9. `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyWarningHardwareService.cs`
10. `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertReminderService.cs`
11. `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertPondKindResolver.cs`
12. `Core/Ecom.Domain/Models/PondAlert/OxyAlertSessionState.cs`
13. `Presentation/Ecom.API/Controllers/V1/PondAlertController.cs`
14. `Core/Ecom.Application/Features/DeviceControl/DeviceControlDtos.cs`
15. `Core/Ecom.Application/Features/Device/Commands/UpdateDevice/UpdateDeviceCommandHandler.cs`
16. `Core/Ecom.Application/Features/Device/Commands/UpdateDevice/UpdateDeviceValidator.cs`

## 4. End-to-End Flow

```txt
IoT device
-> IoTConnectionManager
-> DeviceStatusTelemetryHandler
-> HandleOxyStatusChangesCommandHandler
-> Redis OxiAlertState:{pondId} + OxiAlertPondsIndex
-> OxyAlertBackgroundWorker
-> OxyAlertQueueService
-> OxyAlertSchedulerService
-> OxyWarningHardwareService / OxyAlertReminderService
```

Output suppression mode (2026-06-29):

- `OxyAlertOutputSwitch.OutputsEnabled = false` currently hides Oxy external output.
- Detection, Redis state/index, ack marker, queue, scheduler, and recovery lifecycle still run.
- Notification/FCM and Bell/Speaker methods must return before external publish/control and before notification timestamp updates.

## 5. Telemetry Code 300 Behavior

Implemented behavior:

- `IoTConnectionManager` routes `TelemetryCode.DeviceStatus = 300` payloads to `DeviceStatusTelemetryHandler`.
- `DeviceStatusTelemetryHandler` finds an installed `DeviceHub` by IoT `deviceId`.
- It loads child `Device` records and filters Oxy devices by `DeviceTypeEnum.AirBlower`.
- It resolves pond kind only when `DeviceHub.PondId` exists.
- It maps telemetry payload status:
  - `FAULT` -> `StatusDeviceEnum.Fault`
  - `ON` -> `StatusDeviceEnum.On`
  - anything else -> `StatusDeviceEnum.Off`
- It normalizes the payload to the final item per positive `DeviceId`.
- It updates DB status for devices found in payload, saves changes, then dispatches one `HandleOxyStatusChangesCommand` batch for explicit AirBlower recovery/failure changes, including repeated same-status fault evidence.
- Devices missing from payload are marked `DisConnected` and `UnDefined`, but missing payload entries do not directly dispatch Oxy failure commands.
- An installed hub timeout confirmed by `DeviceHealthMonitor` is handled by `IoTBackgroundService`, which dispatches one Oxy failure batch for that pond's AirBlowers after marking the hub disconnected.
- While the hub remains unhealthy, health checks dispatch an idempotent offline reconcile batch so a lost Redis incident/index can be recreated from confirmed input.
- Missing-device reconciliation filters payload ids to `DeviceId > 0`, so non-device payload entries such as `{ "IsGetNotice": "OFF" }` cannot make a DB device with `DeviceNumber=0` look present.

Agent rule:

- Do not claim that missing Code `300` payload entries automatically trigger Oxy incidents unless source code is changed.
- Treat only health-timeout-confirmed disconnection as a new Oxy failure source; do not promote one missing Code `300` payload item into an incident.

## 6. Failure And Severity Rules

Pond kind resolution:

```txt
PondCategoryEnum.CreateCycle   -> OxyAlertPondKind.Veo
PondCategoryEnum.GenerateCycle -> OxyAlertPondKind.Nursery
unknown + totalOxy <= 1        -> OxyAlertPondKind.Veo
unknown + totalOxy > 1         -> OxyAlertPondKind.Nursery
```

Failure detection:

- `Fault` is always failure.
- `Off` is failure for `Veo`.
- `Off` is failure for `Nursery` only when no other AirBlower is currently `On`.
- Confirmed hub-timeout disconnection is dispatched as failure; missing payload alone is not.

Severity:

- `Veo` with any failed Oxy -> Critical.
- `Nursery` with one failed Oxy and another Oxy still running -> Normal.
- `Nursery` with two or more failed Oxy devices -> Critical.
- `Nursery` with failed primary and failed backup -> Critical.

Primary/backup resolution:

- For `Nursery`, Oxy #1 is `DeviceNumber=21` primary and Oxy #2 is `DeviceNumber=22` backup when those device numbers exist.
- For non-nursery or incomplete numbering, `OxyAlertRuleEvaluator` falls back to sorted/runtime ordering.
- `OxyAlertRuleEvaluator` sorts by `DeviceNumber`, then `CreatedAt`, then `Id`.
- Device-control BA rule: Oxy #1 remains always ON and cannot be toggled; nursery Oxy #2 defaults OFF and can be toggled unless it is `Connected + Fault`.

DeviceNumber integrity:

- `DeviceNumber=0` is allowed only for devices that are not assigned to a hub slot, such as uninstalled devices, removed-from-hub devices, or non-hub devices like Scale.
- Installed devices with `DeviceHubId` must have a valid positive `DeviceNumber` in the protocol range for their `DeviceType`.
- `UpdateDeviceCommandHandler` auto-assigns the next available slot when a device is `Installed`, has a `DeviceHubId`, and the incoming/current `DeviceNumber` is null or `<= 0`.
- When moving a device to another hub without an explicit `DeviceNumber`, `UpdateDeviceCommandHandler` assigns a slot from the target hub instead of preserving the old hub slot.
- If a positive `DeviceNumber` is provided, `UpdateDeviceValidator` validates the range and the handler rejects duplicate slots in the target hub.
- For nursery Oxy, this means Oxy #2 can recover from the bad state `AirBlower + GenerateCycle + Installed + DeviceHubId + DeviceNumber=0` by being updated/reinstalled into the correct available AirBlower slot, normally `22` when `21` is occupied by Oxy #1.

## 7. Redis State Model

Keys:

- `OxiAlertState:{pondId}`: per-pond `OxyAlertSessionState`.
- `OxiAlertAck:{pondId}`: acknowledge marker with 24-hour TTL. It suppresses repeated farmer/admin notification for the same pond until real recovery or TTL self-healing.
- `OxiAlertPondsIndex`: active pond ids for the worker queue.
- `OxiAlertSchedulerState`: global scheduler state.
- `OxyAlertWorker:GlobalScheduler`: distributed worker lock.
- `OxyAlertWorker:PondsIndex`: distributed lock for active pond index mutations.
- `OxiAlertNotiDedup:{pondId}:{scope}:{severity}`: notification dedup key.

Important state fields:

- `FailedDeviceIds`
- `FailedDeviceFaultTimes`
- `Severity`
- `AlertPondKind`
- `IsAcknowledged`
- `WarningHardwareSilencedAt`
- `WarningHardwareSilenceRetryAfter`
- `IsActive`
- `FirstFaultAt`
- `LastFaultTelemetryAt`
- `RecoveryPendingAt`
- `LastSeverityChangedAt`
- `OldestFailedOxyCreatedAt`
- `LastFcmpushed`
- `FarmerNotificationSentAt`
- `EscalationNotificationSentAt`
- `NormalAlertMessage`
- `NormalPhase`
- `NormalPhaseStartTime`

Agent rule:

- Preserve `OxiAlertPondsIndex` behavior unless intentionally redesigning worker discovery.
- Do not replace the active index with broad Redis scans without a scaling review.
- Use the active-index lock pattern for add/remove operations. Do not reintroduce unlocked read-modify-write updates to `OxiAlertPondsIndex`.

## 8. Worker Queue Behavior

`OxyAlertBackgroundWorker`:

- Runs every 2 seconds.
- Acquires `OxyAlertWorker:GlobalScheduler`.
- Skips tick if another instance owns the lock.
- Resolves `IOxyAlertSchedulerService` per scope.

`OxyAlertQueueService`:

- Loads active ponds from `OxiAlertPondsIndex`.
- Removes missing stale index entries from the queue.
- Does not remove inactive recovery-pending states while they are inside `OxyAlertConstants.RecoveryGraceSeconds`.
- Removes inactive fully recovered states only after confirmed recovery outside the recovery grace window.
- Excludes acknowledged states from scheduling while retaining them for recovery reconciliation.
- When output suppression is enabled, acknowledged hardware turn-off is not treated as a successful dispatched shutdown; the pond remains indexed for recovery reconciliation instead of being removed only because hardware output is hidden.
- Retries acknowledged warning-hardware shutoff until the outcome is `Dispatched`, stores `WarningHardwareSilencedAt`, and does not repeat successful Bell/Speaker shutoff each tick.
- Leaves silenced unset for a `NoTarget` outcome and retries resolution after `WarningHardwareSilenceRetryAfter`; transport/dispatch exceptions remain retryable on the next tick.
- Continues fully recovered Redis/index cleanup even if warning hardware shutoff throws.
- Removes stale pond ids from the latest `OxiAlertPondsIndex` under `OxyAlertWorker:PondsIndex` instead of rewriting an older index snapshot.
- Reloads AirBlower devices from DB.
- Re-resolves pond kind and re-evaluates severity.
- Reconciles stale failed devices against current DB state.
- Uses a short fault-persistence grace window before dropping a failed device from state.
- Uses `RecoveryPendingAt` and `LastFaultTelemetryAt` to prevent deleting state too early during ON/FAULT flapping.
- Current recovery grace is 120 seconds, long enough to tolerate Code 300 fault telemetry arriving slightly over one minute apart.
- Uses per-device `RecoveryPendingDeviceTimes` so a transient recovery for one AirBlower does not immediately remove that device from `FailedDeviceIds`; stable recovery must exceed the recovery grace before cleanup.
- Queue reconciliation must not infer Oxy recovery only from DB `UnDefined`/`DisConnected` status. Missing Code 300 payload entries and hub health checks can update DB status without an explicit recovery telemetry event, so failed devices are preserved unless a per-device recovery pending marker exists.
- Logs:
  - `OXY_RECOVERY_PENDING` while an inactive state is waiting for recovery confirmation.
  - `OXY_STATE_DELETED_TOO_EARLY` when reconcile would otherwise remove a recently faulting state.
  - `OXY_RECOVERY_CONFIRMED_CLEAR` when state/ack/index are finally cleared after stable recovery.

Queue sorting:

- Critical queue is processed before Normal queue.
- Normal queue includes only:

```txt
Severity == Normal && AlertPondKind == Nursery
```

- Critical sorting currently groups `Nursery` before `Veo`, then sorts within group.
- Within group, sorting uses `FirstFaultAt`, `OldestFailedOxyCreatedAt`, and `PondId`.
- Simultaneous fault window is 5 seconds.

## 9. Scheduler Behavior

Critical:

- Critical interrupts Normal or NormalCooldown.
- Scheduler clears completed Normal state when Critical exists.
- Each Critical pond gets a 60-second slot.
- First 30 seconds: Bell on, Speaker off.
- After 30 seconds: Bell off, Speaker on.
- At slot end, critical reminder may publish, then scheduler rotates to next Critical pond.

Normal:

- Normal uses Speaker only.
- First Normal play lasts 300 seconds.
- The first Normal speaker play does not publish a notification reminder; the initial farmer notification already covers first activation.
- Repeat Normal plays last 30 seconds.
- Normal notification reminders publish only on repeat Normal plays.
- After all Normal ponds in the round are processed, scheduler enters 900-second cooldown.
- Critical immediately interrupts Normal.

Idle:

- If no queue remains, scheduler turns off current hardware, clears state, and enters Idle.

## 10. Hardware Control

Warning hardware:

- Implemented by `OxyWarningHardwareService`.
- When `OxyAlertOutputSwitch.OutputsEnabled = false`, `SetCriticalAsync`, `SetNormalAsync`, and `TurnOffAllAsync` log `OXY_OUTPUT_SUPPRESSED` and return before WarningDevice resolution or `IWarningDeviceControlService.SendAsync`.
- Resolves `WarningDeviceHub` by `Pond.ZoneId`.
- Loads active installed `WarningDevice` records from the zone hubs.
- There is no direct pond-level WarningDevice mapping in current code.
- Shutdown returns a classified internal outcome: `Dispatched` only when all resolved Bell/Speaker stop commands complete, or `NoTarget` when no pond/targets can be resolved.

Critical hardware:

- Bell is controlled for the first part of the slot.
- Speaker is controlled for the second part of the slot.
- If no Bell exists, Speaker fallback can still run.

Normal hardware:

- Speaker only.
- Bell state is forced off.

Support devices:

- Backend does not automatically send IoT commands to turn on backup AirBlower or PaddleWheel during Oxy incidents.
- IoT/firmware owns automatic support-device behavior for backup AirBlower and PaddleWheel.
- Source currently has no backend Oxy-incident auto-control path for Syphon.

## 11. Notification, Reminder, Escalation

Initial notification:

- Implemented in `HandleOxyStatusChangesCommandHandler`.
- Farmer recipients are users with role code `FARMER`.
- One normalized input batch is evaluated once and sends at most one initial notification at its final severity.
- Initial notification eligibility also uses a 60-second `LastFcmpushed` baseline guard.
- Normal dedup TTL is 900 seconds.
- Critical dedup TTL is 60 seconds.
- Initial notification publish failure is logged and does not block Redis alert state or worker scheduling.
- When `OxyAlertOutputSwitch.OutputsEnabled = false`, initial notification returns before `BulkNotificationEvent` publish and before `LastFcmpushed` / `FarmerNotificationSentAt` updates.
- When initial notification publish fails after dedup acquisition, the dedup key is removed as best effort so a later telemetry attempt can retry notification.
- Initial notification publish happens outside the pond-state lock. After publish, the handler reacquires the pond-state lock and updates `LastFcmpushed` and `FarmerNotificationSentAt`.
- After publish, timestamp updates are merged only if the active state still matches the same `FirstFaultAt` incident and is not acknowledged; a notification side effect does not recreate missing state.
- Severity transition can trigger a new initial notification.

Message builder:

- `OxyAlertMessageBuilder` centralizes notification/reminder text.
- Critical message means the whole Oxy system failed.
- Normal message names the failed Oxy number.
- Warning speaker payload receives `pondNo` and `alertDeviceId`, not the full message text.

Reminder:

- Normal reminder publishes warning notification to farmers.
- Normal reminder is suppressed on the first Normal speaker play to avoid near-immediate duplicate notification after the initial alert.
- Critical reminder publishes warning notification to farmers after critical cooldown.
- When output suppression is enabled, Normal reminder, Critical reminder, and Oxy-off-too-long notification return before publish and before updating notification timestamps.

Escalation:

- Escalation publishes to `ADMIN`, `MANAGER`, and `EMPLOYEE_MANAGER`.
- Escalation delay is read from `OxyAlertSettings.EscalationAfterSeconds`.
- If no config section exists, the `OxyAlertSettings` default is 300 seconds.
- Escalation checks `OxiAlertAck:{pondId}` first. If the ack marker exists, it logs `OXY_ESCALATION_SKIP_ACK_MARKER` and does not publish.
- Escalation baseline priority is:
  1. `FarmerNotificationSentAt`
  2. `LastFcmpushed`
  3. `FirstFaultAt`
  4. `StartTime`
- If fallback baseline is used, it logs `OXY_ESCALATION_FALLBACK_BASELINE`.
- Escalation updates `EscalationNotificationSentAt` only. It must not overwrite `LastFcmpushed`, because `LastFcmpushed` is used by farmer reminder cooldown.
- After escalation publish, the service persists `EscalationNotificationSentAt` and verifies read-back. If the read-back fails, it logs `OXY_ESCALATION_STATE_UPDATE_FAILED`.
- When output suppression is enabled, escalation returns before publish and before `EscalationNotificationSentAt` is updated.

## 12. Acknowledge Flow

Endpoint:

```txt
POST /api/v1/PondAlert/acknowledge/{pondId}
```

Implementation:

- `PondAlertController` sends `AcknowledgeOxyAlertCommand`.
- `AcknowledgeOxyAlertCommandHandler` sets and verifies `OxiAlertAck:{pondId}` first, before state mutation and warning-hardware shutoff.
- If marker verification fails, acknowledge returns failure.
- If marker verification succeeds but the pond-state lock times out, acknowledge returns success because telemetry suppression is already active.
- The handler reads `OxiAlertState:{pondId}` after marker verification.
- If state exists, it sets `IsAcknowledged = true`.
- When `OxyAlertOutputSwitch.OutputsEnabled = false`, the handler returns success after ack/state mutation and does not call warning-hardware turn-off.
- It calls `IOxyWarningHardwareService.TurnOffAllAsync(pondId)` as best effort.
- If acknowledge-time hardware shutdown returns `Dispatched`, it stores `WarningHardwareSilencedAt` in the current alert state.
- If shutdown returns `NoTarget`, it leaves silenced unset and stores a 60-second retry-after marker so `OxyAlertQueueService` can resolve targets again without calling every tick.
- If dispatch throws, it leaves silenced unset without throttling so the worker retries on the next tick.
- Acknowledged ponds remain indexed for recovery reconciliation, but no further Bell/Speaker shutoff command is sent after `WarningHardwareSilencedAt` is recorded.
- It returns success even when no Redis state exists.

Post-ack telemetry:

- New fault telemetry while acknowledged updates/suppresses state and keeps the pond out of scheduler output.
- Recovery clears ack marker and state when all acknowledged failures are gone.

## 13. Recovery Flow

Recovery occurs when an AirBlower transitions from failure to non-failure.

The handler:

- Removes recovered device id from `FailedDeviceIds`.
- Removes that device fault timestamp.
- When no failed devices remain, marks the state inactive, sets `RecoveryPendingAt`, persists state, keeps the pond indexed, and logs `OXY_RECOVERY_PENDING`.
- It does not remove Redis state immediately for unacknowledged recovery.
- If FAULT returns within `RecoveryGraceSeconds`, it clears `RecoveryPendingAt`, sets `LastFaultTelemetryAt`, reactivates state, preserves the incident timeline, and logs `OXY_STATE_RESET_PREVENTED`.
- Keeps the alert active if other failed devices remain.

The queue service also:

- Drops stale failed devices after reconciliation.
- Turns off hardware for fully recovered ponds as best effort.
- Keeps inactive recovery-pending state during recovery grace.
- Clears state, ack marker, and index only after confirmed recovery outside the grace window, logging `OXY_RECOVERY_CONFIRMED_CLEAR`.

## 14. Known Limitations

- No Oxy-specific automated tests were found during the reconciliation.
- Oxy output is currently hard-disabled by `OxyAlertOutputSwitch.OutputsEnabled = false`; missing Oxy FCM or Bell/Speaker output is expected until this switch is restored.
- WarningDevice targeting is zone-level, not pond-level.
- A confirmed hub health timeout is an Oxy failure source; a missing payload item remains non-triggering.
- Existing bad DB rows with `Installed + DeviceHubId + DeviceNumber=0` still need a repair/update action so the new handler can assign a valid slot.
- `OxyAlertConstants.EscalationAfterSeconds` exists but reminder escalation uses options instead.
- Several comments/messages in source are mojibake encoded.
- Redis/cache write failures are logged by cache infrastructure; runtime Redis failure handling should still be validated operationally.

## 15. Agent Checklist

Before editing Oxy code:

1. Read this file and `docs/oxy/oxy-incident-workflow.md`.
2. Search source with `rg "OxyAlert|OxiAlert|AirBlower|AcknowledgeOxyAlert|HandleOxyStatusChanges"`.
3. Inspect telemetry, handler, evaluator, queue, scheduler, hardware, reminder, and acknowledge code.
4. Check `OxyAlertOutputSwitch.OutputsEnabled` before debugging missing FCM, notification timestamps, or Bell/Speaker output.
5. If restoring Oxy output, set the switch to `true`, rebuild Application/Infrastructure, and validate initial notification, reminders, escalation, Oxy-off-too-long, and warning hardware.
6. Separate BA/PM intent from implemented behavior.
7. Preserve Clean Architecture boundaries:
   - Controller stays thin.
   - Application owns rule/message abstractions.
   - Infrastructure owns IoT, Redis worker, and WarningDevice resolution.
8. Preserve the nursery Oxy #1/#2 device-number contract without applying it globally to every pond kind.
9. Do not reintroduce backend automatic backup Oxy or PaddleWheel control without explicit approval.
10. Preserve the `DeviceNumber=0` sentinel only for unassigned/uninstalled/non-hub devices; never leave installed hub devices with `DeviceNumber=0`.
11. Add or update focused tests when changing behavior.
12. For build verification, summarize the phase and provide the recommended build command for the user to run manually unless the user explicitly asks the agent to run it.
13. Report verification honestly if no test/build was run or if build output was user-provided.

