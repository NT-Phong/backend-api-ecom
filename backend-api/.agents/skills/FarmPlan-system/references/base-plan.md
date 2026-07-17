# Farm Plan Backend Base Plan

Last updated: 2026-07-09

## Scope And Decision Record

This plan is based on `DacTa_KeHoach_Ecom.pdf` version 1.0 dated 15/6/2026 and current backend source in `D:\Source_Ecom\backend-api`.

Accepted decisions:
- `PlanType` is a backend contract, not only a UI grouping.
- Delete uses soft delete like the rest of the repository.
- "Farm" in the PDF maps to `Zone`.
- "Pond" in the PDF maps to `Pond`.
- Actual comparison data is manually entered in the MVP.
- Farm Plan is a new module, not a wrapper around `Season`, `Cycle`, `Expense`, or `TaskManagement`.
- Farming plans must link to `Season` through required `SeasonId`; construction plans keep `SeasonId` null.
- `Season` soft delete is blocked when active plans still reference it.
- Farming plans support optional `ExpectedCostItems` as display-only cost detail lines. `ExpectedCost` remains the source of truth for all cost/profit/comparison/summary/trend calculations.
- `ExpectedCostItems` are persisted on `Tbl_Plan` as one nullable PostgreSQL `jsonb` column through EF `ValueConverter`/`ValueComparer`, not as a separate table or owned JSON collection.

Proposed `PlanType` values:
- `Construction`
- `Farming`

Proposed `PlanScopeType` values:
- `Zone`
- `Pond`

Proposed `PlanStatus` values:
- `Draft`
- `InProgress`
- `Completed`

## Senior Backend Assessment

The PDF describes a business planning document with CRUD, farm-or-pond scope, manual status, planned costs, planned production, expected revenue/profit, actual values after completion, comparison views, edit history, and attachments. In the current source, no dedicated Plan/FarmPlan feature exists. The nearest reusable patterns are:

- `Zone` for farm scope.
- `Pond` for pond scope.
- `Expense` for CRUD shape and `DocumentIds`.
- `IssueReportStatusHistory` for history query shape.
- `Document` upload for attachments.
- `BaseEntity` for audit and soft delete.

The backend should implement a dedicated aggregate because the PDF defines a document-like planning workflow independent of active production entities. Reusing `Season` or `Cycle` would incorrectly bind plan lifecycle to real farming operations. Reusing `Expense` would lose planned-vs-actual semantics and status/history behavior.

The most important backend responsibility is to make calculations and comparison rules authoritative. Frontend can calculate in realtime for form feedback, but persisted values and response DTOs should be calculated or validated by backend so list/detail/comparison screens stay consistent across clients.

## Contract Shape

### Core Entity: `FarmPlan` or `Plan`

Prefer `FarmPlan` if the team wants to avoid collision with generic planning terminology. Prefer `Plan` if API simplicity is more important. Keep naming consistent across entity, feature folder, controller, and permissions.

Required fields:
- `Name`: required, max 255.
- `PlanType`: required, `Construction` or `Farming`.
- `ScopeType`: required, `Zone` or `Pond`.
- `ZoneId`: required.
- `PondId`: nullable; required when `ScopeType = Pond`.
- `ExpectedCompletionDate`: nullable.
- `Status`: required, default `Draft`.
- `Description`: nullable.
- `DocumentIds`: nullable `ICollection<Guid>`, stored as `uuid[]`.

Planned cost fields:
- Current implementation uses `ExpectedCost` as the persisted total planned cost.
- `ExpectedCostItems`: optional display-only detail rows for farming plans. Each item has `Name` and `Amount`.
- Do not calculate `ExpectedCost` from `ExpectedCostItems`.
- Do not validate that `sum(ExpectedCostItems.Amount) == ExpectedCost`.
- Do not include `ExpectedCostItems` in summary, comparison, trend, revenue, or profit calculations.

Planned yield/profit fields:
- `ExpectedYieldTons`
- `Fcr`
- `ExpectedSalePricePerKg`

