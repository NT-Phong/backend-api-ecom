# Security Review

Check authentication, authorization, resource ownership, input validation, output minimization, logging/redaction, idempotency replay, rate limiting, upload safety, and external-provider verification.

Commerce priorities:

- Guest cart token is opaque, Secure HttpOnly, and only a hash is persisted.
- Customer/order/address/review resources enforce owner or staff policy.
- Server resolves price, stock, discount, totals, and payment state.
- Payment callbacks verify provider identity/signature and replay key when gateway scope opens.
- Never log credentials, tokens, PII, payment data, connection strings, or raw files.
