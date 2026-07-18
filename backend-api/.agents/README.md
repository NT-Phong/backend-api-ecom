# Agent System

This directory is the project-local navigation and execution layer for agents working on the Thanh Hoa Commerce backend.

## Canonical Sources

| Concern | Canonical source |
| --- | --- |
| Repository-wide workflow and approval gates | `AGENTS.md` |
| Task-to-context routing | `.agents/context/task-router.md` |
| Cross-cutting backend architecture | `.agents/skills/backend-api-architecture/SKILL.md` |
| Commerce domain, schema, and roadmap | `.agents/skills/commerce-system/SKILL.md` |
| Protected areas and escalation | `.agents/context/risk-map.md` |
| Verified Commerce progress | `.agents/skills/commerce-system/references/current-status.md` |

## Minimal Loading

1. Start with `AGENTS.md` and `.agents/context/task-router.md`.
2. Select one primary skill.
3. Load only the reference named for the current task.
4. Search live source and tests before relying on guidance.
5. Reuse already-loaded stable context within the same task.

Do not load all files under `.agents/context` or `.agents/rules`. Source and the current diff override project guidance when they disagree.

## Execution Contract

For non-trivial work, normalize the request with `.agents/templates/task-contract.md`. Use `.agents/templates/code-review.md` for an independent review. Record reusable facts only when current source or verification evidence proves them.

## Verification

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-agent-system.ps1
```

The verifier checks structure, routing, skill metadata, stale references, context budgets, and eval definitions.
