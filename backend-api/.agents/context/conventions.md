# Conventions

- Match local CQRS, TResult/ApiResponse, UnitOfWork, and validation patterns.
- Keep controllers thin and business mutation in aggregate methods.
- Keep Commerce entities/configurations one per file.
- Keep cross-aggregate references ID-based.
- Use no-tracking projections for reads and transactions for multi-write use cases.
- Preserve public API, auth, permissions, schema, and migration contracts unless approved.
- Use stable domain error codes and do not swallow exceptions.
- Never log tokens, payment credentials, guest cart secrets, PII, connection strings, or raw files.
- Do not rename legacy typo-bearing public symbols during unrelated Commerce work.
