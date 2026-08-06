using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders;

public sealed record OrderSummaryDto(Guid Id, string OrderNumber, OrderStatus Status, PaymentStatus PaymentStatus,
    decimal GrandTotalAmount, DateTime PlacedAt);

public sealed record CustomerOrderItemDto(
    Guid Id,
    Guid? ProductVariantId,
    string ProductName,
    string VariantName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount,
    decimal LineTotalAmount);

public sealed record CustomerPaymentDto(PaymentMethod Method, PaymentStatus Status, decimal Amount, DateTime? DueAt, DateTime? PaidAt);

public sealed record CustomerShipmentDto(
    ShipmentStatus Status,
    string ShippingMethod,
    string? CarrierName,
    string? TrackingCode,
    DateTime? ShippedAt,
    DateTime? DeliveredAt);

public sealed record CustomerOrderTimelineItemDto(string Type, string Status, DateTime OccurredAt);

/// <summary>Owner-scoped order representation. It intentionally omits provider references and staff identities.</summary>
public sealed record CustomerOrderDetailDto(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal GrandTotalAmount,
    string CurrencyCode,
    DateTime PlacedAt,
    string RecipientName,
    string RecipientPhone,
    string ShippingAddress,
    IReadOnlyList<CustomerOrderItemDto> Items,
    CustomerPaymentDto Payment,
    CustomerShipmentDto? Shipment,
    IReadOnlyList<CustomerOrderTimelineItemDto> Timeline);
