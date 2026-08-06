using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ManageProductMedia;

public sealed record RemoveProductMediaCommand(Guid ProductId, Guid MediaAssetId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class RemoveProductMediaCommandValidator : AbstractValidator<RemoveProductMediaCommand>
{
    public RemoveProductMediaCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MediaAssetId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class RemoveProductMediaCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<RemoveProductMediaCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        RemoveProductMediaCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var links = await unitOfWork.Repository<ProductMedia>().FindAsync([x => x.ProductId == request.ProductId]);
        var target = links.SingleOrDefault(x => x.MediaAssetId == request.MediaAssetId);
        if (target is null)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        product.RemoveMedia(links, request.MediaAssetId);
        await unitOfWork.Repository<ProductMedia>().DeleteAsync(target, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
