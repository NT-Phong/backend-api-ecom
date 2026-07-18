namespace Ecom.Domain.Models.Commerce;

public sealed record OrderLineSnapshot(
    Guid? ProductVariantId,
    string ProductName,
    string VariantName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount = 0m);
