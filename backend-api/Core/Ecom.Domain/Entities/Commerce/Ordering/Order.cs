namespace Ecom.Domain.Entities;
public class Order : BaseEntity, IAggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string? GuestTokenHashSnapshot { get; private set; }
    public string? CustomerEmailSnapshot { get; private set; }
    public string CustomerPhoneSnapshot { get; private set; } = string.Empty;
    public string RecipientNameSnapshot { get; private set; } = string.Empty;
    public string RecipientPhoneSnapshot { get; private set; } = string.Empty;
    public Guid? AdministrativeAreaId { get; private set; }
    public string ShippingAddressSnapshot { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public string CurrencyCode { get; private set; } = "VND";
    public decimal SubtotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal GrandTotalAmount { get; private set; }
    public DateTime PlacedAt { get; private set; }

    public static Order Create(
        string orderNumber,
        Guid? userId,
        string? guestTokenHash,
        string? customerEmail,
        string customerPhone,
        string recipientName,
        string recipientPhone,
        Guid? administrativeAreaId,
        string shippingAddress,
        decimal shippingAmount,
        DateTime placedAt,
        IEnumerable<OrderLineSnapshot> lines,
        ICollection<OrderItem> orderItems,
        ICollection<OrderStatusHistory> history)
    {
        var normalizedGuestTokenHash = guestTokenHash?.Trim();
        if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(customerPhone) ||
            string.IsNullOrWhiteSpace(recipientName) || string.IsNullOrWhiteSpace(recipientPhone) ||
            string.IsNullOrWhiteSpace(shippingAddress))
            throw new CommerceDomainException("ORDER_DETAILS_REQUIRED", "Order number, customer, recipient, and shipping details are required.");
        if (userId.HasValue == !string.IsNullOrWhiteSpace(normalizedGuestTokenHash))
            throw new CommerceDomainException("ORDER_OWNER_INVALID", "An order must belong to exactly one user or guest cart.");
        if (shippingAmount < 0)
            throw new CommerceDomainException("ORDER_SHIPPING_AMOUNT_INVALID", "Shipping amount cannot be negative.");
        if (placedAt == default)
            throw new CommerceDomainException("ORDER_PLACED_AT_REQUIRED", "Order placement time is required.");

        var snapshots = lines?.ToList() ?? throw new ArgumentNullException(nameof(lines));
        if (snapshots.Count == 0)
            throw new CommerceDomainException("ORDER_ITEMS_REQUIRED", "An order must contain at least one item.");

        var order = new Order
        {
            OrderNumber = orderNumber.Trim(),
            UserId = userId,
            GuestTokenHashSnapshot = normalizedGuestTokenHash,
            CustomerEmailSnapshot = customerEmail?.Trim(),
            CustomerPhoneSnapshot = customerPhone.Trim(),
            RecipientNameSnapshot = recipientName.Trim(),
            RecipientPhoneSnapshot = recipientPhone.Trim(),
            AdministrativeAreaId = administrativeAreaId,
            ShippingAddressSnapshot = shippingAddress.Trim(),
            Status = OrderStatus.Pending,
            ShippingAmount = shippingAmount,
            PlacedAt = placedAt
        };

        foreach (var line in snapshots)
            orderItems.Add(OrderItem.Create(order.Id, line.ProductVariantId, line.ProductName, line.VariantName, line.Sku, line.UnitPrice, line.Quantity, line.DiscountAmount));

        order.SubtotalAmount = orderItems.Where(x => x.OrderId == order.Id).Sum(x => x.UnitPriceSnapshot * x.Quantity);
        order.DiscountAmount = orderItems.Where(x => x.OrderId == order.Id).Sum(x => x.DiscountAmountSnapshot);
        order.GrandTotalAmount = order.SubtotalAmount - order.DiscountAmount + order.ShippingAmount;
        history.Add(OrderStatusHistory.Create(order.Id, null, OrderStatus.Pending, null, userId, placedAt));
        order.AddDomainEvent(new CommerceStateChangedEvent(nameof(Order), order.Id, null, OrderStatus.Pending.ToString()));
        return order;
    }

    public void Confirm(Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history) =>
        TransitionTo(OrderStatus.Confirmed, actorId, changedAt, null, history, OrderStatus.Pending);

    public void StartPreparing(Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history) =>
        TransitionTo(OrderStatus.Preparing, actorId, changedAt, null, history, OrderStatus.Confirmed);

    public void StartShipping(Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history) =>
        TransitionTo(OrderStatus.Shipping, actorId, changedAt, null, history, OrderStatus.Preparing);

    public void Complete(Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history) =>
        TransitionTo(OrderStatus.Completed, actorId, changedAt, null, history, OrderStatus.Shipping);

    public void MarkDeliveryFailed(string reason, Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history)
    {
        EnsureReason(reason);
        TransitionTo(OrderStatus.DeliveryFailed, actorId, changedAt, reason, history, OrderStatus.Shipping);
    }

    public void RetryShipping(Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history) =>
        TransitionTo(OrderStatus.Shipping, actorId, changedAt, null, history, OrderStatus.DeliveryFailed);

    public void Cancel(string reason, Guid? actorId, DateTime changedAt, ICollection<OrderStatusHistory> history)
    {
        EnsureReason(reason);
        TransitionTo(OrderStatus.Cancelled, actorId, changedAt, reason, history,
            OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Preparing, OrderStatus.DeliveryFailed);
    }

    private void TransitionTo(OrderStatus target, Guid? actorId, DateTime changedAt, string? reason, ICollection<OrderStatusHistory> history, params OrderStatus[] allowedFrom)
    {
        if (!allowedFrom.Contains(Status))
            throw new CommerceDomainException("ORDER_STATUS_TRANSITION_INVALID", $"Order cannot transition from {Status} to {target}.");
        if (changedAt == default)
            throw new CommerceDomainException("ORDER_STATUS_TIME_REQUIRED", "A transition time is required.");

        var previous = Status;
        Status = target;
        history.Add(OrderStatusHistory.Create(Id, previous, target, reason, actorId, changedAt));
        AddDomainEvent(new CommerceStateChangedEvent(nameof(Order), Id, previous.ToString(), target.ToString()));
    }

    private static void EnsureReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new CommerceDomainException("ORDER_STATUS_REASON_REQUIRED", "A reason is required.");
    }

    private Order()
    {
    }
}
