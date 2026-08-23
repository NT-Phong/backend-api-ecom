using Ecom.Domain.Entities;
using DomainPermissions = Ecom.Domain.Constants.Permissions;

namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, TResult<CreateUserResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateUserCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TResult<CreateUserResult>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate phone number uniqueness
        if (await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken))
            return TResult<CreateUserResult>.Failure(MessageKey.PhoneNumberAlreadyExists);

        // Check target role exists
        var targetRole = await _context.Roles.FindAsync(new object[] { request.RoleId }, cancellationToken);
        if (targetRole == null)
            return TResult<CreateUserResult>.Failure(MessageKey.RoleNotFound);

        // 2. Authorize creator
        var currentUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (currentUser == null)
            return TResult<CreateUserResult>.Failure(MessageKey.UserNotFound);

        // Only Admin role can create users
        if (currentUser.Role?.Code != DomainPermissions.Roles.Admin)
        {
            return TResult<CreateUserResult>.Failure(MessageKey.InsufficientPermissions);
        }

        // 3. Create User
        var newUser = new User(
            request.FullName,
            request.PhoneNumber,
            request.RoleId,
            null, // No zones in starter template
            Domain.Enums.UserStatusEnum.Active
        );

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Map Result
        var result = new CreateUserResult
        {
            Id = newUser.Id,
            FullName = newUser.FullName ?? string.Empty,
            PhoneNumber = newUser.PhoneNumber ?? string.Empty,
            RoleId = newUser.RoleId ?? Guid.Empty,
            RoleName = targetRole.Name
        };

        return TResult<CreateUserResult>.Success(result);
    }
}
