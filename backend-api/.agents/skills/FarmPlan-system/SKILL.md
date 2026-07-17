---
name: farmplan-system
description: Use this skill when analyzing, planning, implementing, reviewing, or debugging the Ecom backend Farm Plan / Ke hoach module. Use for PlanType, farm-or-pond scope, draft/in-progress/completed status, planned cost/yield/profit calculations, actual result comparison, plan history, soft delete, document attachments, permissions, CQRS handlers, EF Core schema, and API contract work.
---

# FarmPlan System Skill

## Purpose

Guide backend work for the Ecom Farm Plan module defined by `DacTa_KeHoach_Ecom.pdf`.

The accepted base decisions are:
- Use `PlanType` as an explicit backend contract.
- Use soft delete through the repo's `BaseEntity` / repository behavior.
- Treat "farm" scope as `Zone` and "pond" scope as `Pond`.
- Keep actual comparison data manually entered for the MVP unless BA explicitly approves integration with operational `Harvest` or `Expense` data.

## Required Reading

Before Farm Plan implementation or review, read:
1. `AGENTS.md`
2. `.agents/context/task-router.md`
3. `.agents/context/architecture.md`
4. `.agents/context/conventions.md`
5. `.agents/skills/backend-api-architecture/SKILL.md`
6. `.agents/skills/FarmPlan-system/references/base-plan.md`

Then inspect current source. Source code remains the authority if it diverges from this skill.

## Primary Boundaries

Use these source areas first:
- Controllers: `Presentation/Ecom.API/Controllers/V1`
- CQRS features: `Core/Ecom.Application/Features`
- Domain entities/enums/constants: `Core/Ecom.Domain`
- EF Core configuration and DbContext: `Infrastructure/Ecom.Infrastructure/Persistence/Database`
- Document attachment patterns: `Core/Ecom.Application/Features/Document`, `Core/Ecom.Domain/Entities/Document.cs`
- History pattern: `IssueReportStatusHistory` and `GetIssueReportHistories`
- CRUD pattern: `Expense`, `Season`, and `Cycle` features

## Rules

- Do not reuse `Season`, `Cycle`, `Expense`, or `TaskManagement` as the Plan aggregate. Farm Plan is a new planning/document module.
- Do not hard delete Plan records unless explicitly requested. Use soft delete.
- Do not create automatic status transitions by date. Status changes are manual.
- Do not auto-source actual data from operational modules in MVP. Store actual values on Plan or a Plan actual sub-entity.
- Add plan permissions as a new contract: `plan.read`, `plan.create`, `plan.update`, `plan.delete`.
- Keep controllers thin and use MediatR commands/queries with validators.
- Use `[EnableUnitOfWork]` for mutations and preserve local save/transaction patterns.
- Return computed values from backend DTOs; frontend realtime calculations are display helpers only.

## Workflow

1. Reconfirm the requested phase from `references/base-plan.md`.
2. Search source before opening large files.
3. Inspect the nearest working pattern for the phase.
4. Implement only the current phase unless asked to continue.
5. Run the narrowest build verification for changed projects or state why it was not run.
6. Update `references/base-plan.md` only when BA decisions, implementation status, or risks materially change.

