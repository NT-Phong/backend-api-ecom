# Commerce Relational Database

## Baseline

- PostgreSQL EF Core/Npgsql; UUID, audit, soft delete, concurrency stamp.
- `Tbl_<EntityName>` naming and global active-row filter.
- Configuration root: `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce`.
- ERD: `document/DETAILED-ERD.dbml` and `.dbdiagram`.
- Latest scan: 79 entities/configurations, 80 DBML tables including User, 115 relations.

## Principal Relations

| Relation | Rule |
| --- | --- |
| Producer -> Product | One-to-many |
| Category -> Category | Optional parent; prevent cycles in domain/application |
| Product -> Variant | One-to-many; Variant is sellable |
| Product <-> Category | ProductCategory junction; one active primary |
| Variant -> Price | Multiple effective prices |
| Variant -> InventoryItem | At most one active item |
| InventoryItem + Location -> Level | Unique active pair |
| Cart -> CartItem | Item references Variant |
| Order -> OrderItem | Immutable snapshots |
| Order -> Payment/Shipment | Attempts/fulfillment remain independent roots |
| TradeInquiry -> Item/History | One-to-many |

## Existing Constraints

- Cart owner XOR and partial unique active owner.
- One active primary category/default address.
- Order total formula and non-negative money.
- Positive quantities and inventory balance guards.
- Price amount/minimum/time-window checks.
- Attachment parent XOR.
- Filtered unique junction/business keys; enum-as-string.

## Required Upgrades

- PostgreSQL `btree_gist` exclusion for overlapping active VariantPrice scope using normalized PriceListId and `tstzrange`.
- Atomic/locked MAIN inventory reservation inside CreateOrder transaction.
- Idempotency table: scope, owner, key hash, fingerprint, status, resource/result, expiry.
- Replace global low-value IsDeleted/No/CreatedAt indexes with partial/composite access-path indexes.
- Use real PostgreSQL tests, never EF InMemory, for constraints/concurrency.

## Delete Policy

- Soft delete/deactivate catalog, producer, media, CMS.
- Never application-delete Order/Item, PaymentTransaction, InventoryMovement, histories, AuditLog.

## Migration Safety

- Baseline: `20260717150734_AddThanhHoaCommerceSchemaV2`.
- Do not rewrite if shared/applied.
- Add explicit enum-string data mappings in a new migration.
- Never hand-edit the model snapshot.
- Review idempotent SQL and prove staging before production.
