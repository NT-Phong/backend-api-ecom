using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductLifecycle;

public sealed record SubmitProductForReviewCommand(Guid ProductId, Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record PublishProductCommand(Guid ProductId, Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record PauseProductCommand(Guid ProductId, Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record DiscontinueProductCommand(Guid ProductId, Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class SubmitProductForReviewCommandValidator : AbstractValidator<SubmitProductForReviewCommand>
{
    public SubmitProductForReviewCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}
public sealed class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
{
    public PublishProductCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}
public sealed class PauseProductCommandValidator : AbstractValidator<PauseProductCommand>
{
    public PauseProductCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}
public sealed class DiscontinueProductCommandValidator : AbstractValidator<DiscontinueProductCommand>
{
    public DiscontinueProductCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}

public sealed class ProductLifecycleCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access,
    IEffectivePriceResolver effectivePriceResolver) :
    IRequestHandler<SubmitProductForReviewCommand, TResult<ProductManagementResult>>,
    IRequestHandler<PublishProductCommand, TResult<ProductManagementResult>>,
    IRequestHandler<PauseProductCommand, TResult<ProductManagementResult>>,
    IRequestHandler<DiscontinueProductCommand, TResult<ProductManagementResult>>
{
    public Task<TResult<ProductManagementResult>> Handle(SubmitProductForReviewCommand request, CancellationToken ct) =>
        ChangeAsync(request.ProductId, request.ConcurrencyStamp, Permissions.CatalogProducts.Publish, x => x.SubmitForReview(), ct);
    public Task<TResult<ProductManagementResult>> Handle(PauseProductCommand request, CancellationToken ct) =>
        ChangeAsync(request.ProductId, request.ConcurrencyStamp, Permissions.CatalogProducts.Publish, x => x.Pause(DateTime.UtcNow), ct);
    public Task<TResult<ProductManagementResult>> Handle(DiscontinueProductCommand request, CancellationToken ct) =>
        ChangeAsync(request.ProductId, request.ConcurrencyStamp, Permissions.CatalogProducts.Discontinue, x => x.Discontinue(DateTime.UtcNow), ct);

    public async Task<TResult<ProductManagementResult>> Handle(PublishProductCommand request, CancellationToken ct)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Publish); if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId); if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp); if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        var hasPublicProducer = await unitOfWork.Repository<Producer>().QueryNoTracking().AnyAsync(x => x.Id == product.ProducerId && x.PublicStatus == PublicStatus.Published && x.IsVerified, ct);
        var hasPrimaryCategory = await (from map in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
                                        join category in unitOfWork.Repository<Category>().QueryNoTracking() on map.CategoryId equals category.Id
                                        where map.ProductId == product.Id && map.IsPrimary && category.Status == CatalogStatus.Published select map.Id).AnyAsync(ct);
        var hasPrimaryMedia = await (from map in unitOfWork.Repository<ProductMedia>().QueryNoTracking()
                                     join media in unitOfWork.Repository<MediaAsset>().QueryNoTracking() on map.MediaAssetId equals media.Id
                                     where map.ProductId == product.Id && map.IsPrimary && media.Visibility == MediaVisibility.Public && media.ScanStatus == MediaScanStatus.Clean select map.Id).AnyAsync(ct);
        var activeVariants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking().Where(x => x.ProductId == product.Id && x.Status == VariantStatus.Active).Select(x => x.Id).ToListAsync(ct);
        var prices = await effectivePriceResolver.ResolveForVariantsAsync(activeVariants, DateTime.UtcNow, ct);
        product.Publish(DateTime.UtcNow, hasPublicProducer && hasPrimaryCategory, hasPrimaryMedia, activeVariants.Count > 0, prices.Count > 0);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, ct);
        return TResult<ProductManagementResult>.Success(result);
    }

    private async Task<TResult<ProductManagementResult>> ChangeAsync(Guid id, Guid stamp, string permission, Action<Product> transition, CancellationToken ct)
    {
        var authorization = access.Ensure(permission); if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(id); if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, stamp); if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        transition(product);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, ct);
        return TResult<ProductManagementResult>.Success(result);
    }
}
