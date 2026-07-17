namespace Ecom.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthorizationBehaviour<TRequest, TResponse>> _logger;

    public AuthorizationBehaviour(ICurrentUser currentUser, ILogger<AuthorizationBehaviour<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            if (!_currentUser.IsAuthenticated)
            {
                _logger.LogWarning("Authentication required for {RequestType}", typeof(TRequest).Name);
                throw new UnauthorizedAccessException("Authentication required");
            }

            var requiredRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)).SelectMany(a => a.Roles!.Split(',')).Select(r => r.Trim()).Distinct();
            if (requiredRoles.Any() && !requiredRoles.All(_currentUser.HasRole))
            {
                _logger.LogWarning("Access denied for user {UserId} to {RequestType}. Required roles: {Roles}", _currentUser.UserId, typeof(TRequest).Name, string.Join(", ", requiredRoles));
                throw new ForbiddenAccessException("Insufficient permissions");
            }
        }

        return await next();
    }
} 
