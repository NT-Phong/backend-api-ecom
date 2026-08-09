using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Hosting;

namespace Ecom.Infrastructure.Services;

public sealed class MediaStorageStartupValidator(IHostEnvironment environment,
    IOptions<MediaStorageOptions> storage,
    IOptions<MediaProcessingOptions> processing,
    IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsProduction()) return;
        if (!string.Equals(storage.Value.Provider, "Azure", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(storage.Value.AccountUrl))
            throw new InvalidOperationException("Production media storage requires Azure with Managed Identity AccountUrl.");
        if (!processing.Value.DirectPublicUploadEnabled &&
            (!processing.Value.Enabled || string.IsNullOrWhiteSpace(processing.Value.ClamAvHost)))
            throw new InvalidOperationException("Production media processing requires an enabled ClamAV worker.");
        using var scope = scopeFactory.CreateScope();
        var mediaStorage = scope.ServiceProvider.GetRequiredService<IStorageService>();
        await mediaStorage.EnsureReadyAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
