# Authentication threat model and security events

- Method: STRIDE plus abuse-case review
- Baseline: Authentication V1 source as inspected on 2026-07-18
- Status: Proposed; requires security approval

## Assets and trust boundaries

Assets include account ownership, OTP challenges, access/refresh credentials, BFF cookies, external identities, passkeys, QR approvals, roles, invitations and security audit evidence. Trust boundaries are client-to-edge, edge-to-API, API-to-PostgreSQL, API-to-Redis, API-to-SMS/provider and privileged operations. PostgreSQL is durable truth; Redis and provider callbacks are untrusted inputs/dependencies.

## Threat register

| Flow | STRIDE / abuse | Required controls | Cluster 1 disposition |
|---|---|---|---|
| Register | Spoof destination, enumeration, mass creation, role injection | Neutral response, IP+destination limits, verified ownership, customer-only default role, audit | Contain V1; seller approval is target policy |
| OTP request | Flood/cost abuse, OTP disclosure, destination enumeration | CSPRNG, protected storage, purpose binding, cooldown and distributed limits, fail-closed SMS/Redis, neutral response | Implement V1 containment |
| OTP verify | Guessing, replay, cross-purpose use, test bypass | Challenge attempt cap, constant-time compare, single-use/expiry, environment guard, lockout and event | Implement V1 containment |
| Login | Credential stuffing, fixation, malicious redirect, privilege injection | Generic failures, throttling, rotate session, server-derived roles, recent-auth policy | OTP V1 only in this cluster |
| Refresh | Theft, replay, concurrent rotation, family reuse | Protected-at-rest token, atomic single-use rotation, family revoke on reuse, session rate limit | Protect/rotate V1; full family model deferred to V2 |
| Logout | CSRF, partial revoke, dependency outage | Same-origin/CSRF for cookie flow, PostgreSQL revoke, logout-all semantics, idempotency | Correct V1 revoke; BFF deferred |
| Password reset | Enumeration, weak token, account takeover | Neutral response, high-entropy single-use token, rate limits, revoke sessions, audit | Boundary only; no password flow implemented |
| Google callback | Forged state/code, wrong issuer/audience, unsafe auto-link, open redirect | PKCE/state/nonce, exact redirect, token validation, verified-email linking policy, allow-list | Boundary only; exact callback BLOCKED |
| Passkey challenge | Replay, origin/RP confusion, cloned credential | Exact RP/origin, random one-time challenge, signature/counter validation, recent-auth removal | Boundary only; domains BLOCKED |
| QR approve/consume | QR theft, confused deputy, replay/race | No bearer secret in QR, short expiry, display context, bind both sessions, atomic state consume | Boundary only; callback/deep link BLOCKED |
| Privilege escalation | Self-assigned seller/staff/admin, stale authorization | Approval/invitation/bootstrap policy, server-side authorization, session revoke on role change, audit | Policy fixed; implementation outside cluster |
| Session hijacking | XSS/token logging/DB theft/cookie scope | BFF HttpOnly host cookie, no credential logs, protected refresh token, TLS, rotation and revoke | V1 containment plus target ADR |
| Account enumeration | Different status/body/timing | Neutral accepted responses, generic auth failure and minimum response floor | Implement for V1 public flows |
| Credential replay | Reused OTP/refresh/provider code/QR token | Single-use state, atomic consume, token family reuse detection, idempotency | OTP and basic refresh rotation now; full family later |

## Residual and blocked risks

- Exact production TLS hosts, trusted proxy ranges, callback origins, Redis HA behavior and SMS vendor behavior are **BLOCKED** pending platform evidence.
- V1 cannot gain a complete refresh-token family/reuse model without an additive schema change. Protected storage, rotation and revoke are containment; full replay-family detection is deferred behind a migration approval gate.
- Web V1 still returns bearer credentials. The BFF/cookie decision applies to V2 cutover; changing the V1 client contract is outside this cluster.

## Security event taxonomy

Every event contains UTC timestamp, event name/version, outcome/reason code, trace ID, actor/user/session identifiers where known, client class, coarse network fingerprint, risk flags and service version. Destinations are represented only by a keyed fingerprint. Never record raw OTP, access/refresh token, cookie, provider authorization code, passkey assertion, QR secret, password, raw phone/email or full IP unless a separately approved legal/security policy requires it.

| Category | Event names |
|---|---|
| Login | `auth.login.succeeded`, `auth.login.failed`, `auth.login.suspicious` |
| OTP | `auth.otp.requested`, `auth.otp.request_failed`, `auth.otp.verified`, `auth.otp.verify_failed`, `auth.otp.throttled` |
| Refresh | `auth.refresh.succeeded`, `auth.refresh.failed`, `auth.refresh.reuse_detected`, `auth.refresh.throttled` |
| Logout | `auth.logout.succeeded`, `auth.logout_all.succeeded`, `auth.session.revoked` |
| Contact/password | `auth.password.changed`, `auth.password.reset`, `auth.contact.change_requested`, `auth.contact.changed` |
| Identity | `auth.identity.linked`, `auth.identity.link_failed`, `auth.identity.unlinked` |
| Passkey | `auth.passkey.added`, `auth.passkey.removed`, `auth.passkey.login_succeeded`, `auth.passkey.login_failed` |
| QR | `auth.qr.created`, `auth.qr.scanned`, `auth.qr.approved`, `auth.qr.rejected`, `auth.qr.consumed`, `auth.qr.expired` |
| Authorization | `auth.role.changed`, `auth.security_policy.changed`, `auth.invitation.created`, `auth.invitation.accepted`, `auth.admin.bootstrapped` |

Event access is least-privilege and tamper-evident. Retention duration and the approved HMAC/fingerprint key lifecycle are **BLOCKED** pending security/privacy and secret-management decisions.

## Required verification gates

- Negative tests for secret/PII logging and response disclosure.
- Production startup rejection for development bypass.
- Redis/SMS unavailability tests and distributed partition/expiry tests.
- Login/refresh/logout regression and refresh concurrency tests.
- Proxy/header spoofing test at the deployed edge before production activation.
- Security review approval of this model before any V2 provider/passkey/QR implementation.
