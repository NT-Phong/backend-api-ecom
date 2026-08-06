using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Commands.UpdateProductDetails;

public sealed class UpdateProductDetailsCommandHandler(IUnitOfWork unitOfWork, ICatalogProductMutationService mutation)
    : IRequestHandler<UpdateProductDetailsCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(UpdateProductDetailsCommand request, CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp, Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return TResult<ProductManagementResult>.Failure(loaded.Error!, loaded.ErrorCode);
        var product = loaded.Data;
        if (product.Slug != request.Slug.Trim() && await unitOfWork.Repository<Product>().AnyAsync([x => x.Slug == request.Slug.Trim()]))
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        product.UpdateDetails(request.Name, request.Slug, request.ShortDescription, request.Description,
            request.UsageInstructions, request.StorageInstructions, request.WarningText, request.MetaTitle, request.MetaDescription);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
