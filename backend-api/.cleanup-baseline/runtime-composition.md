# Runtime composition inventory

## Entry points

- API host: `Presentation/Ecom.API/Program.cs`
- Application registration: `Core/Ecom.Application/DependencyInjection.cs`
- Infrastructure registration: `Infrastructure/Ecom.Infrastructure/DependencyInjection.cs`

## Current runtime coupling

The host currently registers or starts Ecom-specific seeding, SignalR telemetry, IoT/Event Hub/MQTT, Firebase notifications, camera/media, AI Service Bus, scale/Oxy flows, and background workers.

`Infrastructure` currently has 14 `AddHostedService` registrations. These registrations must be removed or replaced before their associated folders are deleted.

The baseline API starter should retain only controllers, error/security/logging middleware, Swagger, health checks, PostgreSQL persistence, and common CQRS registrations.

