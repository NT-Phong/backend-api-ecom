# AI Agent Orchestrator

## Mục đích

Đây là hướng dẫn dành cho AI Agent **không có quyền đọc source code**. Sau khi đọc entrypoint và các file domain liên quan, Agent phải hiểu được dự án làm gì, thuật ngữ, actor, dữ liệu, API, state machine, UI behavior và phần nào đã/chưa triển khai. Source path chỉ là provenance tùy chọn cho Agent có quyền sửa code; không phải điều kiện để hiểu nghiệp vụ.

## Trình tự bắt buộc

1. Đọc [mục tiêu dự án](../01-project/PRODUCT-SCOPE.md), [thuật ngữ](../01-project/GLOSSARY.md) và [trạng thái chức năng](../reference/source-status.md).
2. Phân loại task: Auth, Catalog, Cart/Checkout, Order/Payment/Shipment, Management, Media, UI/UX hay kiến trúc xuyên suốt.
3. Đọc file domain tương ứng. File domain là bản mô tả nghiệp vụ và contract độc lập; không bắt buộc mở source.
4. Nếu thiết kế xuyên domain, đọc thêm [mô hình dữ liệu](../03-data/ENTITY-RELATIONSHIP-MODEL.md), [entity graphs](../03-data/ENTITY-GRAPHS.md), [API catalog](../04-api/API-CATALOG.md) và [implementation codegraphs](../06-codegraphs/IMPLEMENTATION-CODEGRAPHS.md).
5. Trước khi khẳng định một chức năng đang có, kiểm [danh mục nghiệp vụ đã triển khai](../07-implemented/IMPLEMENTED-BUSINESS-OPERATIONS.md).
6. Phân biệt rõ `IMPLEMENTED`, `PARTIAL`, `ROADMAP` và `LEGACY`. Không biến entity hoặc ý tưởng roadmap thành API đã có.
7. Chỉ khi được giao sửa code mới cần đối chiếu source, test và current diff để phát hiện documentation drift.

Khi nhiệm vụ là đề xuất sản phẩm hoặc chọn nghiệp vụ tiếp theo, đọc [báo cáo capability gap và roadmap ưu tiên](../08-analysis/COMMERCE-CAPABILITY-GAP-AND-PRIORITY-REPORT.md). Mọi endpoint trong báo cáo có nhãn `PROPOSED` là thiết kế tương lai, không phải contract live.

Khi triển khai P0 production/audit/outbox/reconciliation, dùng [execution plan P0](../09-plans/P0-PRODUCTION-AUDIT-OUTBOX-RECONCILIATION-PLAN.md). Tài liệu này là plan, không chứng minh các batch đã hoàn thành.

## Routing nhanh

| Signal | Đọc tiếp | Tài liệu chứa contract |
| --- | --- | --- |
| JWT, OTP, password, session, role | `domains/authentication.md` | endpoint, token/session, error và security rule |
| Product, category, variant, price | `domains/catalog.md` | lifecycle, payload, DTO và publish rule |
| Cart, address, preview, create order | `domains/cart-checkout.md` | guest/user ownership, quote và idempotency |
| Payment, SePay, shipment, refund | `domains/order-payment-shipment.md` | state machine, provider callback và fulfillment |
| Producer, inventory, dashboard, settings | `domains/management.md` | backoffice operation và permission |
| Upload, scan, product image | `domains/media.md` | upload/scan/attach lifecycle |
| FE screen, state, retry | `domains/ui-ux.md` | screen map, states và error UX |
| Entity/FK/index/migration | `03-data/ENTITY-RELATIONSHIP-MODEL.md` | entity dictionary và relationship |
| Codegraph, dependency, luồng đã chạy | `06-codegraphs/IMPLEMENTATION-CODEGRAPHS.md` | API → handler → domain → persistence và error path |
| Chức năng đã có/chưa có | `07-implemented/IMPLEMENTED-BUSINESS-OPERATIONS.md` | actor, precondition, state effect và implementation status |
| Nghiệp vụ nên phát triển tiếp | `08-analysis/COMMERCE-CAPABILITY-GAP-AND-PRIORITY-REPORT.md` | gap database/API, priority, model/API/handler/service đề xuất |
| P0 production/audit/outbox/reconciliation | `09-plans/P0-PRODUCTION-AUDIT-OUTBOX-RECONCILIATION-PLAN.md` | batch, dependency, test, rollout và approval gate |

## Guardrails

- Không tin price, stock, discount, totals, payment result do client gửi.
- Không sửa public API, auth/policy, migration, dependency, runtime config hoặc secret nếu chưa có approval rõ ràng.
- Mutation mới dùng `ITransactionalRequest`; handler không tự `SaveChangesAsync` hoặc điều khiển transaction.
- Không gọi external service khi DB transaction còn mở; dùng post-commit/outbox khi cần bền vững.
- Không log JWT, refresh/guest token, CSRF, payment credential, PII hoặc raw upload.

## Đầu ra chuẩn của Agent

Khi phân tích hoặc lên ý tưởng, Agent phải nêu: capability liên quan; actor; điều kiện bắt đầu; happy path; alternate/error path; state/data bị tác động; API hiện có; phần còn thiếu; ảnh hưởng UI/UX; security/concurrency; tiêu chí nghiệm thu. Không yêu cầu người dùng tự đọc source để hoàn thiện câu trả lời.
