# Code Organization

## Use-case layout chuẩn

```text
Features/<BoundedContext>/<Concern>/
  Commands/<UseCase>/
    <UseCase>Command.cs
    <UseCase>CommandValidator.cs
    <UseCase>CommandHandler.cs
  Queries/<UseCase>/
    <UseCase>Query.cs
    <UseCase>QueryHandler.cs
    <UseCase>Dto.cs
  Services/
```

Controller mỏng, map route ID vào immutable request rồi `Mediator.Send`. FluentValidation kiểm shape; handler kiểm ownership/database facts; domain method giữ invariant; Infrastructure giải quyết persistence/integration.

## Source map

- API: `Presentation/Ecom.API/Controllers`, middleware/filter ở thư mục cùng project.
- Use cases: `Core/Ecom.Application/Features`.
- Shared contracts/behaviors: `Core/Ecom.Application/Common`.
- Commerce entities: `Core/Ecom.Domain/Entities/Commerce`.
- Enum/permissions: `Core/Ecom.Domain/Enums`, `Constants/Permissions.cs`.
- DB: `ApplicationDbContext`, `Configurations/Commerce`, `Migrations`.
- Tests: `Tests/Ecom.Domain.Tests`, `Tests/Ecom.IntegrationTests`.

Existing grouped/legacy handlers không phải template cho code mới. Không tạo generic CRUD service khi aggregate method hoặc focused service diễn đạt nghiệp vụ rõ hơn.
