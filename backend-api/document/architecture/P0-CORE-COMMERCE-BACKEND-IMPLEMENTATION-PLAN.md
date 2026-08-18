# Kế hoạch triển khai P0 — Core Commerce Backoffice API

> **Scope:** Source_Ecom `backend-api`, source snapshot 2026-08-17.
> **Đầu vào FE:** backlog P0 Producer & Farm, Management Orders, Inventory & Stock, Settings & Security Audit.
> **Mục tiêu phase:** Cho phép một staff được phân quyền vận hành luồng **Producer hợp lệ → Catalog Product → Order queue/detail → stock adjustment/ledger → audit/security**. Không mở P1 Promotion/CMS/CRM hoặc P2 B2B/Analytics trong phase này.
> **Không phải bằng chứng runtime:** Đây là kế hoạch source-backed. Không migration, endpoint hoặc deployment nào được thực hiện bởi tài liệu này.
> **Contract FE đã có:** Xem [P0 Core Commerce FE API Guide](P0-CORE-COMMERCE-FE-API-GUIDE.md). Guide này chỉ map endpoint hiện đang có và đánh dấu rõ các capability roadmap chưa live.

## 1. Đánh giá yêu cầu FE và baseline hiện tại

### 1.1 Kết luận

Yêu cầu P0 của FE là đúng về hướng, nhưng không thể triển khai bằng cách chỉ thêm controller CRUD trên các `Tbl_*` hiện có:

| FE yêu cầu | Source hiện tại | Đánh giá và quyết định phase P0 |
| --- | --- | --- |
| Producer/Farm CRUD + verify/facility | Chỉ có `GET /catalog/producers` và `GET /catalog/producers/{id}` picker; entity Producer/Contact/Facility chủ yếu persistence-only, private setter, không có factory/mutation. | Làm Producer workspace có domain methods, CQRS, management read model và lifecycle; không expose entity trực tiếp. |
| Order management list/detail/note/timeline | Chỉ có customer-owned `GET /orders`, `GET /orders/{id}`; management chỉ có action transition/payment/shipment. | Bổ sung management read model và internal note. Giữ action hiện có để không phá public contract. |
| Inventory level/location/adjustment/movement | Entity/configuration và logic aggregate có sẵn; Application chỉ có `ExpireReservationsCommand`, không controller. | Làm ledger/read model + safe adjustment. Không tạo transfer/receipt/purchase module vì chưa có aggregate chứng từ. |
| Settings/audit/sessions | `SystemSetting`, `AuditLog`, `UserSession`, `SecurityEvent` có table; không có management API. `AuditableEntityInterceptor` chỉ set audit fields, **không ghi `AuditLog`**. | Làm settings typed/allow-listed, read-only audit/session/event query và staff session revocation có lý do. |

### 1.2 Các yêu cầu FE phải sửa trước khi chốt contract

1. **Không dùng Producer status `Suspended`.** Enum hiện có là `Draft`, `Verified`, `Published`, `Hidden`; eligibility Catalog đang là `PublicStatus=Published && IsVerified=true`. P0 phải trả hai facts `publicStatus` và `isVerified`, không thêm enum mới nếu chưa có quyết định/migration.
2. **Không gọi `code` là Producer ID.** API relation dùng UUID `Producer.Id`; code chỉ là business key unique/display/search.
3. **Order timeline không được chỉ dựa `AuditLog`.** Table AuditLog hiện không được interceptor populate. P0 detail/timeline phải dựng từ `OrderStatusHistory`, `PaymentTransaction`, `ShipmentHistory`, `OrderNote`; AuditLog là stream bổ sung sau khi có explicit audit writer.
4. **`onHand` FE phải map `stockedQuantity`.** API P0 trả cả `stockedQuantity`, `reservedQuantity`, `incomingQuantity`, `availableQuantity = stocked - reserved`; không gửi một number “stock” mơ hồ.
5. **Không cho raw JSON settings hoặc secret.** `SystemSetting.Value` là JSONB nhưng UI phải dùng DTO typed theo group. JWT/database/payment secret, OTP bypass, storage credential vẫn chỉ thuộc deployment secret store/appsettings.
6. **Không thêm API transfer trong P0.** Có movement type nhưng chưa có Transfer aggregate/chứng từ đối ứng/two-location atomic model. Chỉ adjustment ledger an toàn; Receive/Transfer/Return là phase tiếp theo có use case riêng.

### 1.3 Các guard source hiện có phải bảo toàn