Actual fields for MVP:
- `ActualYieldTons`
- `ActualRevenue`
- `ActualCost`
- `ActualNote`
- `ActualUpdatedAt`
- `ActualUpdatedBy`

Computed response fields:
- `TotalExpectedCost`
- `RequiredFeedTons`
- `ExpectedRevenue`
- `ExpectedProfit`
- `ExpectedProfitPercent`
- `ActualProfit`
- `ActualProfitPercent`
- `HasActualData`
- `CanCompare`

### History Entity

Use a dedicated history entity because `BaseEntity.UpdatedAt/UpdatedBy` only preserves the latest update.

Suggested fields:
- `PlanId`
- `Action`: `Created`, `Updated`, `StatusChanged`, `ActualUpdated`, `Deleted`
- `FromStatus`
- `ToStatus`
- `Summary`
- `CreatedAt`
- `CreatedBy`

## API Plan

Use a new controller under API v1.

Suggested routes:

```text
GET    /api/v1/plans
POST   /api/v1/plans
GET    /api/v1/plans/{id}
PUT    /api/v1/plans/{id}
DELETE /api/v1/plans/{id}
PUT    /api/v1/plans/{id}/actual
GET    /api/v1/plans/{id}/comparison
GET    /api/v1/plans/{id}/histories
```

List filters:
- `planType`
- `scopeType`
- `zoneId`
- `pondId`
- `status`
- `keyword`
- paging and sorting inherited from existing `BaseQueryDto`

Default sorting:
- newest updated first, equivalent to `UpdatedAt ?? CreatedAt` descending.

Permissions:
- `plan.read`
- `plan.create`
- `plan.update`
- `plan.delete`

## Phase-by-Phase Execution Plan

### Phase 0 - Contract Lock

Goal: freeze backend contract before schema work.

Tasks:
1. Confirm entity name: `Plan` or `FarmPlan`.
2. Confirm enum labels and serialized values for `PlanType`, `PlanScopeType`, and `PlanStatus`.
3. Confirm response naming for computed money/yield fields.
4. Confirm actual data is manual for MVP.
5. Confirm new permissions and role seeding expectations.
6. Confirm route prefix with FE.

Acceptance:
- Backend and FE agree on request/response fields.
- No ambiguous "farm" terminology remains; backend uses `ZoneId`.
- PlanType is explicitly required.

Risks:
- If PlanType is later removed, UI tab filtering and list grouping must be redesigned.
- If actual data becomes auto-synced later, comparison logic requires a separate integration phase.

### Phase 1 - Domain And Persistence

Goal: create durable schema and invariants.

Tasks:
1. Add domain enums for `PlanType`, `PlanScopeType`, `PlanStatus`, and history action.
2. Add `Plan` or `FarmPlan` entity with constructor and domain update methods.
3. Add `PlanHistory` entity.
4. Add EF configurations using `BaseEntityConfiguration`.
5. Configure `DocumentIds` as `uuid[]`.
6. Configure enum conversion consistently with nearby entities.
7. Add `DbSet`s to `ApplicationDbContext`.
8. Add migration only after approval.

Validation rules:
- name required and max 255.
- `ZoneId` cannot be empty.
- `PondId` cannot be empty when scope is pond.
- numeric values cannot be negative.
- `Fcr` cannot be negative; decide whether zero is valid.
- `ExpectedSalePricePerKg` cannot be negative.

Acceptance:
- Domain model builds.
- EF model builds.
- Migration reflects only Farm Plan tables and indexes.

Risks:
- Migration touches schema, so review carefully before apply.
- enum storage must be stable because FE/API contracts may rely on values.

### Phase 1.1 - Expected Cost Detail Items

Goal: let FE show user-entered cost detail rows for farming plans without changing backend calculations.

Implemented source anchors:
- Domain value type: `Core/Ecom.Domain/Entities/PlanExpectedCostItem.cs`.
- Aggregate field and normalization: `Core/Ecom.Domain/Entities/Plan.cs`.
- Create/update request contract: `CreatePlanCommand.ExpectedCostItems`, `UpdatePlanCommand.ExpectedCostItems`.
- Detail response contract: `PlanDetailDto.ExpectedCostItems`.
- EF mapping: `Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/PlanConfiguration.cs`.
- Migration: add nullable `ExpectedCostItems` `jsonb` column on `Tbl_Plan`.

