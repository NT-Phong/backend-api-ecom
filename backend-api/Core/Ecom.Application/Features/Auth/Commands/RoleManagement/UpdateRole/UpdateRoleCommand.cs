using System.Text.Json.Serialization;

namespace Ecom.Application.Features.Auth.Commands.RoleManagement.UpdateRole
{
    public record UpdateRoleCommand : IRequest<TResult<UpdateRoleResult>>
    {
        [JsonIgnore]
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int Priority { get; init; }
    }

    public record UpdateRoleResult(Guid Id, string Name, int Priority);
}
