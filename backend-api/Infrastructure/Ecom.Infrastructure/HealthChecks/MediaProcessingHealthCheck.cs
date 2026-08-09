using System.Net.Sockets;
using System.Text;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecom.Infrastructure.HealthChecks;

public sealed class MediaProcessingHealthCheck(
    IOptions<MediaProcessingOptions> processing,
    IServiceScopeFactory scopeFactory,
    ILogger<MediaProcessingHealthCheck> logger) : IHealthCheck
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var options = processing.Value;
        if (!options.Enabled && !options.DirectPublicUploadEnabled)
            return HealthCheckResult.Healthy("Media processing is disabled.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ProbeTimeout);
        try
        {
            using var scope = scopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
            await storage.EnsureReadyAsync(timeout.Token);

            if (options.DirectPublicUploadEnabled)
                return HealthCheckResult.Healthy("Direct public media uploads are enabled; scanner readiness is bypassed.");

            if (string.IsNullOrWhiteSpace(options.ClamAvHost) || options.ClamAvPort is <= 0 or > 65535)
                return HealthCheckResult.Unhealthy("Media scanner configuration is invalid.");

            using var client = new TcpClient();
            await client.ConnectAsync(options.ClamAvHost, options.ClamAvPort, timeout.Token);
            await using var stream = client.GetStream();
            await stream.WriteAsync(Encoding.ASCII.GetBytes("zPING\0"), timeout.Token);
            await stream.FlushAsync(timeout.Token);

            var response = new byte[32];
            var read = await stream.ReadAsync(response, timeout.Token);
            var reply = Encoding.ASCII.GetString(response, 0, read);
            if (!reply.Contains("PONG", StringComparison.Ordinal))
                return HealthCheckResult.Unhealthy("Media scanner returned an invalid readiness response.");

            return HealthCheckResult.Healthy("Media storage and scanner are reachable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Media processing readiness check failed.");
            return HealthCheckResult.Unhealthy("Media storage or scanner is unavailable.");
        }
    }
}
