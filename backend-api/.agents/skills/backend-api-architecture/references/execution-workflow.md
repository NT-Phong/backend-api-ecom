# Backend Agent Execution Workflow

Use this workflow for Ecom backend implementation, debugging, refactoring, and architecture review.

## 1. Plan

Normalize the request:

```markdown
**Task Type:** [New Feature Design / Bug Debugging / Refactoring / Architecture Review]
**Short Description:** [Goal or observed failure]
**Boundary/Module:** [Controller, Handler, Route, Entity, Worker, Hub, etc.]
**Additional Details (Optional):** [Logs, snippets, business rules]
**Expected Output:** [Deliverables]
```

Then identify:

- Smallest relevant boundary.
- Required context files and skill reports.
- Task route from `.agents/context/task-router.md`.
- High-risk areas from `.agents/context/risk-map.md`.
- Existing nearby pattern to inspect before editing.
- Verification scope and whether it is user-run.

## 2. Implement Plan

- Search by boundary term, command/query name, route, entity, or error text before opening many files.
- Trace the actual flow: API/hub/worker -> command/query -> validator -> handler -> domain/repository/service -> infrastructure.
- Prefer source evidence over memory or prior reports when behavior is being changed.
- Keep the patch narrow; do not fold unrelated cleanup into the task.
- Preserve public API, migrations, auth/permission behavior, Redis keys, SignalR groups, and IoT telemetry contracts unless explicitly approved.
- Match nearby `TResult`, `MessageKey`, `ErrorCodes`, logging, transaction, and validation patterns.

## 3. Report

Use concise, evidence-based reporting:

```markdown
## Summary
## Files changed
## Verification run
## Risks / follow-ups
```

For reviews, lead with findings before summary. For debugging, include root cause and evidence.

## 4. Update Skill Files

Update a skill report only when the task changes durable knowledge:

- A confirmed bug/root cause.
- Accepted business behavior.
- Completed implementation plan.
- New or changed risk.
- Verification result that future agents should reuse.
- A stale assumption that must not come back.

Write the update in the relevant skill folder:

- Scale realtime: `.agents/skills/scale_optimise/`
- FCM/notifications: `.agents/skills/FCM-notification/`
- Oxy/pond alert: `.agents/skills/pond_error/`
- Device connection: `.agents/skills/device-connection/`
- General API architecture: `.agents/skills/backend-api-architecture/`

Keep report updates short, source-backed, and delta-first. Do not create generic changelogs or duplicate long context from source files.

Use `.agents/skills/backend-api-architecture/templates/skill-report-update.md` when appending a new update block.

## Evidence Rules

- Claims about code behavior need file evidence or command output.
- Claims about runtime behavior need logs, user-provided verification, or a clearly labeled inference.
- If no automated test exists, say so.
- If verification was not run, state what command should be run and why.
- Do not claim a report is updated unless the file was actually changed.

