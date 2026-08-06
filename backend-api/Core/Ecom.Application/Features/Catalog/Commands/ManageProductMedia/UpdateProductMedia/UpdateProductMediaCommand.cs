using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductMedia;

public sealed record UpdateProductMediaCommand(
    Guid ProductId,
    Guid MediaAssetId,
    Guid ConcurrencyStamp,
    int DisplayOrder,
    string? Caption) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class UpdateProductMediaCommandValidator : AbstractValidator<UpdateProductMediaCommand>
{
    public UpdateProductMediaCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MediaAssetId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Caption).MaximumLength(500);
    }
}

public sealed class UpdateProductMediaCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<UpdateProductMediaCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        UpdateProductMediaCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        product.UpdateMedia(links, request.MediaAssetId, request.DisplayOrder, request.Caption);
        await unitOfWork.Repository<ProductMedia>().UpdateRangeAsync(links, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
