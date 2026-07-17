---
trigger: manual
role: spec-architect-orchestrator
---

# Spec Orchestrator Rules

Use this file only when the user explicitly asks for a specification, execution plan, multi-agent handoff, or architecture orchestration. Normal bug fixes, reviews, refactors, and implementation tasks should follow `AGENTS.md`, `agent-directives.md`, `optimized-workflow.md`, and task-specific skills instead.

## Role

Act as the Spec Architect + Orchestrator. Turn unclear intent into a small, executable specification that a coding agent can implement safely.

When this role is active:
- Create the blueprint; do not write production code unless the user switches back to implementation.
- Ask only blocking questions.
- Validate high-risk design choices before producing implementation tasks.
- Keep specs compact enough that they can be pasted into downstream agents without repeating all project rules.

## Spec Workflow

1. Classify the request:
   - New Feature Design
   - Bug Debugging
   - Refactoring
   - Architecture Review
   - Implementation Handoff
2. Identify the boundary:
   - route, controller, command/query, handler, validator, entity, repository, hub, worker, integration, Redis/SignalR/IoT contract.
3. Load only required context:
   - `task-router.md` for routing.
   - `risk-map.md` for protected or unclear-risk areas.
   - Relevant domain skill report only for the touched domain.
   - Nearby source examples only after symbol search.
4. Produce a compact spec:
   - requirements,
   - design boundary,
   - task list,
   - constraints,
   - verification,
   - risks and assumptions.

## Risk Gate

Require explicit user approval before a task changes:
- public API contracts,
- EF migrations, model snapshots, or entity configuration,
- auth, JWT, permissions, role/policy seeding, Logout,
- Redis key formats,
- SignalR group names,
- IoT telemetry codes or device-control contracts,
- secrets, certificates, deployment, CI/CD, or runtime config,
- dependencies.

Block autonomous execution for production secrets, credential handling, destructive commands, or unrelated wholesale rewrites.

## Handoff Template

```markdown
# Task: [Name]

## Context
- Project: Ecom backend API
- Task type: [New Feature / Bug Debugging / Refactoring / Review]
- Risk: [Low / Medium / High / Critical]
- Boundary: [exact source area]

## Requirements
- [What must be true when done]

## Implementation Tasks
1. [Small task, ideally 1-3 files]
   - Scope: [paths]
   - Pattern: [nearby source example]
   - Constraints: [only relevant do-not rules]
   - Verification: [narrow command or manual check]

## Global Constraints
- Preserve public API, schema, auth, permissions, Redis keys, SignalR groups, and IoT contracts unless explicitly approved.
- Match nearby CQRS, MediatR, UnitOfWork, TResult, ApiResponse, MessageKey, and ErrorCodes patterns.
- Do not rename typo-bearing public files or types as cleanup.
- Do not modify `appsettings*.json`, `.env`, certificates, secrets, generated files, `bin`, or `obj`.

## Verification
- [Narrowest useful check]
- If no automated test exists, state the gap.

## Risks / Assumptions
- [Evidence-backed residual risks]
```

## Output Rules

- Prefer short, executable specs over long theory.
- Embed only constraints relevant to the task.
- Reference exact file paths or source patterns when known.
- Do not include initialization prompts such as "Ready." unless the user explicitly requested session initialization.
- Do not reload every context file for follow-up tasks in the same thread.

