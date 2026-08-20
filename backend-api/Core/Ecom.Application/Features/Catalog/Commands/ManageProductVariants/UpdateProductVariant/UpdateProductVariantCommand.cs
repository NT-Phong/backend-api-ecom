using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductVariants;

public sealed record UpdateProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
    Guid ConcurrencyStamp,
    string Name,
    string? Barcode,
    decimal? WeightGrams,
    int DisplayOrder,
    InventoryMode InventoryMode,
    bool AllowBackorder) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.WeightGrams).GreaterThan(0).When(x => x.WeightGrams.HasValue);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductVariantCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<UpdateProductVariantCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        UpdateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.EnsureContentCanBeChanged();
        var variant = await unitOfWork.Repository<ProductVariant>().FindByIdAsync(request.VariantId);
        if (variant is null || variant.ProductId != request.ProductId)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (variant.InventoryMode != request.InventoryMode &&
            await unitOfWork.Repository<InventoryItem>().AnyAsync([x => x.ProductVariantId == variant.Id]))
            return TResult<ProductManagementResult>.Failure(
                "Inventory mode cannot change after inventory tracking has been initialized.",
                ErrorCodes.UNPROCESSABLE_ENTITY);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        variant.UpdateDetails(request.Name, request.Barcode, request.WeightGrams, request.DisplayOrder);
        variant.ChangeInventoryPolicy(request.InventoryMode, request.AllowBackorder);
        await unitOfWork.Repository<ProductVariant>().UpdateAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);

        return TResult<ProductManagementResult>.Success(result);
    }
}