- `Order`, `Payment`, `Shipment`, `InventoryLevel` có lifecycle và transaction history; không update status/quantity bằng generic `PUT`.
- Cancel/refund/start shipment hiện lock Order/Payment/InventoryLevel; P0 không thay lock bằng client `concurrencyStamp`.
- `InventoryMovement` là append-only; không có update/delete API.
- `InventoryLevel` bị DB check: quantities không âm và `ReservedQuantity <= StockedQuantity`.
- Product publish chỉ chấp nhận Producer `Published + IsVerified`; Producer P0 phải giữ invariant đó.
- `BaseEntity` có soft-delete/audit/stamp; lịch sử giao dịch (`Order*History`, payment transaction, movement, audit) không application-delete.

## 2. Kiến trúc P0 và authorization foundation

### 2.1 Bounded-context ownership

```text
Producer workspace
  Producer -> ProducerContact / ProductionFacility
  └─ eligibility read model cho Catalog picker

Order operations
  Order -> OrderItem snapshot / OrderStatusHistory / OrderNote
  ├─ Payment -> PaymentTransaction
  └─ Shipment -> ShipmentHistory

Inventory operations
  ProductVariant -> InventoryItem -> InventoryLevel
  ├─ InventoryReservation (read only P0)
  └─ InventoryMovement (append-only)

System operations
  SystemSetting (typed group JSON) / AuditLog / UserSession / SecurityEvent
```

Không tạo navigation graph lớn. Query dùng `QueryNoTracking()` và projection DTO; mutation dùng `ITransactionalRequest`, UnitOfWork là commit point duy nhất. Mỗi use case có request, validator, handler riêng; controller chỉ bind route/body rồi `Mediator.Send`.

### 2.2 Permission batch — bắt buộc trước P0 API

`Permissions.cs`, policy seeding và auth là protected surface: cần approval trước implementation. Bổ sung code policy tối thiểu:

| Module | Permission mới | Dùng cho |
| --- | --- | --- |
| Producer | `producers.read`, `producers.create`, `producers.update`, `producers.publish`, `producers.verify` | Workspace, lifecycle, verify. |
| Inventory | `inventory.read`, `inventory.adjust`, `inventory.locations.manage` | Read model, quantity mutation, location lifecycle. |
| System | `settings.read`, `settings.update`, `audit.read`, `security.sessions.read`, `security.sessions.revoke`, `security.events.read` | Settings/audit/session/event admin. |
| Existing | `orders.read`, `orders.manage`, `payments.verify/refund`, `shipments.manage` | Reuse cho Order queue/detail và action hiện hữu. |

**Bắt buộc sửa RoleSeeder trong cùng batch:** hiện seed loop qua mọi Role và soft-delete policy không được grant; điều này có thể xoá policy của custom role ở mỗi startup. Chỉ seed/synchronize grant mặc định cho các system role được định nghĩa (`SYSTEM_ADMIN`, `ADMIN`, `MANAGER`, `USER`); không mutate policy assignment của custom role. Manager mặc định chỉ nhận Producer/Inventory/Order operational permissions đã được product owner phê duyệt; Settings, Audit và session revoke chỉ SystemAdmin/Admin.

Mỗi endpoint vẫn phải có cả `[Authorize(Policy=...)]` và handler check `ICurrentUser.HasPolicy(...)`. FE chỉ dùng policy claim để ẩn CTA, không phải authority.

### 2.3 Explicit audit writer

P0 tạo `IAuditWriter` (Application abstraction) và Infrastructure implementation ghi `AuditLog` trong **cùng UnitOfWork transaction** của mutation:

```text
command validates + authorizes
  -> aggregate/domain mutation
  -> persist aggregate/history
  -> auditWriter.Record(actor, action, entity, id, allowed before/after, correlation, IP)
  -> UnitOfWork commit
```

- `AuditLog` cần factory/domain method thay vì public property mutation.
- Payload chỉ là allow-listed JSON; mask phone/email/address, không ghi token, secret, media storage key, raw payment proof hay raw setting secret.
- Nếu audit insert fail thì mutation P0 rollback; không publish side effect trước commit.
- P0 audit action names: `ProducerCreated`, `ProducerUpdated`, `ProducerVerified`, `ProducerPublished`, `FacilityCreated`, `OrderNoteAdded`, `InventoryAdjusted`, `StockLocationChanged`, `SettingUpdated`, `StaffSessionRevoked`.

## 3. API contract và implementation slice chi tiết

### 3.1 Producer & Farm Workspace

