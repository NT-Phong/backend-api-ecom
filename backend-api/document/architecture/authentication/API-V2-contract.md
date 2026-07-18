# Authentication API V2 boundary

Status: contract proposal only. No endpoint in this document is implemented by Authentication Cluster 1.

## Contract rules

- Namespace is `/api/v2`; JSON uses the existing `ApiResponse<T>` envelope and stable `ErrorCodes`/message-key convention.
- Credentials, OTP values, provider authorization codes, passkey private material, QR secrets and session cookies never appear in response bodies or logs.
- Mutations accept an `Idempotency-Key` where replay could create a new identity/session/invitation. Requests and responses carry a trace ID.
- `401` means no valid authentication; `403` means authenticated but not authorized; `409` is a state conflict; `429` is throttling; `503` is fail-closed dependency unavailability.
- Public recovery/registration responses are neutral and do not confirm whether an identifier exists.
- Browser session endpoints require BFF origin/CSRF controls. Mobile token endpoints return bearer/refresh credentials only to the native client flow.

## Locked namespaces

| Namespace | Boundary |
|---|---|
| `/api/v2/auth/*` | Register, authenticate, recovery and provider callbacks/orchestration |
| `/api/v2/auth/sessions/*` | List current user sessions, revoke one, logout-all, mobile refresh-family operations |
| `/api/v2/auth/identities/*` | List/link/unlink verified phone, email and external identities |
| `/api/v2/auth/passkeys/*` | Registration/authentication options and verification only |
| `/api/v2/auth/qr/*` | Create, scan, approve/reject and atomically consume cross-device challenges |
| `/api/v2/admin/user-invitations/*` | Privileged staff invitation lifecycle; admin authorization required |

## Resource outlines

### Auth

- `POST /api/v2/auth/register`: accepts a normalized destination and consent metadata; always returns a neutral accepted result.
- `POST /api/v2/auth/otp/requests`: issues a purpose-bound challenge; returns an opaque `challengeId`, resend time and expiry, never the OTP.
- `POST /api/v2/auth/otp/verifications`: accepts `challengeId` plus code; success establishes a web BFF session or returns the mobile token result according to an explicitly bound client type.
- `POST /api/v2/auth/token/refresh`: mobile only; rotates one refresh token atomically and rejects reuse.
- `POST /api/v2/auth/logout`: revokes the current session and clears the host-only cookie for BFF callers.
- Provider callbacks consume server-held state and authorization code; the public callback never accepts a local role or redirect outside an allow-list.

### Sessions

- `GET /api/v2/auth/sessions`: metadata only: session ID, client class, coarse device label, created/last-seen/expiry, current flag.
- `DELETE /api/v2/auth/sessions/{sessionId}`: owner or privileged operation with recent-auth rules.
- `DELETE /api/v2/auth/sessions`: logout-all, preserving only an explicitly requested current session if policy permits.

### Identities

- `GET /api/v2/auth/identities`; `POST /link-requests`; `POST /link-verifications`; `DELETE /{identityId}`.
- Link/unlink is recent-auth protected, conflict-safe and audited. At least one verified login method must remain.

### Passkeys

- `POST /registration/options`, `POST /registration/verify`, `POST /authentication/options`, `POST /authentication/verify`, `DELETE /{credentialId}`.
- Challenges are random, purpose/origin/RP/user-bound, short-lived and single-use. Credential counters and backup eligibility/state are validated and audited.

### QR

- `POST /api/v2/auth/qr/challenges`, `POST /{id}/scan`, `POST /{id}/approve`, `POST /{id}/reject`, `POST /{id}/consume`.
- State transition is atomic: `Created -> Scanned -> Approved|Rejected -> Consumed`; expiry is terminal. Approval binds the approving session, intended browser and nonce.

### Staff invitations

- `POST /api/v2/admin/user-invitations`, `GET /{id}`, `POST /{id}/resend`, `POST /{id}/revoke`, and a public acceptance operation under `/api/v2/auth/invitations/{token}/accept`.
- Invitation tokens are random, stored protected, single-use and expiring. Role scope, inviter authority, MFA enrollment and all transitions are audited.

## Versioning and compatibility

V1 remains intact during expand/backfill/cutover. V2 DTOs must not reuse V1 token-bearing response types. Activating any V2 route, changing public V1 semantics beyond a security correction, or adding its persistence schema is a separate approval gate.
