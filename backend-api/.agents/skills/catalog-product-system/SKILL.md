---
name: catalog-product-system
description: Analyze, review, plan, implement, or debug the Ecom Catalog Product slice Product, Category, ProductCategory, ProductMedia, ProductVariant, VariantPrice, public storefront APIs, backoffice Product lifecycle, effective pricing, permissions, CQRS handlers, PostgreSQL constraints, and delivery progress. Use when a request names Catalog, Product, Variant, Price, product APIs, publish flow, product media, category mapping, or needs a source-backed progress assessment without scanning the whole repository.
---

# Catalog Product System

## Goal

Establish current Catalog Product behavior from a small source-backed surface. Keep storefront and staff backoffice separate. Do not widen into Cart, inventory, promotion, producer self-service, traceability, or media scanning unless the user explicitly opens that boundary.

## Start

1. Read `AGENTS.md` and `.agents/context/task-router.md`.
2. Read `references/current-state.md`, then verify only symbols relevant to the request.
3. Select one focused reference:
   - Domain relationships, invariants, lifecycle: `references/domain-map.md`.
   - Routes, DTOs, handlers, services, permissions: `references/api-handler-map.md`.
   - Review, triage, tests, delivery gate: `references/review-checklist.md`.
4. Treat live source, migration history, test output, and API responses as authoritative. References navigate; they do not prove runtime behavior.

## Fast source boundary

Start with these paths; do not scan unrelated Commerce aggregates.

```text
Core/Ecom.Domain/Entities/Commerce/Catalog/
Core/Ecom.Domain/Entities/Commerce/Pricing/VariantPrice.cs
Core/Ecom.Application/Features/Catalog/
Core/Ecom.Application/Common/{Interfaces,Services}/
Presentation/Ecom.API/Controllers/V1/{Products,Categories,CatalogProducts,CatalogCategories,CatalogProductOptions}Controller.cs
Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce/{Catalog,Pricing}/
Infrastructure/Ecom.Infrastructure/Migrations/20260719110000_AddVariantPriceOverlapConstraint.cs
Infrastructure/Ecom.Infrastructure/Migrations/20260719114222_AddProductMediaPrimaryConstraint.cs
Tests/Ecom.Domain.Tests/Commerce/ProductAndCartTests.cs
```

## Working rules

- Classify as public read, backoffice command, domain invariant, persistence/migration, or progress review before reading code.
- Trace only `Controller -> command/query -> validator -> handler/service -> aggregate -> configuration/test`.
- Keep public endpoints anonymous and limited to public facts. Never expose management DTOs, raw price history, private media information, or permissions through storefront endpoints.
- Keep controllers thin, commands transactional through `ITransactionalRequest`, handled failures in `TResult`, and state changes in Domain methods. The transaction behavior is the single commit point; Catalog handlers never call `SaveChangesAsync` or explicit transaction APIs.
- For new/refactored Catalog use cases, use one request, validator where applicable, and handler per folder. Existing grouped legacy handlers are migration targets, not templates.
- Require `ConcurrencyStamp` on every Product mutation. On `409`, refetch management detail; never blindly replay.
- Use `IEffectivePriceResolver` for public list, detail, and publish eligibility. Do not duplicate precedence in handlers.
- Mutate Product categories/media through Product methods and persisted child collections. Do not add a navigation graph only for a handler.
- Do not claim constraints or concurrency are verified without real PostgreSQL evidence. Never use EF InMemory for those claims.

## Review output

Report outcome, exact evidence, contract alignment, P1/P2/P3 findings, and the smallest safe next change with its approval gate.

## Change protocol

- Public routes/DTOs, Catalog policies, EF configurations/migrations, and dependencies require explicit approval.
- Preserve migration history; add forward migrations only after idempotent SQL review and PostgreSQL evidence.
- Update `references/current-state.md` only after durable source and command evidence. Do not mark a gate complete merely because code exists.
