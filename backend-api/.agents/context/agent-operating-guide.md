# Agent Operating Guide

## Purpose
Help agents answer and implement Ecom backend tasks with the right project framing, without repeatedly loading broad context.

## Default Mental Model
Ecom is an aquaculture operations backend with warehouse, pond, zone, season, IoT device, telemetry, notification, realtime weighing, reporting, and media/camera workflows.

The codebase uses pragmatic Clean Architecture:
- Presentation maps HTTP/SignalR requests to MediatR.
- Application owns CQRS use cases, validation, orchestration, and application-facing abstractions.
- Domain owns entities, invariants, constants, and contracts.
- Infrastructure implements persistence, Redis, IoT, messaging, FCM, security, camera/media, telemetry, and workers.

Do not try to purify existing pragmatic deviations unless the user asks for architecture refactoring.

## Task Handling Defaults
- Answer the task the user gave, not a broader nearby problem.
- Start from the requested feature, route, handler, entity, error, or log.
- Search by symbol or domain term before opening source files.
- Inspect one nearby pattern before editing.
- Make the smallest safe change that satisfies the request.
- Preserve public API, database schema, auth behavior, permission constants, Redis keys, SignalR groups, and IoT telemetry contracts unless explicitly approved.
- Do not change broad `catch (Exception)` blocks only for style.
- Do not rename typo-bearing public files/types as cleanup.

## Context Loading Defaults
- In a fresh thread, load AGENTS, core rules, project map, task router, architecture, conventions, and this guide.
- In an ongoing thread, reuse loaded context and read only task-specific files.
- Load `risk-map.md` only for protected or high-risk areas.
- Load `debugging.md` only for bug investigations.
- Load `commands.md` and `testing.md` only when verification is involved.
- Load `codebase-analysis.md` only for broad architecture reviews.
- Load domain skill reports only for the touched domain.

## Response Defaults
- For implementation tasks, report: Summary, Files changed, Verification run, Risks / follow-ups.
- For reviews, lead with findings and file evidence.
- For questions, answer directly and mention only the project context needed to support the answer.
- Keep final answers delta-first: what changed, what was verified, and what remains risky or unknown.

## Source Of Truth
Source code is current truth. Context files and skill reports are routing aids. If source and memory disagree, trust source and update the relevant context or report when the task confirms a durable change.

