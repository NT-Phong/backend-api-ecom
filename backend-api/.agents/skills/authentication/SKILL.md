---
name: authentication
description: Analyze, implement, review, or debug Ecom Authentication V1/V2, password login, JWT/refresh rotation, sessions, logout, roles, permissions, role-policy changes, authorization claims, and security-event behavior. Use for AuthV2 routes, Identity features, role assignment, policy seeding, token/session persistence, or authentication security gates.
---

# Ecom Authentication and Roles

## Start

1. Read `references/current-state.md` for verified implementation and phase status.
2. Read `references/role-authorization.md` for role, policy, claim, and privileged-change behavior.
3. Trace Presentation -> MediatR -> Domain -> persistence/security service before proposing a change.
4. Treat `document/architecture/authentication/` as target contracts; source and tests are current truth when they disagree.

## Boundaries

- Preserve V1 OTP contracts unless the task explicitly opens compatibility/cutover work.
- Password V2 is mobile-token-first. Do not enable browser cookie/BFF behavior incidentally.
- Never log or return passwords, hashes, tokens, verification secrets, raw PII, authorization codes, or session cookies.
- Require approval before migrations, public contracts, configuration/dependency changes, or deployment. Never apply migrations outside the approved environment.
- Do not add privileged bootstrap, staff invitation, Google, Passkey, QR, or seller approval unless the task explicitly includes that phase.

## Workflow

### Password/session

- Inspect feature flags, Development-only bypasses, rate-limit failure mode, transaction boundary, and security event.
- Preserve PostgreSQL as session/token source of truth; Redis remains cache/best-effort only.
- Add PostgreSQL concurrency evidence for refresh rotation, challenge consume, lockout, or revoke claims; unit tests alone do not pass those gates.

### Role/policy

- Trace `Permissions.GetAll` -> seeders -> `RolePolicy`/`UserPolicy` -> JWT `policy` claims -> authorization handler.
- Treat role/policy changes as security-sensitive: determine whether V1/V2 sessions, refresh families, and security stamps are revoked. Require a security event before declaring complete.
- Do not use public registration to create Admin; bootstrap/invitation is a separate missing flow.

## Verification

```powershell
dotnet build Ecom.sln --no-restore /p:UseSharedCompilation=false
dotnet test Tests/Ecom.Domain.Tests/Ecom.Domain.Tests.csproj --no-restore
dotnet test Tests/Ecom.IntegrationTests/Ecom.IntegrationTests.csproj --no-restore
dotnet ef migrations has-pending-model-changes --project Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj --startup-project Presentation/Ecom.API/Ecom.API.csproj --context ApplicationDbContext --no-build
git diff --check
```

State explicitly when PostgreSQL E2E, Redis multi-instance, timing, browser CSRF, or provider-delivery evidence is absent.
