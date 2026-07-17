---
trigger: always_on
---

# Safety Constraints

## Ask Before Changing

- Public APIs, routes, DTOs, handler signatures.
- Database migrations, model snapshots, DbContext, entity configurations.
- Auth, JWT/current-user behavior, permissions, roles, policies, seeding, Logout.
- Redis key formats.
- SignalR group names.
- IoT telemetry code handling or device-control behavior.
- CI/CD, deployment, infrastructure, dependencies.
- Secrets, certificates, `appsettings*.json`, `.env`, or local runtime config.
- Existing typo-bearing public request/handler/controller/entity names.
- Destructive commands or file/folder deletion.

## Never Do Silently

- Commit or log secrets.
- Disable lint, analyzer, typecheck, or test rules globally.
- Replace large modules wholesale.
- Rewrite unrelated code.
- Modify generated files manually.
- Hide errors with broad try/catch.
- Use unsafe casts to bypass type errors without explanation.
- Modify Logout validator or handler.
- Normalize mojibake/encoding across unrelated files.
- Log refresh tokens, JWTs, FCM tokens, IoT credentials, Basic Auth credentials, connection strings, or raw file payloads.

## Evidence Control

Do not invent file paths, APIs, roles, Redis keys, SignalR groups, telemetry codes, or business rules. Verify memory and reports against source before changing behavior. If source and report disagree, source wins.

## Risk Reporting

When touching high-risk areas, report:
- affected module,
- why it is risky,
- what was verified,
- what remains unverified,
- whether automated tests exist for the behavior.
