# Risk Map

## Highest-Risk Areas
- Auth, JWT, refresh tokens, Logout, current-user claims, role and policy management.
- Permission constants and seeded policies.
- EF migrations, `ApplicationDbContext`, entity configurations, UnitOfWork, and repository behavior.
- Scale realtime: Code 503 telemetry, Redis cache keys, SignalR group names, session/scale/cycle identity.
- IoT control flows: DeviceHub, WarningDeviceHub, DeviceControl, firmware, schedules, invoke direct methods.
- FCM notifications and recipient resolution.
- Warehouse, import/export receipt, inventory check, and stock transfer flows.
- Background workers: IoT, MQTT, sensor logging, Oxy alerts, AI Service Bus, camera workers, data maintenance.
- Camera/media services and Basic Auth/TLS handling.
- Large report/export generation.

## Known Debt Signals
- Very large controllers and handlers.
- Broad `catch (Exception)` patterns returning generic `TResult` errors.
- Mojibake in comments and messages.
- Typo-bearing filenames and type names.
- Duplicate or overlapping validation paths between MVC and MediatR.
- MediatR validation appears registered both inside `AddMediatR` and again as a transient behavior.
- Application references EF Core and ASP.NET Core abstractions by design; do not "fix" this with broad architecture rewrites.

## Safe Change Rules
- For high-risk areas, trace the full boundary-to-source flow before editing.
- Preserve existing public contracts and error message keys.
- Prefer adding a narrow guard or fixing a specific handler over moving shared architecture.
- Verify with the narrowest affected build first.
- Report unverified runtime behavior when no automated tests exist.

## Do Not Casually Change
- `Core/Ecom.Application/Features/Auth/Commands/Logout/*`.
- `Core/Ecom.Domain/Constants/Permissions.cs`.
- `Core/Ecom.Domain/Constants/ErrorCodes.cs`.
- `Infrastructure/Ecom.Infrastructure/Migrations/*`.
- `Presentation/Ecom.API/appsettings*.json`.
- SignalR group names: `ScaleSession_{sessionId}`, `Scale_{scaleId}`, `Device_{iotDeviceId}`, `Cycle_{cycleId}`, `Pond_{pondId}`.

