using Ecom.Application.Features.AuthV2;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;

namespace Ecom.Application.Features.AuthV2.Login;

public sealed record CompletePasswordLoginCommand(
    Guid UserId,
    Guid CredentialId,
    string Password,
    string? DeviceId,
    bool RememberMe,
    string IpFingerprint,
    string UserAgentSummary,
    DateTime OccurredAt) : IRequest<TResult<PasswordLoginResult>>, ITransactionalRequest;

public sealed class CompletePasswordLoginCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher hasher,
    IAuthenticationSessionEngine sessions)
    : IRequestHandler<CompletePasswordLoginCommand, TResult<PasswordLoginResult>>
{
    public async Task<TResult<PasswordLoginResult>> Handle(
        CompletePasswordLoginCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await unitOfWork.Repository<PasswordCredential>().FindByIdAsync(request.CredentialId);
        if (credential is null || credential.UserId != request.UserId)
            return TResult<PasswordLoginResult>.Failure(MessageKey.AuthenticationFailed, ErrorCodes.UNAUTHORIZED);

        credential.RecordSuccess(request.OccurredAt);
        if (hasher.NeedsRehash(credential.PasswordHash))
            credential.SetHash(hasher.HashPassword(request.Password), request.OccurredAt);
        await unitOfWork.Repository<PasswordCredential>().UpdateAsync(credential, cancellationToken);

        var created = await sessions.CreateAsync(new VerifiedAuthenticationContext(
            request.UserId,
            AuthenticationMethod.Password,
            SessionClientType.Mobile,
            request.DeviceId,
            request.RememberMe,
            request.IpFingerprint,
            request.UserAgentSummary), cancellationToken);
        if (!created.IsSuccess)
            return TResult<PasswordLoginResult>.Failure(
                created.Error ?? MessageKey.AuthenticationFailed,
                created.ErrorCode);

        var session = created.Data;
        return TResult<PasswordLoginResult>.Success(new PasswordLoginResult(
            session.SessionId,
            session.AccessToken!,
            session.RefreshToken!,
            session.AccessTokenExpiresAt,
            session.RefreshTokenExpiresAt));
    }
}
