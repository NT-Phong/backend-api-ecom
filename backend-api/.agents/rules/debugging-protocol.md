---
trigger: manual
---

# Debugging Protocol

## Root-Cause-First Approach
Before applying a fix, identify:
- Observed failure.
- Exact error message.
- Failing command or user flow.
- Suspected layer.
- Evidence from logs, code, tests, or runtime behavior.
- Proposed root cause.
- Verification plan.

## Bug Classification
Classify the issue as one or more of:
- Validation error.
- API contract mismatch.
- Data mapping issue.
- Async or race condition.
- State management issue.
- Database or query issue.
- Authorization or authentication issue.
- Configuration or build issue.
- Rendering or hydration issue.
- Third-party integration issue.

## Trace Flow
Trace from boundary to source:

```text
UI / API boundary
-> controller / route / handler
-> service / use case
-> domain logic
-> repository / database / external API
```

For this backend, also trace cross-cutting layers:

```text
controller/hub/background worker
-> MediatR behavior or hosted-service scope
-> handler/service
-> UnitOfWork/IApplicationDbContext/cache/external client
-> TResult/ApiResponse/SignalR message/log
```

## Fixing Rules
- Fix root cause, not symptoms.
- Do not hide errors with broad catch blocks.
- Do not add retries unless the failure is actually transient.
- Do not weaken validation unless required by product behavior.
- Add regression coverage when possible.
- Remove temporary logs before final response.
- Check both MVC model validation and MediatR validators for API validation bugs.
- Check permissions, JWT claims, and seeded policies for authorization bugs.
- Check Redis fallback and database fallback for realtime bugs.
- Check `[EnableUnitOfWork]`, explicit `SaveChangesAsync`, and `CommitTransactionAsync` for persistence bugs.

## Final Debugging Report
Return:
- Root cause.
- Fix summary.
- Files changed.
- Verification result.
- Risk areas.
