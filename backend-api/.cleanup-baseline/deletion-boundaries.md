# Deletion boundaries

## Protected until replaced

- `Ecom.sln` and all four project files.
- `Program.cs` and both `DependencyInjection.cs` files.
- `ApplicationDbContext.cs` and `IApplicationDbContext.cs`.
- Base entity, repository contracts, Unit of Work, base EF configuration, and interceptors.
- `BaseController` and the core error middleware.

## Planned vertical-slice deletion order

1. Simplify runtime composition and common pipeline behavior.
2. Reset DbContext and persistence registration.
3. Remove API controllers, hubs, seeders, workers, and integrations.
4. Remove Application features, Domain entities/enums/events, EF configurations, and migrations.
5. Remove unused packages and replace configuration/deployment templates.
6. Add neutral sample CRUD, a new migration, and verification tests.

Do not delete a domain folder in isolation. Its controller, feature, DI registration, hosted worker, configuration binding, EF configuration, migration dependency, and package dependency must be checked as one deletion set.

