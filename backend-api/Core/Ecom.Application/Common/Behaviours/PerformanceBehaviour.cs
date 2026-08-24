using System.Diagnostics;

namespace Ecom.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger, ICurrentUser currentUser)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var actorAuthenticated = _currentUser.IsAuthenticated;
            var actorResolved = actorAuthenticated && _currentUser.UserId != Guid.Empty;
            var actorKind = actorResolved ? "Authenticated" : actorAuthenticated ? "AuthenticatedMissingUserId" : "Anonymous";
            _logger.LogWarning("Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds); ActorKind: {ActorKind}; ActorAuthenticated: {ActorAuthenticated}; ActorResolved: {ActorResolved}",
                requestName, elapsedMilliseconds, actorKind, actorAuthenticated, actorResolved);
        }
        return response;
    }
} 
