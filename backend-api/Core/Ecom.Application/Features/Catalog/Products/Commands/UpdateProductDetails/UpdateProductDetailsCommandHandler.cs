using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Commands.UpdateProductDetails;

public sealed class UpdateProductDetailsCommandHandler(IUnitOfWork unitOfWork, ICatalogProductMutationService mutation,
    ICatalogAuditWriter auditWriter)
    : IRequestHandler<UpdateProductDetailsCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(UpdateProductDetailsCommand request, CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp, Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return TResult<ProductManagementResult>.Failure(loaded.Error!, loaded.ErrorCode);
        var product = loaded.Data;
        var oldSlug = product.Slug;
        var oldConcurrencyStamp = product.ConcurrencyStamp;
        var newSlug = request.Slug.Trim();
        var slugChanged = !string.Equals(oldSlug, newSlug, StringComparison.Ordinal);
        if (slugChanged && await unitOfWork.Repository<Product>().AnyAsync([x => x.Slug == newSlug && x.Id != product.Id]))
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        var conflictingHistory = slugChanged
            ? await unitOfWork.Repository<ProductSlugHistory>().QueryNoTracking()
                .SingleOrDefaultAsync(x => x.Slug == newSlug, cancellationToken)
            : null;
        if (conflictingHistory is not null && conflictingHistory.ProductId != product.Id)
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        product.UpdateDetails(request.Name, newSlug, request.ShortDescription, request.Description,
            request.UsageInstructions, request.StorageInstructions, request.WarningText, request.MetaTitle, request.MetaDescription,
            request.BrandName, request.Standard);
        if (slugChanged)
        {
            if (conflictingHistory is not null)
                await unitOfWork.Repository<ProductSlugHistory>().DeleteAsync(conflictingHistory, cancellationToken);
            await unitOfWork.Repository<ProductSlugHistory>().InsertAsync(ProductSlugHistory.Create(product.Id, oldSlug), cancellationToken);
        }
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        if (slugChanged)
        {
            await auditWriter.WriteAsync("catalog.product.slug-changed", product.Id,
                new { Slug = oldSlug, ConcurrencyStamp = oldConcurrencyStamp },
                new { Slug = product.Slug, product.ConcurrencyStamp }, cancellationToken);
        }
        return TResult<ProductManagementResult>.Success(result);
    }
}
