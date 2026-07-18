---
name: commerce-system
description: Analyze, plan, implement, review, or debug the Ecom Thanh Hoa Commerce domain, including Product, ProductVariant, pricing, inventory, Cart, checkout, Order, Payment, Shipment, TradeInquiry, EF Core configurations, DBML ERD, migrations, relational constraints, implementation progress, and the Commerce 9/10 roadmap.
---

# Commerce System

## Start

Read `AGENTS.md` and `.agents/context/task-router.md`. Select only the reference matching the current aggregate, schema object, or roadmap gate.

## Reference Routing

- Current completion, evidence, and next gate: `references/current-status.md`.
- Aggregate ownership, invariants, and state machines: `references/entity-model.md`.
- Tables, relationships, constraints, indexes, migrations, or ERD: `references/relational-database.md`.
- Batch order and acceptance gates: `references/implementation-roadmap.md`.
- Durable status update rules: `references/update-protocol.md`, only after verified change.

For public API, auth, EF/migration, or protected configuration changes, also load `.agents/context/risk-map.md` and the matching backend architecture reference.

## Workflow

1. Identify one aggregate, route, schema object, or roadmap batch.
2. Search live entities, configurations, handlers/controllers, and tests.
3. Treat source, current diff, and command output as authoritative.
4. Preserve one Commerce entity and one EF configuration per file.
5. Keep cross-aggregate references ID-based and mutate state through aggregate methods.
6. Run the narrowest safe build/test when permitted; do not claim an unmet gate.
7. Update references only when evidence proves a durable fact, decision, risk, or completed gate.

## Critical Rules

- Never apply a Commerce migration without explicit approval, clean builds/tests, reviewed SQL, and staging evidence.
- Never rewrite a shared/applied migration for enum changes; use explicit data migration.
- Never trust client price, stock, discount, payment result, or totals.
- Never send external side effects before commit.
- Never use EF InMemory for relational or concurrency claims.

## Fast Searches

```powershell
rg --files Core/Ecom.Domain/Entities/Commerce
rg -n "IAggregateRoot|static .* Create|CommerceDomainException|AddDomainEvent" Core/Ecom.Domain/Entities/Commerce
rg -n "HasOne|HasIndex|HasCheckConstraint|UniqueWhere|OnDelete" Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce
rg -n "Product|Cart|Checkout|CreateOrder|TradeInquiry" Core/Ecom.Application/Features Presentation/Ecom.API/Controllers
rg -n "Fact|Theory" Tests/Ecom.Domain.Tests Tests/Ecom.IntegrationTests
```