#### Endpoint contract

| Method / route | Request/query | Response | Policy |
| --- | --- | --- | --- |
| `GET /api/v1/management/producers` | `q`, `publicStatus`, `isVerified`, `administrativeAreaId`, `page`, `pageSize`, sort allow-list | paged `ProducerListItemDto` | `producers.read` |
| `GET /api/v1/management/producers/{id}` | UUID path | root, contacts, facilities, linked Product summary/count | `producers.read` |
| `POST /api/v1/management/producers` | code/name/legalName/description/websiteUrl | `ProducerManagementResult` | `producers.create` |
| `PUT /api/v1/management/producers/{id}` | editable root + `concurrencyStamp` | renewed root DTO/stamp | `producers.update` |
| `POST /{id}/verify` | `{ concurrencyStamp }` | status + `verifiedAt`, `verifiedByUserId`, stamp | `producers.verify` |
| `POST /{id}/publish` | `{ concurrencyStamp }` | status/stamp | `producers.publish` |
| `POST /{id}/hide` | `{ concurrencyStamp, reason }` | status/stamp | `producers.publish` |
| `POST /{id}/contacts` / `PUT` / soft deactivate | contact request + stamp | contact DTO + root stamp | `producers.update` |
| `POST /{id}/facilities` / `PUT` / hide | facility request + stamp | facility DTO + root stamp | `producers.update` |

Không đưa `PointOfSale`/`PointOfSaleProduct` vào P0: chưa có FE acceptance rule, facility workflow đã đủ để Product sourcing. Để tránh duplication, giữ `GET /api/v1/catalog/producers` là picker chỉ Published+Verified; nó dùng management data nhưng không trả contact private/facility detail.

#### Domain/service/handler design

1. Thêm factory và mutation methods vào `Producer`, `ProducerContact`, `ProductionFacility`; state transition nằm Domain, không set property trong handler.
2. `Producer.Create` bắt buộc Code/Name; `UpdateDetails` không đổi verified fact; `Verify` set actor/time; `Publish` yêu cầu IsVerified; `Hide` không hard delete. `Unverify` không thuộc P0 vì cần compliance revocation policy.
3. Contact/facility là child use case scoped by `ProducerId`: handler luôn load parent, check root stamp, check child belongs to parent, mutate, renew parent stamp và ghi audit.
4. Queries project join đúng nhu cầu; list không N+1 facility/contact. Detail mới trả contact private cho staff có quyền.

#### Validation/chặn

- `code`: required, trim, max 50, unique active (DB partial unique là authority); `name` max 300; legal name max 300; website URL valid/max 500.
- Contact: `ContactType` enum allow-list; value required/max 500; Email phải `EmailAddress`, Phone chỉ permitted normalized format; displayOrder `>=0`.
- Facility: name required/max 300; address max 500; `latitude [-90,90]`, `longitude [-180,180]`; AdministrativeArea phải tồn tại nếu supplied.
- Verify/Publish/Hide đều require non-empty stamp; hide reason required/max 500. Published Producer không bị hard delete.
- `409`: stale stamp/code unique; `404`: producer/child/area missing; `422`: invalid lifecycle (publish before verify, hide khi product Published depends — P0 phải chọn rule fail-closed và trả impact).

### 3.2 Management Orders Backoffice

#### Endpoint contract

| Method / route | Request/query | Response | Policy |
| --- | --- | --- | --- |
| `GET /api/v1/management/orders` | `q` (order number/phone), `orderStatus`, `paymentMethod`, `paymentStatus`, `shipmentStatus`, `carrierName`, `placedFrom`, `placedTo`, `page`, `pageSize`, sort allow-list | paged `ManagementOrderListItemDto` | `orders.read` |
| `GET /api/v1/management/orders/{id}` | UUID | immutable item snapshots, recipient snapshot (masked by permission policy), payment, shipment, allowed actions, timeline summary | `orders.read` |
| `POST /api/v1/management/orders/{id}/notes` | `{ content }` | created internal note DTO | `orders.manage` |
| `GET /api/v1/management/orders/{id}/timeline` | optional cursor/type | paged normalized timeline | `orders.read` |

Các action **đã có** được giữ route/contract: confirm, cancel, verify-bank-transfer, refund, prepare/start/complete/delivery-failed. P0 detail trả `allowedActions` do backend tính từ persisted order/payment/shipment state và caller policy; FE không tự suy lifecycle.

#### Read model và business logic

