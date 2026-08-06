using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductLifecycle;

public sealed record PublishProductCommand(Guid ProductId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
{
    public PublishProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class PublishProductCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation,
    IEffectivePriceResolver effectivePriceResolver)
    : IRequestHandler<PublishProductCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        PublishProductCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Publish, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        var now = DateTime.UtcNow;

        var hasPublicProducer = await unitOfWork.Repository<Producer>()
            .QueryNoTracking()
            .AnyAsync(x => x.Id == product.ProducerId
                && x.PublicStatus == PublicStatus.Published
                && x.IsVerified, cancellationToken);

        var hasPrimaryCategory = await (
            from map in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join category in unitOfWork.Repository<Category>().QueryNoTracking()
                on map.CategoryId equals category.Id
            where map.ProductId == product.Id
                && map.IsPrimary
                && category.Status == CatalogStatus.Published
            select map.Id).AnyAsync(cancellationToken);

        var hasPrimaryMedia = await (
            from map in unitOfWork.Repository<ProductMedia>().QueryNoTracking()
            join media in unitOfWork.Repository<MediaAsset>().QueryNoTracking()
                on map.MediaAssetId equals media.Id
            where map.ProductId == product.Id
                && map.IsPrimary
                && media.Visibility == MediaVisibility.Public
                && media.ScanStatus == MediaScanStatus.Clean
            select map.Id).AnyAsync(cancellationToken);

        var activeVariants = await unitOfWork.Repository<ProductVariant>()
            .QueryNoTracking()
            .Where(x => x.ProductId == product.Id && x.Status == VariantStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var prices = await effectivePriceResolver.ResolveForVariantsAsync(activeVariants, now, cancellationToken);

        product.Publish(now, hasPublicProducer && hasPrimaryCategory, hasPrimaryMedia,
            activeVariants.Count > 0, prices.Count > 0);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);

        return TResult<ProductManagementResult>.Success(result);
    }
}
