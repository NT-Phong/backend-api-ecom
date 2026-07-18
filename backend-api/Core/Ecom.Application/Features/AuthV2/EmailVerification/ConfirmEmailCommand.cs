using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
namespace Ecom.Application.Features.AuthV2.EmailVerification;
[EnableUnitOfWork]
public sealed record ConfirmEmailCommand(string Token) : IRequest<TResult>;
public sealed class ConfirmEmailCommandHandler(IUnitOfWork uow,IAuthTokenProtector protector) : IRequestHandler<ConfirmEmailCommand,TResult>
{
 public async Task<TResult> Handle(ConfirmEmailCommand r,CancellationToken ct)
 {
  var hash=protector.Protect(r.Token); var now=DateTime.UtcNow;
  var c=await uow.Repository<VerificationChallenge>().FindOneAsync([x=>x.SecretHash==hash && x.Purpose==VerificationChallengePurpose.EmailVerification],"CreatedAt desc");
  if(c is null || !c.IsUsable(now)) return TResult.Failure(MessageKey.AuthenticationFailed,ErrorCodes.UNAUTHORIZED);
  var user=c.UserId.HasValue?await uow.Repository<User>().FindByIdAsync(c.UserId.Value):null;
  if(user is null) return TResult.Failure(MessageKey.AuthenticationFailed,ErrorCodes.UNAUTHORIZED);
  c.Consume(now); user.MarkEmailVerified(now); user.Activate();
  await uow.Repository<VerificationChallenge>().UpdateAsync(c,ct); await uow.Repository<User>().UpdateAsync(user,ct);
  await uow.Repository<SecurityEvent>().InsertAsync(new SecurityEvent(user.Id,null,"EmailVerified",SecurityRiskLevel.Low,true,"not-captured","not-captured",null,now),ct);
  return TResult.Success();
 }
}
