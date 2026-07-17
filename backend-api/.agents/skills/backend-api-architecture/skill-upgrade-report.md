# Agent Skill Upgrade Report

Updated: 2026-05-15

## Scope

This report records the `.agents` skill-set upgrade focused on faster backend comprehension, stricter execution discipline, concise reporting, and reusable skill-memory updates.

## Methodology Integrated

- Progressive disclosure: keep bootstrap and `SKILL.md` files short; load references/templates only when needed.
- Boundary-first analysis: classify task type, identify the smallest route/controller/handler/entity/hub/worker boundary, then search.
- Evidence-first execution: source files, logs, command output, and user-provided verification outrank memory or prior reports.
- Guardrail-first safety: pause before public API, migration, auth, permission, Redis key, SignalR group, IoT telemetry, secret/config, dependency, or destructive changes.
- Structured workflow: `Plan -> Implement Plan -> Report -> Update skill files`.
- Structured intake: normalize user requests into task type, short description, boundary/module, optional details, and expected output.

Research basis:

- OpenAI prompting guidance: clear role/task instructions, reusable prompts, concise examples, and relevant-context loading.
- OpenAI reasoning guidance: keep reasoning-model prompts direct, use delimiters, and avoid asking for exposed chain-of-thought.
- OpenAI agent guidance: use tools, guardrails, handoffs, and traceable workflows for complex agent tasks.
- ReAct-style agent research: interleave planning, evidence-gathering actions, and plan updates for tasks that require external context.

## Files Updated

- `.agents/README.md`: added standard execution model and request intake format.
- `.agents/rules/optimized-workflow.md`: converted default workflow to the four-phase execution model.
- `.agents/rules/agent-directives.md`: added work-shape, progressive loading, and concise evidence-first communication rules.
- `.agents/rules/safety-constraints.md`: added evidence and hallucination-control guardrails.
- `.agents/rules/context-efficiency.md`: added request-intake and domain-report loading guidance.
- `.agents/skills/backend-api-architecture/SKILL.md`: linked the new workflow/reference files and reinforced public API / `catch (Exception)` constraints.
- `.agents/skills/backend-api-architecture/analysis-command-template.md`: aligned analysis input with the standardized request format.
- `.agents/skills/backend-api-architecture/references/execution-workflow.md`: added detailed workflow and skill-report update protocol.
- `.agents/skills/backend-api-architecture/templates/request-intake.md`: added reusable intake template.

## Current Operating Rules

Future backend tasks should:

1. Plan with normalized request fields and risk gates.
2. Implement only after source-backed boundary tracing.
3. Report concise changes, verification, assumptions, and risks.
4. Update the relevant skill report only for durable domain knowledge.

## Update - 2026-05-15 - Router, Report Template, Examples, And Validation

Task Type: Refactoring
Boundary/Module: `.agents` guidance, backend architecture skill, scripts, examples, and selected domain reports.
Status: Implemented

### Delta
- Added `.agents/context/task-router.md` to map task type/domain to the smallest relevant guidance and report files.
- Added `.agents/skills/backend-api-architecture/templates/skill-report-update.md` as the standard delta-first report update template.
- Added prompt examples for Bug Debugging, Refactoring, Architecture Review, and New Feature Design.
- Added `.agents/scripts/verify-skill-docs.ps1` for required guidance files, SKILL frontmatter, markdown links, and stale `.github/skills` references.
- Added lightweight metadata headers to FCM, Oxy, and Device reports without rewriting historical content.

### Evidence
- `.agents/context/task-router.md`
- `.agents/skills/backend-api-architecture/templates/skill-report-update.md`
- `.agents/examples/prompt-bug-debugging.md`
- `.agents/examples/prompt-refactoring.md`
- `.agents/examples/prompt-architecture-review.md`
- `.agents/examples/prompt-new-feature-design.md`
- `.agents/scripts/verify-skill-docs.ps1`

### Verification
- `powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-skill-docs.ps1`: completed successfully after fixing the script to scan Markdown files only.
- `powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-guidance.ps1`: completed successfully.

## Verification Notes

Docs-only verification run:

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-guidance.ps1
powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-skill-docs.ps1
```

Result: completed successfully. The remaining `.github/skills` hits are intentional canonical-memory warnings and the verification scripts themselves.

No application build is required for this guidance-only change.

## Update - 2026-05-16 - Session-Aware AGENTS Bootstrap

Task Type: Refactoring
Boundary/Module: `AGENTS.md`, agent rules/context routing, backend architecture skill memory.
Status: Implemented

### Delta
- Reworked `AGENTS.md` into a senior-backend bootstrap with project mental model, pragmatic Clean Architecture boundaries, CQRS/MediatR workflow, high-risk gates, search recipes, verification guidance, and canonical memory locations.
- Added explicit fresh-thread versus follow-up-task context loading to reduce repeated token use.
- Linked high-value domain reports and plans directly from bootstrap:
  - Scale realtime / Code 503 / live weight.
  - FCM notification and warehouse recipients.
  - Pond/Oxy alert flow.
  - Device hub uninstall/connection behavior.
- Reinforced that source code is current truth and prior reports are routing aids.
- Kept detailed context in `.agents/context/` and `.agents/skills/` instead of duplicating long reports in `AGENTS.md`.

### Evidence
- `AGENTS.md`
- `.agents/rules/context-efficiency.md`
- `.agents/context/task-router.md`
- `.agents/context/agent-operating-guide.md`
- `.agents/skills/backend-api-architecture/SKILL.md`

### Verification
- Guidance verification should run with:
  - `powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-guidance.ps1`
  - `powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-skill-docs.ps1`

### Remaining Risks
- Git status/diff may require `safe.directory` configuration in local environments where repository ownership differs from the active user.
- Historical domain reports remain uneven in style; agents should use the router and source-backed evidence rather than reading all reports broadly.

## Remaining Risks

- Domain-specific reports remain uneven in style because this task standardized workflow rules, not every historical report.
- Some older reports contain mojibake or long historical context; do not bulk-normalize them during unrelated code tasks.
