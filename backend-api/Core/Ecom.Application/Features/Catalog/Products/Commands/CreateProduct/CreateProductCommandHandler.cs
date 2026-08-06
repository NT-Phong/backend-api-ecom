using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<CreateProductCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Create);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        if (!await unitOfWork.Repository<Producer>().ExistsAsync(request.ProducerId))
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        if (await unitOfWork.Repository<Product>().AnyAsync([x => x.Slug == request.Slug.Trim()]))
            return TResult<ProductManagementResult>.Failure("Product slug already exists.", ErrorCodes.ALREADY_EXISTS);

        var product = Product.Create(request.ProducerId, request.Name, request.Slug);
        product.UpdateDetails(request.Name, request.Slug, request.ShortDescription, request.Description,
            request.UsageInstructions, request.StorageInstructions, request.WarningText, request.MetaTitle, request.MetaDescription);
        await unitOfWork.Repository<Product>().InsertAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(new(product.Id, product.Slug, product.Status, product.ConcurrencyStamp));
    }
}
