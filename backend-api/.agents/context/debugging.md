# Debugging

## Default Debug Flow
1. Capture the observed failure and exact error text.
2. Identify the boundary: API endpoint, SignalR hub, background worker, IoT handler, repository, cache, external integration, or seed step.
3. Trace the runtime path with targeted `rg` searches.
4. Compare against a nearby working flow.
5. Identify the root cause before editing.
6. Apply the smallest safe fix.
7. Verify with focused build/test/log inspection.

## API Debug Trace

```text
HTTP route
-> controller action
-> command/query
-> validator
-> pipeline behaviors
-> handler
-> domain/repository/service
-> TResult
-> ApiResponse
```

Also check both validation layers:
- MVC `ModelValidationFilter`
- MediatR `ValidationBehaviour`

## Permission Debug Trace

```text
controller [Authorize(Policy = Permissions.*")]
-> seeded policy / Permissions constant
-> JWT role or policy claims
-> CurrentUser.HasPolicy / HasRole
-> optional Application AuthorizeAttribute
```

## IoT / Realtime Debug Trace

```text
device payload
-> Event Hub / MQTT consumer
-> IoTConnectionManager or background worker
-> telemetry handler
-> Redis snapshot/context
-> persistence fallback if needed
-> SignalR notification service
-> hub group
-> frontend/mobile subscriber
```

## Common Risk Areas
- Missing or misplaced `[EnableUnitOfWork]`.
- Validator and API contract mismatch.
- DTO mapping that drops ids or domain state.
- Redis cache expiry causing disconnected realtime status.
- SignalR group mismatch.
- Query tracking or soft-delete behavior.
- Permission checks missing from security-sensitive flows.
- Logging that exposes sensitive data.
- Generic `catch (Exception)` hiding the original failure from API clients.
- Duplicate MediatR validation registration affecting behavior order.
- `BaseRepository` dynamic `orderBy` with invalid property names.
- Camera/media HTTP client TLS bypass.

## Debugging Output
Return:
- Root cause.
- Evidence.
- Fix summary.
- Files changed.
- Verification.
- Remaining risk.
