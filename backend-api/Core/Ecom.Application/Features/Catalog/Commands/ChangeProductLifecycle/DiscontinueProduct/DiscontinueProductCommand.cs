using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ChangeProductLifecycle;

public sealed record DiscontinueProductCommand(Guid ProductId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class DiscontinueProductCommandValidator : AbstractValidator<DiscontinueProductCommand>
{
    public DiscontinueProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class DiscontinueProductCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<DiscontinueProductCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        DiscontinueProductCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Discontinue, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.Discontinue(DateTime.UtcNow);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);

        return TResult<ProductManagementResult>.Success(result);
    }
}
