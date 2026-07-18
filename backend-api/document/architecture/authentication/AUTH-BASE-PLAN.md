# Authentication Base Plan — bản cập nhật 2.0

- Cập nhật: 2026-07-18
- Trạng thái: **Kế hoạch thực thi hiện hành, chưa là phê duyệt production**
- Nguồn sự thật: source, migration và test hiện có; ADR/threat model là quyết định mục tiêu cho đến khi được phê duyệt.

## 1. Mục đích và cách đọc

Tài liệu này thay thế roadmap 11 phase ban đầu vốn mô tả trạng thái trước khi Password Authentication V2 được thêm vào source. Các chi tiết kiến trúc không lặp lại ở đây được giữ tại:

- [ADR-001](ADR-001-authentication-architecture.md): kiến trúc đích, domain, cookie và ownership cần phê duyệt.
- [API V2 contract](API-V2-contract.md): boundary công khai dự kiến.
- [Threat model](THREAT-MODEL.md): đe dọa, security event và production inputs đang `BLOCKED`.
- [Current state](CURRENT-STATE.md): source map ngắn, được cập nhật theo source.
- [Role authorization](ROLE-AUTHORIZATION.md): policy, JWT claim và các thay đổi quyền nhạy cảm.

### Quy ước trạng thái

| Trạng thái | Ý nghĩa |
|---|---|
| **Complete** | Source và toàn bộ gate của phase đã có bằng chứng phù hợp. |
| **Source implemented — verification pending** | Code/migration đã tồn tại nhưng gate PostgreSQL, API E2E hoặc vận hành chưa được chứng minh. Không được coi là production-ready. |
| **Partial** | Một phần scope đã có; còn thiếu hành vi hoặc gate bắt buộc. |
| **Missing** | Chưa có implementation source-backed. |
| **Blocked** | Không thể chốt an toàn khi chưa có quyết định/ownership môi trường. |

Không có phase nào trong roadmap này hiện được đánh dấu **Complete**. Điều này là chủ ý: test đơn vị và test với fake counter store không thay thế được bằng chứng PostgreSQL, Redis đa instance, email provider hay reverse proxy production.

## 2. Quyết định đang áp dụng

1. Slice đang ưu tiên là **mobile token**: access JWT 10 phút và refresh token quay vòng. Browser BFF/cookie chưa nằm trong acceptance gate hiện hành.
2. Password V2 dùng `Username` hoặc email, password BCrypt. `PasswordAuthenticationV2` mặc định tắt; khi tắt, các endpoint V2 chính trả `404`.
3. User đăng ký V2 được tạo `Pending`, chỉ `Active` sau email verification. Role mặc định là `USER`; đăng ký công khai không tạo Admin/Seller/Staff.
4. Development có thể trả verification token chỉ khi option bật rõ ràng; validator từ chối option đó ngoài Development. Password tối thiểu Development có thể là 5 để kiểm thử; non-Development phải từ 15 ký tự.
5. PostgreSQL là nguồn sự thật của session, refresh family, revoke và security event. Redis chỉ phục vụ distributed throttling/cache; flow tạo account/session mới fail-closed khi limiter báo unavailable. Logout/revoke phải tiếp tục chạy bằng PostgreSQL.
6. OTP V1, Google, Passkey và QR không được thay đổi ngầm trong các phase Password V2. Không xóa migration/table/endpoint legacy.
7. Không apply migration lên staging/Azure/production trong roadmap này nếu chưa có approval riêng.

## 3. Hiện trạng đã xác minh trong source

### 3.1 Endpoint và luồng Password V2 thực tế

| Contract hiện có | Source boundary | Trạng thái |
|---|---|---|
| `POST /api/v2/auth/register` | `AuthV2Controller` → `RegisterPasswordCommand` | Source implemented — verification pending |
| `POST /api/v2/auth/email/verify/confirm` | `ConfirmEmailCommand`, `VerificationChallenge` | Source implemented — verification pending |
| `POST /api/v2/auth/login/password` | dummy BCrypt, account/device/IP throttling → `AuthenticationSessionEngine` | Source implemented — verification pending |
| `POST /api/v2/auth/refresh` | `SessionRefreshService` conditional update / family revoke | Source implemented — verification pending |
| `GET /api/v2/auth/me` | JWT, Active user, session expiry/revoke, security stamp | Source implemented — verification pending |
| `POST /api/v2/auth/logout`, `logout-all`, `DELETE sessions/{id}` | `LogoutCommandHandler` | Partial |
| `password/forgot`, `reset`, `change`, `setup`, `csrf` | Password-management handlers/controller | Partial; chưa là acceptance slice đầy đủ và feature gate chưa nhất quán |

