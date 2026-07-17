using System.Text.Json.Serialization;

namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<TResult<UpdateUserResult>>
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateUserResult
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
