using System.Net;
using System.Net.Sockets;
using System.Text;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.Infrastructure;

public sealed class MediaProcessingHealthCheckTests
{
    [Fact]
    public async Task Disabled_processing_is_healthy_without_probing_dependencies()
    {
        using var provider = BuildProvider(new TestStorage(() => throw new InvalidOperationException("Not called.")));
        var check = CreateCheck(new MediaProcessingOptions { Enabled = false }, provider);

        var result = await CheckAsync(check);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Storage_failure_is_unhealthy()
    {
        using var provider = BuildProvider(new TestStorage(() => throw new IOException("Storage unavailable.")));
        var check = CreateCheck(new MediaProcessingOptions { Enabled = true }, provider);

        var result = await CheckAsync(check);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task Invalid_scanner_ping_is_unhealthy()
    {
        using var listener = StartListener(async client =>
        {
            await using var stream = client.GetStream();
            await ReadPingAsync(stream);
            await stream.WriteAsync(Encoding.ASCII.GetBytes("NOPE\0"));
        });
        using var provider = BuildProvider(new TestStorage(() => { }));
        var check = CreateCheck(EnabledOptions(listener), provider);

        var result = await CheckAsync(check);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task Scanner_ping_and_storage_success_are_healthy()
    {
        using var listener = StartListener(async client =>
        {
            await using var stream = client.GetStream();
            await ReadPingAsync(stream);
            await stream.WriteAsync(Encoding.ASCII.GetBytes("PONG\0"));
        });
        using var provider = BuildProvider(new TestStorage(() => { }));
        var check = CreateCheck(EnabledOptions(listener), provider);

        var result = await CheckAsync(check);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Scanner_read_timeout_is_unhealthy()
    {
        using var listener = StartListener(async client =>
        {
            await using var stream = client.GetStream();
            await ReadPingAsync(stream);
            await Task.Delay(TimeSpan.FromSeconds(10));
        });
        using var provider = BuildProvider(new TestStorage(() => { }));
        var check = CreateCheck(EnabledOptions(listener), provider);

        var result = await CheckAsync(check);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private static MediaProcessingHealthCheck CreateCheck(MediaProcessingOptions options, ServiceProvider provider) =>
        new(Options.Create(options), provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<MediaProcessingHealthCheck>.Instance);

    private static async Task<HealthCheckResult> CheckAsync(MediaProcessingHealthCheck check) =>
        await check.CheckHealthAsync(new HealthCheckContext());

    private static ServiceProvider BuildProvider(IStorageService storage) => new ServiceCollection()
        .AddScoped<IStorageService>(_ => storage)
        .BuildServiceProvider();

    private static MediaProcessingOptions EnabledOptions(TcpListener listener) => new()
    {
        Enabled = true,
        ClamAvHost = "127.0.0.1",
        ClamAvPort = ((IPEndPoint)listener.LocalEndpoint).Port
    };

    private static TcpListener StartListener(Func<TcpClient, Task> handler)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        _ = Task.Run(async () =>
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync();
                await handler(client);
            }
            catch (ObjectDisposedException)
            {
                // The test completed before a connection was accepted.
            }
        });
        return listener;
    }

    private static async Task ReadPingAsync(NetworkStream stream)
    {
        var command = new byte[6];
        var offset = 0;
        while (offset < command.Length)
        {
            var read = await stream.ReadAsync(command.AsMemory(offset));
            if (read == 0) throw new IOException("Scanner probe closed before PING.");
            offset += read;
        }

        Assert.Equal("zPING\0", Encoding.ASCII.GetString(command));
    }

    private sealed class TestStorage(Action ensureReady) : IStorageService
    {
        public Task EnsureReadyAsync(CancellationToken cancellationToken = default)
        {
            ensureReady();
            return Task.CompletedTask;
        }

        public string GetPublicFileUrl(string storageKey) => storageKey;
        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<string> UploadToQuarantineAsync(Stream fileStream, string safeExtension, string contentType,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<string> UploadToPublicAsync(Stream fileStream, string safeExtension, string contentType,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<string> PromoteAsync(string quarantineKey, MediaVisibility targetVisibility,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
