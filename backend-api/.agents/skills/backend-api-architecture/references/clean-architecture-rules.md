# Clean Architecture Rules

## Dependency Direction
- Presentation depends on Application.
- Application depends on Domain and abstractions.
- Infrastructure implements abstractions.
- Domain does not depend on Infrastructure, Presentation, or external frameworks.

This repository is pragmatic. Application currently references EF Core abstractions, ASP.NET Core `IFormFile`, cache abstractions, SignalR interfaces, and IoT interfaces. Do not launch broad architecture cleanup from a local task.

## Layer Ownership
- Presentation owns HTTP, SignalR, filters, middleware, and response wrapping.
- Application owns use cases, validation, authorization decisions, and orchestration.
- Domain owns business invariants and state transitions.
- Infrastructure owns database, cache, messaging, IoT, security providers, telemetry, and external clients.
- Shared contracts such as `Permissions`, `ErrorCodes`, `MessageKey`, `TResult`, and SignalR client interfaces are project conventions.

## Review Questions
- Did new code introduce an outward dependency from Domain or Application?
- Did controller or hub code gain business logic?
- Did Infrastructure leak into request/response contracts?
- Did a refactor move behavior without preserving public API behavior?
- Did the change preserve existing `TResult`, UnitOfWork, permission, and SignalR contracts?
- Did the change avoid rename-only cleanup of typo-bearing existing symbols?
