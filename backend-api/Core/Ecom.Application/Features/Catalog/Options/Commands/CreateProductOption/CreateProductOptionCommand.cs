using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record CreateProductOptionCommand(
    Guid ProductId,
    Guid ConcurrencyStamp,
    string Code,
    string Name,
    int DisplayOrder) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class CreateProductOptionCommandValidator : AbstractValidator<CreateProductOptionCommand>
{
    public CreateProductOptionCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProductOptionCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<CreateProductOptionCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        CreateProductOptionCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var options = await unitOfWork.Repository<ProductOption>().FindAsync([x => x.ProductId == request.ProductId]);
        var option = product.AddOption(options, request.Code, request.Name, request.DisplayOrder);
        await unitOfWork.Repository<ProductOption>().InsertAsync(option, cancellationToken);

        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
