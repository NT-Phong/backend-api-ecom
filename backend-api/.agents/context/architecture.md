# Architecture

## Style
The backend follows Clean Architecture with CQRS through MediatR.

Intended dependency direction:

```text
Presentation -> Application -> Domain
Infrastructure -> Application/Domain abstractions
```

Domain must not depend on Infrastructure or Presentation.

The real codebase is pragmatic rather than pure: Application currently references EF Core abstractions, ASP.NET Core `IFormFile`, cache abstractions, SignalR interfaces, and IoT abstractions. Do not attempt a broad purity refactor unless the user explicitly asks.

## Domain Layer
Location: `Core/Ecom.Domain`

Responsibilities:
- Entities, enums, constants, models, exceptions, domain events, and domain interfaces.
- Business invariants and state changes through entity methods where they exist.
- Base entity behavior for audit, concurrency stamp, soft delete, and domain events.
- Permission and message catalogs in `Constants`.

Rules:
- No Infrastructure or Presentation dependencies.
- Avoid direct mutation from external DTOs when a domain method exists or should exist.
- Keep persistence and transport concerns out of entities.
- Treat `Permissions.cs`, `ErrorCodes.cs`, and `MessageKey` as contract-like.

## Application Layer
Location: `Core/Ecom.Application`

Responsibilities:
- CQRS feature folders under `Features/**`.
- Commands, queries, handlers, validators, DTOs/models, and application services.
- MediatR pipeline behaviors for cross-cutting concerns.
- Abstractions such as `IUnitOfWork`, `ICurrentUser`, cache interfaces, IoT service interfaces, FCM interfaces, and SignalR interfaces.
- Some complex reads and auth flows use `IApplicationDbContext` directly.

Rules:
- Use FluentValidation for command/query validation.
- Mutations requiring transaction safety should use `[EnableUnitOfWork]`.
- Use `IUnitOfWork.Repository<TEntity>()` where the local feature pattern expects repository access.
- Return existing `TResult`/`ErrorCodes`/`MessageKey` patterns for handled business failures.
- For file-upload commands, existing request models may use ASP.NET Core `IFormFile`; match nearby file-command patterns.

## Infrastructure Layer
Location: `Infrastructure/Ecom.Infrastructure`

Responsibilities:
- EF Core PostgreSQL persistence.
- UnitOfWork and generic repository implementation.
- Redis cache and distributed locking, with memory-cache fallback.
- Azure Event Hub, Azure Service Bus, MQTT, Firebase FCM, Azure Blob Storage, camera/media integrations, and AI API clients.
- IoT telemetry handlers and background services.
- Security, identity, telemetry, health checks, seeding, and external service implementations.

Rules:
- Implement abstractions defined in Domain/Application.
- Keep integration details out of Domain.
- Sanitize logs and never expose secrets.
- Treat camera/media Basic Auth and TLS handling as security-sensitive.

## Presentation Layer
Location: `Presentation/Ecom.API`

Responsibilities:
- ASP.NET Core host, controllers, filters, middleware, hubs, serialization, API versioning, Swagger/ReDoc, health checks, and request timeout setup.
- Controllers map HTTP to MediatR commands/queries and call `HandleResult` or explicit `ApiResponse` patterns.
- SignalR hubs manage realtime subscriptions and group delivery.

Rules:
- Controllers should stay as thin as the existing feature allows.
- Do not put new business rules in controllers.
- Preserve existing `ApiResponse` and `BaseController.HandleResult` conventions.
- Some controllers explicitly return `Ok(ApiResponse<object>.Ok(...))` for success; preserve nearby behavior when editing.

## MediatR Flow

```text
Controller
-> Mediator.Send(command/query)
-> pipeline behaviors
-> handler
-> repository/service/domain logic
-> TResult
-> BaseController.HandleResult or explicit ApiResponse
```

Current behaviors include current-user injection, validation, unhandled exception logging, performance, authorization, logging, and UnitOfWork. Validation appears registered both inside `AddMediatR` and as a transient behavior; be careful before changing behavior order.

## Transaction Model
- `[EnableUnitOfWork]` activates `UnitOfWorkBehavior`.
- `UnitOfWork.CommitTransactionAsync` calls `SaveChangesAsync`.
- Many existing handlers still call `unitOfWork.SaveChangesAsync` explicitly before returning.
- Repository insert/update/delete methods usually only track changes. Persistence happens at `SaveChangesAsync` or transaction commit.
- Do not remove explicit saves or change transaction boundaries broadly without tests.

## IoT Realtime Flow

```text
Device
-> Azure IoT Hub / Event Hub or MQTT
-> IoTConnectionManager or background worker
-> telemetry handler
-> Redis and/or database
-> SignalR notification service
-> hub group
-> frontend or mobile client
```

Scale Code 503 flow:

```text
EventHubConsumerService
-> IoTConnectionManager.ProcessTelemetryMessageAsync
-> IScaleMeasurementTelemetryHandler.HandleAsync
-> ScaleCacheService.SetLatestSnapshotAsync
-> TelemetryNotificationService.BroadcastScaleDataAsync
-> TelemetryHub groups
```

For Scale live weighing, `ScaleSession_{sessionId}` is the primary group for active weighing screens. `Scale_{scaleId}`, `Device_{iotDeviceId}`, and `Cycle_{cycleId}` are secondary/compatibility groups.

