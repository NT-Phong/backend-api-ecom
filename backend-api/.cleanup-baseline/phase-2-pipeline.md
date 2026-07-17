# Phase 2 pipeline and persistence boundary

## Implemented

- MediatR behaviors are registered once, in an explicit order.
- `IResult` lets transaction handling rollback a failed result without reflection.
- `ITransactionalRequest` is the opt-in contract for new starter mutations; the legacy request-level attribute remains temporarily for the current clone.
- EF concurrency exceptions become `ConcurrencyConflictException`, mapped to HTTP 409.
- Unhandled-request logging no longer serializes request bodies; response logging no longer reflects or stringifies response data.
- Repository code no longer reads HTTP claims or writes audit/concurrency fields.
- The audit interceptor owns audit, soft delete, and concurrency-stamp updates.
- DbContext obtains the audit interceptor through DI and no longer registers the pre-commit domain-event dispatcher.

## Legacy compatibility retained

- Existing Ecom handlers still use broad catches and explicit saves. They are scheduled for vertical-slice deletion rather than bulk refactoring.
- Document, material, and notification extension helpers remain because active Ecom features use them through extension-method syntax.
- Domain-event dispatch is not enabled for the starter baseline. A later project that needs side effects must choose post-commit delivery or a transactional outbox.

