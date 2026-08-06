using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductVariants;

public sealed record CreateProductVariantCommand(
    Guid ProductId,
    Guid ConcurrencyStamp,
    string Sku,
    string Name,
    InventoryMode InventoryMode,
    bool AllowBackorder,
    string? Barcode,
    decimal? WeightGrams,
    int DisplayOrder) : IRequest<TResult<ProductVariantManagementResult>>, ITransactionalRequest;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.WeightGrams).GreaterThan(0).When(x => x.WeightGrams.HasValue);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProductVariantCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<CreateProductVariantCommand, TResult<ProductVariantManagementResult>>
{
    public async Task<TResult<ProductVariantManagementResult>> Handle(
        CreateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductVariantManagementResult>(loaded);

        var product = loaded.Data;
        product.EnsureContentCanBeChanged();
        if (await unitOfWork.Repository<ProductVariant>().AnyAsync([x => x.Sku == request.Sku.Trim()]))
            return TResult<ProductVariantManagementResult>.Failure("Product SKU already exists.", ErrorCodes.ALREADY_EXISTS);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var variant = ProductVariant.Create(request.ProductId, request.Sku, request.Name, request.InventoryMode,
            request.AllowBackorder, request.Barcode, request.WeightGrams, request.DisplayOrder);
        await unitOfWork.Repository<ProductVariant>().InsertAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);

        return TResult<ProductVariantManagementResult>.Success(new(variant.Id, product.Id, result.ConcurrencyStamp));
    }
}