Luồng login thành công hiện tại là:

```text
Username/email + password
  -> normalized lookup hoặc dummy BCrypt
  -> account/device limiter
  -> kiểm tra credential, lockout, User Active
  -> AuthenticationSessionEngine
  -> UserSession + SessionRefreshToken + LoginSucceeded
  -> transaction commit của caller
  -> JWT + raw refresh token chỉ cho mobile response
```

`AuthenticationSessionEngine` phân timeout theo role, tạo refresh token ngẫu nhiên 32 byte và chỉ lưu token đã protect/hash. JWT session có `session_id`, `security_stamp`, role và policy claims; không thêm phone cho session JWT. `SessionRefreshService` dùng điều kiện `UsedAt is null`, `RevokedAt is null`, `ExpiresAt > now`; reuse revoke family/session và ghi `RefreshTokenReuseDetected`.

### 3.2 Persistence đã có

Ba forward migration đã tồn tại và đã được user chạy trên database local theo log đã cung cấp:

| Migration | Nội dung chính |
|---|---|
| `20260718112936_AddIdentitySessionFoundation` | normalized contact/security stamp, `VerificationChallenge`, `UserSession`, `SessionRefreshToken`, `SecurityEvent`. |
| `20260718124646_AddPasswordAuthenticationV2` | username/normalized username, `PasswordCredential`, Password V2 foundation. |
| `20260718133717_HardenPasswordAuthenticationV2` | `AlgorithmVersion`, `LastFailedAt`, `LockedUntil` cho credential. |

Các unique partial index đã có cho normalized username/email/phone của user (khi chưa soft-delete); `SessionRefreshToken.TokenHash` unique. `SecurityEvent` bị chặn update/delete qua EF `SaveChangesAsync`, nhưng chưa có DB role/trigger để chống raw SQL hoặc DBA mutation.

### 3.3 Test hiện có và giới hạn bằng chứng

| Nhóm | Bằng chứng hiện có | Không chứng minh được |
|---|---|---|
| Domain | `AuthenticationFoundationTests` kiểm tra verify identity trước khi activate, challenge lock, refresh rotate một lần, session revoke idempotent, username/lockout. | Concurrent PostgreSQL, rollback transaction. |
| Password/config | `PasswordAuthenticationV2Tests`, `BcryptPasswordHasherTests`. | API contract thực tế, timing tolerance, benchmark BCrypt production. |
| Containment | `AuthenticationContainmentTests`: redaction, Development guard, SMS fail-closed, rate-limit partition/expiry trên controllable store. | Redis thật/multi-instance, V1 endpoint E2E, toàn repository log path. |

## 4. Đánh giá roadmap cũ theo phase

| Phase cũ | Trạng thái mới | Đánh giá và lý do |
|---|---|---|
| 0 — ADR, contract, threat model | **Partial** | ADR, API boundary, threat model và security taxonomy đã có; còn approval domain/proxy/Redis HA/KMS/retention, nên chưa Complete. |
| 1 — containment OTP/JWT V1 | **Partial** | Đã có hashing/redaction, test-account guard, SMS/limiter fail-closed tests. Chưa có đầy đủ V1 API regression, Redis thật và source-wide log evidence. |
| 2 — identity/session persistence | **Source implemented — verification pending** | Entity, configuration, migration và index đã tồn tại; thiếu PostgreSQL uniqueness/consume/rollback/concurrency và legacy backfill checkpoint. |
| 3 — session engine/login-refresh-logout | **Partial** | Mobile session engine, refresh rotation/replay và `/me` đã có. Chưa có risk/step-up/outbox, PostgreSQL concurrency evidence, cookie BFF/CSRF acceptance hoặc chứng minh invalidate Redis best-effort. |
| 4 — password/register/verification/recovery | **Partial** | Register/verify/login mobile được cài đặt. Không có production email provider/outbox/retry/resend; recovery chưa đạt full acceptance, test/API E2E thiếu. |
| 5 — Google OIDC | **Missing** | Để sau khi password/session/role gates đạt. |
| 6 — seller approval/staff invitation/admin bootstrap | **Missing** | Source chỉ seed `ADMIN` và `USER`; chưa có invite/approval/bootstrap an toàn. |
| 7 — device/audit/risk/step-up | **Partial** | Session/security event có nền tảng. Không có risk assessment, step-up, retention/access control hay DB tamper protection. |
| 8 — Passkey | **Missing** | Chưa thêm entity, API, dependency hoặc RP/origin được phê duyệt. |
| 9 — QR handoff | **Missing** | Chưa thêm entity, state machine hay API. |
| 10 — cutover/pentest/decommission V1 | **Missing** | V1 vẫn chạy; chưa có telemetry/backfill/capability routing/pentest. |

