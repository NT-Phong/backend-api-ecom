# SePay production gates

## Schema

- Obtain approval for public contract, schema, configuration, and deployment.
- Build Infrastructure; run `dotnet ef migrations has-pending-model-changes`; review forward-only idempotent SQL.
- Run migration/constraint/rollback/concurrency tests on approved PostgreSQL. Missing `ECOM_TEST_POSTGRES` is blocked, never passed.

## Configuration

- Store merchant ID and secrets only in approved secret configuration.
- Configure merchant IPN auth type `SECRET_KEY` and public HTTPS `POST /api/v1/payments/sepay/ipn`.
- Sandbox uses `pay-sandbox.sepay.vn`; Production uses `pay.sepay.vn` with distinct credentials.
- Keep secrets, raw IPN, card data, phone, and address out of logs.

## Release evidence

- Ordered form/signature, ownership, paid/duplicate/void/mismatch/late IPN tests pass.
- Sandbox demonstrates IPN, not redirect, creates local paid state.
- Staff with `payments.verify` can access reconciliation; V1 has no automatic provider refund/void action.
