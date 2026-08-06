using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductMedia;

public sealed record SetPrimaryProductMediaCommand(Guid ProductId, Guid MediaAssetId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class SetPrimaryProductMediaCommandValidator : AbstractValidator<SetPrimaryProductMediaCommand>
{
    public SetPrimaryProductMediaCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MediaAssetId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class SetPrimaryProductMediaCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<SetPrimaryProductMediaCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        SetPrimaryProductMediaCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        var asset = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(request.MediaAssetId);
        if (asset is null)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        product.SetPrimaryMedia(links, request.MediaAssetId, asset.IsPubliclyUsable);
        await unitOfWork.Repository<ProductMedia>().UpdateRangeAsync(links, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
