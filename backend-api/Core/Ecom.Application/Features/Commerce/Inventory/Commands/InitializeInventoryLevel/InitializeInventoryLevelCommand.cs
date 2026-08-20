using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Inventory.Commands.InitializeInventoryLevel;

public sealed record InitializeInventoryLevelCommand(Guid ProductVariantId, Guid StockLocationId, bool RequiresShipping = true)
    : IRequest<TResult<InventoryLevelDto>>, ITransactionalRequest;
public sealed class InitializeInventoryLevelCommandValidator : AbstractValidator<InitializeInventoryLevelCommand>
{ public InitializeInventoryLevelCommandValidator() { RuleFor(x => x.ProductVariantId).NotEmpty(); RuleFor(x => x.StockLocationId).NotEmpty(); } }
public sealed class InitializeInventoryLevelCommandHandler(IUnitOfWork uow, ICurrentUser currentUser) : IRequestHandler<InitializeInventoryLevelCommand, TResult<InventoryLevelDto>>
{
    public async Task<TResult<InventoryLevelDto>> Handle(InitializeInventoryLevelCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<InventoryLevelDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Inventory.Adjust)) return TResult<InventoryLevelDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var variant = await uow.Repository<ProductVariant>().FindByIdAsync(request.ProductVariantId);
        if (variant is null) return TResult<InventoryLevelDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (variant.Status == VariantStatus.Discontinued)
            return TResult<InventoryLevelDto>.Failure("Inventory cannot be initialized for a discontinued variant.", ErrorCodes.UNPROCESSABLE_ENTITY);
        if (variant.InventoryMode != InventoryMode.Tracked)
            return TResult<InventoryLevelDto>.Failure("Inventory levels are only available for tracked variants.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var location = await uow.Repository<StockLocation>().FindByIdAsync(request.StockLocationId);
        if (location is null || !location.IsActive) return TResult<InventoryLevelDto>.Failure("Stock location is not active.", ErrorCodes.UNPROCESSABLE_ENTITY);
        var item = await uow.Repository<InventoryItem>().FindOneAsync([x => x.ProductVariantId == request.ProductVariantId]);
        if (item is null) { item = InventoryItem.Create(request.ProductVariantId, request.RequiresShipping); await uow.Repository<InventoryItem>().InsertAsync(item, ct); }
        if (await uow.Repository<InventoryLevel>().AnyAsync([x => x.InventoryItemId == item.Id && x.StockLocationId == location.Id])) return TResult<InventoryLevelDto>.Failure("Inventory level already exists for this item and location.", ErrorCodes.ALREADY_EXISTS);
        var level = InventoryLevel.Create(item.Id, location.Id); await uow.Repository<InventoryLevel>().InsertAsync(level, ct);
        var product = await uow.Repository<Product>().FindByIdAsync(variant.ProductId);
        return TResult<InventoryLevelDto>.Success(new(item.Id, item.ProductVariantId, variant.Sku, product?.Name ?? string.Empty, variant.Name, location.Id, location.Code, 0, 0, 0, 0));
    }
}
