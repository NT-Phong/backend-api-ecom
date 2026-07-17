namespace Ecom.Application.Features.Auth.Commands.RoleManagement.DeleteRole
{
    public record DeleteRoleCommand(Guid Id) : IRequest<TResult>;
}
