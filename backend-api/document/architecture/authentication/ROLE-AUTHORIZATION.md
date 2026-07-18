# Role and policy control map

## Source of truth and bootstrap

`Permissions.GetAll()` defines 13 policy codes. `PolicySeeder` persists and deactivates/reactivates system policies. `RoleSeeder` creates only `ADMIN` and `USER`:

| Role | Seeded policy result |
|---|---|
| `ADMIN` | Every active policy defined in code |
| `USER` | `user.read`, `user.update` |

Public Password V2 registration always resolves `USER`. There is no source-backed Admin bootstrap, staff invitation, seller approval, or organization-membership flow.

## Authorization path

```text
Permissions.GetAll
  -> Tbl_Policy / PolicySeeder
  -> Tbl_Role + Tbl_RolePolicy / RoleSeeder
  -> Tbl_User.RoleId + optional Tbl_UserPolicy
  -> UserAuthorizationSnapshotService
  -> JWT role + policy claims at V2 login/refresh
  -> PermissionAuthorizationHandler checks "policy" claim
```

`UserPolicy` grants add a policy and denies remove it from the role-derived set while unexpired. Authorization handler is claim-based; it does not query DB per request.

## Existing privileged routes and checks

- V1 Identity policy listing/role-policy adjustment requires `ADMIN` policy/role checks.
- V1 role CRUD and user-role assignment use policy attributes such as `roles.assign_role`.
- `UpdateUserRoleCommandHandler` additionally requires current role code `ADMIN`, blocks self-role change, and revokes legacy `JwtRefreshToken` records.
- `AdjustRolePolicyCommandHandler` supports role-priority comparison for non-Admin callers, but its route itself requires Admin.

## Required review questions for every role/policy change

1. Is caller authorization enforced both at route and handler?
2. Can caller change self, peer, or higher-priority role?
3. Are V1 refresh tokens, V2 `UserSession`, V2 `SessionRefreshToken`, and security stamp invalidated?
4. Is a `SecurityEvent` written with no raw PII/secret?
5. Will existing JWT policy claims remain overprivileged until expiry? If yes, is revocation/step-up required?

## Verified gaps

- `UpdateUserRoleCommandHandler` does not revoke `UserSession`/`SessionRefreshToken`, rotate `SecurityStamp`, or write a security event.
- `AdjustRolePolicyCommandHandler` does not invalidate affected active sessions/tokens or write a security event.
- No Admin bootstrap/invitation approval workflow exists. Do not seed a public/admin credential as a substitute for that production flow.
- No explicit seller/staff/organization role policy/session matrix exists in source; session engine has timeout branches for those strings but seeder creates only ADMIN and USER.
