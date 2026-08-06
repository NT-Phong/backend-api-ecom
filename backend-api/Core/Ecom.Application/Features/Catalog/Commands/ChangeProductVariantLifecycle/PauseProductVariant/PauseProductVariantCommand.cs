using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductVariantLifecycle;

public sealed record PauseProductVariantCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class PauseProductVariantCommandValidator : AbstractValidator<PauseProductVariantCommand>
{
    public PauseProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class PauseProductVariantCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<PauseProductVariantCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        PauseProductVariantCommand request,
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

        variant.Pause();
        await unitOfWork.Repository<ProductVariant>().UpdateAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);

        return TResult<ProductManagementResult>.Success(result);
    }
}
