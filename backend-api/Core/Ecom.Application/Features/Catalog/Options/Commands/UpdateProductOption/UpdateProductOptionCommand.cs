using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record UpdateProductOptionCommand(
    Guid ProductId,
    Guid OptionId,
    Guid ConcurrencyStamp,
    string Name,
    int DisplayOrder) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class UpdateProductOptionCommandValidator : AbstractValidator<UpdateProductOptionCommand>
{
    public UpdateProductOptionCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductOptionCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<UpdateProductOptionCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        UpdateProductOptionCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var options = await unitOfWork.Repository<ProductOption>().FindAsync([x => x.ProductId == request.ProductId]);
        product.UpdateOption(options, request.OptionId, request.Name, request.DisplayOrder);
        await unitOfWork.Repository<ProductOption>().UpdateRangeAsync(options, cancellationToken);

        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