- `GetManagementOrdersQuery`: query Order → Payment/Shipment projection; sort cố định `placedAt desc`, `orderNumber`, `grandTotal`; reject sort lạ; avoid `Include` graph/N+1.
- `GetManagementOrderByIdQuery`: dùng `OrderItem` snapshot, không lấy Product hiện tại; join histories và PaymentTransaction/ShipmentHistory. PII only in staff DTO, never public/customer DTO.
- `GetManagementOrderTimelineQuery`: union normalized events (`order-status`, `payment-transaction`, `shipment-status`, `internal-note`) có `occurredAt`, `actorUserId`, `type`, `summary`, reason/reference safe. Không claim AuditLog coverage cho historical event.
- `AddInternalOrderNoteCommand`: thêm `OrderNote.CreateInternal(orderId, currentUserId, content)`; force `NoteType=Internal`, `IsVisibleToCustomer=false`; audit `OrderNoteAdded`.
- Controller tách read và mutation attribute: anti-forgery/rate-limit chỉ áp dụng unsafe management action; không đặt blindly trên GET. BFF phải cấp CSRF token nếu dùng cookie anti-forgery.

#### Validation/chặn

- Filter dates UTC, `from <= to`, span tối đa 366 days per request; page 1..100, pageSize 1..100; q max 100; carrier max 100.
- Note content required, trim, max 4,000; Order must exist; no public note endpoint in P0.
- Read detail returns `404` when ID missing, not caller-owned customer route semantics. Existing action handlers continue DB lock/lifecycle validation and use `422` for invalid transition/payment condition.
- Không add generic `PUT /orders/{id}/status`, no client price/quantity/address mutation, no delete Order/OrderItem/history/payment transaction.

### 3.3 Inventory & Stock

#### Endpoint contract

| Method / route | Request/query | Response | Policy |
| --- | --- | --- | --- |
| `GET /api/v1/management/inventory/levels` | `q`, `sku`, `productId`, `variantId`, `stockLocationId`, `lowStockOnly`, page/sort | paged level DTO | `inventory.read` |
| `GET /api/v1/management/inventory/locations` | `q`, `isActive`, page | paged location DTO | `inventory.read` |
| `POST /api/v1/management/inventory/locations` | code/name/area/address | location DTO | `inventory.locations.manage` |
| `PUT /api/v1/management/inventory/locations/{id}` | editable fields + stamp | renewed DTO/stamp | `inventory.locations.manage` |
| `POST /api/v1/management/inventory/locations/{id}/deactivate` | stamp + reason | renewed DTO | `inventory.locations.manage` |
| `POST /api/v1/management/inventory/adjustments` | productVariantId/locationId/quantityDelta/reasonCode/reason | adjustment result + movement ID | `inventory.adjust` |
| `GET /api/v1/management/inventory/movements` | item/variant/location/orderItem/type/date window/cursor | append-only ledger page | `inventory.read` |

#### Domain/service/handler design

1. Add `StockLocation.Create/Update/Deactivate`; deactivation must not allow a new adjustment/reservation but preserves history.
2. Add `InventoryManagementStore.LockOrCreateLevelAsync(variantId, locationId, ct)`: resolve `InventoryItem` for a **Tracked** Variant, create it if absent, atomically get/create InventoryLevel, then lock the row in PostgreSQL. This prevents two first adjustments from creating duplicate level/item and coordinates with checkout reservation lock.
3. `AdjustInventoryCommand` uses the locked `InventoryLevel.Adjust(quantityDelta, now, reason)`, inserts one `InventoryMovement` type `Adjust`, updates level, writes audit, returns new balances. It does not touch `ReservedQuantity` directly.
4. First positive adjustment may initialize an empty level. Negative adjustment needs an existing level and must not reduce stocked below reserved. `NotTracked` and `Preorder` variants are rejected; no fake stock for non-tracked variants.
5. `GetInventoryLevelsQuery` calculates/returns `availableQuantity` server-side and joins Product/Variant/Location labels. `GetInventoryMovementsQuery` never mutates or recomputes ledger.

#### Validation/chặn

- `quantityDelta != 0`, decimal scale at most database supported scale, absolute value bounded to an agreed operational maximum; reasonCode allow-list (`OpeningBalance`, `StocktakeGain`, `StocktakeLoss`, `Damage`, `Expiry`, `Correction`), reason required/max 1,000.
- Variant/location must exist, be active/eligible; location inactive rejects adjustment.
- DB check plus domain guard enforces non-negative stock/reservation and `reserved <= stocked`; map insufficient/under-reserved adjustment to `422`, lock/conflict to `409`.
- Location code required/max 50/unique, name required/max 200, optional area existence/address max 500; deactivation reject when active reservations exist or make explicit process pause/relocate first.
- `InventoryMovement`, `InventoryReservation`, `InventoryLevel` do not receive generic DELETE/PATCH endpoints. No FE-supplied `availableQuantity`, `reservedQuantity`, movement type or actor.

