using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Commerce;

public sealed record CartPrincipal(Guid? UserId, string? GuestTokenHash)
{
    public string OwnerScope => UserId.HasValue ? $"user:{UserId.Value:N}" : $"guest:{GuestTokenHash}";
    public bool IsGuest => !UserId.HasValue;
}

public sealed record CheckoutRecipient(string RecipientName, string RecipientPhone, string ShippingAddress,
    Guid? AdministrativeAreaId, string? CustomerEmail);

public sealed record CheckoutLine(Guid CartItemId, Guid ProductVariantId, string ProductName, string VariantName,
    string Sku, int Quantity, decimal UnitPrice, bool IsTracked);

public sealed record CheckoutQuote(IReadOnlyList<CheckoutLine> Lines, decimal SubtotalAmount,
    decimal ShippingAmount, decimal GrandTotalAmount, string Fingerprint, DateTime QuoteExpiresAt);

public sealed record InventoryLockRequest(Guid ProductVariantId, int Quantity);

public sealed record LockedInventory(Guid ProductVariantId, Guid InventoryItemId, InventoryLevel Level);

public sealed record LockedCheckoutCart(Cart Cart, ICollection<CartItem> Items);

public sealed record InventoryLevelLockRequest(Guid InventoryItemId, Guid StockLocationId);

public enum IdempotencyBeginKind { Started, Completed, Processing, Mismatch }

public sealed record IdempotencyBeginResult(IdempotencyBeginKind Kind, IdempotencyRecord Record);
