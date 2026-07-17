namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<TResult>;
