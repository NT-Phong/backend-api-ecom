using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

/// <summary>The single shipping-fee authority for checkout and coupon validation.</summary>
public sealed class CheckoutShippingService(IUnitOfWork unitOfWork) : ICheckoutShippingService
{
    private const string ShippingFeeSettingKey = "checkout.shipping.standardFeeVnd";
    private const decimal ColdChainPackagingFee = 15000m;

    public async Task<TResult<decimal>> ResolveShippingAmountAsync(string? packagingOption,
        CancellationToken cancellationToken)
    {
        if (!CheckoutPackaging.TryNormalize(packagingOption, out var canonicalPackaging))
            return TResult<decimal>.Failure("Packaging option is not supported.", ErrorCodes.BAD_REQUEST);

        var feeSetting = await unitOfWork.Repository<SystemSetting>().FindOneAsync(
            [x => x.SettingKey == ShippingFeeSettingKey]);
        if (feeSetting is null || !TryParseFee(feeSetting.Value, out var shippingAmount))
            return TResult<decimal>.Failure("Shipping is not configured.", ErrorCodes.SERVICE_UNAVAILABLE);

        if (canonicalPackaging == CheckoutPackaging.ColdChain)
            shippingAmount += ColdChainPackagingFee;

        return TResult<decimal>.Success(shippingAmount);
    }

    private static bool TryParseFee(string value, out decimal fee)
    {
        var raw = value.Trim().Trim('"');
        return decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out fee) && fee >= 0;
    }
}
