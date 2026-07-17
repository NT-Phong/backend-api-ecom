using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Auth.Commands.RoleManagement.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, TResult<CreateRoleResult>>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<TResult<CreateRoleResult>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
            return TResult<CreateRoleResult>.Failure(MessageKey.RoleAlreadyExists);

        var role = new Role
        {
            Name = request.Name,
            Code = request.Name.ToUpper().Replace(" ", "_"),
            Description = request.Description,
            Priority = request.Priority,
            IsSystemRole = false
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        // Lấy tất cả các policies hiện có
        var allPolicies = await _context.Policies.ToListAsync(cancellationToken);

        // Tạo RolePolicy (isDeleted = true) cho tất cả policies của role mới
        if (allPolicies.Any())
        {
            var rolePolicies = allPolicies.Select(p => new RolePolicy
            {
                RoleId = role.Id,
                PolicyId = p.Id,
                IsDeleted = true // Mặc định chưa được cấp quyền
            });

            _context.RolePolicies.AddRange(rolePolicies);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return TResult<CreateRoleResult>.Success(new CreateRoleResult(role.Id, role.Name, role.Code, role.Priority));
    }
}