## 5. Findings chặn hoặc giới hạn rollout

| ID | Severity | Evidence | Rủi ro | Quyết định roadmap |
|---|---|---|---|---|
| AUTH-001 | High | `UpdateUserRoleCommandHandler`, `AdjustRolePolicyCommandHandler` chỉ revoke legacy refresh hoặc không revoke gì. | JWT/session V2 có thể còn policy/role cũ đến expiry sau thay đổi quyền. | Phase R2 bắt buộc trước khi cấp Seller/Staff/Admin qua V2. |
| AUTH-002 | High | Refresh/challenge/session concurrency hiện chỉ có domain/fake-store test. | Không đủ bằng chứng atomicity/replay/rollback trong PostgreSQL. | Phase R1 là gate trước khi bật cohort Password V2. |
| AUTH-003 | High | Không có sender/outbox/retry cho V2 email verification; Development token là cơ chế test. | Không thể phát hành registration/password recovery production an toàn. | Phase R3 bắt buộc trước production. |
| AUTH-004 | Medium | `password/forgot`, `reset`, `change`, `setup`, `csrf` không cùng mức feature gate/acceptance với slice chính. | Có surface chưa được rollout/kiểm thử theo cùng policy. | Phase R3 chuẩn hóa boundary/flags/contract. |
| AUTH-005 | Medium | Session engine chưa có risk assessment, step-up hoặc publish-after-commit. | Không đủ kiểm soát hành vi rủi ro cao/privileged. | Phase R5 sau khi role/onboarding policy chốt. |
| AUTH-006 | Medium | Security event append-only chỉ enforced qua EF. | Raw SQL/DB privileged actor vẫn có thể sửa audit. | Phase R5 + platform/data approval. |
| AUTH-007 | Medium | API contract proposal ghi namespace/route khác một phần route đang expose (ví dụ refresh). | Frontend/other client có thể tích hợp nhầm contract. | Phase R0R phải reconcile và version contract. |
| AUTH-008 | Medium | Browser cookie code có tồn tại nhưng BFF, CSRF, proxy/domain chưa được acceptance. | Không được bật web cookie chỉ vì có code. | Tách riêng Phase R4. |
| AUTH-009 | Medium | MessagePack 2.5.187 có cảnh báo moderate/high trong restore log do user cung cấp. | Dependency security risk ngoài logic Auth V2. | Theo dõi dependency/security remediation riêng trước production. |

## 6. Roadmap thay thế theo thứ tự thực thi

Các phase dưới đây thay thế thứ tự “Phase 0 → 10” cũ. Một phase chỉ chuyển sang `Complete` khi đủ gate của chính nó; không bỏ qua chỉ vì source đã có code.

### Phase R0R — Rebaseline, contract và approval record

- **Trạng thái:** In progress (tài liệu này là một deliverable; approval chưa có).
- Reconcile [API V2 contract](API-V2-contract.md) với endpoint thực tế và chốt canonical names/DTO/error code trước khi frontend dùng rộng.
- Chốt owner/evidence cho root domain, callback URL, trusted proxy CIDR, Redis HA/SLO, KMS/Data Protection, event retention.
- Lập ADR bổ sung nếu session lifetime source đang khác lifetime trong ADR; không tự đổi timeout code trong phase tài liệu.
- Đưa AUTH-001…009 vào backlog có owner, severity và release gate.

