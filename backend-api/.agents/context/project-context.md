# Project Context

## Product
Ecom backend API supports aquaculture operations, warehouse flows, IoT device management, telemetry, notifications, scale weighing sessions, ponds, zones, seasons, materials, receipts, reports, and related operational workflows.

## Repository Type
This repository is a .NET backend API using Clean Architecture:
- Domain model and contracts in `Core/Ecom.Domain`.
- Application CQRS use cases in `Core/Ecom.Application`.
- Infrastructure implementations in `Infrastructure/Ecom.Infrastructure`.
- HTTP API, SignalR hubs, middleware, and filters in `Presentation/Ecom.API`.

## Agent Goal
Help agents make small, correct, architecture-aligned changes with minimal context load.

## Operating Guide
Use `.agents/context/agent-operating-guide.md` as the concise task framing guide for follow-up work. It summarizes the project mental model, context loading defaults, protected contract boundaries, and response expectations.

## Canonical Memory
- Bootstrap: `AGENTS.md`.
- Reusable rules: `.agents/rules/`.
- Project facts: `.agents/context/`.
- Backend architecture procedure: `.agents/skills/backend-api-architecture/SKILL.md`.
- Prior domain investigations: `.agents/skills/**`.

## High-Value Prior Reports
- Scale realtime and optimization: `.agents/skills/scale_optimise/scale-review.md`, `.agents/skills/scale_optimise/PLAN.md`.
- FCM notification flow: `.agents/skills/FCM-notification/report-bug.md`.
- Pond/Oxy alert flow: `.agents/skills/pond_error/`.
- Warning device control: `.agents/skills/warning-device-control/work-flow.md`.

