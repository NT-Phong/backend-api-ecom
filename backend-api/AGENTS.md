# AGENTS.md

## Purpose

Guide AI coding agents working on the Ecom backend API safely and efficiently. This file is the bootstrap only; keep detailed routing, prior investigations, examples, and scripts under `.agents/`.

## Project Model

Ecom is a .NET backend API for aquaculture operations: ponds, zones, seasons, warehouse flows, IoT devices, telemetry, realtime scale weighing, notifications, reports, and camera/media integrations.

Architecture is pragmatic Clean Architecture with CQRS and MediatR:

```text
Presentation -> Application -> Domain
Infrastructure -> Application/Domain abstractions
```

- `Presentation/Ecom.API`: controllers, SignalR hubs, middleware, Swagger/ReDoc, health checks, runtime setup.
- `Core/Ecom.Application`: CQRS features, MediatR handlers, validators, DTOs, services, abstractions.
- `Core/Ecom.Domain`: entities, invariants, constants, messages, domain events, contracts.
- `Infrastructure/Ecom.Infrastructure`: EF Core/PostgreSQL, UnitOfWork/repositories, Redis, IoT, messaging, FCM, security, camera/media, workers.

Application may pragmatically reference EF Core abstractions, ASP.NET Core `IFormFile`, cache, SignalR, and IoT abstractions. Do not "purify" this unless explicitly asked.

## Context Budget Rules

If these rules are already configured in the agent tool, do not ask the user to paste or resend an initialization prompt. Start from the task and load only the smallest task-specific context.

Fresh thread:
1. Use this file and the always-on rules.
2. Use `.agents/context/task-router.md` if present to choose domain context.
3. Use `.agents/context/quick-scan.md` when the task starts from a route, symbol, log line, error code, or broad feature name and the right files are not obvious.
4. Read `.agents/rules/spec-orchestrator.md` only for spec, orchestration, or handoff work.

Follow-up task:
- Do not reload stable bootstrap/context unless the user says it changed, compaction removed needed details, or exact wording is needed.
- Prefer targeted source search over broad context reading.
- Source code wins when source and memory disagree.

## Task Workflow

1. Restate the task briefly.
2. Classify it: New Feature Design, Bug Debugging, Refactoring, Architecture Review, or Implementation.
3. Identify the smallest boundary: route, controller, command/query, handler, validator, entity, repository, hub, worker, integration, Redis/SignalR/IoT contract.
4. Search before opening files.
5. Inspect a nearby working pattern before editing.
6. Make the smallest safe change.
7. Preserve local conventions over generic best practices.
8. Verify with the narrowest relevant check, or explain why verification was not run.
9. Report summary, files changed, verification, and risks.
10. Update a relevant skill/domain report only when the task creates durable knowledge.

## Search Strategy

Prefer `rg`:

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\find-related-files.ps1 -Term "<term>"
powershell -ExecutionPolicy Bypass -File .agents\scripts\summarize-module.ps1 -Path "Core\Ecom.Application\Features\<Feature>"
rg "Route|HttpGet|HttpPost|HttpPut|HttpDelete|<route-term>" Presentation/Ecom.API/Controllers
rg "<ActionOrFeature>|IRequest|IRequestHandler" Core/Ecom.Application/Features
rg "AbstractValidator|RuleFor" Core/Ecom.Application/Features/<Feature>
rg "EnableUnitOfWork|SaveChangesAsync|CommitTransactionAsync|Repository<" Core/Ecom.Application/Features/<Feature>
rg "Permissions\.|Authorize\(" Presentation/Ecom.API Core/Ecom.Application/Features/<Feature>
rg "ScaleSession_|Scale_|Device_|Cycle_|TelemetryHub|Code 503" Presentation Infrastructure Core
rg "TelemetryHandler|IoT|EventHub|MQTT|Code 300|Oxy" Infrastructure/Ecom.Infrastructure
rg "Fact|Theory|describe\(|it\(|test\("
```

Open large files only after symbol search, especially large controllers, `DependencyInjection.cs`, report generators, IoT managers, `Permissions.cs`, and `ErrorCodes.cs`.

## Code Quality Rules

- Keep controllers thin; put business rules in Application/Domain.
- Use MediatR commands/queries and FluentValidation according to nearby patterns.
- Use existing `TResult`, `ApiResponse`, `MessageKey`, and `ErrorCodes` conventions.
- Use `[EnableUnitOfWork]`, `IUnitOfWork`, repositories, `IApplicationDbContext`, and explicit `SaveChangesAsync` according to local patterns.
- Prefer domain methods for state transitions when they exist.
- Do not add abstractions unless they remove real duplication, match an existing pattern, or materially improve isolation/testability.
- Do not broad-refactor large controllers/handlers during a local fix.
- Add comments only for non-obvious domain rules or important tradeoffs.

## Non-Negotiable Safety Rules

Do not change these without explicit approval:
- public APIs, routes, request/response DTOs, or handler signatures,
- auth, JWT/current-user behavior, permissions, roles, seeded policies, Logout,
- EF migrations, model snapshots, `ApplicationDbContext`, entity configurations,
- Redis key formats,
- SignalR group names,
- IoT telemetry codes/contracts or device-control behavior,
- `appsettings*.json`, `.env`, certificates, secrets, keys, local runtime config,
- dependencies, CI/CD, deployment, infrastructure.

Never hardcode or log secrets, JWTs, refresh tokens, FCM tokens, IoT credentials, Basic Auth credentials, connection strings, or raw file payloads.

Do not rename typo-bearing existing files or public types only for cleanup. Names such as `Hanlder`, `Vadilator`, and `CreateImportRecept` may be referenced.

## High-Risk Areas

Load risk/domain context before touching:
- Auth, JWT, refresh tokens, Logout, current-user claims, roles, policies, permissions.
- EF migrations, DbContext, entity configurations, UnitOfWork, repository behavior.
- Scale realtime: Code 503 telemetry, Redis contexts/snapshots, SignalR groups, session/scale/cycle identity.
- IoT control: DeviceHub, WarningDeviceHub, firmware, schedules, direct methods.
- FCM notifications and recipient resolution.
- Warehouse receipts, inventory check, stock transfer.
- Background workers, camera/media services, Basic Auth/TLS handling.
- Large report/export generation.

## Verification

Builds are user-run by default unless the user asks the agent to run them. For code changes, recommend the narrowest relevant command. If no dedicated test project exists, state the gap and recommend focused build verification.

Docs or guidance changes should run available guidance/skill verification scripts when present.

## Final Response Format

- Summary
- Files changed
- Verification run
- Risks / follow-ups

