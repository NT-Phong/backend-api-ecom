# Codebase Analysis

Last aligned: 2026-07-18

## Current Direction

The repository is transitioning from an aquaculture-oriented backend to the Thanh Hoa Commerce platform. Legacy modules remain compiled source, while new development defaults to Commerce.

## Current Commerce State

- Broad normalized persistence model: 79 entities/configurations and detailed ERD.
- Product/Variant/Price/Inventory boundaries and transaction snapshots are structurally strong.
- Rich Domain Model Batch 1 exists in source and awaits build/test verification.
- Catalog, Cart, Checkout, CreateOrder, lifecycle APIs, idempotency, concurrency, migration upgrade, and PostgreSQL integration tests remain roadmap work.
- Use `.agents/skills/commerce-system/references/current-status.md` for the live status.

## Reusable Platform Strengths

- Clean Architecture/CQRS/MediatR pipeline.
- FluentValidation, TResult/ApiResponse/error catalogs.
- UnitOfWork and EF PostgreSQL configuration patterns.
- JWT/policy infrastructure and centralized middleware.

## Debt to Avoid Carrying Forward

- Large controllers/handlers, broad generic catches, mutation outside aggregates.
- Build-only verification without domain/PostgreSQL tests.
- Generic indexes unrelated to access paths.
- Premature multi-vendor, multi-channel, gateway, or legacy IoT coupling.
