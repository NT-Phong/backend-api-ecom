using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductVariantLifecycle;

public sealed record PauseProductVariantCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record ActivateProductVariantCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;
public sealed record DiscontinueProductVariantCommand(Guid ProductId, Guid VariantId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class PauseProductVariantCommandValidator : AbstractValidator<PauseProductVariantCommand>
{
    public PauseProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VariantId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
public sealed class ActivateProductVariantCommandValidator : AbstractValidator<ActivateProductVariantCommand>
{
    public ActivateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VariantId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
public sealed class DiscontinueProductVariantCommandValidator : AbstractValidator<DiscontinueProductVariantCommand>
{
    public DiscontinueProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VariantId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class ProductVariantLifecycleCommandHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access) :
    IRequestHandler<PauseProductVariantCommand, TResult<ProductManagementResult>>,
    IRequestHandler<ActivateProductVariantCommand, TResult<ProductManagementResult>>,
    IRequestHandler<DiscontinueProductVariantCommand, TResult<ProductManagementResult>>
{
    public Task<TResult<ProductManagementResult>> Handle(PauseProductVariantCommand request, CancellationToken cancellationToken) =>
        ChangeAsync(request.ProductId, request.VariantId, request.ConcurrencyStamp, variant => variant.Pause(), cancellationToken);

    public Task<TResult<ProductManagementResult>> Handle(ActivateProductVariantCommand request, CancellationToken cancellationToken) =>
        ChangeAsync(request.ProductId, request.VariantId, request.ConcurrencyStamp, variant => variant.Activate(), cancellationToken);

    public Task<TResult<ProductManagementResult>> Handle(DiscontinueProductVariantCommand request, CancellationToken cancellationToken) =>
        ChangeAsync(request.ProductId, request.VariantId, request.ConcurrencyStamp, variant => variant.Discontinue(), cancellationToken);

    private async Task<TResult<ProductManagementResult>> ChangeAsync(Guid productId, Guid variantId, Guid concurrencyStamp,
        Action<ProductVariant> transition, CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Update);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(authorization);

        var product = await unitOfWork.Repository<Product>().FindByIdAsync(productId);
        if (product is null) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var version = CatalogCommandSupport.EnsureVersion(product, concurrencyStamp);
        if (version is not null) return CatalogCommandSupport.Failure<ProductManagementResult>(version);
        product.EnsureContentCanBeChanged();

        var variant = await unitOfWork.Repository<ProductVariant>().FindByIdAsync(variantId);
        if (variant is null || variant.ProductId != product.Id)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        transition(variant);
        await unitOfWork.Repository<ProductVariant>().UpdateAsync(variant, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
