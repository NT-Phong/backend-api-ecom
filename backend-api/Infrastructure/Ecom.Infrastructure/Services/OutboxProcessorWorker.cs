using Microsoft.Extensions.Hosting;

namespace Ecom.Infrastructure.Services;

public sealed class OutboxProcessorWorker(IServiceScopeFactory scopeFactory,
    IOptions<OutboxProcessorOptions> options, ILogger<OutboxProcessorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<OutboxProcessor>().ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox processor worker failed.");
            }
        }
    }
}
