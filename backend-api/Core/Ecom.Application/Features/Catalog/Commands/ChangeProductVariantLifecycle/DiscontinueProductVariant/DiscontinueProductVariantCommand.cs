using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductVariantLifecycle;

public sealed record DiscontinueProductVariantCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class DiscontinueProductVariantCommandValidator : AbstractValidator<DiscontinueProductVariantCommand>
{
    public DiscontinueProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class DiscontinueProductVariantCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<DiscontinueProductVariantCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        DiscontinueProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        product.EnsureContentCanBeChanged();

        var variant = await unitOfWork.Repository<ProductVariant>().FindByIdAsync(request.VariantId);
        if (variant is null || variant.ProductId != product.Id)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        variant.Discontinue();
        await unitOfWork.Repository<ProductVariant>().UpdateAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);

        return TResult<ProductManagementResult>.Success(result);
    }
}
