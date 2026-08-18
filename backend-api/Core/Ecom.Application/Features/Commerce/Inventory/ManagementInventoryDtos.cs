using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory;

public sealed record StockLocationDto(Guid Id, string Code, string Name, Guid? AdministrativeAreaId, string? AddressLine, bool IsActive, Guid ConcurrencyStamp);
public sealed record InventoryLevelDto(Guid InventoryItemId, Guid ProductVariantId, string Sku, string ProductName, string VariantName,
    Guid StockLocationId, string StockLocationCode, decimal StockedQuantity, decimal ReservedQuantity, decimal IncomingQuantity, decimal AvailableQuantity);
public sealed record InventoryMovementDto(Guid Id, Guid InventoryItemId, Guid StockLocationId, Guid? OrderItemId,
    InventoryMovementType MovementType, decimal QuantityDelta, string? Reason, DateTime OccurredAt);
