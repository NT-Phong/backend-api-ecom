using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductVariants;

public sealed record CreateProductVariantCommand(Guid ProductId, Guid ConcurrencyStamp, string Sku, string Name, InventoryMode InventoryMode,
    bool AllowBackorder, string? Barcode, decimal? WeightGrams, int DisplayOrder) : IRequest<TResult<ProductVariantManagementResult>>, ITransactionalRequest;
public sealed record UpdateProductVariantCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp, string Name, string? Barcode,
    decimal? WeightGrams, int DisplayOrder, InventoryMode InventoryMode, bool AllowBackorder) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.Sku).NotEmpty().MaximumLength(100); RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Barcode).MaximumLength(100); RuleFor(x => x.WeightGrams).GreaterThan(0).When(x => x.WeightGrams.HasValue); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VariantId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Barcode).MaximumLength(100); RuleFor(x => x.WeightGrams).GreaterThan(0).When(x => x.WeightGrams.HasValue); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class ProductVariantCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access) :
    IRequestHandler<CreateProductVariantCommand, TResult<ProductVariantManagementResult>>, IRequestHandler<UpdateProductVariantCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductVariantManagementResult>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update); if (!authorization.IsSuccess) return TResult<ProductVariantManagementResult>.Failure(authorization.Error!, authorization.ErrorCode);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId);
        if (product is null) return TResult<ProductVariantManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp);
        if (version is not null) return CatalogCommandSupport.Failure<ProductVariantManagementResult>(version);
        product.EnsureContentCanBeChanged();
        if (await unitOfWork.Repository<ProductVariant>().AnyAsync([x => x.Sku == request.Sku.Trim()])) return TResult<ProductVariantManagementResult>.Failure("Product SKU already exists.", ErrorCodes.ALREADY_EXISTS);
        var variant = ProductVariant.Create(request.ProductId, request.Sku, request.Name, request.InventoryMode, request.AllowBackorder,
            request.Barcode, request.WeightGrams, request.DisplayOrder);
        await unitOfWork.Repository<ProductVariant>().InsertAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductVariantManagementResult>.Success(new(variant.Id, product.Id, result.ConcurrencyStamp));
    }

    public async Task<TResult<ProductManagementResult>> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update); if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId);
        if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp);
        if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        product.EnsureContentCanBeChanged();
        var variant = await unitOfWork.Repository<ProductVariant>().FindByIdAsync(request.VariantId);
        if (variant is null || variant.ProductId != request.ProductId) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        variant.UpdateDetails(request.Name, request.Barcode, request.WeightGrams, request.DisplayOrder);
        variant.ChangeInventoryPolicy(request.InventoryMode, request.AllowBackorder);
        await unitOfWork.Repository<ProductVariant>().UpdateAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
