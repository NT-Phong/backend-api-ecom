# Project Map

## Solution

- `Core/Ecom.Domain`: entities, aggregates, invariants, enums, events, contracts.
- `Core/Ecom.Application`: CQRS, validators, DTOs, services, abstractions, pipeline behaviors.
- `Infrastructure/Ecom.Infrastructure`: EF Core/PostgreSQL, UnitOfWork, security, cache, storage, messaging, external adapters.
- `Presentation/Ecom.API`: versioned controllers, middleware, filters, OpenAPI/health host.
- `Tests/Ecom.Domain.Tests`: Commerce domain invariant/state tests.

## Commerce Boundaries

- Entities: `Core/Ecom.Domain/Entities/Commerce`.
- Enums: `Core/Ecom.Domain/Enums/Commerce`.
- Configurations: `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce`.
- Context/migrations: `Infrastructure/Ecom.Infrastructure/Persistence/Database` and `Migrations`.
- Future CQRS: `Core/Ecom.Application/Features/<CommerceFeature>`.
- Future API: `Presentation/Ecom.API/Controllers/V1`.
- ERD/documents: `document/DETAILED-ERD.dbml` and Commerce overview/reports.

## Target Vertical Flow

```text
GET Product catalog/detail
-> Cart user/guest
-> Checkout preview
-> CreateOrder transaction
-> Inventory reservation + Payment
-> Order/Shipment lifecycle
```

Legacy aquaculture/IoT source remains in the solution but is outside the default roadmap.

## Search

```powershell
rg -n "Product|Cart|Order|Payment|Shipment|TradeInquiry" Core/Ecom.Domain/Entities/Commerce
rg -n "HasOne|HasIndex|HasCheckConstraint" Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce
rg -n "IRequestHandler|AbstractValidator" Core/Ecom.Application/Features
rg -n "HttpGet|HttpPost|HttpPatch|HttpDelete" Presentation/Ecom.API/Controllers/V1
```
