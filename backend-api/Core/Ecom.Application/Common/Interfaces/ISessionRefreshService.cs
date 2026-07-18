namespace Ecom.Application.Common.Interfaces;
public sealed record SessionRefreshResult(string AccessToken,string RefreshToken,DateTime AccessTokenExpiresAt,DateTime RefreshTokenExpiresAt);
public interface ISessionRefreshService { Task<TResult<SessionRefreshResult>> RotateAsync(string refreshToken,CancellationToken ct); }
