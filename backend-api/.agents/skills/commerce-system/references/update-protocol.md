# Commerce Reference Update Protocol

After durable Commerce changes:

1. Re-read affected source and current diff.
2. Update `current-status.md` only with verified state.
3. Update roadmap only when batch status/gate changes.
4. Update entity model for accepted invariant/state/boundary changes.
5. Update relational database for table/FK/constraint/index/migration/delete-policy changes.
6. Keep business document and ERD aligned with public vocabulary/relationships.

Statuses: Not started; In progress; Source implemented - verification pending; Verified; Staging verified. Never call a batch complete with an unmet gate.

Record evidence:

```markdown
### YYYY-MM-DD - Batch N
- Source: symbols changed
- Verification: exact command/result
- Migration: none/generated/reviewed/staging-applied
- Remaining risk: unresolved item
```

Drift searches:

```powershell
rg -n "enum (OrderStatus|PaymentStatus|ShipmentStatus|TradeInquiryStatus)" Core/Ecom.Domain
rg -n "IAggregateRoot|CommerceDomainException" Core/Ecom.Domain/Entities/Commerce
rg -n "IRequestHandler|HttpGet|HttpPost" Core/Ecom.Application/Features Presentation/Ecom.API/Controllers
git diff -- document/DETAILED-ERD.dbml Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce
```
