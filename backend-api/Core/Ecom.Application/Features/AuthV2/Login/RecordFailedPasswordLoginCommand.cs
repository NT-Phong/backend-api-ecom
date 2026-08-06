using Ecom.Application.Features.AuthV2;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;

namespace Ecom.Application.Features.AuthV2.Login;

public sealed record RecordFailedPasswordLoginCommand(
    Guid? UserId,
    Guid? CredentialId,
    string IpFingerprint,
    string UserAgentSummary,
    DateTime OccurredAt) : IRequest<TResult>, ITransactionalRequest;

public sealed class RecordFailedPasswordLoginCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RecordFailedPasswordLoginCommand, TResult>
{
    public async Task<TResult> Handle(RecordFailedPasswordLoginCommand request, CancellationToken cancellationToken)
    {
        if (request.CredentialId.HasValue)
        {
            var credential = await unitOfWork.Repository<PasswordCredential>().FindByIdAsync(request.CredentialId.Value);
            if (credential is not null)
            {
                credential.RecordFailure(request.OccurredAt);
                await unitOfWork.Repository<PasswordCredential>().UpdateAsync(credential, cancellationToken);
            }
        }

        await unitOfWork.Repository<SecurityEvent>().InsertAsync(new SecurityEvent(
            request.UserId,
            null,
            "LoginFailed",
            SecurityRiskLevel.Medium,
            false,
            request.IpFingerprint,
            request.UserAgentSummary,
            null,
            request.OccurredAt), cancellationToken);

        return TResult.Success();
    }
}
