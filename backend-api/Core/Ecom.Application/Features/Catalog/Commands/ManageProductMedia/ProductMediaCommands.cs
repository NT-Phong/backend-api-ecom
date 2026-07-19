using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductMedia;

public sealed record AttachProductMediaCommand(Guid ProductId, Guid ConcurrencyStamp, Guid MediaAssetId, int DisplayOrder, bool MakePrimary, string? Caption)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record UpdateProductMediaCommand(Guid ProductId, Guid MediaAssetId, Guid ConcurrencyStamp, int DisplayOrder, string? Caption)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record SetPrimaryProductMediaCommand(Guid ProductId, Guid MediaAssetId, Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record RemoveProductMediaCommand(Guid ProductId, Guid MediaAssetId, Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class AttachProductMediaCommandValidator : AbstractValidator<AttachProductMediaCommand>
{
    public AttachProductMediaCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.MediaAssetId).NotEmpty(); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); RuleFor(x => x.Caption).MaximumLength(500); }
}
public sealed class UpdateProductMediaCommandValidator : AbstractValidator<UpdateProductMediaCommand>
{
    public UpdateProductMediaCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.MediaAssetId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); RuleFor(x => x.Caption).MaximumLength(500); }
}
public sealed class SetPrimaryProductMediaCommandValidator : AbstractValidator<SetPrimaryProductMediaCommand>
{
    public SetPrimaryProductMediaCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.MediaAssetId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}
public sealed class RemoveProductMediaCommandValidator : AbstractValidator<RemoveProductMediaCommand>
{
    public RemoveProductMediaCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.MediaAssetId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}

public sealed class ProductMediaCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access,
    ICommerceMediaService commerceMediaService) :
    IRequestHandler<AttachProductMediaCommand, TResult<ProductManagementResult>>,
    IRequestHandler<UpdateProductMediaCommand, TResult<ProductManagementResult>>,
    IRequestHandler<SetPrimaryProductMediaCommand, TResult<ProductManagementResult>>,
    IRequestHandler<RemoveProductMediaCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(AttachProductMediaCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId);
        if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp);
        if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        await commerceMediaService.AttachToProductAsync(request.ProductId, request.MediaAssetId, request.DisplayOrder,
            request.MakePrimary, request.Caption, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }

    public async Task<TResult<ProductManagementResult>> Handle(UpdateProductMediaCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update); if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId); if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp); if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        product.UpdateMedia(links, request.MediaAssetId, request.DisplayOrder, request.Caption);
        await unitOfWork.Repository<ProductMedia>().UpdateRangeAsync(links, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }

    public async Task<TResult<ProductManagementResult>> Handle(SetPrimaryProductMediaCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update); if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId); if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp); if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        var asset = await unitOfWork.Repository<MediaAsset>().FindByIdAsync(request.MediaAssetId); if (asset is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        product.SetPrimaryMedia(links, request.MediaAssetId, asset.IsPubliclyUsable);
        await unitOfWork.Repository<ProductMedia>().UpdateRangeAsync(links, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }

    public async Task<TResult<ProductManagementResult>> Handle(RemoveProductMediaCommand request, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update); if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);
        var product = await unitOfWork.Repository<Product>().FindByIdAsync(request.ProductId); if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, request.ConcurrencyStamp); if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        var target = links.SingleOrDefault(x => x.MediaAssetId == request.MediaAssetId); if (target is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        product.RemoveMedia(links, request.MediaAssetId);
        await unitOfWork.Repository<ProductMedia>().DeleteAsync(target, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
