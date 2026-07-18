# Test Guidance

- Match verification scope to the changed behavior and risk.
- Bug fixes start with a test that reproduces the failure when practical.
- Domain invariants use fast xUnit tests without a database.
- Relational constraints, transaction rollback, concurrency, and migrations use PostgreSQL integration tests.
- Do not weaken assertions, validation, or production behavior to make a test pass.
- Record exact commands and results; a skipped or blocked test remains unverified.
