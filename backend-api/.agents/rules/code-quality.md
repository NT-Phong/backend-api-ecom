---
trigger: manual
---

# Code Quality Rules

## General
- Match existing project style before applying generic best practices.
- Prefer readability over cleverness.
- Prefer small functions with clear names.
- Keep business logic out of transport layers.
- Avoid unnecessary abstractions.
- Avoid broad refactors when solving local issues.
- In large existing files, add the smallest local change instead of opportunistically splitting modules.
- Preserve typo-bearing existing names unless the task is specifically a rename/migration.
- Do not bulk-fix encoding/mojibake, whitespace, or formatting in unrelated files.

## Design Patterns
Use design patterns only when they solve a real problem.

Do not introduce a pattern just because the prompt asks for senior-quality code.

A pattern is justified only when:
- A similar pattern already exists in the codebase.
- Behavior varies by strategy, provider, or type.
- Duplication appears in multiple places.
- Testability or dependency isolation materially improves.

## Error Handling
- Preserve existing error handling conventions.
- Do not swallow errors silently.
- Include enough context in errors for debugging.
- Avoid leaking secrets or sensitive user data in logs.
- Prefer `TResult.Failure(MessageKey.*, ErrorCodes.*)` for handled business failures.
- For hot paths and integrations, log operation name plus ids, not full request bodies or tokens.
- Add broad `catch (Exception)` only when matching a nearby handler style or isolating background-worker failures.

## Project-Specific Maintainability
- Do not split `PondController`, receipt handlers, report handlers, or IoT workers during unrelated fixes.
- Avoid new reflection-based or assembly-scanning behavior unless the existing `RegisAllService` pattern is directly relevant.
- Avoid adding new cache key formats when existing Redis keys can be extended safely.
- Do not change SignalR group names unless coordinating a frontend/mobile contract change.

## Comments And Documentation
- Add comments only for public APIs, complex domain rules, non-obvious behavior, or important tradeoffs.
- Do not comment obvious implementation.
- Use XML comments only where the project already expects them.
- Existing Vietnamese comments/messages may be mojibake. Do not rewrite them unless the task is localization/encoding cleanup.
