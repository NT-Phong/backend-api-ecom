# Security Review

## Authentication And Authorization
- Confirm protected endpoints require authentication where expected.
- Confirm permission checks match nearby feature conventions.
- Do not weaken authorization to fix validation or data access bugs.
- For controllers, check `[Authorize(Policy = Permissions.*)]`.
- For SignalR hubs, check hub authorization and per-subscription access services.
- For Application request authorization, check custom `AuthorizeAttribute` only where it is already used.

## Input
- Validate request DTOs with FluentValidation where the project expects it.
- Do not trust ids, user ids, tenant/zone ids, device ids, or role ids from client payloads without checking authorization and ownership.

## Output
- Do not expose tokens, secrets, passwords, internal exceptions, connection strings, or sensitive user data.
- Preserve existing `ApiResponse` error shape.
- Do not expose refresh tokens, FCM tokens, JWT query-string `access_token`, IoT auth values, Basic Auth, or raw file payloads.

## Logging
- Log enough context for root cause analysis.
- Do not log full secret-bearing payloads.
- Prefer ids and request names over raw tokens or credentials.
- Be careful: `UnhandledExceptionBehaviour` serializes requests on failure. Do not add sensitive fields to request models without considering logging exposure.

## High-Risk Areas
- Auth, Logout, permissions, role assignment, current-user resolution.
- IoT device control commands.
- Warehouse/notification flows that affect many users.
- Migrations and data repair scripts.
- SignalR subscriptions and realtime access checks.
- Camera/media services using Basic Auth or TLS bypass.
