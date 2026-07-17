# AI Project Memory

This directory contains reusable memory for AI coding agents working on the Ecom backend API.

## Loading Model
- Always start with `AGENTS.md`.
- Use `.agents/rules/` for reusable behavior.
- Use `.agents/context/` for project-specific facts.
- Use `.agents/context/task-router.md` during Plan to choose the smallest relevant context and skill reports.
- Use `.agents/context/quick-scan.md` when the agent needs to locate source files quickly from a route, symbol, log line, error code, or feature name.
- Use `.agents/context/agent-operating-guide.md` as the concise project/task framing guide before opening domain-specific files.
- Use `.agents/skills/` for task-specific procedures and prior analysis.
- Use `.agents/examples/`, `.agents/templates/`, and `.agents/scripts/` only when they directly help the current task.
- Tool-specific adapters should stay thin and point back to `AGENTS.md`.

## Standard Execution Model
Use this sequence for backend work:

```text
Plan -> Implement Plan -> Report -> Update skill files
```

- Plan: classify the request, normalize scope, identify the smallest boundary, load only relevant context, and name risk gates before editing.
- Implement Plan: use quick scan or targeted `rg`, inspect nearby patterns, make the smallest safe change, and keep evidence tied to files or command output.
- Report: summarize what changed, files changed, verification run or pending user-run verification, assumptions, and remaining risks.
- Update skill files: record reusable findings in the relevant `.agents/skills/<domain>/` report when the task changes domain knowledge, accepted behavior, risks, or completed plans.

## Request Intake
Normalize backend requests into this shape when possible:

```markdown
**Task Type:** [New Feature Design / Bug Debugging / Refactoring / Architecture Review]
**Short Description:** [Goal to achieve or observed failure]
**Boundary/Module:** [Controller, Handler, Route, Entity name, etc.]
**Additional Details (Optional):** [Error logs, current code snippets, or specific business requirements]
**Expected Output:** [Deliverables satisfying the request]
```

Infer missing fields from code and context when safe. Ask only when the missing field would change public API behavior, data contracts, security behavior, migrations, Redis/SignalR/IoT contracts, or product intent.

## Maintenance Rules
- Keep bootstrap files short.
- Move long explanations into context files.
- Move repeated output structures into templates.
- Keep context loading session-aware: reuse already loaded core context in the same thread and load only task-specific deltas.
- Keep prior investigation reports under the relevant skill folder.
- Keep skill entrypoints concise; move detailed checklists and templates into `references/` or `templates/`.
- Avoid duplicating the same rule across many files. Prefer one canonical file and links from other guidance.
- Update `.agents/context/project-map.md` after major folder or module changes.
- Update `.agents/context/task-router.md` after adding a major domain skill or changing canonical report locations.
- Update `.agents/context/decisions.md` after confirmed architectural decisions.
- Update `.agents/context/codebase-analysis.md` and `.agents/context/risk-map.md` after broad architecture reviews.
- Do not duplicate long rules into Cursor, Claude, Windsurf, or Antigravity files.

## Verification
- For guidance-only changes, run `powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-guidance.ps1`.
- For skill/report/template changes, run `powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-skill-docs.ps1`.

