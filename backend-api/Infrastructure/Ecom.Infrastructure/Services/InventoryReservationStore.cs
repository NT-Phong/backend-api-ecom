using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

public sealed class InventoryReservationStore(ApplicationDbContext db) : IInventoryReservationStore
{
    public async Task<TResult<IReadOnlyDictionary<Guid, LockedInventory>>> LockTrackedInventoryAsync(
        IReadOnlyCollection<InventoryLockRequest> requests, CancellationToken cancellationToken)
    {
        var locked = new Dictionary<Guid, LockedInventory>();
        foreach (var request in requests.OrderBy(x => x.ProductVariantId))
        {
            var level = await db.InventoryLevels.FromSqlInterpolated($@"
SELECT l.* FROM ""Tbl_InventoryLevel"" AS l
INNER JOIN ""Tbl_InventoryItem"" AS i ON i.""Id"" = l.""InventoryItemId"" AND i.""IsDeleted"" = false
INNER JOIN ""Tbl_StockLocation"" AS s ON s.""Id"" = l.""StockLocationId"" AND s.""IsDeleted"" = false
WHERE i.""ProductVariantId"" = {request.ProductVariantId} AND s.""Code"" = {"MAIN"} AND s.""IsActive"" = true AND l.""IsDeleted"" = false
FOR UPDATE OF l").SingleOrDefaultAsync(cancellationToken);
            if (level is null || level.AvailableQuantity < request.Quantity)
                return TResult<IReadOnlyDictionary<Guid, LockedInventory>>.Failure("Available inventory is insufficient.", ErrorCodes.UNPROCESSABLE_ENTITY);
            locked[request.ProductVariantId] = new LockedInventory(request.ProductVariantId, level.InventoryItemId, level);
        }
        return TResult<IReadOnlyDictionary<Guid, LockedInventory>>.Success(locked);
    }

    public async Task<TResult<IReadOnlyDictionary<InventoryLevelLockRequest, InventoryLevel>>> LockInventoryLevelsAsync(
        IReadOnlyCollection<InventoryLevelLockRequest> requests, CancellationToken cancellationToken)
    {
        var locked = new Dictionary<InventoryLevelLockRequest, InventoryLevel>();
        foreach (var request in requests.Distinct()
                     .OrderBy(x => x.InventoryItemId)
                     .ThenBy(x => x.StockLocationId))
        {
            var level = await db.InventoryLevels.FromSqlInterpolated($@"
SELECT l.* FROM ""Tbl_InventoryLevel"" AS l
WHERE l.""InventoryItemId"" = {request.InventoryItemId}
  AND l.""StockLocationId"" = {request.StockLocationId}
  AND l.""IsDeleted"" = false
FOR UPDATE OF l").SingleOrDefaultAsync(cancellationToken);

            if (level is null)
                return TResult<IReadOnlyDictionary<InventoryLevelLockRequest, InventoryLevel>>.Failure(
                    "Inventory level was not found.", ErrorCodes.NOT_FOUND);

            locked[request] = level;
        }

        return TResult<IReadOnlyDictionary<InventoryLevelLockRequest, InventoryLevel>>.Success(locked);
    }
}
