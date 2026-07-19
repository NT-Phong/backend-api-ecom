# Authentication V2 source map and phase status

Verified against the working tree on 2026-07-19. Source/tests override older plans and proposal documents.

## V1 phone-first OTP slice

`POST /api/v1/auth/send-otp` is the single phone-first entry point. It normalizes Vietnamese `0...` and `+84...` forms to one canonical value, creates a Pending `USER` for a new number, and returns the same accepted response for both registration and login. `POST /api/v1/auth/register` remains a compatibility wrapper.

After `POST /api/v1/auth/verify-otp`, a user without `FullName` receives `LoginStatus = OPTIONAL_BASIC_PROFILE`, `CanSkipProfile = true`, and `ProfileState = BASIC_PROFILE_MISSING`; a name can be saved later with authenticated `PATCH /api/v1/auth/profile/basic`. This changes neither `IsProfileCompleted` nor the V2 password/session flow. Checkout integration is not yet present in source, so no checkout gate has been wired.

Development explicitly uses the in-memory auth rate-limit backend and fixed OTP `0000`; production remains Redis-backed and fails closed with `503` after a bounded Redis operation timeout.

## Implemented mobile-password slice

| Concern | Current source boundary |
|---|---|
| Feature gate | `PasswordAuthenticationV2` defaults disabled; controller returns 404 while disabled. Production rejects exposed Development verification tokens. |
| Register | `AuthV2Controller` -> `RegisterPasswordCommand`; default `USER`, Pending user, BCrypt credential, hashed 256-bit challenge, generic 202 response, destination/IP throttling. |
| Development verify | `POST /api/v2/auth/email/verify/confirm`; `EmailVerification` challenge, one-time domain consume, email verified then Active. Raw token is returned only in Development with explicit option. No production email/outbox/provider yet. |
| Password login | `POST /api/v2/auth/login/password`; username/email lookup, dummy BCrypt hash, generic 401, account/device/IP limits, lockout fields, `LoginFailed` event. |
| Completion | `AuthenticationSessionEngine`; checks Active, resolves role timeout and policy snapshot, creates `UserSession`, refresh family/token, `LoginSucceeded`, then commits through caller transaction before response. Risk/step-up/outbox notification are absent. |
| JWT/refresh | JWT includes `session_id`, `security_stamp`, role and resolved policy claims. `SessionRefreshService` does conditional PostgreSQL claim/rotation and revokes family/session on replay. |
| Protected access | `GET /api/v2/auth/me` verifies current security stamp, session ownership/activity/expiry, and Active user status. |
| Logout | V2 route delegates to `LogoutCommandHandler`, which resolves V2 refresh hash/session and revokes session tokens; logout-all also rotates stamp. Cookie clear/Redis invalidation are not proven. |

## Contract actually exposed

`POST /api/v2/auth/register`, `POST /api/v2/auth/email/verify/confirm`, `POST /api/v2/auth/login/password`, `POST /api/v2/auth/refresh`, `GET /api/v2/auth/me`, `POST /api/v2/auth/logout`, `POST /api/v2/auth/logout-all`, `DELETE /api/v2/auth/sessions/{sessionId}`.

`password/forgot`, `password/reset`, `password/change`, `password/setup`, and `csrf` also exist but are not all feature-flag guarded or acceptance-complete.

## Password and environment policy

- BCrypt work factor is configurable; credential stores `AlgorithmVersion = bcrypt-v1`.
- Normal rule: 15-128 characters, no trim, small common-password deny list, no composition rule.
- Development-only override permits minimum 5. `PasswordSettingsValidator` rejects <15 outside Development.
- Username: 4-32, letters/numbers/`.`/`_`/`-`, at least one letter, no `@`, not all numeric, reserved names rejected by public validation.

## Persistence and migrations

- `AddIdentitySessionFoundation`, `AddPasswordAuthenticationV2`, and `HardenPasswordAuthenticationV2` are forward migrations; latest adds `AlgorithmVersion`, `LastFailedAt`, `LockedUntil`.
- `VerificationChallenge`, `UserSession`, `SessionRefreshToken`, `SecurityEvent`, and `PasswordCredential` exist.
- `SecurityEvent` is append-only only through EF `SaveChangesAsync`; raw SQL/DB-admin protections are absent.

## Phase conclusion

| Phase | Status | Gate gap |
|---|---|---|
| 0-1 baseline/containment | Partial | production operations evidence incomplete; MessagePack advisories remain. |
| 2 persistence | Partial | no PostgreSQL uniqueness/concurrency/rollback proof; no legacy backfill checkpoint. |
| 3 session engine | Partial | no risk/step-up/outbox, cookie/CSRF proof, or real concurrency acceptance. |
| 4 password mobile slice | Conditional | V2 implementation exists, but no real PostgreSQL/Redis/API E2E proof and no production email delivery. |
| Google/Passkey/QR/onboarding | Missing | explicitly outside current scope. |

## High-priority gates before broader methods

1. PostgreSQL tests: challenge consume, failed-login counter, refresh/replay, session revoke/logout-all, rollback.
2. Production email sender/outbox/retry/resend and approved callback domain.
3. Role/policy mutation must revoke V2 sessions/refresh families, rotate stamp, and write security events.
4. Browser BFF/CSRF/cookie clear-after-commit needs a separate accepted phase.
5. OTP dual-write/read and legacy refresh migration remain cutover work.
