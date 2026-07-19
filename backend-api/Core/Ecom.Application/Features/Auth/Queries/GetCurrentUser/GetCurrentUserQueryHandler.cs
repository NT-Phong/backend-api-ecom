using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, TResult<CurrentUserResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResult<CurrentUserResult>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty)
                return TResult<CurrentUserResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

            var user = await _unitOfWork.Repository<User>()
                .FindByIdAsync(userId, u => u.Role!);

            if (user == null)
                return TResult<CurrentUserResult>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);

            // ================= POLICIES =================
            var policies = await GetUserPoliciesAsync(user);

            return TResult<CurrentUserResult>.Success(new CurrentUserResult
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Email = user.Email,
                FullName = user.FullName,
                AvatarId = user.AvatarId,
                Status = user.Status.ToString(),
                RoleCode = user.Role?.Code,
                RoleName = user.Role?.Name,
                RoleId = user.RoleId,
                Policies = policies.ToList(),
                LastLoginAt = user.LastLoginAt,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                EmailConfirmed = user.EmailConfirmed,
                CanSkipProfile = string.IsNullOrWhiteSpace(user.FullName),
                ProfileState = string.IsNullOrWhiteSpace(user.FullName) ? "BASIC_PROFILE_MISSING" : "READY"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return TResult<CurrentUserResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
        }
    }

    private async Task<IEnumerable<string>> GetUserPoliciesAsync(User user)
    {
        var policies = new HashSet<string>();

        if (user.RoleId.HasValue)
        {
            var rolePolicies = await _unitOfWork.Repository<RolePolicy>()
                .FindAsync(
                    filters: [rp => rp.RoleId == user.RoleId.Value],
                    includes: [rp => rp.Policy!]);

            foreach (var rp in rolePolicies.Where(x => !x.IsDeleted && x.Policy != null && x.Policy.IsActive))
            {
                policies.Add(rp.Policy!.Code);
            }
        }

        var userPolicies = await _unitOfWork.Repository<UserPolicy>()
            .FindAsync(
                filters: [
                    up => up.UserId == user.Id,
                    up => up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow
                ],
                includes: [up => up.Policy!]);

        foreach (var up in userPolicies.Where(x => x.Policy != null && x.Policy.IsActive))
        {
            if (up.IsGranted)
                policies.Add(up.Policy!.Code);
            else
                policies.Remove(up.Policy!.Code);
        }

        return policies;
    }
}
