namespace Ecom.Application.Features.Auth.Queries.GetRoles;

public record GetRolesQuery : IRequest<TResult<List<RoleResult>>>;

public record RoleResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Priority { get; init; }
    public bool IsSystemRole { get; init; }
}