### 3.4 Settings, Audit, Sessions và Security Events

#### Endpoint contract

| Method / route | Request/query | Response | Policy |
| --- | --- | --- | --- |
| `GET /api/v1/management/settings/{group}` | group is fixed allow-list | typed group DTO + stamp | `settings.read` |
| `PUT /api/v1/management/settings/{group}` | typed group body + stamp | typed DTO + new stamp | `settings.update` |
| `GET /api/v1/management/audit-logs` | actor/entity/action/correlation/date/cursor filters | redacted audit page | `audit.read` |
| `GET /api/v1/management/security/sessions` | user/status/date/page | session DTO (no token) | `security.sessions.read` |
| `POST /api/v1/management/security/sessions/{id}/revoke` | `{ reason }` | revoked session DTO | `security.sessions.revoke` |
| `GET /api/v1/management/security/events` | user/risk/success/type/date/cursor | redacted event page | `security.events.read` |

#### Settings registry and logic

P0 supports exactly these business, non-secret groups:

| Group/key | Typed value | Explicitly excluded |
| --- | --- | --- |
| `store-profile` | display name, contact, business address, public URLs | storage/JWT/connection secret |
| `checkout-policy` | guest checkout, min order, cancel window, customer copy/legal links | server price/tax formula override |
| `payment-methods` | non-secret enabled flags/instruction text | SePay key/webhook secret |
| `notification-policy` | non-secret event/channel enablement | SMTP/SMS/FCM credential |

Implement `ISettingsDefinitionRegistry`; one typed definition owns key, DTO, default, JSON serialization and FluentValidation. Store one `SystemSetting` row per group, `Value` as JSONB, `IsPublic=false`. Unknown group/key returns `404`; body cannot set `isPublic`, raw key, description or arbitrary JSON. `GET` is side-effect-free: a missing row returns the typed default with `exists=false` and no stamp. `PUT` creates the row only when `exists=false` and its nullable expected stamp is `null`; every subsequent PUT requires the current group row stamp. Audit stores masked before/after allow-list.

#### Session/audit guard

- Session list includes user display identifier, client/auth strength, device ID, times, revoked info — never refresh token, security stamp, raw IP fingerprint or metadata.
- Revoke requires reason max 200, session exists, is active; atomically revoke session + its `SessionRefreshToken` records, record `SecurityEvent` and AuditLog. Only `revoke-all` rotates user security stamp; single-session revoke must not kill unrelated staff sessions.
- Audit/event query has stable date/cursor sorting and redacted projection; no raw `BeforeData`, `AfterData` or `SecurityEvent.Metadata` response by default.

## 4. Delivery batches, schema impact, tests và rollout gate

### 4.1 Implementation order

| Batch | Scope / folders | Migration | Acceptance outcome |
| --- | --- | --- | --- |
| 0 — Authorization/audit foundation | `Permissions`, Policy/Role seeding, `IAuditWriter`, audit factory, shared query DTO/paging/error helpers. | Likely indexes only; no entity table required. | New permissions seeded without touching custom role grants; authorized/unauthorized tests pass. |
| 1 — Producer workspace | `Features/Commerce/Producers/{Queries,Commands}`, `ManagementProducersController`, Producer domain mutations. | No new columns expected; add management-list access index only after query plan review. | A verified+published Producer appears in Catalog picker; hidden/unverified one does not. |
| 2 — Order management read + notes | `Features/Commerce/Orders/Management/*`, `ManagementOrdersController` reads, OrderNote factory. | No table change expected; optional indexes after Explain plan. | Staff sees all orders/detail/timeline; customer endpoint scope unchanged; internal note never leaks. |
| 3 — Inventory core | `Features/Commerce/Inventory/{Queries,Commands,Services}`, `ManagementInventoryController`, StockLocation domain, PostgreSQL lock store. | No column change expected; forward indexes only if needed. | Adjustment creates exactly one movement, never oversells/violates reservation balance. |
| 4 — Settings/security operations | `Features/System/*`, management system controller, typed registry, managed session service. | Audit/session/event composite indexes likely require forward migration. | No secret read/write; audit populated for every P0 mutation; revocation invalidates target refresh session. |
| 5 — FE contract/staging | OpenAPI/FE guide/BFF proxy routes, API auth and PostgreSQL tests, staging smoke. | Apply reviewed forward migration only to approved target. | All P0 pages use live contract; no mock/DB direct fallback. |

