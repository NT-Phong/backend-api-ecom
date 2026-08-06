# Tech Stack

- .NET 10, ASP.NET Core API host.
- Clean Architecture projects with MediatR CQRS and FluentValidation.
- EF Core/Npgsql/PostgreSQL, UnitOfWork and repository/query abstractions.
- JWT Bearer and policy-based authorization.
- Swagger/ReDoc, structured logging, health checks.
- xUnit for Domain tests; PostgreSQL integration tests use an explicitly configured dedicated external test database. Docker/Testcontainers are not used.

Legacy packages may remain because old modules still compile. Do not treat them as preferred dependencies for new Commerce work.
