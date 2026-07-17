namespace Ecom.Application.Features.Auth.Commands.UserManagement.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, TResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeleteUserCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return TResult.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);

        // SỬ DỤNG CONSTANT ĐỂ BẢO VỆ SUPER ADMIN
        if (user.PhoneNumber == ApplicationConstants.SystemConstants.SuperAdminPhone)
        {
            return TResult.Failure(MessageKey.InsufficientPermissions, ErrorCodes.BAD_REQUEST);
        }

        var currentUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        // SỬ DỤNG CONSTANT ĐỂ CẤP QUYỀN XÓA CHO NGƯỜI GỌI
        bool isCallerSuperAdmin = currentUser?.PhoneNumber == ApplicationConstants.SystemConstants.SuperAdminPhone;

        if (!isCallerSuperAdmin)
        {
            if (user.Id == _currentUser.UserId || user.Role?.Priority <= currentUser?.Role?.Priority)
            {
                return TResult.Failure(MessageKey.InsufficientPermissions, ErrorCodes.BAD_REQUEST);
            }
        }

        // 4. Thực hiện xóa (hoặc Soft Delete nếu hệ thống của bạn yêu cầu)
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return TResult.Success();
    }
}