**Gate:** ADR/threat/API boundary được Security, Platform, Product/Operations phê duyệt hoặc input chưa có phải tiếp tục ghi `Blocked`; không kích hoạt production V2.

### Phase R1 — Password V2 verification hardening (mobile acceptance)

- **Trạng thái:** Missing verification; source foundation đã có.
- Viết PostgreSQL integration tests bằng hai DbContext/connection cho unique normalized username/email, atomic challenge consume/supersede, failed-login counter, refresh rotation/replay, session revoke/logout-all và transaction rollback.
- Viết API E2E cho register → Development confirm → username/email login → `/me` → refresh → replay → logout → logout-all.
- Kiểm thử Redis unavailable với implementation thật/test container; verify register/login/refresh trả `503`, logout vẫn `200`/idempotent.
- Kiểm thử actual log pipeline không ghi password/hash/JWT/refresh/raw email; kiểm thử unknown và wrong-password cùng response/timing tolerance.
- Benchmark BCrypt cost 12 trên host tương đương production; lưu kết quả và quyết định cost riêng.

**Gate:** không test skip; PostgreSQL/API/Redis evidence pass; `dotnet ef migrations has-pending-model-changes` sạch; feature vẫn default off cho đến rollout approval.

### Phase R2 — Role/policy mutation security

- **Trạng thái:** Missing; ưu tiên cao trước elevated user rollout.
- Khi đổi role, user policy hoặc role policy: transactionally revoke `UserSession` và `SessionRefreshToken` bị ảnh hưởng, rotate `SecurityStamp`, revoke legacy token tương thích và ghi `SecurityEvent` không có PII/secret.
- Xác định rõ impact của đổi `RolePolicy`: revoke mọi user có role đó, hoặc dùng policy-version/session claim có database validation; không để stale JWT claim mang quyền cao.
- Siết caller authority, self/peer/higher-role checks và audit; route + handler cùng enforce.
- Thêm PostgreSQL/API regression cho session cũ sau role/policy change.

**Gate:** role/policy mutation không để token V2 còn quyền cũ; audit/security-event tồn tại; chưa mở Admin bootstrap/invitation.

### Phase R3 — Email delivery, password recovery và rollout controls

- **Trạng thái:** Missing/Partial.
- Thêm email verification resend và password reset delivery bằng outbox after-commit, retry/idempotency/observability; provider failure không leak account và không rollback account/challenge sai thứ tự.
- Chốt callback base URL từ approved config, không nhận request host; production startup fail nếu provider/callback/secret storage thiếu.
- Đưa forgot/reset/change/setup vào feature boundary thống nhất hoặc disable có chủ ý; atomic consume/revoke-all/security stamp/event cho recovery/change.
- Bổ sung flags cho registration/login/recovery, rollout internal → staging → buyer cohort nhỏ; rollback chỉ tắt flag, không drop schema.

**Gate:** email provider/outbox test pass; production config validation pass; reset token replay/expiry và session invalidation PostgreSQL E2E pass.

### Phase R4 — Web BFF, cookie và CSRF (tách biệt mobile)

- **Trạng thái:** Missing acceptance.
- Chỉ bắt đầu sau approved public/admin host và proxy topology. BFF dùng host-only `__Host-ecom_session`/admin cookie, `HttpOnly`, `Secure`, `Path=/`, `SameSite=Lax`, không `Domain`.
- Cookie session creation/revoke chỉ sau transaction commit; clear cookie sau logout commit; POST/PATCH/PUT/DELETE cookie-authenticated yêu cầu antiforgery/origin controls.
- API trả JSON 401/403, không redirect HTML. Test cookie flags, fixation, CSRF, forwarded-header trust và Redis-down logout.

**Gate:** staging browser E2E và reverse-proxy review pass. Mobile token contract không thay đổi trong phase này.

### Phase R5 — Session assurance, audit và privileged onboarding

- **Trạng thái:** Missing/Partial.
- Thêm risk assessment/step-up và notification-after-commit; finalize security event access/retention/tamper evidence.
- Thiết kế Seller/organization approval; Staff invitation single-use/expiry/MFA; audited one-time admin bootstrap. Public registration vẫn chỉ `USER`.
- Định nghĩa role/session matrix có source-backed role codes/policies thay vì chỉ branch timeout string.

