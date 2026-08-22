# System Architecture

## Dependency direction

```text
Presentation/Ecom.API
        ↓
Core/Ecom.Application
        ↓
Core/Ecom.Domain

Infrastructure/Ecom.Infrastructure
        → Application abstractions + Domain
```

- Presentation: HTTP, versioning, auth filters, middleware, binding, `ApiResponse`.
- Application: use case, MediatR, validation, authorization, transaction marker, DTO/orchestration.
- Domain: entity, enum, invariant, state transition, domain event; framework-light.
- Infrastructure: EF Core/PostgreSQL, repositories, Redis, storage, SePay, auth implementation, workers/outbox.

## Runtime stack trong source

.NET 10, ASP.NET Core, MediatR 14, FluentValidation 12, EF Core/Npgsql 10, PostgreSQL; Redis cho distributed cache/lock/rate-limit khi configured; OpenTelemetry/Serilog; Azure Blob/ImageSharp/media worker; SePay services; Outbox tùy `Outbox:Enabled`.

## HTTP pipeline đáng chú ý

Security headers → authentication timing → structured logging → error handling → proxy authorization → authentication → authorization → controllers. Route API được version theo `/api/v{version}`.

## Transaction model

`ITransactionalRequest` kích hoạt `UnitOfWorkBehavior`. Behavior sở hữu transaction và commit/save một lần. Query không transactional và ưu tiên no-tracking. Handled failure, exception, cancellation hoặc concurrency failure phải rollback khi behavior sở hữu transaction.

## Background work

Source đăng ký Media storage validator, Media processing worker, reservation expiry worker và Outbox processor khi enabled. Sự đăng ký không chứng minh external dependency đang reachable.
