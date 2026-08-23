using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecom.Infrastructure.Seeding;

public static class SystemSettingSeeder
{
    private const string ShippingFeeSettingKey = "checkout.shipping.standardFeeVnd";
    private const string DefaultShippingFee = "0";

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SystemSettingSeeder");

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existingSetting = await db.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == ShippingFeeSettingKey, cancellationToken);

            if (existingSetting is null)
            {
                var newSetting = SystemSetting.Create(
                    ShippingFeeSettingKey,
                    DefaultShippingFee,
                    isPublic: false,
                    description: "Standard checkout shipping fee in VND."
                );
                db.SystemSettings.Add(newSetting);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Seeded default system setting '{Key}' with value '{Value}'.", ShippingFeeSettingKey, DefaultShippingFee);
            }
            else
            {
                var isInvalidOrOutdated = !decimal.TryParse(existingSetting.Value.Trim().Trim('"'), global::System.Globalization.NumberStyles.Number, global::System.Globalization.CultureInfo.InvariantCulture, out var parsedFee) || parsedFee < 0 || existingSetting.Value == "30000";
                if (isInvalidOrOutdated)
                {
                    existingSetting.UpdateValue(DefaultShippingFee);
                    existingSetting.ConcurrencyStamp = Guid.NewGuid();
                    existingSetting.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated system setting '{Key}' to value '{Value}'.", ShippingFeeSettingKey, DefaultShippingFee);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding system settings.");
        }
    }
}
