# Commerce System Overview

This is the human and agent entrypoint for the Thanh Hoa commerce backend.

Agents must start with `.agents/skills/commerce-system/SKILL.md`, which routes to current status, aggregate/entity structure, relational database design, roadmap, and update protocol.

## Current Snapshot

- Persistence has 79 commerce entities/configurations and an 80-table DBML including User.
- Product, Variant, Price, Inventory, Cart, Order, Payment, Shipment, Trust, CMS, B2B, and audit concerns are separated.
- Rich Domain Model Batch 1 exists in source and awaits Domain build/test verification.
- Catalog, Cart, Checkout, CreateOrder, idempotency, stock concurrency, migration upgrade, and PostgreSQL integration tests remain open.
- Never apply a commerce migration before its build, tests, SQL review, and staging gate.

## Canonical Artifacts

- Business specification: `document/Bo-tai-lieu-dac-ta-nghiep-vu-va-kien-truc-du-an-Thuong-mai-dien-tu-Thanh-Hoa.docx`
- Database report: `document/DATABASE-DESIGN-REPORT.md`
- Product/Variant/Price/Inventory decisions: `document/PRODUCT-VARIANT-PRICE-INVENTORY-DECISION-LOG.md`
- Detailed ERD: `document/DETAILED-ERD.dbml`
- Agent entrypoint: `.agents/skills/commerce-system/SKILL.md`

Live source and verification evidence override this overview.
