using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Demo.QrLogin;

public enum DemoQrLoginStatus
{
    Pending,
    Approved,
    Rejected,
    Expired
}

public sealed record DemoQrLoginAttempt(
    Guid Id,
    DemoQrLoginStatus Status,
    DateTime ExpiresAt,
    Guid? ApprovedUserId,
    DateTime? ApprovedAt);

public enum DemoQrLoginTransitionResult
{
    Updated,
    MissingOrExpired,
    AlreadyCompleted,
    Busy
}

public sealed record StartDemoQrLoginCommand : IRequest<TResult<StartDemoQrLoginResult>>;

public sealed record StartDemoQrLoginResult(
    Guid Id,
    string ApprovalPath,
    DateTime ExpiresAt,
    int PollIntervalMilliseconds);

public sealed class StartDemoQrLoginCommandHandler(
    IDemoQrLoginStore store,
    IOptions<DemoQrLoginOptions> options)
    : IRequestHandler<StartDemoQrLoginCommand, TResult<StartDemoQrLoginResult>>
{
    public async Task<TResult<StartDemoQrLoginResult>> Handle(StartDemoQrLoginCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(options.Value.TtlSeconds);
        var attempt = new DemoQrLoginAttempt(Guid.NewGuid(), DemoQrLoginStatus.Pending, expiresAt, null, null);

        try
        {
            await store.CreateAsync(attempt, cancellationToken);
        }
        catch (DemoQrLoginStoreUnavailableException)
        {
            return TResult<StartDemoQrLoginResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
        }

        return TResult<StartDemoQrLoginResult>.Success(new StartDemoQrLoginResult(
            attempt.Id,
            $"/api/v1/demo/qr-login/{attempt.Id}/approval-page",
            expiresAt,
            options.Value.PollIntervalMilliseconds));
    }
}

public sealed record GetDemoQrLoginStatusQuery(Guid Id) : IRequest<TResult<DemoQrLoginStatusResult>>;

public sealed record DemoQrLoginStatusResult(DemoQrLoginStatus Status, DateTime? ExpiresAt);

public sealed class GetDemoQrLoginStatusQueryHandler(IDemoQrLoginStore store)
    : IRequestHandler<GetDemoQrLoginStatusQuery, TResult<DemoQrLoginStatusResult>>
{
    public async Task<TResult<DemoQrLoginStatusResult>> Handle(GetDemoQrLoginStatusQuery request, CancellationToken cancellationToken)
    {
        DemoQrLoginAttempt? attempt;
        try
        {
            attempt = await store.GetAsync(request.Id, cancellationToken);
        }
        catch (DemoQrLoginStoreUnavailableException)
        {
            return TResult<DemoQrLoginStatusResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
        }
        if (attempt is null || attempt.ExpiresAt <= DateTime.UtcNow)
            return TResult<DemoQrLoginStatusResult>.Success(new(DemoQrLoginStatus.Expired, null));

        return TResult<DemoQrLoginStatusResult>.Success(new(attempt.Status, attempt.ExpiresAt));
    }
}

public sealed record ApproveDemoQrLoginCommand(Guid Id) : IRequest<TResult<DemoQrLoginStatusResult>>;
public sealed record RejectDemoQrLoginCommand(Guid Id) : IRequest<TResult<DemoQrLoginStatusResult>>;

public abstract class DemoQrLoginTransitionHandlerBase(
    IDemoQrLoginStore store,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
{
    protected async Task<TResult<DemoQrLoginStatusResult>> TransitionAsync(
        Guid id,
        DemoQrLoginStatus targetStatus,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
            return TResult<DemoQrLoginStatusResult>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);

        var user = await unitOfWork.Repository<User>().FindByIdAsync(currentUser.UserId);
        if (user is null || user.Status != UserStatusEnum.Active)
            return TResult<DemoQrLoginStatusResult>.Failure(MessageKey.AuthenticationFailed, ErrorCodes.UNAUTHORIZED);

        DemoQrLoginTransitionResult result;
        try
        {
            result = await store.TryTransitionAsync(id, targetStatus, user.Id, DateTime.UtcNow, cancellationToken);
        }
        catch (DemoQrLoginStoreUnavailableException)
        {
            return TResult<DemoQrLoginStatusResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
        }
        return result switch
        {
            DemoQrLoginTransitionResult.Updated => TResult<DemoQrLoginStatusResult>.Success(new(targetStatus, null)),
            DemoQrLoginTransitionResult.MissingOrExpired => TResult<DemoQrLoginStatusResult>.Success(new(DemoQrLoginStatus.Expired, null)),
            DemoQrLoginTransitionResult.AlreadyCompleted => TResult<DemoQrLoginStatusResult>.Failure("Demo QR request is already completed.", ErrorCodes.ALREADY_EXISTS),
            _ => TResult<DemoQrLoginStatusResult>.Failure("Demo QR request is busy. Please try again.", ErrorCodes.ALREADY_EXISTS)
        };
    }
}

public sealed class ApproveDemoQrLoginCommandHandler(
    IDemoQrLoginStore store,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : DemoQrLoginTransitionHandlerBase(store, currentUser, unitOfWork),
        IRequestHandler<ApproveDemoQrLoginCommand, TResult<DemoQrLoginStatusResult>>
{
    public Task<TResult<DemoQrLoginStatusResult>> Handle(ApproveDemoQrLoginCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.Id, DemoQrLoginStatus.Approved, cancellationToken);
}

public sealed class RejectDemoQrLoginCommandHandler(
    IDemoQrLoginStore store,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : DemoQrLoginTransitionHandlerBase(store, currentUser, unitOfWork),
        IRequestHandler<RejectDemoQrLoginCommand, TResult<DemoQrLoginStatusResult>>
{
    public Task<TResult<DemoQrLoginStatusResult>> Handle(RejectDemoQrLoginCommand request, CancellationToken cancellationToken) =>
        TransitionAsync(request.Id, DemoQrLoginStatus.Rejected, cancellationToken);
}
