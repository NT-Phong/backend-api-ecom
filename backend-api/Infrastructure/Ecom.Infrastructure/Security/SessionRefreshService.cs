using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Ecom.Application.Common.Configuration;

namespace Ecom.Infrastructure.Security;
public sealed class SessionRefreshService(ApplicationDbContext db,IAuthTokenProtector protector,IJwtTokenService jwt,
 IUserAuthorizationSnapshotService authorization,IAuthRateLimitService rateLimiter):ISessionRefreshService
{
 public async Task<TResult<SessionRefreshResult>> RotateAsync(string raw,CancellationToken ct)
 {
  var hash=protector.Protect(raw);var now=DateTime.UtcNow;
  var snapshot=await db.SessionRefreshTokens.AsNoTracking().FirstOrDefaultAsync(x=>x.TokenHash==hash,ct);
  if(snapshot is null)return TResult<SessionRefreshResult>.Failure(MessageKey.AuthenticationFailed,ErrorCodes.UNAUTHORIZED);
  var limit=await rateLimiter.AcquireAsync(AuthRateLimitPolicyNames.RefreshSession,snapshot.SessionId.ToString("N"),ct);
  if(limit.Status==AuthRateLimitStatus.Unavailable)return TResult<SessionRefreshResult>.Failure(MessageKey.AuthDependencyUnavailable,ErrorCodes.SERVICE_UNAVAILABLE);
  if(limit.Status==AuthRateLimitStatus.Rejected)return TResult<SessionRefreshResult>.Failure(MessageKey.TooManyRequests,ErrorCodes.TOO_MANY_REQUESTS);
  await using var tx=await db.Database.BeginTransactionAsync(ct);
  var claimed=await db.SessionRefreshTokens.Where(x=>x.Id==snapshot.Id&&x.UsedAt==null&&x.RevokedAt==null&&x.ExpiresAt>now)
   .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.UsedAt,now),ct);
  if(claimed!=1)
  {
   await tx.RollbackAsync(ct);
   await using var replayTx=await db.Database.BeginTransactionAsync(ct);
   await db.SessionRefreshTokens.Where(x=>x.FamilyId==snapshot.FamilyId&&x.RevokedAt==null).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.RevokedAt,now),ct);
   await db.UserSessions.Where(x=>x.Id==snapshot.SessionId&&x.RevokedAt==null).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.RevokedAt,now).SetProperty(x=>x.RevocationReason,"RefreshTokenReuse"),ct);
   var sessionUser=await db.UserSessions.AsNoTracking().Where(x=>x.Id==snapshot.SessionId).Select(x=>x.UserId).FirstOrDefaultAsync(ct);
   db.SecurityEvents.Add(new SecurityEvent(sessionUser==Guid.Empty?null:sessionUser,snapshot.SessionId,"RefreshTokenReuseDetected",SecurityRiskLevel.High,false,"not-captured","not-captured",null,now));
   await db.SaveChangesAsync(ct);await replayTx.CommitAsync(ct);
   return TResult<SessionRefreshResult>.Failure(MessageKey.AuthenticationFailed,ErrorCodes.UNAUTHORIZED);
  }
  var session=await db.UserSessions.FirstOrDefaultAsync(x=>x.Id==snapshot.SessionId,ct);
  var user=session is null?null:await db.Users.Include(x=>x.Role).FirstOrDefaultAsync(x=>x.Id==session.UserId,ct);
  if(session is null||user is null||!session.IsActive(now,user.SecurityStamp)){await tx.RollbackAsync(ct);return TResult<SessionRefreshResult>.Failure(MessageKey.AuthenticationFailed,ErrorCodes.UNAUTHORIZED);}
  var newRaw=B64(RandomNumberGenerator.GetBytes(32));var replacement=new SessionRefreshToken(session.Id,snapshot.FamilyId,protector.Protect(newRaw),now,snapshot.ExpiresAt);
  db.SessionRefreshTokens.Add(replacement);await db.SaveChangesAsync(ct);
  await db.SessionRefreshTokens.Where(x=>x.Id==snapshot.Id).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.ReplacedByTokenId,replacement.Id),ct);
  db.SecurityEvents.Add(new SecurityEvent(user.Id,session.Id,"RefreshSucceeded",SecurityRiskLevel.Low,true,"not-captured","not-captured",null,now));await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
  var policies=await authorization.ResolvePoliciesAsync(user,ct);
  return TResult<SessionRefreshResult>.Success(new(jwt.GenerateAccessToken(user,policies,session.Id,user.SecurityStamp),newRaw,jwt.GetAccessTokenExpiration(),snapshot.ExpiresAt));
 }
 private static string B64(byte[]b)=>Convert.ToBase64String(b).TrimEnd('=').Replace('+','-').Replace('/','_');
}
