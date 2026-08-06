using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record UpdateProductOptionValueCommand(
    Guid ProductId,
    Guid OptionId,
    Guid ValueId,
    Guid ConcurrencyStamp,
    string Value,
    int DisplayOrder) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class UpdateProductOptionValueCommandValidator : AbstractValidator<UpdateProductOptionValueCommand>
{
    public UpdateProductOptionValueCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.ValueId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductOptionValueCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<UpdateProductOptionValueCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        UpdateProductOptionValueCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var options = await unitOfWork.Repository<ProductOption>().FindAsync([x => x.ProductId == request.ProductId]);
        var values = await unitOfWork.Repository<ProductOptionValue>().FindAsync(
            [x => options.Select(option => option.Id).Contains(x.ProductOptionId)]);
        product.UpdateOptionValue(options, values, request.OptionId, request.ValueId, request.Value, request.DisplayOrder);
        await unitOfWork.Repository<ProductOptionValue>().UpdateRangeAsync(values, cancellationToken);

        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
