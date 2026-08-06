namespace Ecom.Domain.Tests.Commerce;

public class OrderTests
{
    [Fact]
    public void Order_calculates_totals_and_records_valid_transition_history()
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var now = DateTime.UtcNow;
        var order = CreateOrder(items, history, now);

        Assert.Equal(200m, order.SubtotalAmount);
        Assert.Equal(20m, order.DiscountAmount);
        Assert.Equal(210m, order.GrandTotalAmount);

        order.Confirm(Guid.NewGuid(), now.AddMinutes(1), history);
        order.StartPreparing(Guid.NewGuid(), now.AddMinutes(2), history);
        order.StartShipping(Guid.NewGuid(), now.AddMinutes(3), history);
        order.Complete(Guid.NewGuid(), now.AddMinutes(4), history);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(5, history.Count);
        Assert.Throws<CommerceDomainException>(() => order.Cancel("late", null, now.AddMinutes(5), history));
    }

    [Fact]
    public void Order_cancel_requires_reason_and_respects_transition_matrix()
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var order = CreateOrder(items, history, DateTime.UtcNow);

        Assert.Throws<CommerceDomainException>(() => order.Cancel("", null, DateTime.UtcNow, history));
        order.Cancel("Customer request", null, DateTime.UtcNow, history);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Throws<CommerceDomainException>(() => order.Confirm(null, DateTime.UtcNow, history));
    }

    [Fact]
    public void Order_requires_exactly_one_owner()
    {
        var now = DateTime.UtcNow;
        OrderLineSnapshot[] snapshots = [new OrderLineSnapshot(Guid.NewGuid(), "Product", "Variant", "SKU-OWNER", 100m, 1)];

        Assert.Throws<CommerceDomainException>(() => Order.Create("ORD-NO-OWNER", null, null, null, "0900000000", "Buyer", "0900000000", null, "Address", 0m, now, snapshots, new List<OrderItem>(), new List<OrderStatusHistory>()));
        Assert.Throws<CommerceDomainException>(() => Order.Create("ORD-TWO-OWNERS", Guid.NewGuid(), "guest-hash", null, "0900000000", "Buyer", "0900000000", null, "Address", 0m, now, snapshots, new List<OrderItem>(), new List<OrderStatusHistory>()));
    }

    private static Order CreateOrder(ICollection<OrderItem> items, ICollection<OrderStatusHistory> history, DateTime now) =>
        Order.Create("ORD-001", null, "guest-hash", null, "0900000000", "Buyer", "0900000000", null, "Address",
            30m, now,
            [new OrderLineSnapshot(Guid.NewGuid(), "Product", "Variant", "SKU-1", 100m, 2, 20m)],
            items, history);
}