Do not combine all P0 in one commit. Commit in dependency order: authorization/audit foundation → producer → order → inventory → system/security → docs/tests. Stage only owned hunks; preserve current dirty worktree.

### 4.2 Migration decision

Basic P0 CRUD can use current tables. Do **not** generate a migration merely because a controller is added. A forward migration is required only after implementation if query profiles justify indexes, at minimum evaluate:

- `Tbl_AuditLog (EntityName, EntityId, OccurredAt DESC)` and `(ActorUserId, OccurredAt DESC)`.
- `Tbl_SecurityEvent (UserId, OccurredAt DESC)` and risk/time access path.
- management Order filters and Inventory movement cursor path with `IsDeleted=false` partial indexes.

Never edit baseline/applied migration or model snapshot manually. Before an approved migration: build, PostgreSQL tests, generated migration review, `has-pending-model-changes`, idempotent SQL review, dedicated staging database proof.

### 4.3 Required test matrix

| Layer | Mandatory tests |
| --- | --- |
| Domain | Producer lifecycle/verify-publish-hide; contact/facility ownership; location deactivate; inventory adjustment below reserved rejection; OrderNote always internal. |
| Application | Permission denied, filter validation, stale stamp `409`, parent/child ownership `404`, masked audit payload, settings group schema/secret rejection, session revoke ownership. |
| PostgreSQL integration | Producer code partial uniqueness; concurrent producer/location updates; `LockOrCreateLevel` race; adjustment vs reservation/ship transaction; non-negative/reserved constraints; append-only movement; session token revocation; audit transaction rollback. |
| API authorization | Every management route 401/403/success; customer order route cannot read all orders; internal note/PII redaction; unknown settings group; no secret field response. |
| Regression | Existing Catalog producer picker still filters `Published && IsVerified`; existing order/payment/shipment lifecycle tests retain behavior. |
| Staging smoke | Seed verified producer, create product draft, stock adjustment, checkout/order, staff process timeline, setting update/audit, session revoke; verify persisted rows and HTTP responses separately. |

EF InMemory is forbidden for relational lock/constraint/concurrency assertions. A successful build is not proof of PostgreSQL locking, media scan, payment provider or BFF behavior.

### 4.4 Failure contract for FE

| HTTP | Meaning | FE action |
| --- | --- | --- |
| `400` | JSON/field/filter format invalid | Render `validationErrors`; no retry. |
| `401` / `403` | Missing/insufficient session policy | Refresh/login or hide action; never bypass BFF. |
| `404` | Resource absent/not in allowed operational scope/unknown settings group | Close/refetch; do not substitute guessed ID. |
| `409` | concurrency/unique/lock conflict | Refetch read model; manual reapply, never blind retry. |
| `422` | lifecycle, stock, producer eligibility, secret/typed setting business rule | Show actionable prerequisite/reason. |
| `429` | protected mutation throttled | Respect `Retry-After`; avoid repeat submit. |
| `5xx` | unexpected failure | Generic error + correlation ID if returned; no raw exception. |

## 5. Approval gates and definition of done

### Approval required before coding

1. New public management routes/DTOs and BFF route mapping.
2. Permission codes, RoleSeeder custom-role preservation and default role matrix.
3. Producer lifecycle semantics (`Verify`, `Publish`, `Hide`) and hidden-producer impact on already Published Products.
4. Inventory adjustment reason-code policy, operational maximum and whether first positive adjustment is opening balance.
5. Settings group schemas, legal owner, maker-checker requirement for payment/checkout policy.
6. PII visibility/redaction policy for management order/audit/session read models.
7. Any forward migration, dependency or deployment configuration change.

### Phase P0 done only when

- All routes in section 3 are implemented with controller → validator → handler/service → domain method → persistence traceability.
- Every mutation is transactional, authorized, audited, rate-limited/CSRF-protected as appropriate and has a deterministic failure contract.
- No raw entity, secret, storage key, refresh token or client-calculated financial/stock field leaks in DTOs.
- PostgreSQL and API authorization test matrix passes against an explicit dedicated external test database.
- Idempotent migration SQL, staging smoke and FE BFF mapping are approved; only then enable P0 menu routes in production navigation.
