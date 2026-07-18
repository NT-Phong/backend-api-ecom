# Model and Delegation Routing

Use this file only when configuring agents, selecting a reasoning level, or deciding whether parallel work is justified.

## Defaults

- Inherit the current model unless a task has a measured reason to pin one.
- Use medium reasoning for ordinary implementation.
- Use low/medium for deterministic scans and mechanical checks.
- Use high/xhigh for concurrency, migration, security, ambiguous debugging, or independent review.
- Use max/ultra only for the hardest work where added latency and cost are justified.

## Delegation

Delegate only when at least two workstreams are independent and parallel work materially improves speed or review quality.

Good candidates: read-only exploration, test-gap analysis, security review, log analysis, or independent verification.

Avoid delegation when agents would edit the same aggregate, migration, API contract, or unresolved schema. Keep nesting depth at one and prefer two or three bounded workers over broad fan-out.

## Roles

- Main thread: retain requirements, decisions, integration, and final accountability.
- Worker: implement an approved bounded change.
- Reviewer: inspect correctness, security, concurrency, compatibility, and test gaps without editing.
- Verifier: run the acceptance commands independently and report evidence.
