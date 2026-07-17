# Tech Stack

## Runtime And Language
- .NET 10
- C#
- Nullable reference types enabled
- Implicit usings enabled
- Microsoft .NET analyzers referenced in all projects

## API
- ASP.NET Core hosted from a `Microsoft.NET.Sdk.Worker` project.
- API Versioning with URL segment versions.
- Swagger/ReDoc via Swashbuckle.
- SignalR with optional Redis backplane.
- Health checks: `/healthz`, `/livez`, `/readyz`.
- Prometheus scraping endpoint.
- Request timeouts.
- Forwarded headers for reverse proxy deployments.
- Custom JSON converters for enum strings and UTC DateTime handling.

## Application
- MediatR.
- FluentValidation.
- Custom pipeline behaviors for current user, validation, authorization, logging, performance, unhandled exceptions, and UnitOfWork.
- `TResult<T>` / `TResult` for handled application results.
- `MessageKey` and `ErrorCodes` constants for user-facing errors.

## Persistence
- EF Core 10.
- Npgsql / PostgreSQL.
- Generic UnitOfWork and repository pattern.
- Direct `IApplicationDbContext` in some complex features.
- Soft delete, audit, concurrency stamp, and domain event interceptors.
- EF migrations live under Infrastructure and are high-risk generated artifacts.

## Cache And Realtime
- Redis through `Microsoft.Extensions.Caching.StackExchangeRedis`.
- In-memory cache/lock fallback when Redis is not configured.
- SignalR for realtime client delivery.
- Typed SignalR client interface in Application.

## Integrations
- Azure Event Hubs.
- Azure Service Bus.
- MQTTnet.
- Firebase Admin SDK / FCM.
- Azure Blob Storage.
- MediaMTX and Dahua camera services.
- AI API client and AI Service Bus messaging.

## Observability
- Serilog.
- OpenTelemetry.
- Prometheus exporter.
- Health checks.
- Structured logging middleware.

## Security
- JWT Bearer authentication.
- Permission-based authorization.
- Dynamic permission policy provider and handler.
- Current-user abstraction based on JWT claims.
- BCrypt password hashing package exists, though OTP flows are prominent.
- SignalR tokens can arrive through `access_token` query string for `/hubs`.

## Tooling Notes
- Use `rg` for search.
- Use `dotnet build <project> --no-restore` for focused verification when dependencies are already restored.
- No dedicated test project is currently present; build verification is the default safety check.
- Avoid modifying local config files that may contain real development secrets.
