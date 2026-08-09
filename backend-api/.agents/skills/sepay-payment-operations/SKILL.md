---
name: sepay-payment-operations
description: Source-first workflow for reviewing, planning, implementing, testing, or operating Thanh Hoa Commerce SePay Hosted Checkout and IPN. Use whenever a task mentions SePay, Hosted Checkout, payment form/signature, IPN/webhook, X-Secret-Key, PaymentMethod.SePay, payment reconciliation, SePay Sandbox/Production configuration, or SePay payment migration.
---

# SePay Payment Operations

Use this skill for the SePay payment boundary only. Read `AGENTS.md` and `.agents/context/task-router.md`, then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/sepay-payment-operations/scripts/scan-sepay-payment.ps1
```

Read `references/source-map.md` for every task. Read `references/production-gates.md` for migrations, configuration, IPN security, Sandbox, staging, or go-live. Load `commerce-checkout-order-operations` only when Cart, stock reservation, Order, Shipment, or cancellation scope expands.

## Operating model

```text
Customer -> owner-scoped checkout endpoint -> ordered signed POST form -> SePay
SePay -> anonymous HTTPS IPN -> authenticated normalized notification -> transaction -> Payment.MarkPaid
Redirect -> UX only -> GET order status; never payment authority
```

- `PaymentGatewayAttempt` correlates one local SePay payment with invoice and expected amount.
- `PaymentGatewayNotification` is append-only normalized audit data. Never persist raw IPN JSON, card data, customer data, source IP, or secret.
- `ORDER_PAID` can call `Payment.MarkPaid` only after exact local verification. `TRANSACTION_VOID`, mismatch, duplicate collision, and late payment become reconciliation records. V1 does not auto-refund, cancel, or mutate an already non-pending order.

## Non-negotiable invariants

1. Use the existing local Order/Payment; never trust client amount, currency, invoice, status, or payment result.
2. Compute HMAC-SHA256/Base64 on the server. Return `fields` as an ordered list; frontend appends hidden inputs in exactly that order and submits them unchanged.
3. Treat success/error/cancel redirect as navigation only; read the owner-scoped order API for payment status.
4. Require fixed-time `X-Secret-Key` validation. This V1 supports only merchant auth type `SECRET_KEY`.
5. Before `MarkPaid`, verify provider, invoice, VND, exact amounts, `CAPTURED`, `APPROVED`, local pending states, and unique provider transaction within the UnitOfWork.
6. Let `UnitOfWorkBehavior` commit once. Do not call provider APIs, refund, send side effects, or log raw payment data inside the transaction.
7. Obtain explicit approval before public contract, auth, migration, configuration, secret, or deployment changes.

## Change workflow

1. Trace controller -> request/validator -> handler -> domain -> EF config -> migration -> tests.
2. Preserve canonical field ordering and the exact signature test vector for form changes.
3. Define accepted, duplicate, reconciliation, malformed, unauthorized, and transient IPN responses. Persist reconciliation before its 200 acknowledgement.
4. Generate forward-only migrations, inspect idempotent SQL, run `has-pending-model-changes`, and use PostgreSQL tests. Never edit an applied migration or snapshot by hand.
5. Report source, test, migration, runtime, and production evidence separately.

## Verification minimums

- Domain: pending/paid/reconciliation transitions and no automatic void/refund.
- Service: ordered fields, signature vector, fixed-time secret, environment URL validator.
- API: ownership, 16 KB JSON IPN, paid, duplicate, mismatch, void, late, unauthorized.
- PostgreSQL: migration, constraints, rollback, and duplicate-IPN concurrency. Never use EF InMemory for these claims.
- Sandbox: public HTTPS IPN, `SECRET_KEY` auth, separate credentials, and evidence that IPN—not redirect—changes local payment state.

## Official contract refresh

Before changing provider fields, endpoint URLs, events, or auth behavior, recheck:

- https://developer.sepay.vn/vi/cong-thanh-toan/API/don-hang/form-thanh-toan
- https://developer.sepay.vn/vi/cong-thanh-toan/IPN
- https://developer.sepay.vn/vi/cong-thanh-toan/sandbox
