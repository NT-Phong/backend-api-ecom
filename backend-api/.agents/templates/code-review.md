# Independent Code Review

Review the requirement and final diff without assuming the implementation is correct.

Check:

- Behavioral correctness and requirement coverage.
- Security, authorization, privacy, and secret handling.
- Transaction, concurrency, idempotency, and rollback behavior.
- Public API and persistence compatibility.
- Test quality, missing boundaries, and false-positive tests.
- Changes outside the approved scope.

Report only evidence-backed findings, ordered by severity, with exact file and line references. If no actionable finding exists, state the residual verification gaps.
