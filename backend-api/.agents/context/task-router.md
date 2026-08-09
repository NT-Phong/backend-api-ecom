# Task Router

## Bootstrap

Load only:

1. `AGENTS.md`.
2. This router.

Then choose one primary skill and only the references required by the current boundary. Reuse already-loaded bootstrap context during the same task.

## Routes

| Signal | Primary skill/context | Conditional reference |
| --- | --- | --- |
| SePay, Hosted Checkout, payment form/signature, IPN/webhook, X-Secret-Key, PaymentMethod.SePay, payment reconciliation, SePay Sandbox/Production | `sepay-payment-operations` | Its source map; production gates for schema/config/runtime; load checkout skill only if Cart/Order/stock scope expands |
| Catalog Product, ProductCategory, ProductMedia, ProductVariant, VariantPrice, public Product API, Catalog backoffice lifecycle | `catalog-product-system` | Current state for progress; domain map for invariants; API-handler map for contracts; review checklist for improvement |
| Product, Variant, Price, Inventory, Cart, Checkout, Order, Payment, Shipment, TradeInquiry | `commerce-system` | Current status for progress; entity model for invariants; relational database for EF/schema; roadmap for sequencing |
| Controller, MediatR, validation, authorization, UnitOfWork, errors, performance | `backend-api-architecture` | API checklist or dependency/security reference matching the question |
| Auth, JWT, password/session, roles, permissions, identity, secrets | `authentication` | `references/current-state.md`, `references/role-authorization.md`, `.agents/context/risk-map.md` |
| EF migration, constraint, index, concurrency | `commerce-system` | Relational database, risk map, and commands |
| Plan, specification, or handoff | Matching primary skill | `.agents/rules/spec-orchestrator.md` and task contract |
| Independent code review | Matching primary skill | `.agents/templates/code-review.md` |
| Agent-system maintenance | `backend-api-architecture` | `.agents/context/model-routing.md` and verifier scripts |

## Loading Rules

- Search the exact route, symbol, entity, error, or test before broad reading.
- Load at most one primary skill before source discovery; add the second only for a real cross-boundary dependency.
- Load risk guidance only for protected or unclear-risk work.
- Load broad architecture analysis only for repository-wide reviews.
- Source, tests, current diff, and command output override guidance.
- Do not mark roadmap progress from intention or unverified source changes.

## Legacy Boundary

Aquaculture, Device, IoT, Pond, Scale, Camera, Oxy, and farm-warehouse modules remain legacy source. Do not extend them unless the user explicitly requests legacy maintenance.
