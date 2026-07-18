# Backend Execution Workflow

## Plan

Normalize task type, use case, boundary, public/schema/security impact, expected output, verification, and Commerce roadmap batch.

## Implement

- Search before opening broad files.
- Trace HTTP -> MediatR -> aggregate/query -> persistence.
- Match existing TResult, validation, authorization, and UnitOfWork mechanics.
- Keep Commerce business rules in Domain/Application.
- Do not combine unrelated legacy cleanup.

## Verify

- Domain changes: Domain build/tests.
- Application/API: narrow build then API tests.
- Schema/concurrency: PostgreSQL integration tests and migration SQL review.
- Record command/result; do not call unverified source complete.

## Update Durable Knowledge

Update `.agents/skills/commerce-system/references/` when a verified batch, invariant, API contract, relationship, migration, or risk changes. Use its update protocol.
