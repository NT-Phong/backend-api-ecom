---
trigger: always_on
---

# Context Efficiency

## Goal

Minimize token, request, and credit usage while preserving correctness.

## Rules

- Do not read the entire repository unless explicitly required.
- Reuse loaded bootstrap, architecture, conventions, and domain conclusions in the same thread.
- Prefer targeted search over broad reading.
- Prefer interfaces, exports, tests, and nearby examples before full implementations.
- Do not paste or regenerate entire files when only a small patch is needed.
- Avoid repeating unchanged code in responses.
- Do not summarize huge files unless the summary will be reused.
- Load domain reports only for the touched domain.
- Load risk context only for protected/high-risk areas or unclear risk.
- Load technical stack context only for runtime, package, framework, or dependency questions.
- Load broad codebase analysis only for architecture reviews.
- Source code is the source of truth.

## Ignore Unless Explicitly Required

- `node_modules`
- `vendor`
- `dist`
- `build`
- `.next`
- `coverage`
- `bin`
- `obj`
- generated clients
- lock files unless dependency changes are required
- EF migration designer files and model snapshots unless schema work is required
