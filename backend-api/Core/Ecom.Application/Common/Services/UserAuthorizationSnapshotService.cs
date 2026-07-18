using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class UserAuthorizationSnapshotService(IUnitOfWork unitOfWork) : IUserAuthorizationSnapshotService
{
    public async Task<IReadOnlyCollection<string>> ResolvePoliciesAsync(User user, CancellationToken ct)
    {
        var policies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (user.RoleId.HasValue)
        {
            var rolePolicies = await unitOfWork.Repository<RolePolicy>().FindAsync(
                filters: [x => x.RoleId == user.RoleId.Value, x => !x.IsDeleted], includes: [x => x.Policy!]);
            foreach (var item in rolePolicies.Where(x => x.Policy is { IsActive: true })) policies.Add(item.Policy!.Code);
        }
        var overrides = await unitOfWork.Repository<UserPolicy>().FindAsync(
            filters: [x => x.UserId == user.Id, x => x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow],
            includes: [x => x.Policy!]);
        foreach (var item in overrides.Where(x => x.Policy is { IsActive: true }))
            if (item.IsGranted) policies.Add(item.Policy!.Code); else policies.Remove(item.Policy!.Code);
        return policies;
    }
}
