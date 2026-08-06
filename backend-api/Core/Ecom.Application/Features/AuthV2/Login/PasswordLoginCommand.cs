using Ecom.Application.Features.AuthV2;
using Ecom.Domain.Enums;
using Ecom.Domain.Entities;
using Ecom.Application.Common.Configuration;

namespace Ecom.Application.Features.AuthV2.Login;
public sealed record PasswordLoginCommand(string Identifier,string Password,string? DeviceId,bool RememberMe) : IRequest<TResult<PasswordLoginResult>>
{ public string IpFingerprint { get; init; }="not-captured"; public string UserAgentSummary { get; init; }="not-captured"; }
public sealed record PasswordLoginResult(Guid SessionId,string AccessToken,string RefreshToken,DateTime AccessTokenExpiresAt,DateTime RefreshTokenExpiresAt);
public sealed class PasswordLoginCommandValidator : AbstractValidator<PasswordLoginCommand>
{ public PasswordLoginCommandValidator(){RuleFor(x=>x.Identifier).NotEmpty().MaximumLength(255);RuleFor(x=>x.Password).NotEmpty().MaximumLength(128);RuleFor(x=>x.DeviceId).MaximumLength(200);} }
public sealed class PasswordLoginCommandHandler(IUnitOfWork uow,IPasswordHasher hasher,
 IAuthRateLimitService rateLimiter,IAuthTokenProtector protector,ISender sender)
 : IRequestHandler<PasswordLoginCommand,TResult<PasswordLoginResult>>
{
 private const string DummyHash="$2a$12$C6UzMDM.H6dfI/f/IKcEe.ouHjFm7kJqgB8NJuBKPvS0oFQqJmM9S";
 public async Task<TResult<PasswordLoginResult>> Handle(PasswordLoginCommand r,CancellationToken ct)
 {
  var key=r.Identifier.Trim().ToUpperInvariant();
  foreach(var partition in new[]{(AuthRateLimitPolicyNames.PasswordLoginAccount,protector.Protect(key)),(AuthRateLimitPolicyNames.PasswordLoginDevice,protector.Protect(r.DeviceId??"unknown"))})
  { var limit=await rateLimiter.AcquireAsync(partition.Item1,partition.Item2,ct); if(limit.Status==AuthRateLimitStatus.Unavailable)return TResult<PasswordLoginResult>.Failure(MessageKey.AuthDependencyUnavailable,ErrorCodes.SERVICE_UNAVAILABLE);if(limit.Status==AuthRateLimitStatus.Rejected)return TResult<PasswordLoginResult>.Failure(MessageKey.TooManyRequests,ErrorCodes.TOO_MANY_REQUESTS); }
  var user=await uow.Repository<User>().FindOneAsync(r.Identifier.Contains('@')?[u=>u.NormalizedEmail==key]:[u=>u.NormalizedUsername==key]);
  var credential=user is null?null:await uow.Repository<PasswordCredential>().FindOneAsync([x=>x.UserId==user.Id]);
  var valid=hasher.VerifyPassword(r.Password,credential?.PasswordHash??DummyHash); var now=DateTime.UtcNow;
  if(!valid || user is null || credential is null || credential.LockedUntil>now || user.Status!=UserStatusEnum.Active)
  {
   await sender.Send(new RecordFailedPasswordLoginCommand(user?.Id,credential?.Id,r.IpFingerprint,r.UserAgentSummary,now),ct);
   return TResult<PasswordLoginResult>.Failure(MessageKey.AuthenticationFailed,ErrorCodes.UNAUTHORIZED);
  }
  return await sender.Send(new CompletePasswordLoginCommand(user.Id,credential.Id,r.Password,r.DeviceId,r.RememberMe,
   r.IpFingerprint,r.UserAgentSummary,now),ct);
 }
}
