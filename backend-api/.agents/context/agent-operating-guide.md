# Agent Operating Guide

## Default Mental Model

Build a reliable Commerce API around Product/Variant/Pricing/Inventory, Cart/Checkout/Order, Payment/Shipment, Producer/Trust, CMS/Engagement, and B2B inquiries.

## Handling

- Start with the requested commerce entity, route, use case, constraint, or roadmap batch.
- Search live source; inspect a nearby API/CQRS pattern only for reusable mechanics.
- Keep scope within the current batch and stop at its quality gate.
- Preserve public/schema/security contracts unless explicitly approved.
- Report verified facts separately from intended or unverified source.

Legacy modules are reference patterns only when technically useful; never let their business vocabulary shape new Commerce contracts.
