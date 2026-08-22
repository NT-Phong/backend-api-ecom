# Request Pipeline Codegraph

```mermaid
flowchart LR
  FE[FE/BFF/Provider] --> MW[ASP.NET middleware]
  MW --> C[Versioned Controller]
  C --> M[MediatR request]
  M --> A[Authorization + Validation]
  A --> U{ITransactionalRequest?}
  U -- Query --> H[Handler / Read Store]
  U -- Mutation --> T[UnitOfWorkBehavior]
  T --> H
  H --> D[Domain method / focused service]
  D --> R[Repository / EF tracking]
  R --> DB[(PostgreSQL)]
  T --> COMMIT[Save + Commit once]
  H --> RES[TResult]
  RES --> API[ApiResponse]
```

External side effect không được chen vào transaction mở. Khi cần durable delivery: persist domain/outbox fact → commit → worker/post-commit dispatcher → external system.

## Trace template

`METHOD route` → Controller action → Command/Query → Validator → Handler → Domain methods → Repository/read store → tables → response DTO → tests. Đây là format chuẩn khi AI Agent viết codegraph mới.
