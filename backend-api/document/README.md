# Thanh Hoa Commerce — Documentation Source of Truth

> Snapshot đối chiếu: commit `56f0fa6634569f71107e9f6e1334ae41bba0388c`, ngày 2026-08-22. Source, tests, migration và cấu hình hiệu lực luôn thắng tài liệu khi có khác biệt.

Đây là cửa vào duy nhất cho AI Agent, Backend, Frontend, QA và BA. Bắt đầu tại [AI Agent Orchestrator](00-governance/AI-AGENT-ORCHESTRATOR.md); không đọc toàn bộ cây nếu task chỉ thuộc một domain.

## Bản đồ tài liệu

| Nhóm | Tài liệu |
| --- | --- |
| Điều phối | [AI Agent Orchestrator](00-governance/AI-AGENT-ORCHESTRATOR.md), [quy tắc cập nhật](00-governance/DOCUMENT-GOVERNANCE.md) |
| Dự án | [mục tiêu và phạm vi](01-project/PRODUCT-SCOPE.md), [thuật ngữ](01-project/GLOSSARY.md) |
| Kiến trúc | [system architecture](02-architecture/SYSTEM-ARCHITECTURE.md), [code organization](02-architecture/CODE-ORGANIZATION.md) |
| Dữ liệu | [entity dictionary](03-data/ENTITY-RELATIONSHIP-MODEL.md), [entity graphs theo bounded context](03-data/ENTITY-GRAPHS.md) |
| API/codegraph | [API catalog](04-api/API-CATALOG.md), [request/response examples](04-api/REQUEST-RESPONSE-EXAMPLES.md), [contract chung](04-api/API-CONVENTIONS.md), [request pipeline](06-codegraphs/REQUEST-PIPELINE.md), [E2E flows](06-codegraphs/COMMERCE-FLOWS.md), [implementation codegraphs](06-codegraphs/IMPLEMENTATION-CODEGRAPHS.md) |
| Domain | [Auth](domains/authentication.md), [Catalog](domains/catalog.md), [Cart & Checkout](domains/cart-checkout.md), [Order & Payment](domains/order-payment-shipment.md), [Management](domains/management.md), [Media](domains/media.md), [UI/UX](domains/ui-ux.md), [Future capabilities](domains/future-capabilities.md) |
| Trạng thái | [danh mục nghiệp vụ đã triển khai](07-implemented/IMPLEMENTED-BUSINESS-OPERATIONS.md), [implemented và roadmap](reference/source-status.md), [legacy boundary](reference/roadmap-and-legacy.md) |
| Phân tích/roadmap | [capability gap và roadmap ưu tiên](08-analysis/COMMERCE-CAPABILITY-GAP-AND-PRIORITY-REPORT.md) |
| Kế hoạch thực thi | [P0 production, audit, outbox và reconciliation](09-plans/P0-PRODUCTION-AUDIT-OUTBOX-RECONCILIATION-PLAN.md) |

## Quy tắc tối quan trọng

- `ProductVariant`, không phải Product/SKU giả, là định danh mua hàng; giá và tồn do server quyết định.
- Tạo Order cần quote fingerprint và idempotency key. Payment redirect/QR không phải bằng chứng Paid.
- Các mutation được bảo vệ antiforgery (Cart/Checkout/Order/Address, Management, Password V2 change/setup và media retry) dùng CSRF; Catalog hiện dựa trên Bearer + policy + concurrency. Provider webhook/IPN dùng cơ chế xác thực riêng.
- Entity có trong DbContext không đồng nghĩa API đã tồn tại. Xem [source status](reference/source-status.md).
- Build pass không chứng minh PostgreSQL, Redis, SePay, Blob/scanner, browser/BFF hoặc production.