Rules:
- Only `PlanType.Farming` persists provided items.
- `PlanType.Construction` clears items to an empty list even if the request includes them.
- Item `Name` is required when an item exists, trimmed, and max 255 characters.
- Item `Amount` is required and must be `>= 0`.
- Null database value or invalid legacy JSON should read as an empty list.
- Save-time serialization should not swallow errors.

Rollout note:
- Keep handler and command contract changes in the same commit. A partial commit where `UpdatePlanCommandHandler` calls `request.ExpectedCostItems` but `UpdatePlanCommand` lacks the property causes `CS1061` in clean CI builds even if the local workspace builds after uncommitted edits.

### Phase 1.2 - Current Form Scope Decisions

Goal: keep backend scope aligned with the simplified create/edit form.

Implemented decisions:
- `ExpectedCostItems` remains display-only. FE may calculate allocated/remaining/over-budget display from items, but backend does not calculate or persist those budget fields.
- Overlap warning for duplicated pond/season/date-range plans is out of current backend scope.
- `SeasonId` is required for `PlanType.Farming`; create/update reject empty season values with `MessageKey.SeasonIdRequired`. Construction plans keep `SeasonId` null.
- `PlanType` is immutable after create. `UpdatePlanCommandHandler` rejects updates where `request.PlanType` differs from the persisted plan type.
- Plan create/update validates that non-empty `DocumentIds` exist in `Document`; file type and file size validation remain owned by the Document upload flow.
- Farming detail and list responses expose `CanHaveYieldMetrics` so FE can hide yield/FCR/sale-price controls. Current mapping treats `PondCategoryEnum.CreateCycle` and `PondCategoryEnum.GenerateCycle` as yield-capable pond categories. Zone-scoped farming plans return yield-capable.

Source anchors:
- `Core/Ecom.Application/Features/Plan/Commands/UpdatePlan/UpdatePlanCommandHandler.cs`
- `Core/Ecom.Application/Features/Plan/Commands/CreatePlan/CreatePlanCommandHandler.cs`
- `Core/Ecom.Domain/DTOs/Plan/PlanDetailDto.cs`
- `Core/Ecom.Domain/DTOs/Plan/PlanListDto.cs`
- `Core/Ecom.Application/Features/Plan/Queries/GetPlanDetail/GetPlanDetailQueryHandler.cs`
- `Core/Ecom.Application/Features/Plan/Queries/GetPlanList/GetPlanListQueryHandler.cs`

### Phase 2 - Core CRUD CQRS

Goal: support list, create, detail, update, and delete.

Tasks:
1. Create `Features/Plan/Commands/CreatePlan`.
2. Create `UpdatePlan`.
3. Create `DeletePlan`.
4. Create `GetPlans`.
5. Create `GetPlanById`.
6. Add validators next to each command/query.
7. Validate `Zone` exists.
8. Validate `Pond` exists and belongs to `Zone` when `ScopeType = Pond`.
9. Use `[EnableUnitOfWork]` for mutations.
10. Persist explicit history entries for create/update/delete.

Response DTO:
- include scope names (`ZoneName`, `PondName`) for list/detail.
- include updated user display name if needed by list UI.
- include computed totals.

Acceptance:
- CRUD behavior matches PDF.
- delete is soft delete.
- list filters work.
- default sort is newest updated first.

Risks:
- Do not hard delete even though PDF says "khong khoi phuc"; repo pattern is soft delete.
- Avoid putting calculation logic in controller.

### Phase 3 - API Controller And Permissions

Goal: expose stable API through Presentation layer.

Tasks:
1. Add `PlanController`.
2. Add routes and `ProducesResponseType` declarations following `ExpenseController`.
3. Add permissions to `Permissions.cs`.
4. Add permission definitions in `GetAll()`.
5. Ensure controller maps route id into update/delete commands as local patterns do.

