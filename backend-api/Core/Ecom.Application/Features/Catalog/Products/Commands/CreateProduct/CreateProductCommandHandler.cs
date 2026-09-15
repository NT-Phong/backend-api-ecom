using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access,
    ICatalogAuditWriter auditWriter)
    : IRequestHandler<CreateProductCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Create);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        if (!await unitOfWork.Repository<Producer>().ExistsAsync(request.ProducerId))
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var slug = request.Slug.Trim();
        if (await unitOfWork.Repository<Product>().AnyAsync([x => x.Slug == slug])
            || await unitOfWork.Repository<ProductSlugHistory>().AnyAsync([x => x.Slug == slug]))
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        var product = Product.Create(request.ProducerId, request.Name, slug, request.Standard);
        product.UpdateDetails(request.Name, slug, request.ShortDescription, request.Description,
            request.UsageInstructions, request.StorageInstructions, request.WarningText, request.MetaTitle, request.MetaDescription,
            request.BrandName, request.Standard);
        await unitOfWork.Repository<Product>().InsertAsync(product, cancellationToken);
        await auditWriter.WriteAsync("catalog.product.created", product.Id, null,
            new { product.Id, product.Slug, Status = product.Status.ToString(), product.ConcurrencyStamp, product.ProducerId,
                ChangedFields = new[] { "Name", "Slug", "ShortDescription", "Description", "UsageInstructions", "StorageInstructions", "WarningText", "MetaTitle", "MetaDescription", "BrandName", "Standard" } },
            cancellationToken);
        return TResult<ProductManagementResult>.Success(new(product.Id, product.Slug, product.Status, product.ConcurrencyStamp));
    }
}
