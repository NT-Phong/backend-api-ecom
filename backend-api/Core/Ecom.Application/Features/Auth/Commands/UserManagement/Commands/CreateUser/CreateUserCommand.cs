namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.CreateUser;

public record CreateUserCommand : IRequest<TResult<CreateUserResult>>
{
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
}

public record CreateUserResult
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
}
