# Codebase Analysis

Last reviewed: 2026-06-19

## Scope Reviewed
- Solution and project files.
- Application dependency injection and MediatR behaviors.
- Infrastructure dependency injection, persistence, Redis, SignalR, IoT, messaging, security, telemetry, and background workers.
- API host setup, controllers, middleware, filters, hubs, and response handling.
- Representative features: Auth, Scale, ScaleRecord, Warehouse, Notification, IssueReport, Plan, Camera/media, Device/DeviceHub, SensorDevice, Pond/Zone, Inventory/Receipt flows.
- Current `.agents` guidance structure.

## Architecture Findings
- The solution has four projects: Domain, Application, Infrastructure, and API host.
- The API project uses `Microsoft.NET.Sdk.Worker` while hosting ASP.NET Core controllers, SignalR, health checks, Swagger/ReDoc, and background services through Infrastructure.
- Clean Architecture is the intended shape, but the real codebase allows pragmatic Application-layer dependencies on EF Core abstractions, ASP.NET Core `IFormFile`, cache abstractions, SignalR interfaces, and IoT service interfaces.
- Domain is mostly isolated from Infrastructure and Presentation. It contains constants, entities, domain events, repository interfaces, and large message/permission catalogs.
- Application uses CQRS feature folders with MediatR `IRequest`/`IRequestHandler`, FluentValidation validators, `TResult`, `MessageKey`, `ErrorCodes`, `IUnitOfWork`, and direct `IApplicationDbContext` in some features.
- Infrastructure owns EF Core, UnitOfWork, repositories, Redis cache, distributed locks, Azure Event Hub, Azure Service Bus, MQTT, Firebase FCM, media/camera services, telemetry, seeding, and security.
- Presentation owns controllers, middleware, filters, SignalR hubs, response envelopes, request timeout, forwarded headers, Swagger/ReDoc, health endpoints, and SignalR backplane configuration.
- The current checkout contains 1,510 C# files, 59 Application feature folders, and 41 V1 controller files. Treat these numbers as a lightweight routing snapshot, not a substitute for targeted `rg` searches.
- Source code remains the authority. Agent reports and historical memory are orientation only; verify any domain claim against the current controller, command/query, handler, entity, and integration implementation before changing behavior.
- Recent high-change areas include `Plan` mutation APIs and permissions, camera stream/viewer-limit behavior, notification routing/read contracts, and scale realtime/session lifecycle.

## Code Quality And Maintainability
- Strong patterns: feature folders, thin-enough controllers for many resources, `TResult` response style, permission constants, typed SignalR client interface, Redis-backed scale realtime context, UnitOfWork behavior, no dedicated source-generated clients, and centralized API error handling.
- Technical debt: several very large controllers and handlers exist, especially `PondController`, report/excel handlers, receipt/inventory flows, and device control handlers.
- Naming debt exists and should not be casually renamed: `Hanlder`, `Handller`, `Vadilator`, `CreateImportRecept`, `ExportImporReceip`, and files with trailing spaces.
- Comments and Vietnamese messages in several files show mojibake. Do not bulk-fix encoding as part of unrelated changes because it creates noisy diffs and may alter user-visible text.
- Some controller actions manually wrap `ApiResponse<object>.Ok(...)` instead of always using `HandleResult`. Match the nearby controller style when editing.
- Many handlers catch `Exception` and return `TResult.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR)`. Preserve this style for local consistency, but do not add broad catch blocks that hide root causes in new critical code.

## Data And Control Flow
- HTTP flow: controller action -> `Mediator.Send` -> MediatR behaviors -> handler -> repository/service/domain -> `TResult` -> `BaseController.HandleResult` or explicit `ApiResponse`.
- Validation flow: MVC model validation uses `ModelValidationFilter`; MediatR command/query validation uses `ValidationBehaviour`.
- Authentication/authorization flow: ASP.NET Core `[Authorize(Policy = Permissions.*)]` protects endpoints; Application has a custom `AuthorizeAttribute` checked by `AuthorizationBehaviour` for request-level rules where used.
- Current-user flow: `CurrentUser` reads JWT claims such as `userId`, `role`, and `policy`; `CurrentUserBehaviour` fills `IUserRequest.UserId` before validation.
- Transaction flow: `[EnableUnitOfWork]` activates `UnitOfWorkBehavior`; `CommitTransactionAsync` calls `SaveChangesAsync`, but many handlers still call `SaveChangesAsync` explicitly before commit.
- Persistence flow: most features use `IUnitOfWork.Repository<TEntity>()`; some complex queries use `IApplicationDbContext` and EF Core LINQ directly.
- Realtime flow: telemetry -> `IoTConnectionManager` -> scoped telemetry handler -> Redis/cache -> `TelemetryNotificationService` -> `TelemetryHub` groups.
- Camera stream flow: `CameraController.GetStream` -> `GetCameraStreamHandler` -> `IMediaMtxService`/`MediaMtxService`; current viewer-limit handling is a warning-shaped confirmation flow that may return HTTP 429 with an `ApiResponse.Ok(...)` payload.
- Plan module flow: `PlanController` -> Plan create/update/delete commands -> `Plan`/`PlanHistory` domain and persistence; the current slice is mutation-oriented and uses explicit Plan permissions.

## Testing And Verification
- No dedicated test project is currently visible in `Ecom.sln`.
- Verification is mostly build-based: focused project builds, then API build for cross-layer changes.
- There are feature commands named `Test*Connection`, but they are application features, not automated tests.
- Behavior-changing guidance should ask for focused tests if a test project is later introduced; today it should document the missing test harness and run focused builds.

## Security And Scalability Observations
- JWT Bearer, permission policies, custom policy provider, and seeded permissions are established.
- SignalR accepts tokens from `access_token` query string for `/hubs`, which is required for WebSockets but should be treated as sensitive in logs.
- `UnhandledExceptionBehaviour` serializes full requests for error logging; future changes should avoid adding secrets or large payloads to request models without considering log exposure.
- MediaMTX HTTP client uses `DangerousAcceptAnyServerCertificateValidator`; treat camera/media changes as security-sensitive.
- Redis is optional; fallback memory cache and in-memory locks exist. Realtime/scale behavior should be checked in both Redis and fallback modes when relevant.
- Hot paths include Scale Code 503 telemetry, sensor telemetry logging, FCM notifications, AI Service Bus consumers, camera workers, and large report/export generation.

