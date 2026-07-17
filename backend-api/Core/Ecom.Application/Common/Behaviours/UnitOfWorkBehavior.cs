using Microsoft.EntityFrameworkCore;

namespace Ecom.Application.Common.Behaviours;

public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var hasLegacyAttribute = request.GetType().GetCustomAttributes(typeof(EnableUnitOfWorkAttribute), true).Any();
        if (request is not ITransactionalRequest && !hasLegacyAttribute)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var response = await next();

            if (response is IResult { IsSuccess: false })
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _unitOfWork.ClearChangeTracker();
                return response;
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for {RequestName}", requestName);
            return response;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict detected for {RequestName}. Rolling back transaction...", requestName);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _unitOfWork.ClearChangeTracker();
            throw new ConcurrencyConflictException();
        }
        catch (OperationCanceledException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            _unitOfWork.ClearChangeTracker();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for {RequestName}. Rolling back...", requestName);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _unitOfWork.ClearChangeTracker();
            throw;
        }
    }
}

