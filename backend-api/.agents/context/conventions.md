# Conventions

## General
- Match nearby code style before introducing a new convention.
- Keep changes small and scoped to the requested behavior.
- Avoid broad reformatting.
- Prefer explicit, readable code over clever abstractions.
- Preserve existing typo-bearing public names and filenames unless the user explicitly asks for a rename/migration.
- Do not bulk-fix mojibake comments/messages in unrelated changes.

## Project Layout
- Domain objects live in `Core/Ecom.Domain`.
- CQRS use cases live in `Core/Ecom.Application/Features/<Feature>`.
- Infrastructure implementations live in `Infrastructure/Ecom.Infrastructure`.
- API controllers live in `Presentation/Ecom.API/Controllers` and `Presentation/Ecom.API/Controllers/V1`.

## CQRS
- Commands and queries should be MediatR requests.
- Handlers should orchestrate use cases and call domain methods, repositories, DbContext abstractions, or services as the local feature does.
- Validators should use FluentValidation and live near the command/query.
- Models/DTOs should stay near the feature unless shared by an established local pattern.
- Commands are often mutable records/classes because controllers assign route ids into request objects.
- Queries may inherit or contain paging/filter DTOs; preserve existing query DTO patterns.

## Transactions
- Add `[EnableUnitOfWork]` to mutation requests or handlers where the existing pattern requires transaction safety.
- Do not call transaction APIs directly unless the existing feature pattern does.
- Ensure changes are persisted through the existing UnitOfWork flow.
- Remember `CommitTransactionAsync` saves changes.
- Existing handlers often call `SaveChangesAsync` explicitly; do not remove that pattern broadly during local fixes.

## Controllers
- Controllers should be thin.
- Map route/body/query data into a command/query.
- Call `Mediator.Send`.
- Return via `HandleResult` or the established explicit `ApiResponse` pattern used nearby.
- Do not place new business logic, persistence logic, or integration logic in controllers.
- Controller-level `[Authorize]` plus action-level `[Authorize(Policy = Permissions.*)]` is common.
- Several controllers group many child resources in one controller, especially Pond/Zone. Do not split controllers during bug fixes.

## Domain
- Prefer domain methods for state transitions.
- Keep entities free from infrastructure concerns.
- Preserve audit, soft-delete, concurrency, and domain event behavior.
- Treat permission, message, and error constants as shared contracts.

## Persistence
- Use existing EF Core configurations and repository conventions.
- Use `QueryNoTracking` or no-tracking patterns for read-only queries when available.
- Do not manually change migrations without explicit approval.
- Generic repositories filter `IsDeleted` by default.
- `BaseRepository` string `orderBy` uses EF property names with first-letter capitalization; validate field names before passing user-controlled sort keys.

## Errors
- Use existing `TResult`, `MessageKey`, and `ErrorCodes` conventions for handled business errors.
- Do not swallow exceptions.
- Do not leak secrets or sensitive data into errors or logs.
- Many existing handlers catch `Exception` and return generic `TResult` failures. For new code, prefer catching specific expected exceptions unless matching a local handler style.
- Use `MessageKey` constants when a suitable key already exists; do not create duplicate literal messages.

## Logging
- Include useful context such as request name, entity id, device id, session id, or correlation id.
- Avoid logging full payloads when they may include secrets, tokens, or personal data.
- Be extra careful with refresh tokens, FCM tokens, JWT query-string `access_token`, IoT auth data, Basic Auth, and file payloads.

## Protected Areas
- Do not modify Logout validator or Logout handler unless explicitly requested.
- Do not modify `appsettings*.json`, `.env`, certificates, keys, or local runtime configuration unless explicitly requested.
- Treat `Permissions.cs`, `ErrorCodes.cs`, EF migrations, SignalR group names, IoT telemetry codes, and cache key formats as contract-like.

