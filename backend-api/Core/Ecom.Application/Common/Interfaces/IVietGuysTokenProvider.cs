namespace Ecom.Application.Common.Interfaces;

/// <summary>Provider-specific contract kept isolated until SMS integration is configured for Ecom.</summary>
public interface IVietGuysTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<string> ForceRefreshAsync(CancellationToken cancellationToken = default);
}
