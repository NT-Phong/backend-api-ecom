# Testing and Verification

## Projects

- `Tests/Ecom.Domain.Tests`: domain invariants and state machines.
- `Tests/Ecom.IntegrationTests`: PostgreSQL constraints, transactions, concurrency, migrations, and API flows when implemented.

## Policy

- Bug fix: reproduce the failure, prove the fix, then run focused regression tests.
- Domain change: build Domain and run matching Domain tests.
- Application/API change: build the narrowest affected projects and run matching acceptance tests.
- Schema/concurrency change: use PostgreSQL integration tests and review migration SQL.
- Never use EF InMemory as evidence for relational constraints or concurrency.
- Record exact command, exit code, and relevant result.
- A skipped or blocked check is not a pass; state the gap and remaining risk.
