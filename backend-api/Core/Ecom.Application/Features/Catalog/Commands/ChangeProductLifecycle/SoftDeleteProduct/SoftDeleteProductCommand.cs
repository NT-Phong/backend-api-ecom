using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductLifecycle;

public sealed record SoftDeleteProductCommand(Guid ProductId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductSoftDeleteResult>>, ITransactionalRequest;

public sealed class SoftDeleteProductCommandValidator : AbstractValidator<SoftDeleteProductCommand>
{
    public SoftDeleteProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class SoftDeleteProductCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<SoftDeleteProductCommand, TResult<ProductSoftDeleteResult>>
{
    public async Task<TResult<ProductSoftDeleteResult>> Handle(
        SoftDeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Discontinue, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductSoftDeleteResult>(loaded);

        var variants = await unitOfWork.Repository<ProductVariant>().FindAsync([x => x.ProductId == request.ProductId]);
        var variantIds = variants.Select(x => x.Id).ToArray();
        var inventoryItems = variantIds.Length == 0
            ? []
            : await unitOfWork.Repository<InventoryItem>().FindAsync([x => variantIds.Contains(x.ProductVariantId)]);
        var inventoryItemIds = inventoryItems.Select(x => x.Id).ToArray();

        if (await HasDeleteBlockerAsync(variantIds, inventoryItemIds, cancellationToken))
            return TResult<ProductSoftDeleteResult>.Failure(
                "Product cannot be deleted while it has active carts, inventory, or reservations.",
                ErrorCodes.ALREADY_EXISTS);

        var options = await unitOfWork.Repository<ProductOption>().FindAsync([x => x.ProductId == request.ProductId]);
        var optionIds = options.Select(x => x.Id).ToArray();
        var optionValues = optionIds.Length == 0
            ? []
            : await unitOfWork.Repository<ProductOptionValue>().FindAsync([x => optionIds.Contains(x.ProductOptionId)]);
        var optionValueIds = optionValues.Select(x => x.Id).ToArray();
        var variantOptionValues = variantIds.Length == 0 && optionValueIds.Length == 0
            ? []
            : await unitOfWork.Repository<ProductVariantOptionValue>().FindAsync(
                [x => variantIds.Contains(x.ProductVariantId) || optionValueIds.Contains(x.ProductOptionValueId)]);
        var prices = variantIds.Length == 0
            ? []
            : await unitOfWork.Repository<VariantPrice>().FindAsync([x => variantIds.Contains(x.ProductVariantId)]);
        var media = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        var categories = await unitOfWork.Repository<ProductCategory>().FindAsync([x => x.ProductId == request.ProductId]);
        var slugHistory = await unitOfWork.Repository<ProductSlugHistory>().FindAsync([x => x.ProductId == request.ProductId]);

        if (variantOptionValues.Count > 0)
            await unitOfWork.Repository<ProductVariantOptionValue>().DeleteRangeAsync(variantOptionValues, cancellationToken);
        if (optionValues.Count > 0)
            await unitOfWork.Repository<ProductOptionValue>().DeleteRangeAsync(optionValues, cancellationToken);
        if (options.Count > 0)
            await unitOfWork.Repository<ProductOption>().DeleteRangeAsync(options, cancellationToken);
        if (prices.Count > 0)
            await unitOfWork.Repository<VariantPrice>().DeleteRangeAsync(prices, cancellationToken);
        if (media.Count > 0)
            await unitOfWork.Repository<ProductMedia>().DeleteRangeAsync(media, cancellationToken);
        if (categories.Count > 0)
            await unitOfWork.Repository<ProductCategory>().DeleteRangeAsync(categories, cancellationToken);
        if (variants.Count > 0)
            await unitOfWork.Repository<ProductVariant>().DeleteRangeAsync(variants, cancellationToken);
        if (slugHistory.Count > 0)
            await unitOfWork.Repository<ProductSlugHistory>().DeleteRangeAsync(slugHistory, cancellationToken);
        await unitOfWork.Repository<Product>().DeleteAsync(loaded.Data, cancellationToken);

        return TResult<ProductSoftDeleteResult>.Success(new ProductSoftDeleteResult(request.ProductId, true));
    }

    private async Task<bool> HasDeleteBlockerAsync(
        IReadOnlyCollection<Guid> variantIds,
        IReadOnlyCollection<Guid> inventoryItemIds,
        CancellationToken cancellationToken)
    {
        if (variantIds.Count > 0)
        {
            var cartItems = await unitOfWork.Repository<CartItem>().FindAsync([x => variantIds.Contains(x.ProductVariantId)]);
            if (cartItems.Count > 0)
            {
                var cartIds = cartItems.Select(x => x.CartId).Distinct().ToArray();
                var hasActiveCart = await unitOfWork.Repository<Cart>().AnyAsync(
                    [x => cartIds.Contains(x.Id) && x.Status == CartStatus.Active]);
                if (hasActiveCart)
                    return true;
            }
        }

        if (inventoryItemIds.Count == 0)
            return false;

        var hasInventoryBalance = await unitOfWork.Repository<InventoryLevel>().AnyAsync(
            [x => inventoryItemIds.Contains(x.InventoryItemId)
                  && (x.StockedQuantity > 0 || x.ReservedQuantity > 0 || x.IncomingQuantity > 0)]);
        if (hasInventoryBalance)
            return true;

        return await unitOfWork.Repository<InventoryReservation>().AnyAsync(
            [x => inventoryItemIds.Contains(x.InventoryItemId) && x.Status == InventoryReservationStatus.Active]);
    }
}
