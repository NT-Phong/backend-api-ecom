using Ecom.Application.Features.AuthV2;
using Ecom.Domain.Enums;
using Ecom.Domain.Entities;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace Ecom.Application.Features.AuthV2.Register;
[EnableUnitOfWork]
public sealed record RegisterPasswordCommand(string Username, string Email, string Password) : IRequest<TResult<RegisterPasswordResult>>
{ public string IpFingerprint { get; init; } = "not-captured"; }
public sealed record RegisterPasswordResult(string Status,
 [property: JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] string? DevelopmentVerificationToken = null);
public sealed class RegisterPasswordCommandValidator : AbstractValidator<RegisterPasswordCommand>
{
 public RegisterPasswordCommandValidator(IOptions<PasswordSettings> settings) { var min=settings.Value.MinLength; RuleFor(x=>x.Username).Must(PasswordRules.ValidUsername); RuleFor(x=>x.Email).NotEmpty().EmailAddress().MaximumLength(255); RuleFor(x=>x.Password).Must(x=>PasswordRules.ValidPassword(x,min)); }
}
public sealed class RegisterPasswordCommandHandler(IUnitOfWork uow, IPasswordHasher hasher, IAuthTokenProtector protector,
 IOptions<PasswordAuthenticationV2Options> options, IHostEnvironment environment, IAuthRateLimitService rateLimiter)
 : IRequestHandler<RegisterPasswordCommand,TResult<RegisterPasswordResult>>
{
 public async Task<TResult<RegisterPasswordResult>> Handle(RegisterPasswordCommand r, CancellationToken ct)
 {
  var un=r.Username.Trim().ToUpperInvariant(); var email=r.Email.Trim().ToUpperInvariant();
  var limit=await rateLimiter.AcquireAsync(AuthRateLimitPolicyNames.RegisterDestinationDaily,protector.Protect(email),ct);
  if(limit.Status==AuthRateLimitStatus.Unavailable) return TResult<RegisterPasswordResult>.Failure(MessageKey.AuthDependencyUnavailable,ErrorCodes.SERVICE_UNAVAILABLE);
  if(limit.Status==AuthRateLimitStatus.Rejected) return TResult<RegisterPasswordResult>.Failure(MessageKey.TooManyRequests,ErrorCodes.TOO_MANY_REQUESTS);
  var exists=await uow.Repository<User>().AnyAsync([u=>u.NormalizedUsername==un || u.NormalizedEmail==email]);
  if(exists) return TResult<RegisterPasswordResult>.Success(new("Accepted"));
  var role=await uow.Repository<Role>().FindOneAsync([x=>x.Code==Permissions.Roles.User]);
  var user=new User(null,role?.Id); user.SetUsername(r.Username); user.SetEmail(r.Email);
  await uow.Repository<User>().InsertAsync(user,ct);
  await uow.Repository<PasswordCredential>().InsertAsync(new PasswordCredential(user.Id,hasher.HashPassword(r.Password),DateTime.UtcNow),ct);
  var raw=Base64Url(RandomNumberGenerator.GetBytes(32)); var now=DateTime.UtcNow;
  await uow.Repository<VerificationChallenge>().InsertAsync(new VerificationChallenge(user.Id,VerificationChallengePurpose.EmailVerification,
    protector.Protect(email),protector.Protect(raw),5,now.AddMinutes(30),r.IpFingerprint),ct);
  await uow.Repository<SecurityEvent>().InsertAsync(new SecurityEvent(user.Id,null,"RegistrationAccepted",SecurityRiskLevel.Low,true,"not-captured","not-captured",null,now),ct);
  var expose=environment.IsDevelopment()&&options.Value.ExposeDevelopmentVerificationToken;
  return TResult<RegisterPasswordResult>.Success(new("Accepted",expose?raw:null));
 }
 private static string Base64Url(byte[] b)=>Convert.ToBase64String(b).TrimEnd('=').Replace('+','-').Replace('/','_');
}
