using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class MediaAccessService(ICurrentUser currentUser) : IMediaAccessService
{
    public TResult EnsureOwnerOrManager(MediaAsset mediaAsset)
    {
        if (!currentUser.IsAuthenticated)
            return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        if (currentUser.HasPolicy(Permissions.Media.Manage) ||
            (mediaAsset.CreatedBy.HasValue && mediaAsset.CreatedBy.Value == currentUser.UserId))
            return TResult.Success();

        return TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
    }
}
