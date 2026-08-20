using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders;

public sealed record ManagementOrderListItemDto(Guid Id, string OrderNumber, OrderStatus Status, PaymentStatus PaymentStatus,
    PaymentMethod? PaymentMethod, ShipmentStatus? ShipmentStatus, string RecipientName, string RecipientPhone,
    int ItemCount, decimal SubtotalAmount, decimal DiscountAmount, decimal ShippingAmount, decimal GrandTotalAmount,
    string CurrencyCode, DateTime PlacedAt);

public sealed record ManagementOrderAnalyticsKpisDto(int OrdersPlaced, int CompletedOrderCount, decimal CollectedGross,
    decimal RefundAmount, decimal NetCollected, decimal CompletedSales);

public sealed record ManagementOrderAnalyticsSeriesItemDto(string Period, int OrdersPlaced, int CompletedOrderCount,
    decimal CollectedGross, decimal RefundAmount, decimal NetCollected, decimal CompletedSales);

public sealed record ManagementOrderStatusBreakdownDto(OrderStatus Status, int OrderCount, decimal GrandTotalAmount);

public sealed record ManagementPaymentMethodCashBreakdownDto(PaymentMethod PaymentMethod, decimal CollectedGross,
    decimal RefundAmount, decimal NetCollected);

public sealed record ManagementTopProductSalesDto(Guid? ProductVariantId, string ProductName, string VariantName,
    string Sku, int QuantitySold, decimal SalesAmount);

public sealed record ManagementOrderAnalyticsOverviewDto(string CurrencyCode, string From, string To,
    ManagementOrderAnalyticsKpisDto Kpis, IReadOnlyList<ManagementOrderAnalyticsSeriesItemDto> Series,
    IReadOnlyList<ManagementOrderStatusBreakdownDto> StatusBreakdown,
    IReadOnlyList<ManagementPaymentMethodCashBreakdownDto> PaymentMethodBreakdown,
    IReadOnlyList<ManagementTopProductSalesDto> TopProducts);

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
