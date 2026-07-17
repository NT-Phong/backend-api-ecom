---
trigger: always_on
---

# Optimized Workflow

Use this shape:

```text
Plan -> Implement -> Verify -> Report -> Update durable knowledge if needed
```

## Input Hygiene

Prefer high-signal input over raw dumps. For debugging, the ideal user input is:
- endpoint or feature name,
- request shape or important parameters,
- status code and exact error line,
- stack trace frame closest to project code,
- recent change or suspected module when known.

If the user provides huge logs, HTML error pages, or unrelated output, extract actionable lines first and ignore noise. Ask for a narrower log only when there is no route, exception, stack frame, trace id, or reproduction clue.

Do not ask the user to paste initialization prompts, `AGENTS.md`, broad architecture rules, or full files already available in the workspace. Use local search and targeted reads.

## Plan

Normalize the request into:
- Task Type: New Feature Design / Bug Debugging / Refactoring / Architecture Review / Implementation.
- Goal or observed failure.
- Boundary/module.
- Known evidence: logs, snippets, business rules, assumptions.
- Expected output.

Infer missing fields when safe. Ask only for choices that affect public API, schema, auth/permission behavior, migrations, Redis/SignalR/IoT contracts, dependencies, or product intent.

## Locate And Inspect

- Use `rg` before opening files.
- Load risk/domain guidance only for touched high-risk areas.
- Inspect at least one nearby implementation with similar behavior.
- Match naming, layering, validation, transaction handling, error handling, and response style.

## Implement

- Preserve public APIs unless requested.
- Avoid unrelated formatting or refactoring.
- Do not introduce dependencies without clear need and approval.
- Prefer simple local changes over speculative abstractions.
- Keep claims grounded in source, logs, or command output.

## Verify

Recommend or run only the narrowest relevant check. Builds are user-run by default unless the user asks the agent to run them. If verification fails, separate new failures from pre-existing or unrelated failures. If no test project exists, state the gap.

## Report

Keep the final report delta-first:
- what changed,
- files touched,
- verification run or pending,
- assumptions, risks, and follow-ups.
