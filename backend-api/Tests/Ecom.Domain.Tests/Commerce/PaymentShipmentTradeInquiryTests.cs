namespace Ecom.Domain.Tests.Commerce;

public class PaymentShipmentTradeInquiryTests
{
    [Fact]
    public void Bank_transfer_requires_matching_amount_and_can_be_refunded()
    {
        var payment = Payment.Create(Guid.NewGuid(), PaymentMethod.BankTransfer, 150m);
        Assert.Equal(PaymentStatus.AwaitingConfirmation, payment.Status);
        Assert.Throws<CommerceDomainException>(() => payment.MarkPaid(149m, "manual", "ref", DateTime.UtcNow));

        payment.MarkPaid(150m, "manual", "ref", DateTime.UtcNow);
        payment.Refund(150m, "manual", "refund", DateTime.UtcNow);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void Shipment_records_delivery_flow_and_rejects_invalid_transition()
    {
        var history = new List<ShipmentHistory>();
        var shipment = Shipment.Create(Guid.NewGuid(), "Standard", DateTime.UtcNow, history);

        Assert.Throws<CommerceDomainException>(() => shipment.MarkDelivered(null, DateTime.UtcNow, history));
        shipment.MarkReady(null, DateTime.UtcNow, history);
        shipment.StartShipping("Carrier", "TRACK", null, DateTime.UtcNow, history);
        shipment.MarkDelivered(null, DateTime.UtcNow, history);

        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.Equal(4, history.Count);
    }

    [Fact]
    public void Trade_inquiry_follows_document_state_machine()
    {
        var history = new List<TradeInquiryStatusHistory>();
        var inquiry = TradeInquiry.Create("INQ-001", null, "Contact", null, null, "0900000000",
            TradeInquiryType.BulkPurchase, null, DateTime.UtcNow, history);

        inquiry.Assign(Guid.NewGuid(), null, DateTime.UtcNow, history);
        inquiry.StartProgress(null, DateTime.UtcNow, history);
        inquiry.MarkQuoted(null, DateTime.UtcNow, history);
        inquiry.MarkWon(null, DateTime.UtcNow, history);

        Assert.Equal(TradeInquiryStatus.Won, inquiry.Status);
        Assert.Equal(5, history.Count);
        Assert.Throws<CommerceDomainException>(() => inquiry.Close("done", null, DateTime.UtcNow, history));
    }
}
