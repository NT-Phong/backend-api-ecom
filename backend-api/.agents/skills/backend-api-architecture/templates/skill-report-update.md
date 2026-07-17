# Skill Report Update

Use this template when a completed task changes durable domain knowledge.

```markdown
## Update - YYYY-MM-DD - <short title>

Task Type: <New Feature Design | Bug Debugging | Refactoring | Architecture Review>
Boundary/Module: <route/controller/handler/entity/service/worker/hub>
Status: <Planned | Implemented | Verified | Partially verified | Blocked>

### Delta
- <new fact, changed behavior, fixed issue, or invalidated old assumption>

### Evidence
- `<file:line or command/log evidence>`

### Files Changed
- `<path>`: `<reason>`

### Verification
- `<command/log/manual check>`: `<result or pending>`

### Remaining Risks
- `<risk or follow-up>`
```

## Rules

- Append or revise the relevant existing report; do not create a new report file unless no suitable report exists.
- Keep the update delta-first. Do not paste large source blocks.
- Reuse existing issue IDs when the domain report already has them.
- State when verification is pending or user-run.
- Do not update reports for trivial formatting-only changes.
