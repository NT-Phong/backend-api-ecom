namespace Ecom.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var actorAuthenticated = _currentUser.IsAuthenticated;
        var actorResolved = actorAuthenticated && _currentUser.UserId != Guid.Empty;
        var actorKind = actorResolved ? "Authenticated" : actorAuthenticated ? "AuthenticatedMissingUserId" : "Anonymous";
        var correlationId = Guid.NewGuid().ToString();
        var requestType = typeof(TRequest).Name.EndsWith("Command") ? "Command" : "Query";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestName"] = requestName,
            ["RequestType"] = requestType,
            ["ActorKind"] = actorKind,
            ["ActorAuthenticated"] = actorAuthenticated,
            ["ActorResolved"] = actorResolved
        }))
        {
            // BEGIN log with structured properties
            _logger.LogInformation("[BEGIN] Executing {RequestType} {RequestName} | ActorKind: {ActorKind}; ActorAuthenticated: {ActorAuthenticated}; ActorResolved: {ActorResolved}; CorrelationId: {CorrelationId}",
                requestType, requestName, actorKind, actorAuthenticated, actorResolved, correlationId);
            
            try
            {
                var response = await next();
                
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;
                var success = response is not IResult result || result.IsSuccess;

                _logger.LogInformation("[END] {RequestType} {RequestName} completed in {Duration}ms | Success: {Success}; ActorKind: {ActorKind}; ActorAuthenticated: {ActorAuthenticated}; ActorResolved: {ActorResolved}; CorrelationId: {CorrelationId}",
                    requestType, requestName, duration, success, actorKind, actorAuthenticated, actorResolved, correlationId);
                    
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;
                var success = false;
                
                _logger.LogError("[END] {RequestType} {RequestName} failed in {Duration}ms | Success: {Success}; ActorKind: {ActorKind}; ActorAuthenticated: {ActorAuthenticated}; ActorResolved: {ActorResolved}; ExceptionType: {ExceptionType}; CorrelationId: {CorrelationId}",
                    requestType, requestName, duration, success, actorKind, actorAuthenticated, actorResolved, ex.GetType().Name, correlationId);
                    
                throw;
            }
        }
    }

}

