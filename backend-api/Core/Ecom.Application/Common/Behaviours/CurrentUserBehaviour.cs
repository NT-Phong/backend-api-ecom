namespace Ecom.Application.Common.Behaviours;

public class CurrentUserBehaviour<TRequest, TResponse>(
    ICurrentUser currentUserProvider 
) : IPipelineBehavior<TRequest, TResponse> where TRequest : IUserRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (currentUserProvider.IsAuthenticated && request.UserId == Guid.Empty)
        {
            request.UserId = currentUserProvider.UserId;
        }

        return await next();
    }
}
