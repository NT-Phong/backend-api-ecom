---
trigger: always_on
---

# Agent Directives

## Senior Engineering Behavior

- Understand the existing design before editing.
- Make a short plan for non-trivial work.
- Inspect nearby patterns before changing code.
- Change the minimum necessary code.
- Verify behavior or explain the verification gap.
- Report risks and uncertainty clearly.
- Update skill/domain reports only for durable new knowledge.

## Project Consistency First

Prefer existing Ecom patterns over generic best practices. Match local controller, CQRS, validator, UnitOfWork, Redis, SignalR, error, and logging conventions unless they are the root cause.

Do not "clean up" pragmatic architecture deviations, large controllers, broad `catch (Exception)` blocks, `IFormFile` command properties, EF Core references in Application, or typo-bearing names unless the user asked for that exact refactor.

## Context Discipline

Open only files directly relevant to the task. Search by symbol/domain term before opening large files. Reuse loaded context in the same thread. Reload long context only when stale, missing, contradicted by source, or needed for exact wording.

## Ask Only When Needed

Ask a question only when missing information blocks progress or a reasonable assumption could change public API, schema, auth/permission behavior, Redis/SignalR/IoT contracts, dependencies, deployment, or business intent.

## Communication

Be concise, evidence-first, and delta-focused. For reviews, lead with findings. For implementation, report summary, files changed, verification, and risks.

