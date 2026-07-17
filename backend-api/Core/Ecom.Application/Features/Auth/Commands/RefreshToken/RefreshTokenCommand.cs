namespace Ecom.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<TResult<RefreshTokenResult>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm Access Token hết hạn (nên trả về dưới dạng UTC hoặc Unix Timestamp)
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// Thời điểm Refresh Token hết hạn
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
}
