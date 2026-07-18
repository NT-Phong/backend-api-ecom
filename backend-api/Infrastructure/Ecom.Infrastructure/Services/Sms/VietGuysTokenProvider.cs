using Ecom.Application.Common.Interfaces;

namespace Ecom.Infrastructure.Services.Sms;

/// <summary>Provider adapter reserved for a future, explicitly configured VietGuys integration.</summary>
internal sealed class VietGuysTokenProvider : IVietGuysTokenProvider
{
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        Task.FromException<string>(new InvalidOperationException("VietGuys is not configured for Ecom."));

    public Task<string> ForceRefreshAsync(CancellationToken cancellationToken = default) =>
        Task.FromException<string>(new InvalidOperationException("VietGuys is not configured for Ecom."));
}
