# ADR-001: Authentication architecture and trust boundaries

- Status: Proposed; security and platform approval required
- Scope: Authentication V1 containment and the target V2 boundary
- Date: 2026-07-18

## Context

The current V1 API authenticates a phone number with OTP and returns an access JWT plus a refresh token. The target product needs web, mobile, staff/admin, social identity, passkey and cross-device sign-in without making Redis or a browser-readable token the security source of truth.

## Decisions

### Client authentication

- Public and admin web use a same-origin BFF. The browser receives only an opaque, `HttpOnly`, `Secure`, `SameSite=Lax` session cookie. Access and refresh tokens are never exposed to browser JavaScript or persisted in local/session storage.
- Mobile uses a short-lived bearer JWT and a rotated, single-use refresh token stored in the platform secure store. A refresh-token reuse event revokes the token family.
- Phone OTP remains one identity/login method. It is not the canonical user identity and must not be assumed to be the only future method.

### Domain topology

The intended production topology is:

| Boundary | Intended host | Responsibility |
|---|---|---|
| Auth | `auth.<root-domain>` | Authentication UI, external-provider callbacks and cross-device approval pages |
| Public web | `www.<root-domain>` | Same-origin public BFF and storefront |
| Admin | `admin.<root-domain>` | Separate admin BFF, stricter session and authorization policy |
| API | `api.<root-domain>` | Mobile/service API; browser access only through an approved BFF/CORS allow-list |

The exact root domain, certificate coverage, callback URLs and reverse-proxy network ranges are **BLOCKED** pending platform/environment ownership. Wildcard callback URLs are forbidden.

### Cookies

- Public cookie: `__Host-ecom_session`; admin cookie: `__Host-ecom_admin_session`.
- Both cookies are host-only (`Domain` omitted), `Path=/`, `HttpOnly`, `Secure`, `SameSite=Lax`.
- Authentication callbacks must bind and consume one-time `state`/nonce values. Any cross-site POST that changes session state requires an anti-forgery token or strict Origin validation.
- Admin and public cookies are never shared. A production exception would require a new security ADR.

### Session source of truth and failure modes

- PostgreSQL is the source of truth for sessions, refresh-token families, revocation, identities, invitations and security events.
- Redis is an ephemeral distributed cache, atomic rate-limit store and pub/sub revocation accelerator. Redis is not the sole session or revocation source of truth.
- Redis unavailable: registration/OTP issuance and any new-session flow that requires distributed throttling fail closed with `503`; logout/revoke continues against PostgreSQL; validation of an existing session may fall back to PostgreSQL with a bounded timeout.
- A degraded-mode alert is emitted without credentials or raw PII.

### Lifetimes

| Principal | Access/session idle | Absolute/refresh lifetime | Re-authentication |
|---|---:|---:|---|
| Customer web | 30 minutes | 30 days | Contact/security change, suspicious login |
| Customer mobile | Access JWT 10 minutes | Refresh family 30 days | Contact/security change, suspicious login |
| Seller/organization | 15 minutes | 12 hours | Payout, ownership or security change |
| Staff/admin | 10 minutes | 8 hours, no persistent login | Privilege change and high-risk action |

Sliding renewal cannot extend an absolute lifetime. These values are configuration defaults and require product/security approval before production cutover.

### Identity, role and account policy

- A self-registered user receives customer-level permissions only. Seller/organization membership and privileged capabilities require an application plus approval; self-registration cannot assign them.
- Staff accounts are created through expiring, single-use invitations sent to a verified destination. The inviter cannot grant a role above their own authority. Acceptance requires MFA enrollment before privileged use.
- The first production admin is bootstrapped once through an audited deployment/operations command using a secret supplied by the deployment secret store. Startup seed passwords and public admin-registration endpoints are forbidden. Later admins use invitations and dual-control approval.
- Google may auto-link to an existing account only when Google returns `email_verified=true`, the provider/issuer and audience are validated, the local email is verified, policy explicitly allows that tenant/domain, and no conflicting high-risk identity exists. Otherwise require an authenticated link flow or step-up verification.
- Identity unlink must leave at least one verified sign-in method and requires recent authentication.

### Passkey and QR assumptions

- Passkey RP ID is the registrable root domain; origins are exact HTTPS origins for approved public/auth/admin hosts. Development localhost origins are explicit and never copied to production.
- QR callback/approval URLs use exact HTTPS allow-listed origins, one-time server-side challenges, audience binding, short expiry and atomic consume. The QR payload contains no bearer session or reusable secret.
- Exact RP ID, origins and mobile deep-link association files are **BLOCKED** pending final production domains and mobile application identifiers.

## Migration strategy

1. **Expand:** add V2 session/identity/security-event structures and dual-read token protection without removing V1 entities or routes.
2. **Backfill:** create identities/session-family metadata for eligible active users; never reconstruct or copy plaintext credentials into new structures.
3. **Cutover:** route clients by capability/version, observe security and refresh-reuse metrics, and keep a documented rollback switch.
4. **Contract:** retire legacy fields/routes only after client adoption, retention expiry, data reconciliation and explicit API/schema approval. Existing baseline migrations are immutable; every schema change is additive in a new migration.

## Security and operational approval gates

- Security: ADR, threat model, cookie/CSRF model, lifetimes, Google-link policy and security-event retention.
- Platform: root domains, TLS, trusted proxy CIDRs, Redis HA/SLO, secret store/KMS and callback/deep-link configuration.
- Product/operations: seller approval, staff invitation owners, admin bootstrap custodians and account-recovery policy.
- Data/privacy: PII minimization, retention, deletion and security-event access.
- API/database: any public V2 activation, migration, cutover or V1 contract removal.

## Blocked production inputs

| Input | Status | Required owner/evidence |
|---|---|---|
| Root domain, exact HTTPS origins and callback URLs | BLOCKED | Platform DNS/TLS inventory |
| Trusted reverse-proxy CIDRs | BLOCKED | Infrastructure topology |
| Redis production HA endpoint and failure SLO | BLOCKED | Platform runbook and failover test |
| SMS vendor, sender/template IDs and credentials | BLOCKED | Vendor contract and secret-store references |
| Secret store/KMS and rotation ownership | BLOCKED | Deployment security design |
| Security-event and identity retention | BLOCKED | Security/privacy approval |

## Consequences

This separates browser and mobile credential exposure, keeps durable revocation in PostgreSQL, and makes throttling failure explicit. It adds BFF/session infrastructure and operational ownership that must be completed before V2 production activation.
