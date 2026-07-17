# Project Map

## Solution
- `Ecom.sln` is the solution root.
- `Core/Ecom.Domain/Ecom.Domain.csproj` contains domain entities, enums, constants, exceptions, events, models, and interfaces.
- `Core/Ecom.Application/Ecom.Application.csproj` contains CQRS features, MediatR behaviors, validators, DTOs, application services, and application-facing abstractions.
- `Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj` contains EF Core, Redis, IoT, messaging, security, seeding, telemetry, camera/media services, and integration implementations.
- `Presentation/Ecom.API/Ecom.API.csproj` uses `Microsoft.NET.Sdk.Worker` and hosts ASP.NET Core controllers, SignalR hubs, middleware, filters, Swagger/ReDoc, health checks, and runtime setup.

Current scan on 2026-06-19 found 1,510 C# files, 59 Application feature folders, and 41 V1 controller files. Use targeted search.
For first-pass source location, use `.agents/context/quick-scan.md` and the helper scripts under `.agents/scripts/`.

## Main Directories
- `Core/Ecom.Domain/Common`: base entity, domain event, audit, concurrency, and soft-delete primitives.
- `Core/Ecom.Domain/Entities`: domain entities, including IoT and AI entities.
- `Core/Ecom.Domain/Interfaces`: domain-facing contracts including repositories and services.
- `Core/Ecom.Domain/Constants`: permissions, error codes, message keys, and test account constants.
- `Core/Ecom.Application/Common`: MediatR behaviors, attributes, models, exceptions, interfaces, helpers, and shared application infrastructure.
- `Core/Ecom.Application/Features`: feature folders containing commands, queries, validators, models, services, and handlers.
- `Infrastructure/Ecom.Infrastructure/Persistence/Database`: EF Core DbContext, configurations, interceptors, UnitOfWork, repositories, migrations, and scripts.
- `Infrastructure/Ecom.Infrastructure/Caching`: distributed cache abstractions and Redis-backed services.
- `Infrastructure/Ecom.Infrastructure/IoT`: Event Hub consumer, connection manager, MQTT/background workers, telemetry handlers, and Oxy alert flow.
- `Infrastructure/Ecom.Infrastructure/Event`: MediatR notification handlers for FCM and report/export events.
- `Infrastructure/Ecom.Infrastructure/Messaging`: Azure Service Bus AI messaging publisher/consumer/timeout scheduler.
- `Infrastructure/Ecom.Infrastructure/Services/Camera`: MediaMTX, Dahua, camera registration/listener/snapshot services.
- `Infrastructure/Ecom.Infrastructure/Security`: JWT, authorization policy provider/handler, current user, and password hashing.
- `Infrastructure/Ecom.Infrastructure/Telemetry`: Serilog/OpenTelemetry setup.
- `Presentation/Ecom.API/Controllers`: `BaseController` and non-versioned controllers such as `AuthController`.
- `Presentation/Ecom.API/Controllers/V1`: versioned API controllers.
- `Presentation/Ecom.API/Hubs`: SignalR hubs.
- `Presentation/Ecom.API/Middleware`: security headers, structured logging, error handling, proxy authorization.
- `Presentation/Ecom.API/Filters`: MVC validation filters.

## Feature Discovery
Most business features live under:

```text
Core/Ecom.Application/Features/<FeatureName>/
Presentation/Ecom.API/Controllers/V1/<FeatureName>Controller.cs
Core/Ecom.Domain/Entities/<EntityName>.cs
Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/
```

Common feature folders include `Auth`, `Device`, `DeviceControl`, `DeviceHub`, `ExportReceipt`, `ImportReceipt`, `InventoryCheck`, `IssueReport`, `Notification`, `Plan`, `Pond`, `PondAlert`, `Scale`, `ScaleRecord`, `SensorDevice`, `Warehouse`, `WarningDevice`, `WarningDeviceHub`, `Zone`, and `Zone3DImage`.

Some existing filenames contain typos or trailing spaces. Search by symbols first, not only guessed filenames.

## Search Recipes
- Quick related-file scan: `powershell -ExecutionPolicy Bypass -File .agents\scripts\find-related-files.ps1 -Term "<term>"`.
- Module symbol summary: `powershell -ExecutionPolicy Bypass -File .agents\scripts\summarize-module.ps1 -Path "Core\Ecom.Application\Features\<Feature>"`.
- Find an endpoint: `rg "Route|HttpGet|HttpPost|HttpPut|HttpDelete|<route-term>" Presentation/Ecom.API/Controllers`.
- Find a command/query: `rg "<ActionOrFeature>" Core/Ecom.Application/Features`.
- Find validators: `rg "AbstractValidator|RuleFor" Core/Ecom.Application/Features/<Feature>`.
- Find transactions: `rg "EnableUnitOfWork" Core/Ecom.Application/Features/<Feature>`.
- Find persistence writes: `rg "SaveChangesAsync|InsertAsync|UpdateAsync|DeleteAsync|CommitTransactionAsync" Core/Ecom.Application/Features/<Feature>`.
- Find permission checks: `rg "Authorize\\(|Permissions\\." Presentation/Ecom.API Core/Ecom.Application/Features/<Feature>`.
- Find repositories and persistence: `rg "<EntityName>" Infrastructure/Ecom.Infrastructure/Persistence Core/Ecom.Application/Features`.
- Find domain rules: `rg "<EntityName>|Update\\(|Create\\(" Core/Ecom.Domain`.
- Find SignalR realtime code: `rg "Hub|Group|SignalR|ScaleSession_|Scale_|Device_|Cycle_|Pond_" Presentation Infrastructure Core`.
- Find IoT handlers: `rg "TelemetryHandler|IoT|EventHub|MQTT|Code 503|Oxy" Infrastructure/Ecom.Infrastructure`.
- Find broad exception mapping: `rg "catch \\(Exception" Core/Ecom.Application/Features/<Feature> Infrastructure`.

## Large / High-Friction Files
Open these only when relevant and prefer symbol search first:
- `Presentation/Ecom.API/Controllers/V1/PondController.cs`
- `Presentation/Ecom.API/Controllers/V1/ZoneController.cs`
- `Infrastructure/Ecom.Infrastructure/DependencyInjection.cs`
- `Infrastructure/Ecom.Infrastructure/Event/ReportExcel/Handler/ReportExcelEventHandler.cs`
- `Infrastructure/Ecom.Infrastructure/IoT/IoTConnectionManager.cs`
- `Core/Ecom.Domain/Constants/Permissions.cs`
- `Core/Ecom.Domain/Constants/ErrorCodes.cs`

## Paths To Avoid Unless Needed
- `bin/`
- `obj/`
- `.vs/`
- `.dotnet-home/`
- build outputs
- logs
- local configuration and secret files
- `Infrastructure/Ecom.Infrastructure/Migrations/*.Designer.cs`
- `Infrastructure/Ecom.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`

## Tests
No dedicated test project is currently visible in `Ecom.sln`. Use focused builds and any future test projects as verification.

