using System.Security.Cryptography;
using System.Text;
using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class CheckoutPricingService(IUnitOfWork unitOfWork, IEffectivePriceResolver prices) : ICheckoutPricingService
{
    private const string ShippingFeeSettingKey = "checkout.shipping.standardFeeVnd";

    public async Task<TResult<CheckoutQuote>> CreateQuoteAsync(CartPrincipal principal,
        IReadOnlyCollection<Guid> cartItemIds, CheckoutRecipient recipient, PaymentMethod paymentMethod,
        CancellationToken cancellationToken)
    {
        if (cartItemIds.Count == 0)
            return TResult<CheckoutQuote>.Failure("At least one cart item is required.", ErrorCodes.BAD_REQUEST);

        var now = DateTime.UtcNow;
        var cart = await unitOfWork.Repository<Cart>().FindOneAsync(
            principal.UserId.HasValue
                ? [x => x.UserId == principal.UserId && x.Status == CartStatus.Active && (x.ExpiresAt == null || x.ExpiresAt > now)]
                : [x => x.GuestTokenHash == principal.GuestTokenHash && x.Status == CartStatus.Active && (x.ExpiresAt == null || x.ExpiresAt > now)]);
        if (cart is null)
            return TResult<CheckoutQuote>.Failure("Active cart was not found.", ErrorCodes.NOT_FOUND);

        var ids = cartItemIds.Distinct().ToArray();
        var items = await unitOfWork.Repository<CartItem>().QueryNoTracking()
            .Where(x => x.CartId == cart.Id && ids.Contains(x.Id)).ToListAsync(cancellationToken);
        if (items.Count != ids.Length)
            return TResult<CheckoutQuote>.Failure("One or more cart items are unavailable.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var variantIds = items.Select(x => x.ProductVariantId).Distinct().ToArray();
        var variants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => variantIds.Contains(x.Id) && x.Status == VariantStatus.Active).ToListAsync(cancellationToken);
        if (variants.Count != variantIds.Length)
            return TResult<CheckoutQuote>.Failure("One or more product variants are unavailable.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var products = await unitOfWork.Repository<Product>().QueryNoTracking()
            .Where(x => variants.Select(v => v.ProductId).Contains(x.Id) && x.Status == ProductStatus.Published)
            .ToListAsync(cancellationToken);
        if (products.Count != variants.Select(x => x.ProductId).Distinct().Count())
            return TResult<CheckoutQuote>.Failure("One or more products are unavailable.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var effectivePrices = await prices.ResolveForVariantsAsync(variantIds, now, cancellationToken);
        if (effectivePrices.Count != variantIds.Length)
            return TResult<CheckoutQuote>.Failure("One or more variants do not have an active price.", ErrorCodes.UNPROCESSABLE_ENTITY);

        var trackedVariants = variants.Where(x => x.InventoryMode == InventoryMode.Tracked).Select(x => x.Id).ToArray();
        if (trackedVariants.Length > 0)
        {
            var available = await (
                from inventory in unitOfWork.Repository<InventoryItem>().QueryNoTracking()
                join level in unitOfWork.Repository<InventoryLevel>().QueryNoTracking() on inventory.Id equals level.InventoryItemId
                join location in unitOfWork.Repository<StockLocation>().QueryNoTracking() on level.StockLocationId equals location.Id
                where trackedVariants.Contains(inventory.ProductVariantId) && location.Code == "MAIN" && location.IsActive
                select new { inventory.ProductVariantId, level.AvailableQuantity }).ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                if (trackedVariants.Contains(item.ProductVariantId) &&
                    available.Where(x => x.ProductVariantId == item.ProductVariantId).Sum(x => x.AvailableQuantity) < item.Quantity)
                    return TResult<CheckoutQuote>.Failure("Available inventory is insufficient.", ErrorCodes.UNPROCESSABLE_ENTITY);
            }
        }

        var feeSetting = await unitOfWork.Repository<SystemSetting>().FindOneAsync([x => x.SettingKey == ShippingFeeSettingKey]);
        if (feeSetting is null || !TryParseFee(feeSetting.Value, out var shippingAmount))
            return TResult<CheckoutQuote>.Failure("Shipping is not configured.", ErrorCodes.SERVICE_UNAVAILABLE);

        var productById = products.ToDictionary(x => x.Id);
        var variantById = variants.ToDictionary(x => x.Id);
        var lines = items.Select(item =>
        {
            var variant = variantById[item.ProductVariantId];
            var product = productById[variant.ProductId];
            return new CheckoutLine(item.Id, variant.Id, product.Name, variant.Name, variant.Sku, item.Quantity,
                effectivePrices[variant.Id].Amount, variant.InventoryMode == InventoryMode.Tracked);
        }).OrderBy(x => x.CartItemId).ToList();
        var subtotal = lines.Sum(x => x.UnitPrice * x.Quantity);
        var fingerprint = CreateFingerprint(lines, shippingAmount, recipient, paymentMethod);
        return TResult<CheckoutQuote>.Success(new CheckoutQuote(lines, subtotal, shippingAmount, subtotal + shippingAmount, fingerprint));
    }

    private static bool TryParseFee(string value, out decimal fee)
    {
        var raw = value.Trim().Trim('"');
        return decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out fee) && fee >= 0;
    }

    private static string CreateFingerprint(IEnumerable<CheckoutLine> lines, decimal shippingAmount,
        CheckoutRecipient recipient, PaymentMethod paymentMethod)
    {
        var canonical = string.Join('|', lines.Select(x => $"{x.CartItemId:N}:{x.ProductVariantId:N}:{x.Quantity}:{x.UnitPrice:0.00}"))
            + $"|{shippingAmount:0.00}|{paymentMethod}|{recipient.RecipientName.Trim()}|{recipient.RecipientPhone.Trim()}|{recipient.ShippingAddress.Trim()}|{recipient.AdministrativeAreaId:N}|{recipient.CustomerEmail?.Trim()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
