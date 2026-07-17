namespace Ecom.Application.Features.Auth.Commands.RoleManagement.CreateRole;

public record CreateRoleCommand : IRequest<TResult<CreateRoleResult>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Priority { get; init; }
}

public record CreateRoleResult(Guid Id, string Name, string Code, int Priority);