**Gate:** privilege escalation tests pass, product/operations owner rõ, không dùng test admin/public seed làm bootstrap production.

### Phase R6 — OTP V1 compatibility và cutover preparation

- **Trạng thái:** Missing.
- Dual-write/read `OtpToken` → `VerificationChallenge`, route OTP successful authentication qua shared engine, migrate refresh metadata theo batch/checkpoint, telemetry fallback/reconciliation.
- Không xóa `OtpToken`/`JwtRefreshToken` hay V1 route trong phase này.

**Gate:** V1 regression, dual-read/write PostgreSQL tests, durable checkpoint, adoption metrics ổn định.

### Phase R7 — Google OIDC and verified identity linking

- **Trạng thái:** Missing; không được bắt đầu trước R1–R5.
- PKCE/state/nonce, exact issuer/audience/redirect validation, approved `email_verified` auto-link policy, conflict/step-up handling, identity audit/revoke.

**Gate:** provider callback/domain approval, mix-up/open-redirect/link-conflict tests pass.

### Phase R8 — Passkey/WebAuthn

- **Trạng thái:** Missing.
- Additive schema/API only after approved RP ID/origin; exact origin/RP validation, one-time challenge, signature/counter/user-verification and recovery policy.

**Gate:** phishing-resistant admin path, replay/concurrency/origin tests pass.

### Phase R9 — QR cross-device handoff

- **Trạng thái:** Missing.
- Server-side state machine, short TTL, bind browser/approving session/context, atomic consume and no bearer secret in QR.

**Gate:** replay/race/SSE hijack/expiry/reject tests and approved deep-link/domain topology.

### Phase R10 — Cutover, independent security validation và legacy contract

- **Trạng thái:** Missing.
- Expand → backfill → dual-read/write → cohort cutover → contract only. Run SAST/dependency scan, DAST/staging, manual penetration test, operational restore/incident exercise.
- Disable V1 issuance only after client adoption and token-retention expiry; removal needs a new explicit API/schema approval.

**Gate:** no Critical/High auth findings, all required tests pass, telemetry proves adoption, Security/Platform sign-off.

## 7. Standard verification per implementation phase

```powershell
dotnet build Ecom.sln --no-restore /p:UseSharedCompilation=false
dotnet test Tests/Ecom.Domain.Tests/Ecom.Domain.Tests.csproj --no-restore
dotnet test Tests/Ecom.IntegrationTests/Ecom.IntegrationTests.csproj --no-restore
dotnet ef migrations has-pending-model-changes --project Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj --startup-project Presentation/Ecom.API/Ecom.API.csproj --context ApplicationDbContext --no-build
git diff --check
```

Ngoài các lệnh trên, phase liên quan persistence/session bắt buộc có PostgreSQL integration evidence; phase Redis/email/BFF phải có dependency hoặc staging evidence. “Skipped”, fake in-memory store, hoặc source-only inspection không được đánh dấu gate pass.

## 8. Thay đổi so với bản plan cũ

- Đã **loại khỏi roadmap thực thi trực tiếp** các câu mô tả “chưa có Password/Session V2”, vì source hiện đã có foundation và mobile slice.
- Không xóa mục tiêu Google, Passkey, QR, seller/staff/admin onboarding; chúng được giữ lại ở R7–R9/R5 nhưng bị đặt sau security gates thực tế.
- Tách rõ web BFF (R4) khỏi mobile Password V2 (R1), để code cookie tồn tại không bị hiểu nhầm là web flow đã được nghiệm thu.
- Đưa role/policy session invalidation thành R2 độc lập vì đây là khoảng trống High rõ ràng trong source hiện tại.
- Không đánh dấu migration đã chạy local là deployment approval. Mọi apply staging/Azure/production vẫn cần phê duyệt riêng.

## 9. Gate kế tiếp

**Gate gần nhất: Phase R1 — Password V2 verification hardening.**

Không mở Google/Passkey/QR hay cohort production trước khi có PostgreSQL/API/Redis evidence cho register, login, refresh replay, logout/logout-all. Nếu hệ thống bắt đầu tạo hoặc đổi quyền Seller/Staff/Admin, Phase R2 phải được thực hiện trước hoặc cùng release đó.
