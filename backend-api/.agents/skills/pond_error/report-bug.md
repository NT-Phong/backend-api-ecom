# Oxy Alert Flow - Source-Backed Bug Report

Agent Report Metadata:
- Task domain: Oxy/Pond alert telemetry, scheduler, reminder, warning hardware, and notification flow.
- Canonical route: `.agents/context/task-router.md` -> Oxy, pond alerts, Code 300, warning hardware.
- Update rule: append delta-first updates with `.agents/skills/backend-api-architecture/templates/skill-report-update.md` when Oxy behavior, risks, or verification results change.
- Source of truth: source code and runtime/test evidence outrank older report text.

This file is the skill-level report for the Oxy incident bug-fix/reconciliation work.

For canonical project documentation, also read:

- `docs/oxy/oxy-incident-workflow.md`
- `docs/oxy/oxy-implementation-review.md`
- `docs/oxy/oxy-bug-fix-report.md`
- `docs/oxy/oxy-open-issues.md`

Source code and tests are the source of truth. No Oxy-specific automated tests were found during the documentation reconciliation, so the status below is based on source inspection.

## Latest Update - 2026-06-29 (Oxy Output Suppression)

Business decision for this session: temporarily hide external Oxy outputs only. Keep Code 300 telemetry handling, rule evaluation, Redis incident state, `OxiAlertPondsIndex`, `OxiAlertAck:{pondId}`, recovery grace, and cleanup behavior available for reuse.

Implemented switch:

- `Core/Ecom.Application/Common/Constants/OxyAlertOutputSwitch.cs` defines `OutputsEnabled = false`.
- All output gates read the same switch, so restoring the feature should require changing one source constant and rebuilding.

Suppressed outputs:

- Initial farmer notification in `HandleOxyStatusChangeCommandHandler`.
- Normal reminder, Critical reminder, escalation, and Oxy-off-too-long notification in `OxyAlertReminderService`.
- Bell/Speaker `SetCriticalAsync`, `SetNormalAsync`, and `TurnOffAllAsync` in `OxyWarningHardwareService`.

Preserved behavior:

- `OxiAlertState:{pondId}` and `OxiAlertPondsIndex` are still created/updated by telemetry and worker reconciliation.
- `POST /api/v1/PondAlert/acknowledge/{pondId}` still writes/verifies `OxiAlertAck:{pondId}` and returns the same public response shape.
- Recovery still passes through the existing grace/cleanup path.
- Queue and scheduler may still run, but output methods log `OXY_OUTPUT_SUPPRESSED` and return before publishing FCM or sending WarningDevice commands.

Review actions from this session:

- Added a second guard inside `TryPublishInitialFarmerNotificationAsync` so a future refactor cannot bypass the output switch by calling the helper directly.
- Fixed the suppressed-hardware edge case where a no-op `TurnOffAllAsync` could be interpreted as successful hardware silence. `AcknowledgeOxyAlertCommandHandler` and `OxyAlertQueueService` now check `OutputsEnabled` before treating the acknowledged hardware turn-off path as actionable.
- In output-suppressed mode, acknowledge no longer removes an active pond from `OxiAlertPondsIndex` solely because hardware output is hidden.

Restore checklist:

1. Set `OxyAlertOutputSwitch.OutputsEnabled = true`.
2. Rebuild Application and Infrastructure.
3. Verify initial farmer notification, Normal/Critical reminder, escalation, Oxy-off-too-long, and Bell/Speaker output against Redis/FCM/WarningDevice integrations.
4. Confirm `LastFcmpushed`, `FarmerNotificationSentAt`, `EscalationNotificationSentAt`, and `OxyOffTooLongNotificationSentAt` are updated only after real publishes.

Verification completed in this session:

- `dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore -p:UseSharedCompilation=false` passed with 0 errors and existing warnings.
- `dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore -p:UseSharedCompilation=false` passed with 0 errors and one existing warning.
- Runtime Redis/FCM/WarningDevice validation was not executed.

## Latest Update - 2026-05-26 (OxyOffTooLong Notification)

