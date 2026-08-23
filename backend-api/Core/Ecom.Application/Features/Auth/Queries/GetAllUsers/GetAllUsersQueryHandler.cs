using Ecom.Application.Features.Auth.Queries.GetCurrentUser;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Auth.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, TResult<PaginatedList<CurrentUserResult>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<TResult<PaginatedList<CurrentUserResult>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // 1. Get current user
        var currentUser = await _unitOfWork.Repository<User>()
            .FindByIdAsync(_currentUser.UserId, u => u.Role!);

        if (currentUser == null)
            return TResult<PaginatedList<CurrentUserResult>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var roleCode = currentUser.Role?.Code;

        // 2. Check permission
        var allowedRoles = new[]
        {
            Permissions.Roles.Admin
        };

        if (!allowedRoles.Contains(roleCode))
            return TResult<PaginatedList<CurrentUserResult>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);

        // 3. Base Query
        IQueryable<User> query = _unitOfWork.Repository<User>().QueryNoTracking();

        // 4. Data permission: only Admin can see all users
        if (roleCode != Permissions.Roles.Admin)
        {
            query = query.Where(u => u.Role != null && u.Role.Code != Permissions.Roles.Admin);
        }

        // 5. Filter Request
        if (request.UserId.HasValue)
        {
            query = query.Where(u => u.Id == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.ToLower();

            query = query.Where(u =>
                (u.PhoneNumber ?? string.Empty).Contains(search) ||
                (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        // 6. Count
        var totalCount = await query.CountAsync(cancellationToken);

        // 7. Get Data
        var users = await query
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Skip(request.Skip())
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // 8. Map DTO
        var results = users.Select(u => new CurrentUserResult
        {
            UserId = u.Id,
            PhoneNumber = u.PhoneNumber ?? string.Empty,
            FullName = u.FullName,
            Email = u.Email,
            Status = u.Status.ToString(),
            RoleCode = u.Role?.Code,
            RoleName = u.Role?.Name,
            RoleId = u.RoleId,
            LastLoginAt = u.LastLoginAt,
            PhoneNumberConfirmed = u.PhoneNumberConfirmed,
            EmailConfirmed = u.EmailConfirmed
        }).ToList();

        // 9. Return
        return TResult<PaginatedList<CurrentUserResult>>.Success(
            PaginatedList<CurrentUserResult>.Create(
                results,
                totalCount,
                request.Page,
                request.PageSize));
    }
}
