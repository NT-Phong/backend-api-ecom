---
name: backend-api-architecture
description: Use this skill when analyzing, designing, reviewing, debugging, or refactoring Ecom backend API architecture. Use it for controllers, routes, MediatR commands/queries, handlers, validators, repositories, Clean Architecture boundaries, UnitOfWork, EF Core, Redis, SignalR, IoT telemetry, security, performance, and API testing.
argument-hint: "Task focus: architecture review | flow analysis | bug debug | new feature design | refactor plan | implementation"
user-invocable: true
---

# Backend API Architecture Skill

## Purpose
Guide agents to analyze and modify Ecom backend API architecture safely, consistently, and efficiently.

## Required Context
Before using this skill in a fresh thread, read:
1. `AGENTS.md`
2. `.agents/rules/context-efficiency.md`
3. `.agents/rules/spec-orchestrator.md` (if creating specs for downstream execution)
4. `.agents/context/project-map.md`
5. `.agents/context/task-router.md`
6. `.agents/context/agent-operating-guide.md`
7. `.agents/context/architecture.md`
8. `.agents/context/conventions.md`

In a thread where those files were already loaded, reuse the remembered summary and do not reload them unless exact wording is needed.

Load optional context only when the task needs it:
- `.agents/context/tech-stack.md` for framework, package, runtime, or dependency questions.
- `.agents/context/codebase-analysis.md` for broad architecture reviews, not normal implementation.
- `.agents/context/risk-map.md` when touching high-risk modules.
- `.agents/context/commands.md` before recommending or running verification.
- `.agents/context/testing.md` when verification or tests are involved.
- `.agents/context/debugging.md` when fixing bugs.

For reusable analysis prompts, use:

```text
.agents/skills/backend-api-architecture/analysis-command-template.md
```

For the standard execution workflow and report-update protocol, use:

```text
.agents/skills/backend-api-architecture/references/execution-workflow.md
.agents/skills/backend-api-architecture/templates/request-intake.md
.agents/skills/backend-api-architecture/templates/skill-report-update.md
```

## Standard Execution
Run backend work as:

```text
Plan -> Implement Plan -> Report -> Update skill files
```

During Plan, normalize the request into task type, short description, boundary/module, optional details, and expected output. During Update skill files, revise the relevant domain report only when the task created reusable facts, completed plans, or changed risks.

For follow-up tasks, keep the plan delta-first: name what is new, what context is already known, and the smallest source boundary to inspect next.

## Analysis Workflow
1. Identify API boundary:
   - route
   - controller
   - hub
   - middleware/filter
   - request/response DTO
   - command/query
2. Trace dependency flow:
   - Presentation
   - Application
   - Domain
   - Infrastructure
   - database/cache/external service
3. Check architecture boundaries.
4. Check validation and error handling.
5. Check authorization and security-sensitive logic.
6. Check transaction and UnitOfWork behavior.
7. Check test or build coverage.
8. Propose the smallest safe improvement.
9. Preserve existing pragmatic patterns unless they are the root cause.
10. Update the relevant skill report when durable domain knowledge changed.

## Refactoring Rules
- Preserve public API contracts unless requested.
- Do not move logic across layers without explaining why.
- Do not introduce new abstractions unless justified by existing patterns or real duplication.
- Keep business rules out of controllers and hubs.
- Keep infrastructure details out of Domain and Application.
- Prefer dependency injection over global/static access.
- Avoid broad formatting-only diffs.
- Do not rename typo-bearing existing files/classes unless the task is a coordinated rename.
- Do not change SignalR group names, Redis key formats, telemetry codes, or permission constants without explicit contract approval.
- Do not replace broad `catch (Exception)` blocks only for style; preserve local behavior unless the catch is the root cause or the task asks for error-handling redesign.

## Debugging Rules
For API bugs, inspect:
- request payload
- validation schema
- route/controller/hub
- command/query and handler
- pipeline behaviors
- application service
- domain entity methods
- repository/query/cache access
- response mapping
- tests or build checks covering the path

For IoT/realtime bugs, inspect:
- raw telemetry payload shape
- Event Hub/MQTT consumer path
- telemetry handler
- Redis snapshot/context keys
- fallback DB lookup
- SignalR group naming
- hub subscribe/replay behavior

For persistence bugs, inspect:
- `[EnableUnitOfWork]`
- explicit `SaveChangesAsync`
- `CommitTransactionAsync`
- soft-delete filters
- no-tracking reads
- concurrency stamp updates

## Delta-First Prior Reports
Before deep analysis of known domains, read the matching prior report and reuse its issue IDs:
- Scale realtime, Redis, SignalR, background scale: `.agents/skills/scale_optimise/scale-review.md`, `.agents/skills/scale_optimise/PLAN.md`
- FCM notification and Firebase delivery: `.agents/skills/FCM-notification/report-bug.md`
- Pond/Oxy alert flow: `.agents/skills/pond_error/`
- Warning device control: `.agents/skills/warning-device-control/work-flow.md`

Report only new findings, changed severity, invalidated assumptions, or implementation deltas.

## Reference Files
- `.agents/skills/backend-api-architecture/references/api-architecture-checklist.md`
- `.agents/skills/backend-api-architecture/references/clean-architecture-rules.md`
- `.agents/skills/backend-api-architecture/references/dependency-boundaries.md`
- `.agents/skills/backend-api-architecture/references/security-review.md`

## Output Format
For architecture reviews, return:
- Findings
- Impact
- Evidence
- Adjustment Plan
- Score
- Next actions

For implementation or debugging work, return:
- Summary
- Files changed
- Verification run
- Risks / follow-ups

