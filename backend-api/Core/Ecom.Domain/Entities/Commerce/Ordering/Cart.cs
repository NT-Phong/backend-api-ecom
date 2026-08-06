namespace Ecom.Domain.Entities;
public class Cart : BaseEntity, IAggregateRoot
{
    public Guid? UserId { get; private set; }
    public string? GuestTokenHash { get; private set; }
    public CartStatus Status { get; private set; }
    public string CurrencyCode { get; private set; } = "VND";
    public DateTime? ExpiresAt { get; private set; }

    public static Cart CreateForUser(Guid userId, DateTime? expiresAt = null)
    {
        if (userId == Guid.Empty)
            throw new CommerceDomainException("CART_USER_REQUIRED", "A user is required.");
        return new Cart { UserId = userId, Status = CartStatus.Active, ExpiresAt = expiresAt };
    }

    public static Cart CreateForGuest(string guestTokenHash, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(guestTokenHash))
            throw new CommerceDomainException("CART_GUEST_TOKEN_REQUIRED", "A guest token hash is required.");
        if (expiresAt == default)
            throw new CommerceDomainException("CART_EXPIRY_REQUIRED", "A guest cart expiry is required.");
        return new Cart { GuestTokenHash = guestTokenHash.Trim(), Status = CartStatus.Active, ExpiresAt = expiresAt };
    }

    public CartItem AddItem(ICollection<CartItem> items, Guid productVariantId, int quantity)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(items);
        var existing = items.SingleOrDefault(x => x.CartId == Id && x.ProductVariantId == productVariantId && !x.IsDeleted);
        if (existing is not null)
        {
            existing.Increase(quantity);
            return existing;
        }

        var item = CartItem.Create(Id, productVariantId, quantity);
        items.Add(item);
        return item;
    }

    public void ChangeQuantity(CartItem item, int quantity)
    {
        EnsureOwns(item);
        item.ChangeQuantity(quantity);
    }

    public void RemoveItem(CartItem item)
    {
        EnsureOwns(item);
        item.IsDeleted = true;
    }

    public void MergeFrom(Cart source, ICollection<CartItem> targetItems, IEnumerable<CartItem> sourceItems)
    {
        EnsureActive();
        if (source.Id == Id || source.Status != CartStatus.Active)
            throw new CommerceDomainException("CART_MERGE_INVALID", "Only another active cart can be merged.");

        foreach (var item in sourceItems.Where(x => x.CartId == source.Id && !x.IsDeleted))
            AddItem(targetItems, item.ProductVariantId, item.Quantity);

        source.MarkConverted();
    }

    public void MarkConverted()
    {
        EnsureActive();
        Status = CartStatus.Converted;
        AddDomainEvent(new CommerceStateChangedEvent(nameof(Cart), Id, CartStatus.Active.ToString(), Status.ToString()));
    }

    public void CheckoutSelectedItems(ICollection<CartItem> items, IEnumerable<Guid> cartItemIds)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(items);
        var selectedIds = cartItemIds?.Distinct().ToHashSet() ?? throw new ArgumentNullException(nameof(cartItemIds));
        if (selectedIds.Count == 0)
            throw new CommerceDomainException("CART_ITEMS_REQUIRED", "At least one cart item is required.");

        var selected = items.Where(x => x.CartId == Id && !x.IsDeleted && selectedIds.Contains(x.Id)).ToList();
        if (selected.Count != selectedIds.Count)
            throw new CommerceDomainException("CART_ITEM_NOT_FOUND", "One or more cart items do not belong to this active cart.");

        foreach (var item in selected)
            item.IsDeleted = true;

        if (!items.Any(x => x.CartId == Id && !x.IsDeleted))
            MarkConverted();
    }

    public void Expire()
    {
        EnsureActive();
        Status = CartStatus.Expired;
        AddDomainEvent(new CommerceStateChangedEvent(nameof(Cart), Id, CartStatus.Active.ToString(), Status.ToString()));
    }

    public bool IsExpiredAt(DateTime now) =>
        Status == CartStatus.Active && ExpiresAt is not null && ExpiresAt <= now;

    public bool ExpireIfDue(DateTime now)
    {
        if (!IsExpiredAt(now))
            return false;

        Expire();
        return true;
    }

    private void EnsureOwns(CartItem item)
    {
        EnsureActive();
        if (item.CartId != Id || item.IsDeleted)
            throw new CommerceDomainException("CART_ITEM_NOT_FOUND", "The cart item does not belong to this active cart.");
    }

    private void EnsureActive()
    {
        if (Status != CartStatus.Active)
            throw new CommerceDomainException("CART_NOT_ACTIVE", "Only an active cart can be changed.");
    }

    private Cart()
    {
    }
}
