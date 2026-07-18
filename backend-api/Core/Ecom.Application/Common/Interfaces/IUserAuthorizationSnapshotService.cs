using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

public interface IUserAuthorizationSnapshotService
{
    Task<IReadOnlyCollection<string>> ResolvePoliciesAsync(User user, CancellationToken ct);
}
