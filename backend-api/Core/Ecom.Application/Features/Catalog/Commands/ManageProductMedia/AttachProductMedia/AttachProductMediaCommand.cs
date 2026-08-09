using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductMedia;

public sealed record AttachProductMediaCommand(
    Guid ProductId,
    Guid ConcurrencyStamp,
    Guid MediaAssetId,
    int DisplayOrder,
    bool MakePrimary,
    string? Caption) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class AttachProductMediaCommandValidator : AbstractValidator<AttachProductMediaCommand>
{
    public AttachProductMediaCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.MediaAssetId).NotEmpty();
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Caption).MaximumLength(500);
    }
}

public sealed class AttachProductMediaCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation,
    IMediaAccessService mediaAccess)
    : IRequestHandler<AttachProductMediaCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        AttachProductMediaCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        var asset = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(request.MediaAssetId);
        if (asset is null)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var mediaAuthorization = mediaAccess.EnsureOwnerOrManager(asset);
        if (!mediaAuthorization.IsSuccess)
            return TResult<ProductManagementResult>.Failure(mediaAuthorization.Error!, mediaAuthorization.ErrorCode);
        if (!asset.IsPubliclyUsable)
            return TResult<ProductManagementResult>.Failure("MEDIA_NOT_READY", ErrorCodes.BAD_REQUEST);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == product.Id]);
        var existing = links.ToList();
        var link = product.AttachMedia(links, request.MediaAssetId, request.DisplayOrder, request.MakePrimary,
            asset.IsPubliclyUsable, request.Caption);
        if (request.MakePrimary && existing.Count > 0)
            await unitOfWork.Repository<ProductMedia>().UpdateRangeAsync(existing, cancellationToken);
        await unitOfWork.Repository<ProductMedia>().InsertAsync(link, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
