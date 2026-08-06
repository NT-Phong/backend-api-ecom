using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record DeleteProductOptionCommand(
    Guid ProductId,
    Guid OptionId,
    Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class DeleteProductOptionCommandValidator : AbstractValidator<DeleteProductOptionCommand>
{
    public DeleteProductOptionCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class DeleteProductOptionCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<DeleteProductOptionCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        DeleteProductOptionCommand request,
        CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess)
            return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);

        var product = loaded.Data;
        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var options = await unitOfWork.Repository<ProductOption>().FindAsync([x => x.ProductId == request.ProductId]);
        var option = options.SingleOrDefault(x => x.Id == request.OptionId);
        if (option is null)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var values = await unitOfWork.Repository<ProductOptionValue>().FindAsync(
            [x => x.ProductOptionId == request.OptionId]);
        var mappings = values.Count == 0
            ? []
            : await unitOfWork.Repository<ProductVariantOptionValue>().FindAsync(
                [x => values.Select(value => value.Id).Contains(x.ProductOptionValueId)]);
        var removedValues = product.RemoveOption(options, values, mappings, request.OptionId);
        if (removedValues.Count > 0)
            await unitOfWork.Repository<ProductOptionValue>().DeleteRangeAsync(removedValues, cancellationToken);
        await unitOfWork.Repository<ProductOption>().DeleteAsync(option, cancellationToken);

        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
