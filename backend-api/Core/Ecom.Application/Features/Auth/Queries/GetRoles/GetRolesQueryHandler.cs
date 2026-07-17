namespace Ecom.Application.Features.Auth.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, TResult<List<RoleResult>>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TResult<List<RoleResult>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        // Lấy danh sách Role, sắp xếp theo Priority (Số nhỏ lên trước - quyền cao lên trước)
        var roles = await _context.Roles
            .AsNoTracking()
            .Where(r => r.Code != Permissions.Roles.Admin)
            .OrderBy(r => r.Priority)
            .Select(r => new RoleResult
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Description = r.Description,
                Priority = r.Priority,
                IsSystemRole = r.IsSystemRole
            })
            .ToListAsync(cancellationToken);

        // Trả về kết quả bọc trong TResult
        return TResult<List<RoleResult>>.Success(roles);
    }
}
