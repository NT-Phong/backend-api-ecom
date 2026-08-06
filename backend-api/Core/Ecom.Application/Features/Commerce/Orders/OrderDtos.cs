using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders;

public sealed record OrderSummaryDto(Guid Id, string OrderNumber, OrderStatus Status, PaymentStatus PaymentStatus,
    decimal GrandTotalAmount, DateTime PlacedAt);
