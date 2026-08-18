using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders;

public sealed record ManagementOrderListItemDto(Guid Id, string OrderNumber, OrderStatus Status, PaymentStatus PaymentStatus,
    ShipmentStatus? ShipmentStatus, string RecipientName, string RecipientPhone, decimal GrandTotalAmount, string CurrencyCode, DateTime PlacedAt);

public sealed record ManagementOrderItemDto(Guid Id, Guid? ProductVariantId, string ProductName, string VariantName, string Sku,
    decimal UnitPrice, int Quantity, decimal DiscountAmount, decimal LineTotalAmount);

public sealed record ManagementOrderNoteDto(Guid Id, Guid? AuthorUserId, OrderNoteType NoteType, string Content,
    bool IsVisibleToCustomer, DateTime CreatedAt);

public sealed record ManagementOrderTimelineItemDto(string Type, string Status, string? Reason, Guid? ActorUserId, DateTime OccurredAt);

public sealed record ManagementOrderDetailDto(Guid Id, string OrderNumber, OrderStatus Status, string? CustomerEmail,
    string CustomerPhone, string RecipientName, string RecipientPhone, string ShippingAddress, decimal SubtotalAmount,
    decimal DiscountAmount, decimal ShippingAmount, decimal GrandTotalAmount, string CurrencyCode, DateTime PlacedAt,
    CustomerPaymentDto Payment, CustomerShipmentDto? Shipment, IReadOnlyList<ManagementOrderItemDto> Items,
    IReadOnlyList<ManagementOrderNoteDto> Notes, IReadOnlyList<ManagementOrderTimelineItemDto> Timeline);
