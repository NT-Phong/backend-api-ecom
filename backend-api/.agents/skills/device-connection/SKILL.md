---
name: device-connection
description: Use this skill when debugging, reviewing, planning, or changing Ecom device connection, device hub assignment, installation/uninstallation behavior, DeviceHubId cleanup, device code generation, and physical device state transitions.
argument-hint: "Task focus: device uninstall | DeviceHubId cleanup | installation status | device code | connection status"
user-invocable: true
---

# Device Connection Skill

## Purpose

Guide work on device installation state, DeviceHub assignment, and uninstall cleanup without reintroducing old attachment bugs.

## When To Use

Use this skill for:
- `PUT /api/v1/device/{id}` update behavior.
- `InstallationStatus = Uninstalled` or `isInstalled = false`.
- stale `DeviceHubId`, `PondName`, `DeviceCode`, `DeviceNumber`, or physical assignment data.
- device connection state changes caused by install/uninstall.

For broader DeviceHub, DeviceControl, Code 300 telemetry, or toggle behavior, prefer `device-system`.

## Required Reference

Read only when this domain is touched:

```text
.agents/skills/device-connection/report-bug.md
```

That report records the known bug where uninstalled devices stayed attached to a hub when frontend payloads still included the old `DeviceHubId`.

## Current Rules

- Uninstall intent must win over stale hub fields sent by clients.
- Uninstalled devices should clear physical hub assignment values and return to undefined connection/status state.
- Device code behavior for uninstalled devices should follow the existing source convention.
- Source code is current truth; use the report for context, then verify against `UpdateDeviceCommandHandler` and nearby validators/entities.

## Source Areas

Start with targeted search:

```powershell
rg "UpdateDeviceCommand|InstallationStatus|DeviceHubId|GenerateDeviceCode|Uninstalled" Core Presentation Infrastructure
```

Likely areas:
- `Core/Ecom.Application/Features/Device/Commands/UpdateDevice`
- Device domain entity methods.
- Device validators and DTOs.

## Protected Contracts

Do not change public API, request DTOs, enum values, seeded data, or device-code format without explicit approval.

## Verification

For code changes, recommend the narrowest build covering Application and API. If no tests cover this flow, report the test gap and include a manual payload check for uninstall with stale `DeviceHubId`.

