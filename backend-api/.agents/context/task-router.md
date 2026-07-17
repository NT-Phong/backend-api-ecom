# Task Router

Use this file during Plan to decide which guidance, context, skill reports, and source areas to load. Keep routing narrow. If a task touches multiple domains, start with the highest-risk domain and load additional routes only when evidence requires it.

## Always Load

Load these once per fresh thread, then reuse the remembered summary unless the user says they changed:

1. `AGENTS.md`
2. `.agents/rules/agent-directives.md`
3. `.agents/rules/optimized-workflow.md`
4. `.agents/rules/safety-constraints.md`
5. `.agents/rules/context-efficiency.md`
6. `.agents/rules/spec-orchestrator.md` (if acting as spec architect/orchestrator)
7. `.agents/context/project-map.md`
8. `.agents/context/quick-scan.md` (when locating source from a route, symbol, log line, error code, or broad feature name)
9. `.agents/context/agent-operating-guide.md`
10. `.agents/context/architecture.md`
11. `.agents/context/conventions.md`

Load `.agents/context/risk-map.md` only when the task touches or may touch high-risk modules, or when risk classification is unclear.

## Context Reuse Decision

At the start of a follow-up task in the same thread:
- If the core context was already loaded, do not reload it.
- If the task gives a specific route, command, handler, entity, error, or module, use quick scan or search source directly.
- If the task is broad or ambiguous, use this router and `project-map.md` rather than reading every context file.
- If compaction removed details or the task needs exact rule wording, reload the smallest canonical file.

## By Task Type

| Task Type | Load | Primary Output |
| --- | --- | --- |
| New Feature Design | `.agents/context/architecture.md`, `.agents/context/conventions.md`, `.agents/context/tech-stack.md`, relevant domain skill report | Plan with API boundary, command/query shape, validation, persistence, verification, and report-update target |
| Bug Debugging | `.agents/context/debugging.md`, `.agents/context/risk-map.md`, relevant domain report, `.agents/skills/backend-api-architecture/templates/debugging-report.md` | Root cause, evidence, smallest safe fix, verification, report update |
| Refactoring | `.agents/rules/code-quality.md`, `.agents/context/conventions.md`, `.agents/skills/backend-api-architecture/templates/refactor-plan.md` | Small refactor plan, constraints, files to change, verification |
| Architecture Review | `.agents/context/codebase-analysis.md`, relevant references under `.agents/skills/backend-api-architecture/references/` | Findings, impact, evidence, adjustment plan, score, next actions |

## By Domain

| Domain / Signal | Read First | Then Inspect Source |
| --- | --- | --- |
| General API, CQRS, MediatR, UnitOfWork | `.agents/skills/backend-api-architecture/SKILL.md`, `.agents/skills/backend-api-architecture/references/execution-workflow.md` | `Presentation/Ecom.API/Controllers`, `Core/Ecom.Application/Features`, `Core/Ecom.Domain`, `Infrastructure/Ecom.Infrastructure` |
| Auth, JWT, Logout, permissions | `.agents/context/risk-map.md`, `.agents/skills/backend-api-architecture/references/security-review.md` | `Core/Ecom.Application/Features/Auth`, `Infrastructure/Ecom.Infrastructure/Security`, `Core/Ecom.Domain/Constants/Permissions.cs` |
| Scale realtime, live weight, Code 503 | `.agents/skills/scale_optimise/SKILL.md`, `.agents/skills/scale_optimise/scale-review.md`, `.agents/skills/scale_optimise/IMPLEMENTED_PLANS.md`, `.agents/skills/scale_optimise/PLAN.md` | Scale controllers, `ScaleRecord` features, telemetry handlers, Redis cache, `TelemetryHub` |
| Camera/media, live stream, MediaMTX, viewer limit | `.agents/skills/camera-media/SKILL.md` | `CameraController`, camera commands/queries under `Features/Device`, `IMediaMtxService`, `MediaMtxService`, camera workers, notification target helpers |
| FCM, notification delivery, warehouse notification recipients | `.agents/skills/FCM-notification/SKILL.md`, then `report-bug.md` or `review.md` only as needed | `NotificationService`, `BulkNotificationEventHandler`, `FcmNotificationEventHandler`, `FirebaseFcmService`, `NotificationRecipientResolver` |
| Oxy, pond alerts, Code 300, warning hardware | `.agents/skills/pond_error/SKILL.md`, then `report-bug.md` or `flow-worker.md` only as needed | Oxy alert handlers, queue, scheduler, reminder, hardware services, `IoTConnectionManager` |
| Device hub / DeviceControl / Code 300 telemetry | `.agents/skills/Device-system/SKILL.md` | DeviceHub, Device, DeviceControl, telemetry handlers, warning hardware, schedules |
| Device hub connection / uninstall behavior | `.agents/skills/device-connection/SKILL.md`, then `report-bug.md` only as needed | `Device` feature handlers, device hub assignment, domain device update behavior |
| Warehouse, receipts, inventory check | `.agents/context/risk-map.md`, FCM report if notifications are involved | `ImportReceipt`, `ExportReceipt`, `InventoryCheck`, warehouse notification paths |
| Farm Plan / Ke hoach | `.agents/skills/FarmPlan-system/SKILL.md`, then `references/base-plan.md` only when implementation or review needs accepted BA decisions | `PlanController`, `Core/Ecom.Application/Features/Plan`, `Plan` domain/entity/configuration, permissions |
| Redis cache, distributed locks | `.agents/context/risk-map.md`, domain skill if present | cache service, key constants, lock service, affected handlers |
| SignalR hubs / realtime groups | `.agents/context/risk-map.md`, relevant realtime skill | hub methods, notification services, group name usage |
| EF migrations / schema | `.agents/context/risk-map.md`, `.agents/context/commands.md` | ask before editing migrations or snapshots |

## Routing Rules

- Prefer symbol search before opening domain files.
- Prefer `.agents/scripts/find-related-files.ps1` for the first broad term, then narrow with `rg` before opening large files.
- If a domain report exists, read it before source only for orientation; source remains the current truth.
- If source invalidates a report, update that report after the task.
- If no domain route fits, use backend-api-architecture as the default route.
- Do not load every skill report for a cross-cutting task. Load one, inspect evidence, then add only the next necessary route.
- Do not load `codebase-analysis.md`, `tech-stack.md`, `testing.md`, or `commands.md` unless the task type requires them.