New feature: FCM notification when primary AirBlower (Oxy #1) has been continuously off/fault/disconnected for over 30 minutes at a Nursery pond, with repeat every 5 minutes until recovery or acknowledge.

- `OxyAlertSessionState` now has `OxyOffTooLongNotificationSentAt` (DateTimeOffset). `default` = not yet sent in current incident. Set each time a notification is dispatched (T=30, T=35, T=40…). Reset naturally when state is deleted (recovery/acknowledge).
- `OxyAlertConstants` adds `OxyOffTooLongThresholdSeconds = 1800` (30 min) and `OxyOffTooLongRepeatIntervalSeconds = 300` (5 min).
- `IOxyAlertReminderService` / `OxyAlertReminderService` add `RemindOxyOffTooLongAsync(pondId, state, pondName, primaryFaultStartTime)`. Publishes FCM to FARMER with dynamic elapsed-minutes message, updates `OxyOffTooLongNotificationSentAt` and `LastFcmpushed` in Redis state.
- `OxyAlertQueueService` injects `IOxyAlertReminderService` and calls `CheckOxyOffTooLongAsync` best-effort (wrapped in try/catch, logs `OXY_OFF_TOO_LONG_CHECK_FAILED`) after state persist for any Normal-severity pond. Helper `TryGetPrimaryFaultStartTime` guards that exactly 1 device is failing and it is the sorted-first (primary) device.
- Scope: applies only when `severity == Normal` (Nursery, machine #1 fail, machine #2 still running). Critical ponds are excluded from this check.
- Log markers: `OXY_OFF_TOO_LONG_PUBLISH` on success, `OXY_OFF_TOO_LONG_CHECK_FAILED` on exception.
- Build: Domain ✅, Application ✅, Infrastructure ✅, API ✅ — 0 errors.

## Previous Update - 2026-05-26 (Remaining Gap Fixes)

- OX-01: `IOxyWarningHardwareService.TurnOffAllAsync` returns a classified `Dispatched`/`NoTarget` result. Acknowledge and worker write `WarningHardwareSilencedAt` only for dispatched shutdown; `NoTarget` stores `WarningHardwareSilenceRetryAfter` for a 60-second retry and dispatch exceptions remain retryable on the next tick.
- OX-02: Code 300 and confirmed hub-offline inputs now dispatch one `HandleOxyStatusChangesCommand` batch per pond input. Severity and initial notification are evaluated once after all AirBlower changes are merged, preventing Normal then Critical notifications for the same two-device event.
- OX-03: `IoTBackgroundService` dispatches the confirmed-offline failure batch during each unhealthy health interval. A missing Redis incident/index can therefore be recreated from confirmed device-health input without reintroducing notification-side-effect recreation.
- OX-04: `OxyAlertReminderService` recipient selection has been restored to role-code-only behavior; active user/role filtering is not part of the incident fix scope.
- OX-05: the escalation notification-intent title touched by this patch has been corrected; project and skill documentation now records the final backend/IoT boundary.

## Previous Update - 2026-05-20

Implemented the Oxy state-reset/admin-escalation hardening plan. Current source status:

- 2026-05-25 follow-up delta: `OxyAlertReminderService` previously published from a worker snapshot and wrote the entire stale `OxyAlertSessionState` back after Normal/Critical reminder or escalation. Concurrent telemetry or acknowledge could therefore lose newer ack/recovery/failed-device changes. Reminder paths now re-check current active/ack state before publishing, escalation reloads current state, and all post-publish timestamp updates merge into the latest cached state under `OxyAlert:PondState:{pondId}`.
- 2026-05-25 business-boundary delta: backend owns Bell/Speaker warning-device scheduling only; IoT/firmware owns automatic backup Oxy and PaddleWheel behavior. Confirmed hub timeout now dispatches Oxy incidents from `IoTBackgroundService`, while missing Code 300 payload items still do not directly create incidents.
- 2026-05-25 incident-integrity delta: initial/reminder/escalation timestamp writes now require the same active unacknowledged `FirstFaultAt` incident; notification side effects no longer recreate missing Redis alert state. Acknowledge records `WarningHardwareSilencedAt` after successful Bell/Speaker shutdown so the worker retries failures but does not send successful shutdown commands each tick.
- 2026-05-21 delta: runtime Redis snapshots showed `OxiAlertState:{pondId}` resetting after about 65 seconds while the same two AirBlower devices were still faulting. Because `RecoveryGraceSeconds` was 60 seconds, the worker could confirm recovery and delete state just before the next fault telemetry arrived, resetting `StartTime`/`FirstFaultAt` and preventing the 300-second admin escalation window from ever being reached. Source now uses a 120-second recovery grace in both `HandleOxyStatusChangeCommandHandler` and `OxyAlertConstants`.
- 2026-05-21 follow-up delta: Redis still showed `failedDeviceIds=[]`, `isActive=false`, and then a new Critical state while both physical Oxy devices were reported as still faulting. The root behavior was recovery telemetry removing failed devices immediately. Source now stores per-device `RecoveryPendingDeviceTimes` in Redis and only removes a failed device after its recovery remains stable beyond `RecoveryGraceSeconds`; FAULT telemetry for that device cancels its pending recovery.
- 2026-05-21 second follow-up delta: if no recovery telemetry exists, the worker must not infer recovery solely from DB status. Code 300 missing payload handling and hub health monitoring can mark child AirBlower rows `UnDefined`/`DisConnected`; previously queue reconciliation treated that as recovered/replaced and removed Redis failed devices. `OxyAlertQueueService` now preserves failed devices when DB status is non-failure but no explicit per-device recovery pending marker exists, and logs `OXY_RECONCILE_PRESERVE_FAILED_DEVICE`.
- State stability fix: `OxyAlertSessionState` now has Redis-only `LastFaultTelemetryAt` and `RecoveryPendingAt` fields. `OxyAlertConstants.RecoveryGraceSeconds` is 120 seconds.
- Recovery grace fix: unacknowledged recovery no longer removes `OxiAlertState:{pondId}` immediately. Handler marks the state inactive, sets `RecoveryPendingAt`, persists the state, keeps the pond indexed, and logs `OXY_RECOVERY_PENDING`.
- Flapping fix: if FAULT returns during recovery grace, handler preserves the incident timeline (`StartTime`, `FirstFaultAt`, `LastFcmpushed`, `FarmerNotificationSentAt`, `EscalationNotificationSentAt`) and logs `OXY_STATE_RESET_PREVENTED`.
- Phase C hardening: initial farmer notification timestamp updates run under the pond-state lock only for the same active, unacknowledged `FirstFaultAt` incident; side effects do not recreate missing incident state.
- Worker cleanup fix: `OxyAlertQueueService` respects `RecoveryPendingAt` and `LastFaultTelemetryAt`. It logs `OXY_STATE_DELETED_TOO_EARLY` instead of deleting recently faulting state, and only clears state/ack/index after `OXY_RECOVERY_CONFIRMED_CLEAR`.
- Escalation ack guard: `OxyAlertReminderService` checks `OxiAlertAck:{pondId}` first and logs `OXY_ESCALATION_SKIP_ACK_MARKER`. It verifies `EscalationNotificationSentAt` read-back after publish and logs `OXY_ESCALATION_STATE_UPDATE_FAILED` if persistence fails.

Previously implemented acknowledge/state hardening plan for repeated Oxy incident notifications:

- Follow-up ack ordering fix: `AcknowledgeOxyAlertCommandHandler` now sets and verifies `OxiAlertAck:{pondId}` before acquiring `OxyAlert:PondState:{pondId}` and before warning-hardware shutoff. If marker verification fails, acknowledge returns failure and does not report a false success. If state lock times out after marker verification, acknowledge returns success because telemetry suppression is already active.
- Escalation fallback fix: `OxyAlertReminderService` no longer skips management escalation forever when `FarmerNotificationSentAt` is missing. It falls back to `LastFcmpushed`, `FirstFaultAt`, then `StartTime`, logs `OXY_ESCALATION_FALLBACK_BASELINE`, and publishes escalation with `OXY_ESCALATION_PUBLISH`. If no `ADMIN`/`MANAGER`/`EMPLOYEE_MANAGER` recipients resolve, it logs and does not mark escalation as sent.
- Added ack marker `OxiAlertAck:{pondId}` with 24-hour TTL. Acknowledge sets this marker even when `OxiAlertState:{pondId}` is missing, so continued fault telemetry for the same pond is suppressed until real recovery or TTL self-healing.
- Added per-pond state lock `OxyAlert:PondState:{pondId}` around telemetry, acknowledge, and worker state/ack handling. `OxyAlertWorker:PondsIndex` remains scoped to `OxiAlertPondsIndex` mutations.
- Telemetry now updates acknowledged state without publishing notifications or adding the pond to the active scheduler queue. Acknowledged recovery clears `OxiAlertAck:{pondId}` and state when no failed devices remain. Unacknowledged recovery goes through recovery grace before worker cleanup.
- Initial farmer notification is suppressed if Redis state persistence or active-index persistence cannot be verified; logs include `OXY_STATE_SET_FAILED` and `OXY_INDEX_SET_FAILED`.
- Worker now logs `OXY_STATE_MISSING` and removes stale index entries when an indexed pond has no active state. Acknowledged ponds are not scheduled for reminder/escalation; they remain indexed for recovery reconciliation and retry unsilenced warning hardware according to shutdown outcome.
- Development local Redis fallback key allow-list now includes `OxiAlertAck:` so local/dev ack behavior remains visible to the application cache layer.

Changed source files:

- `Core/Ecom.Application/Features/PondAlert/Commands/AcknowledgeOxyAlert/AcknowledgeOxyAlertCommandHandler.cs`
- `Core/Ecom.Application/Common/Interfaces/IOxyWarningHardwareService.cs`
- `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangesCommand.cs`
- `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangeCommandHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/DeviceStatusTelemetryHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/IoTBackgroundService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyWarningHardwareService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertQueueService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertSchedulerService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertReminderService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertConstants.cs`
- `Infrastructure/Ecom.Infrastructure/Caching/DistributedCacheService.cs`
- `Core/Ecom.Domain/Models/PondAlert/OxyAlertSessionState.cs`

## 1. Files Verified

Primary implementation files:

- `Infrastructure/Ecom.Infrastructure/IoT/IoTConnectionManager.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/DeviceStatusTelemetryHandler.cs`
- `Core/Ecom.Application/Features/PondAlert/Commands/HandleOxyStatusChange/HandleOxyStatusChangeCommandHandler.cs`
- `Core/Ecom.Application/Features/PondAlert/Commands/AcknowledgeOxyAlert/AcknowledgeOxyAlertCommandHandler.cs`
- `Core/Ecom.Application/Features/PondAlert/Services/OxyAlertRuleEvaluator.cs`
- `Core/Ecom.Application/Features/PondAlert/Services/OxyAlertMessageBuilder.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertQueueService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertSchedulerService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyWarningHardwareService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertReminderService.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertPondKindResolver.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert/OxyAlertConstants.cs`
- `Core/Ecom.Application/Common/Configuration/OxyAlertSettings.cs`
- `Core/Ecom.Domain/Models/PondAlert/OxyAlertSessionState.cs`
- `Core/Ecom.Application/Features/DeviceControl/DeviceControlDtos.cs`
- `Core/Ecom.Application/Features/Device/Commands/UpdateDevice/UpdateDeviceCommandHandler.cs`
- `Core/Ecom.Application/Features/Device/Commands/UpdateDevice/UpdateDeviceValidator.cs`
- `Presentation/Ecom.API/Controllers/V1/PondAlertController.cs`

Related DI/config files:

- `Core/Ecom.Application/DependencyInjection.cs`
- `Infrastructure/Ecom.Infrastructure/DependencyInjection.cs`
- `Presentation/Ecom.API/appsettings.Development.json`

## 2. Historical Fix Verification

| ID | Original issue / claim | Current code evidence | Status | Notes |
|---|---|---|---|---|
| H1 | Acknowledge must stop warning immediately. | `AcknowledgeOxyAlertCommandHandler` sets `IsAcknowledged`, calls `IOxyWarningHardwareService.TurnOffAllAsync`, and records `WarningHardwareSilencedAt` only for a `Dispatched` result while retaining state/index for recovery reconciliation. | Confirmed by code | `NoTarget` is throttled for retry; dispatch exceptions retry without a false-success marker. |
| H2 | Oxy rule must not be hardcoded to Oxy #1 / Oxy #2. | Superseded for `Nursery` by BA rule: `DeviceNumber=21` is primary and `DeviceNumber=22` is backup when those numbers exist; fallback still uses sorted/runtime ordering. | Updated by OF8 | Docs should state the nursery-specific device-number contract, not a global static rule for every pond kind. |
| H3 | Pond type classification must be correct. | `OxyAlertPondKindResolver` maps `CreateCycle -> Veo`, `GenerateCycle -> Nursery`; fallback uses total Oxy count. | Partially confirmed | Code is clear, but BA/PM should confirm naming and fallback semantics. |
| H4 | Backend must not auto-control backup Oxy or PaddleWheel during Oxy incidents. | `HandleOxyStatusChangeCommandHandler` does not call support-device IoT control services. | Confirmed by code | IoT/firmware owns backup AirBlower and PaddleWheel automatic behavior. |
| H5 | Notification and speaker message should be consistent. | `OxyAlertMessageBuilder` centralizes notification/reminder text; speaker hardware receives `pondNo` and `alertDeviceId`. | Partially confirmed | Text consistency is implemented for notifications/reminders, not for full speaker text payload. |
| M1 | Escalation must come from configuration/options. | `OxyAlertReminderService` reads `IOptions<OxyAlertSettings>.EscalationAfterSeconds`. | Confirmed by code | Default value is 300 seconds if no config section exists. |
| M2 | WarningDevice pond-level mapping limitation must be documented. | `OxyWarningHardwareService` resolves WarningDeviceHub by `Pond.ZoneId`, then active installed warning devices. | Confirmed by code | Current model is zone-level; no pond-level WarningDevice mapping is used. |
| R1 | `OxiAlertPondsIndex` read-modify-write could lose active ponds under concurrent telemetry/ack/recovery. | Index add/remove now uses distributed lock key `OxyAlertWorker:PondsIndex`; worker cleanup removes stale ids from the latest index instead of rewriting an older snapshot. | Confirmed fixed by code | Active index remains the worker discovery source. |
| R2 | Normal reminder notification could publish too soon after the initial farmer notification. | `OxyAlertSchedulerService` now sends `RemindNormalAsync` only when `playCount > 0`. | Confirmed fixed by code | First Normal speaker play still controls warning hardware, but does not send a reminder notification. |
| R3 | Escalation could suppress Critical farmer reminders by updating `LastFcmpushed`. | `OxyAlertReminderService` now updates `EscalationNotificationSentAt` without overwriting `LastFcmpushed`. | Confirmed fixed by code | Farmer reminder cooldown and escalation timestamp are separated. |
| R4 | Acknowledge/worker cleanup could be blocked by warning hardware shutoff exceptions or false success. | Acknowledge marks state acknowledged, records only a dispatched shutdown, leaves state/index for recovery reconciliation, throttles `NoTarget`, and retries exceptions. | Confirmed fixed by code | API remains idempotent and successful shutdown commands are not repeated each tick. |
| R5 | Acknowledge log wording could incorrectly report missing state after handling existing state. | Acknowledge handler now logs separate existing-state and missing-state paths. | Confirmed fixed by code | API behavior remains idempotent. |
| R6 | Notification, reminder, or escalation side effects could block scheduler hardware flow. | `OxyAlertSchedulerService` wraps escalation, normal reminder, and critical reminder in local best-effort helpers. | Confirmed fixed by code | Notification failure is logged but no longer blocks hardware scheduling or slot rotation. |
| R7 | Scheduler warning hardware shutoff failures could abort scheduler state progression. | `OxyAlertSchedulerService` wraps scheduler `TurnOffAllAsync` cleanup calls in a local best-effort helper. | Confirmed fixed by code | Activation calls `SetCriticalAsync` and `SetNormalAsync` still throw because they represent failure to start warning hardware. |
| R8 | Critical -> Normal downgrade could keep stale Normal alert message. | `OxyAlertQueueService.ResetNormalStateAsync` now rebuilds `NormalAlertMessage` on downgrade even when a previous message exists. | Confirmed fixed by code | Current failed Oxy evaluation is used for Normal reminder text. |
| R9 | Initial notification dedup check was non-atomic. | `HandleOxyStatusChangeCommandHandler.TryAcquireNotificationDedupAsync` now uses `IDistributedLockService` around the existing dedup key get/set. | Confirmed fixed by code | Existing dedup key format is preserved; a derived lock key is used only for acquisition. |
| R10 | State could be deleted/recreated while Oxy was still faulting, resetting escalation/farmer baselines. | Handler uses `RecoveryPendingAt`/`LastFaultTelemetryAt` and recovery grace; notification timestamp writes skip missing or changed incidents rather than recreating state. | Confirmed fixed by source, pending runtime validation | Watch for `OXY_RECOVERY_PENDING`, `OXY_STATE_RESET_PREVENTED`, `OXY_NOTIFICATION_TIMESTAMP_SKIP`, and `OXY_RECOVERY_CONFIRMED_CLEAR`. |
| R11 | One two-device event could create Normal then Critical farmer notifications. | Status changes are normalized and merged through `HandleOxyStatusChangesCommand` before final severity notification. | Confirmed fixed by source, pending runtime validation | Test Code 300 and hub-timeout two-device batches. |
| R12 | Confirmed offline incident could disappear with lost Redis state. | Health processing repeatedly dispatches idempotent offline batches while the hub remains unhealthy. | Confirmed fixed by source, pending runtime validation | Recreation is input-driven and logs `OXY_OFFLINE_INCIDENT_RECREATED`. |

## 2A. Latest Regression Fix Review

| ID | Bug? / risk? | Review evidence | Resolved by | Changed files |
|---|---|---|---|---|
| OF1 | Bug - scheduler notification side effects blocked hardware flow. | Scheduler called escalation/reminders directly before hardware transitions. | Added failure-isolated helper calls for escalation, Critical reminder, and Normal reminder. | `OxyAlertSchedulerService.cs` |
| OF2 | Bug - stale Normal message after Critical -> Normal downgrade. | Queue reset skipped message rebuild when `NormalAlertMessage` was already non-empty. | Always rebuild Normal message from current evaluation when severity changes back to Normal. | `OxyAlertQueueService.cs` |
| OF3 | Risk - acknowledge hardware shutoff failure removed worker retry path. | Acknowledge removed pond from `OxiAlertPondsIndex` even after hardware shutoff threw. | Keep acknowledged pond indexed when hardware shutoff fails, so worker can retry best-effort shutoff. | `AcknowledgeOxyAlertCommandHandler.cs` |
| OF4 | Risk - scheduler hardware shutoff failure aborted state progression. | Scheduler called `TurnOffAllAsync` directly during interruption, rotation, completion, and idle cleanup. | Added best-effort scheduler shutoff helper with reason logging. | `OxyAlertSchedulerService.cs` |
| OF5 | Risk - initial notification dedup was non-atomic. | Dedup used separate cache get and set with no lock. | Wrapped dedup get/set with `IDistributedLockService`; kept `OxiAlertNotiDedup:{pondId}:{scope}:{severity}` unchanged. | `HandleOxyStatusChangeCommandHandler.cs` |
| OF6 | Bug - worker could record successful acknowledged shutdown without a dispatched Bell/Speaker command. | A no-target warning-device resolution previously completed without an outcome callers could distinguish from success. | Worker records silenced only on `Dispatched`, stores a retry-after value for `NoTarget`, and retries thrown dispatch failures on the next tick. | `IOxyWarningHardwareService.cs`, `OxyWarningHardwareService.cs`, `AcknowledgeOxyAlertCommandHandler.cs`, `OxyAlertQueueService.cs` |
| OF7 | Risk - initial notification failure could suppress immediate retry through an already-set dedup key. | Initial notification dedup key was acquired before publish. | Initial notification publish is failure-isolated and removes the dedup key on publish failure as best effort, so a later attempt can retry. | `HandleOxyStatusChangeCommandHandler.cs` |
| OF8 | Bug - BA rule for nursery Oxy #2 was not represented in device control. | Toggle/create/update paths treated every `AirBlower` as always-on and not controllable. | Centralized Oxy control rule: Oxy #1 (`DeviceNumber=21`) remains always ON/non-toggle; nursery Oxy #2 (`DeviceNumber=22`, `GenerateCycle`) defaults OFF and can toggle unless `Connected + Fault`; Oxy evaluator treats #21 as primary and #22 as backup for nursery ponds. | `DeviceControlDtos.cs`, `ToggleDeviceCommandHandler.cs`, `CreateDeviceCommandHandler.cs`, `AddDeviceToHubCommandHandler.cs`, `AssignDeviceToHubCommandHandler.cs`, `UpdateDeviceCommandHandler.cs`, `OxyAlertRuleEvaluator.cs` |
| OF9 | Bug - backend auto-controlled support devices during Oxy incidents though IoT/firmware owns that behavior. | Oxy alert handler previously called support-device IoT control for backup AirBlower and PaddleWheel. | Removed backend auto-control calls and service registration; warning Bell/Speaker scheduler remains unchanged. | `HandleOxyStatusChangeCommandHandler.cs`, `DependencyInjection.cs` |
| OF10 | Bug - installed hub devices could remain with `DeviceNumber=0`. | `UpdateDeviceCommandHandler` preserved `request.DeviceNumber ?? device.DeviceNumber`, so reinstall/update with a previous zero kept `0`; telemetry matching depends on `DeviceNumber == DeviceId`. | Update handler now auto-assigns a valid slot for `Installed + DeviceHubId` when number is null/`<=0`, assigns from the target hub when moving hubs, rejects invalid/duplicate positive slots, and telemetry missing-device reconciliation ignores payload ids `<=0`. | `UpdateDeviceCommandHandler.cs`, `UpdateDeviceValidator.cs`, `DeviceStatusTelemetryHandler.cs` |
| OF11 | Bug - one payload or hub timeout with two AirBlowers could notify Normal then Critical. | Per-device command dispatch evaluated intermediate severity. | `DeviceStatusTelemetryHandler` and `IoTBackgroundService` dispatch one normalized batch evaluated once by `HandleOxyStatusChangesCommandHandler`. | `HandleOxyStatusChangesCommand.cs`, `HandleOxyStatusChangeCommandHandler.cs`, `DeviceStatusTelemetryHandler.cs`, `IoTBackgroundService.cs` |
| OF12 | Bug - hub still offline but lost Redis state/index was not restored. | Offline failure dispatch previously occurred only on the first DB disconnected transition. | Every unhealthy health interval sends an idempotent confirmed-offline reconcile batch, permitting source-driven recreation. | `IoTBackgroundService.cs`, `HandleOxyStatusChangeCommandHandler.cs` |

## 3. Documentation-vs-Code Mismatches

| Topic | Old/possible claim | Actual code behavior | Status | Required action |
|---|---|---|---|---|
| Missing Code 300 payload item | Missing item means Oxy failed. | Missing devices are marked `DisConnected` and `UnDefined`; no Oxy failure command is directly dispatched. | Incorrect if claimed | Document as limitation / BA confirmation needed. |
| Disconnected device | Disconnected Oxy means failure. | Missing Code 300 items remain DB-only; a health-timeout-confirmed hub offline state dispatches a failure batch and continues reconciliation while unhealthy. | Updated boundary | Document only confirmed health source as incident input. |
| Static Oxy #1 primary | Oxy #1 is always primary and Oxy #2 is always backup. | Current code applies this for `Nursery` when `DeviceNumber=21/22` exist; other cases fall back to sorted/runtime ordering. | Updated | Preserve the nursery-specific rule only. |
| Worker discovery | Worker scans all Redis Oxy state keys. | Worker loads pond ids from `OxiAlertPondsIndex`. | Incorrect if claimed | Keep active-index model in docs. |
| Normal queue | Any Normal alert is queued. | Normal queue includes `Severity == Normal && AlertPondKind == Nursery`. | Partially implemented | Document the exact predicate. |
| Critical ordering | Ao veo always processed before ao nuoi. | Critical sorting currently groups `Nursery` before `Veo`, then sorts within priority. | Incorrect if claimed | Use code behavior or confirm BA change request. |
| Speaker message | Speaker receives full notification text. | Warning hardware sends `pondNo` and `alertDeviceId`; notifications/reminders use text builder. | Partially implemented | Separate text message from hardware command payload. |
| Escalation constant | `OxyAlertConstants.EscalationAfterSeconds` drives escalation. | Reminder service uses `OxyAlertSettings` options, not the constant. | Outdated | Prefer options in docs/code changes. |
| Test coverage | Oxy flow has automated regression tests. | No Oxy-specific tests were found. | Missing | Add tests before risky changes. |
| Active index mutation | Index list updates are naturally safe because each handler writes a normalized list. | Index updates require `OxyAlertWorker:PondsIndex` because telemetry, ack, recovery, and worker cleanup can mutate the same cached list. | Fixed in current code | Preserve the active-index lock pattern. |
| Normal reminder timing | The 5-second guard is enough to prevent spam. | Reminder must not publish on first Normal play; first activation is covered by initial notification. | Fixed in current code | Keep `playCount > 0` guard. |
| Escalation/reminder timestamp | Escalation can update `LastFcmpushed`. | `LastFcmpushed` is farmer-reminder cooldown state; escalation uses `EscalationNotificationSentAt`. | Fixed in current code | Do not couple escalation to farmer reminder cooldown. |
| Installed device number | Installed hub device can safely keep `DeviceNumber=0` until telemetry arrives. | Telemetry matches by `DeviceNumber == DeviceId`; installed hub devices must have a valid positive slot before telemetry/toggle can work. | Fixed in update path | Existing bad rows still need update/repair. |

## 4. Implemented Behavior Summary

Telemetry:

- Code `300` is routed by `IoTConnectionManager`.
- Only AirBlower devices participate in Oxy incident dispatch.
- Payload status maps to `Fault`, `On`, or `Off`.
- Final payload item per positive device id is applied and explicit AirBlower changes are sent as one post-save batch.
- Missing payload devices are DB-state updates, not direct alert triggers.
- Confirmed hub-offline health checks repeatedly dispatch an idempotent offline batch so missing Redis state can be restored.

Rules:

- `Veo` any failed Oxy -> Critical.
- `Nursery` one failed Oxy with another running -> Normal.
- `Nursery` two or more failed Oxy -> Critical.
- For `Nursery`, Oxy #1 is `DeviceNumber=21` primary and Oxy #2 is `DeviceNumber=22` backup when those device numbers exist; other cases fall back to existing ordering.
- Oxy #1 remains always ON/non-toggle. Nursery Oxy #2 defaults OFF and can be toggled manually unless it is `Connected + Fault`.
- Installed hub devices must not remain at `DeviceNumber=0`; update/reinstall auto-assigns a valid slot when missing or zero, and rejects invalid/duplicate positive slots.

State:

- Per-pond Redis state is `OxiAlertState:{pondId}`.
- Acknowledge marker is `OxiAlertAck:{pondId}` with 24-hour TTL.
- Active worker index is `OxiAlertPondsIndex`.
- Active worker index mutations are protected by `OxyAlertWorker:PondsIndex`.
- Scheduler state is `OxiAlertSchedulerState`.
- Notification dedup key is `OxiAlertNotiDedup:{pondId}:{scope}:{severity}`.
- Initial notification dedup acquisition is protected by a derived distributed lock key, while preserving the existing dedup key format.
- If initial farmer notification publishing fails, the dedup key is removed as best effort so a later telemetry attempt can retry notification.
- If initial farmer notification publishing succeeds but state is missing, suppressed, or belongs to a newer incident, the handler skips timestamp persistence and does not recreate state.
- Temporary full recovery sets `RecoveryPendingAt` and keeps state/index through a 120-second recovery grace window.
- FAULT returning during recovery grace preserves the existing incident timeline and prevents farmer/admin baseline reset.
- `WarningHardwareSilenceRetryAfter` throttles acknowledged shutdown retries only when warning hardware resolution returns `NoTarget`.

Worker:

- Runs every 2 seconds.
- Uses distributed lock `OxyAlertWorker:GlobalScheduler`.
- Reconciles Redis state with DB device state.
- Removes missing stale index entries.
- Does not remove inactive recovery-pending states while they are inside the recovery grace window.
- Removes acknowledged or fully recovered states only after warning hardware cleanup/confirmed recovery rules allow cleanup.
- Removes stale pond ids from the latest active index under `OxyAlertWorker:PondsIndex`.
- Keeps acknowledged ponds indexed until recovery cleanup, records hardware silencing only after `Dispatched`, throttles `NoTarget`, and retries thrown dispatch failures.
- Continues fully recovered cleanup even if warning hardware shutoff fails.

Scheduler:

- Critical interrupts Normal.
- Critical uses 60-second slots: Bell first 30 seconds, Speaker after 30 seconds.
- Normal uses Speaker only: first play 300 seconds, repeats 30 seconds after a 900-second global cooldown.
- Normal notification reminders are sent only on repeat Normal plays, not on the first Normal speaker play.
- Escalation, Critical reminder, and Normal reminder failures are logged and do not block scheduler hardware flow.
- Scheduler warning-hardware shutoff cleanup is best effort; failures are logged and scheduler state progression continues.

Hardware:

- Warning hardware is zone-level through `Pond.ZoneId`.
- Shutdown reports `Dispatched` only after all resolved Bell/Speaker stop commands complete; no pond/target resolves as `NoTarget`.
- Normal does not auto-turn-on backup AirBlower from backend.
- Critical does not auto-turn-on PaddleWheel from backend.
- Code 300 missing-device reconciliation ignores payload entries with `DeviceId <= 0`, preventing non-device data from masking bad zero-numbered DB rows.

Acknowledge:

- `POST /api/v1/PondAlert/acknowledge/{pondId}`
- Sets `IsAcknowledged` when state exists.
- Turns off warning hardware as best effort.
- Keeps the acknowledged pond in `OxiAlertPondsIndex` for recovery reconciliation.
- Records `WarningHardwareSilencedAt` only after shutdown returns `Dispatched`; `NoTarget` creates a 60-second retry-after marker and exceptions retry on the next tick.
- Sets and verifies `OxiAlertAck:{pondId}` first.
- Returns success even when state is already missing after the ack marker is verified.
- If state lock times out after marker verification, returns success because telemetry suppression is already active.

Changed file summary:

- `IOxyWarningHardwareService.cs`, `OxyWarningHardwareService.cs`: classify Bell/Speaker shutdown as dispatched or no-target.
- `AcknowledgeOxyAlertCommandHandler.cs`: keep acknowledged alerts retryable without recording no-target false success.
- `HandleOxyStatusChangesCommand.cs`, `HandleOxyStatusChangeCommandHandler.cs`: merge batch changes and issue one initial notification for final severity.
- `DeviceStatusTelemetryHandler.cs`, `IoTBackgroundService.cs`: post-save batch dispatch and persistent confirmed-offline reconciliation.
- `OxyAlertConstants.cs`: centralize Oxy Redis keys and scheduler timing values used by worker/scheduler services.
- `OxyAlertQueueService.cs`: keep acknowledged ponds indexed until worker hardware shutoff succeeds, and rebuild Normal messages from current evaluation data.
- `OxyAlertReminderService.cs`: keep reminder/escalation timestamps aligned with current incident while preserving role-only recipient contract.
- `OxyAlertSchedulerService.cs`: isolate reminder/escalation/shutoff side effects so scheduler hardware flow does not stop unexpectedly.

## 5. Code Quality Findings

Strengths:

- Rule evaluation is centralized in `OxyAlertRuleEvaluator`.
- Message building is centralized in `OxyAlertMessageBuilder`.
- Queue service reconciles Redis with DB instead of trusting stale state.
- Scheduler uses distributed lock for multi-instance safety.
- Acknowledge is safe to call repeatedly from API behavior.

Risks:

- No automated tests were found for Oxy flow.
- Redis key strings are duplicated in some handlers instead of all coming from one shared constant.
- `OxyAlertConstants.EscalationAfterSeconds` is misleading because escalation uses options.
- WarningDevice resolution is zone-level and can be too broad if pond-level targeting is required.
- Mojibake comments/messages make maintenance and BA validation harder.
- Redis/cache write failures are logged by cache infrastructure; production Redis failure behavior still needs operational validation.
- Existing data rows already saved with `Installed + DeviceHubId + DeviceNumber=0` require an update/reinstall action or controlled DB repair; the code fix prevents the bad state from persisting through the update path going forward.
- State-reset/admin-escalation fixes are build-verified but still need runtime validation with real Redis, Code 300 telemetry cadence, FCM recipients, and WarningDevice availability.
- Latest regression fixes are source-reviewed and build-verified; runtime validation against real Redis/FCM/WarningDevice integrations is still pending.

## 6. Recommended Next Fixes

Priority 1:

- Add unit tests for `OxyAlertRuleEvaluator`.
- Add handler tests for create/update/recovery of `OxiAlertState:{pondId}`.
- Add acknowledge tests for hardware stop failure isolation, silenced timestamp persistence, and retry suppression.
- Add scheduler tests for Critical interruption, Normal cooldown, and Normal reminder repeat-only behavior.
- Add queue tests for locked `OxiAlertPondsIndex` cleanup preserving unrelated active ponds.
- Add regression tests for notification side-effect isolation, Critical -> Normal message rebuild, acknowledge retry index retention, scheduler shutoff failure isolation, and locked notification dedup.
- Add update-device tests for `Installed + DeviceHubId + DeviceNumber null/0`, moving hubs without a number, invalid positive number, duplicate slot, and uninstalled reset to zero.

Priority 2:

- Add explicit `OxyAlert:EscalationAfterSeconds` to environment configs.
- Decide whether missing telemetry payload or `Disconnected` should become failure triggers.
- Decide whether WarningDevice schema needs pond-level mapping.

Priority 3:

- Move Redis key constants to a shared location accessible from Application and Infrastructure.
- Remove or rename unused escalation constant.
- Clean mojibake comments/messages when touching Oxy files.

## 7. Verification Notes

Performed during reconciliation:

- Read historical Oxy Markdown documents.
- Searched Oxy-related source files and symbols.
- Inspected telemetry routing, command handlers, evaluator, queue, scheduler, hardware control, reminder, escalation, DI, and API endpoint.
- Searched for test projects and common test attributes.
- Checked referenced source paths exist.
- Verified the Oxy regression fixes through user-run manual build output:
  - `Ecom.Application` succeeded.
  - `Ecom.Infrastructure` succeeded.
  - `Ecom.API` succeeded.
  - Build summary: 3 succeeded, 0 failed, 1 up-to-date, 0 skipped.

Performed during latest regression fix:

- Implemented source changes for scheduler side-effect isolation, queue Normal message rebuild, acknowledge retry preservation, scheduler shutoff isolation, and locked notification dedup.
- Ran targeted source inspections with `rg` to confirm direct scheduler reminder/shutoff call sites were replaced by local helper methods.
- Implemented follow-up fixes for worker acknowledged cleanup retry preservation, initial notification publish isolation, and best-effort dedup-key cleanup after publish failure.
- Implemented Oxy BA/device-control fixes: nursery Oxy #1/#2 rule, no backend auto-control of backup AirBlower/PaddleWheel, and `DeviceNumber=0` repair guard for installed hub devices.
- Verified latest changes with user-requested local builds:
  - `dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore` succeeded with existing warnings.
  - `dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore` succeeded with existing warnings.
  - `dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore` succeeded with existing warnings.

Performed during 20/05 state-reset/admin-escalation fix:

- Implemented Redis-only recovery fields in `OxyAlertSessionState`.
- Implemented recovery grace in telemetry handler and worker.
- Superseded state recreation after initial farmer notification Phase C `StateMissing`; current behavior skips timestamp persistence when the incident no longer exists or no longer matches.
- Implemented escalation ack-marker guard and escalation timestamp read-back verification.
- Verified source/build:
  - `dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore` succeeded with existing warnings.
  - `dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore -p:UseSharedCompilation=false` succeeded with existing warnings.
  - `dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore -p:OutDir=C:\tmp\Ecom-api-build\` succeeded with existing warnings.

Not performed:

- No runtime IoT/Redis/FCM/WarningDevice test was executed.
- No automated Oxy tests were run because no Oxy-specific tests were found.
- Runtime verification against real Redis/FCM/WarningDevice integrations is still pending.

## 8. Final Score

- Implementation correctness: 8.7/10
- Documentation accuracy after reconciliation: 9/10
- Test coverage: 2/10
- Maintainability: 7.5/10
- Operational risk: Medium

