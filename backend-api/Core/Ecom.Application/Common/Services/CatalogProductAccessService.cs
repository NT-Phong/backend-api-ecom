namespace Ecom.Application.Common.Services;

public sealed class CatalogProductAccessService(ICurrentUser currentUser) : ICatalogProductAccessService
{
    public TResult Ensure(string permission)
    {
        if (!currentUser.IsAuthenticated)
            return TResult.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        return currentUser.HasPolicy(permission)
            ? TResult.Success()
            : TResult.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
    }
}
