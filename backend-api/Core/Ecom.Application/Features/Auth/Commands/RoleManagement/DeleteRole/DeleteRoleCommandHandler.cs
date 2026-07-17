namespace Ecom.Application.Features.Auth.Commands.RoleManagement.DeleteRole
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, TResult>
    {
        private readonly IApplicationDbContext _context;
        public DeleteRoleCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<TResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (role == null) return TResult.Failure(MessageKey.RoleNotFound, ErrorCodes.NOT_FOUND);

            // 1. Chặn xóa Role hệ thống (Admin, Manager...)
            if (role.IsSystemRole)
                return TResult.Failure(MessageKey.DeleteSystemRoleFailed, ErrorCodes.BAD_REQUEST);

            // 2. Chặn xóa nếu đang có người dùng thuộc Role này
            if (role.Users.Any())
                return TResult.Failure(MessageKey.AccountLockedWithMinutes, ErrorCodes.BAD_REQUEST);

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);
            return TResult.Success();
        }
    }
}
