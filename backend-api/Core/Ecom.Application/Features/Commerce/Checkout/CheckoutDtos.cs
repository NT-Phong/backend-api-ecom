using Ecom.Application.Common.Commerce;

namespace Ecom.Application.Features.Commerce.Checkout;

public sealed record CheckoutLineDto(Guid CartItemId, Guid ProductVariantId, string ProductName, string VariantName,
    string Sku, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record CheckoutPreviewDto(IReadOnlyList<CheckoutLineDto> Lines, decimal SubtotalAmount,
    decimal ShippingAmount, decimal GrandTotalAmount, string QuoteFingerprint, DateTime QuoteExpiresAt);

internal static class CheckoutDtoMapper
{
    internal static CheckoutPreviewDto Map(CheckoutQuote quote) => new(quote.Lines.Select(x =>
        new CheckoutLineDto(x.CartItemId, x.ProductVariantId, x.ProductName, x.VariantName, x.Sku, x.Quantity,
            x.UnitPrice, x.UnitPrice * x.Quantity)).ToList(), quote.SubtotalAmount, quote.ShippingAmount,
        quote.GrandTotalAmount, quote.Fingerprint, quote.QuoteExpiresAt);
}
