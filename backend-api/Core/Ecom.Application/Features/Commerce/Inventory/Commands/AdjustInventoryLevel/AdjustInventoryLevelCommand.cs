using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.AdjustInventoryLevel;

public sealed record AdjustInventoryLevelCommand(Guid InventoryItemId, Guid StockLocationId, decimal QuantityDelta, string Reason)
    : IRequest<TResult<InventoryMovementDto>>, ITransactionalRequest;
public sealed class AdjustInventoryLevelCommandValidator : AbstractValidator<AdjustInventoryLevelCommand>
{ public AdjustInventoryLevelCommandValidator() { RuleFor(x => x.InventoryItemId).NotEmpty(); RuleFor(x => x.StockLocationId).NotEmpty(); RuleFor(x => x.QuantityDelta).NotEqual(0).InclusiveBetween(-1000000m, 1000000m); RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000); } }
public sealed class AdjustInventoryLevelCommandHandler(IUnitOfWork uow, ICurrentUser currentUser, IInventoryReservationStore inventoryStore)
    : IRequestHandler<AdjustInventoryLevelCommand, TResult<InventoryMovementDto>>
{
    public async Task<TResult<InventoryMovementDto>> Handle(AdjustInventoryLevelCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<InventoryMovementDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.Adjust)) return TResult<InventoryMovementDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var locks = await inventoryStore.LockInventoryLevelsAsync([new InventoryLevelLockRequest(request.InventoryItemId, request.StockLocationId)], ct);
        if (!locks.IsSuccess) return TResult<InventoryMovementDto>.Failure(locks.Error!, locks.ErrorCode);
        var level = locks.Data[new InventoryLevelLockRequest(request.InventoryItemId, request.StockLocationId)];
        var movement = level.Adjust(request.QuantityDelta, DateTime.UtcNow, request.Reason);
        await uow.Repository<InventoryLevel>().UpdateAsync(level, ct); await uow.Repository<InventoryMovement>().InsertAsync(movement, ct);
        return TResult<InventoryMovementDto>.Success(new(movement.Id, movement.InventoryItemId, movement.StockLocationId, movement.OrderItemId, movement.MovementType, movement.QuantityDelta, movement.Reason, movement.OccurredAt));
    }
}
