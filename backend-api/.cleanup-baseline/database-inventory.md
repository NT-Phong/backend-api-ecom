# Database inventory

## Current persistence shape

- `ApplicationDbContext` declares 80 `DbSet` properties.
- The EF configuration tree contains 83 C# files.
- Migration roots:
  - `Infrastructure/Ecom.Infrastructure/Migrations` (82 files)
  - `Infrastructure/Ecom.Infrastructure/Persistence/Database/Migrations` (22 files)

## Retain only after decoupling

- Base entity and repository contracts.
- Generic repository and Unit of Work implementation.
- Base entity EF configuration.
- Audit and domain-event interceptors, after their current-user/audit ownership is simplified.

## Reset requirement

`ApplicationDbContext`, `IApplicationDbContext`, entity configurations, migrations, and the model snapshot must be reduced together. The old migrations must not be kept for the new starter database.