Acceptance:
- routes are protected by plan permissions.
- controller remains thin.
- response envelope follows `HandleResult` / `ApiResponse` conventions.

Risks:
- Permission changes affect seed/runtime authorization; coordinate deployment and role policy assignment.

### Phase 4 - Actual Data And Comparison

Goal: support planned-vs-actual tab.

Tasks:
1. Add `UpdatePlanActualCommand`.
2. Store manual actual yield/revenue/cost/note.
3. Return `HasActualData` and `CanCompare`.
4. Add `GetPlanComparisonQuery`.
5. Enforce comparison only when status is `Completed` and actual values exist.
6. Add history entry `ActualUpdated`.

Comparison DTO:
- planned yield vs actual yield.
- planned revenue vs actual revenue.
- planned total cost vs actual cost.
- planned profit/profit percent vs actual profit/profit percent.

Acceptance:
- incomplete plan returns `CanCompare=false`.
- completed plan with actual values returns full comparison.
- calculations are stable for zero revenue and null values.

Risks:
- Division by zero on profit percent.
- Unit mismatch between tons and kg.

### Phase 5 - Previous-Plan Trend Comparison

Goal: support "so sanh giua cac vu" from PDF.

Tasks:
1. Add previous plans query or include section in comparison endpoint.
2. Filter by same `PlanType`, `ScopeType`, and same `ZoneId/PondId`.
3. Include only `Completed` plans with actual data.
4. Sort by expected completion date or updated date descending.
5. Support top N or paging.

Acceptance:
- current plan appears with prior completed plans when appropriate.
- plans without actual data are excluded.

Risks:
- For `Construction`, "vu" wording may not apply; FE should label trend by PlanType.

### Phase 6 - Attachments

Goal: integrate with existing document workflow without changing Document storage.

Tasks:
1. Accept `DocumentIds` in create/update.
2. Optionally validate all document IDs exist.
3. Return `DocumentIds` in list/detail.
4. If FE needs URLs, either use existing document URL endpoint or enrich detail response after confirming expected contract.

Acceptance:
- supports PDF, PNG, JPG, DOCX through existing upload validator.
- Plan stores document IDs as `uuid[]`.

Risks:
- `Document.ParentId` is `long?`; do not use it for Guid Plan IDs.
- Avoid deleting physical files when a plan is soft-deleted unless a separate cleanup policy is approved.

### Phase 7 - Verification And Rollout

Goal: prove the feature works without broad changes.

Build checks:

```powershell
dotnet build Core/Ecom.Domain/Ecom.Domain.csproj
dotnet build Core/Ecom.Application/Ecom.Application.csproj
dotnet build Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj
dotnet build Presentation/Ecom.API/Ecom.API.csproj
```

Manual smoke:
1. Create draft construction plan scoped to zone.
2. Create farming plan scoped to pond.
3. List by PlanType/status/keyword.
4. Update status to `InProgress`.
5. Update to `Completed`.
6. Add actual data.
7. Fetch comparison.
8. Fetch histories.
9. Soft delete and confirm it disappears from normal list.

Acceptance:
- all touched projects build.
- smoke workflow passes.
- no unrelated API/config/IoT/SignalR/Redis contracts change.

## Implementation Guardrails

- Do not modify auth/current-user internals.
- Do not alter existing `Season`, `Cycle`, `Expense`, `Harvest`, or `Document` behavior unless explicitly required.
- Do not modify Redis key formats, SignalR groups, IoT telemetry, or appsettings.
- Do not bulk-fix mojibake comments/messages as part of Farm Plan work.
- Keep migrations scoped to Farm Plan tables only.
- Keep DTOs version-stable once FE starts integration.

## Suggested First Implementation Slice

Start with a vertical MVP:
1. Domain enums and `Plan` entity.
2. EF config and `DbSet`.
3. Create/list/detail/update/delete commands and queries.
4. Controller and permissions.
5. Build verification.

Then add:
1. `PlanHistory`.
2. Actual data command.
3. Comparison queries.
4. Previous plan trend query.


