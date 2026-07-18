# Debugging

## Trace

```text
HTTP request -> controller -> command/query -> validation/authorization
-> handler -> aggregate/query/service -> EF/PostgreSQL -> TResult/ApiResponse
```

For Commerce bugs verify identity/ownership, current product/variant state, effective price, inventory balance/reservation, transaction boundary, idempotency fingerprint, status transition, soft-delete filter, and concurrency exception handling.

Use exact payload/error/log evidence. Separate source inference from reproduced runtime behavior. Never expose customer/payment/token data in diagnostic output.
