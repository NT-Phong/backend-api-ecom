namespace Ecom.Application.Features.Auth.Commands.RoleManagement.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, TResult<UpdateRoleResult>>
    {
        private readonly IApplicationDbContext _context;
        public UpdateRoleCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<TResult<UpdateRoleResult>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Roles.FindAsync(new object[] { request.Id }, cancellationToken);
            if (role == null) return TResult<UpdateRoleResult>.Failure(MessageKey.RoleNotFound);

            // Chặn đổi Priority của Role hệ thống để bảo vệ logic phân cấp
            if (role.IsSystemRole && request.Priority != role.Priority)
                return TResult<UpdateRoleResult>.Failure(MessageKey.InsufficientPermissions, ErrorCodes.BAD_REQUEST);

            role.Name = request.Name;
            role.Description = request.Description;
            role.Priority = request.Priority;

            await _context.SaveChangesAsync(cancellationToken);
            return TResult<UpdateRoleResult>.Success(new UpdateRoleResult(role.Id, role.Name, role.Priority));
        }
    }
}
