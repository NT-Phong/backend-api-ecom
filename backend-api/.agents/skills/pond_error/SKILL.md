---
name: pond-error
description: Use this skill when debugging, reviewing, planning, or changing Ecom pond Oxy alert flows, Code 300 telemetry fault handling, AirBlower fault/recovery state, Oxy Redis state, acknowledgement, reminder/escalation, warning hardware, and pond alert notifications.
argument-hint: "Task focus: Oxy alert | PondAlert | Code 300 | AirBlower fault | Redis state | acknowledge | escalation"
user-invocable: true
---

# Pond Oxy Alert Skill

## Purpose

Guide work on pond Oxy incident detection, acknowledgement, recovery, reminder/escalation, warning hardware, and notification flows.

## When To Use

Use this skill for:
- Oxy/AirBlower fault and recovery behavior.
- Code 300 telemetry that affects pond alerts.
- `OxiAlertState:{pondId}`, ack markers, pond alert Redis indexes, or recovery grace.
- farmer/admin notification timing for Oxy incidents.
- warning hardware shutoff or scheduler/reminder workers.

## Required References

Read only what the task needs:

```text
docs/oxy/visily-su-co-1-tu-1-ao.md            (Visily Original Business Rules: 1 Hub 1 Pond)
docs/oxy/visily-su-co-nhieu-tu-nhieu-ao.md    (Visily Original Business Rules: Multi Hub Multi Pond)
.agents/skills/pond_error/report-bug.md
.agents/skills/pond_error/flow-worker.md
.agents/skills/pond_error/bug-20-05.md
.agents/skills/pond_error/api-doc.md
.agents/skills/pond_error/doc/
```

Use the Visily business rule specification files (`visily-su-co-1-tu-1-ao.md` and `visily-su-co-nhieu-tu-nhieu-ao.md`) as the primary reference to reconcile business logic requirements. Use `report-bug.md` as the technical reference for current source code implementation status, and `flow-worker.md` when analyzing worker, scheduler, or queue execution.

## Current Rules

- Recovery must be explicit and stable before clearing failed devices or deleting alert state.
- Do not infer recovery only from DB status when no recovery telemetry exists.
- Ack marker behavior is part of the suppression contract.
- Current output suppression mode (2026-06-29): `OxyAlertOutputSwitch.OutputsEnabled = false` hides Oxy notification/FCM and Bell/Speaker output, while telemetry, rule evaluation, Redis state, ack marker, active index, and recovery cleanup remain active.
- When outputs are disabled, do not treat warning-hardware no-op as a dispatched/silenced hardware command. Acknowledged ponds must not be removed from `OxiAlertPondsIndex` only because output was suppressed.
- Fast restore path: set `OxyAlertOutputSwitch.OutputsEnabled = true`, rebuild Application and Infrastructure, then verify initial farmer notification, Normal/Critical reminder, escalation, Oxy-off-too-long notification, and Bell/Speaker paths.
- Preserve existing Redis key formats and Oxy alert lifecycle semantics unless explicitly approved.
- Source code and runtime logs outrank older report text.

## Source Areas

Start with targeted search:

```powershell
rg "OxyAlert|OxiAlert|PondAlert|AirBlower|Code 300|HandleOxyStatusChange|AcknowledgeOxyAlert" Core Infrastructure Presentation
```

Likely areas:
- `Core/Ecom.Application/Features/PondAlert`
- `Infrastructure/Ecom.Infrastructure/IoT/OxyAlert`
- `Infrastructure/Ecom.Infrastructure/IoT/DeviceStatusTelemetryHandler.cs`
- pond alert controllers and DTOs.

## Protected Contracts

Do not change Redis key formats, warning hardware behavior, notification recipient rules, public alert APIs, or IoT Code 300 payload handling without explicit approval.

## Verification

For code changes, recommend narrow Application/Infrastructure/API builds as appropriate. Runtime verification should include fault, ack, recovery grace, escalation, and worker cleanup observations when those paths are touched.

