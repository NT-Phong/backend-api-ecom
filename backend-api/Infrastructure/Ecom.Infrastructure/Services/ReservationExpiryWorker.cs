using Ecom.Application.Features.Commerce.Inventory.Commands.ExpireReservations;
using Microsoft.Extensions.Hosting;

namespace Ecom.Infrastructure.Services;

public sealed class ReservationExpiryWorker(IServiceScopeFactory scopeFactory, ILogger<ReservationExpiryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(new ExpireReservationsCommand(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception exception) { logger.LogError(exception, "Reservation expiry worker failed."); }
        }
    }
}
