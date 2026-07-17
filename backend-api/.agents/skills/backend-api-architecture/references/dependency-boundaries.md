# Dependency Boundaries

## Allowed
- `Presentation/Ecom.API` may reference Application and Infrastructure through project references and DI setup.
- `Infrastructure/Ecom.Infrastructure` may reference Application and Domain abstractions.
- `Core/Ecom.Application` may reference Domain.
- `Core/Ecom.Application` currently references EF Core abstractions and ASP.NET Core file abstractions where existing features require them.
- `Core/Ecom.Domain` should stay framework-light and independent.

## Avoid
- Domain referencing EF Core, ASP.NET Core, Redis, Azure SDKs, Firebase, SignalR, or logging providers.
- Application handlers directly constructing infrastructure clients.
- Controllers using DbContext, Redis, or external clients directly.
- Infrastructure-specific DTOs leaking into public API contracts.
- New Infrastructure dependencies in Domain.
- New direct Redis/Azure/Firebase/SignalR implementation dependencies in controllers.

## Escalate Before
- Changing project references.
- Moving classes across layers.
- Replacing repository/UnitOfWork patterns.
- Changing public contracts to fix internal layering.
- Removing Application EF Core references as a broad "cleanup".

