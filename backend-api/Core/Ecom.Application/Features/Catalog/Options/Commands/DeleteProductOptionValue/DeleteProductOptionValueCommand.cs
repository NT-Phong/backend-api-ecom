using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record DeleteProductOptionValueCommand(
    Guid ProductId,
    Guid OptionId,
    Guid ValueId,
    Guid ConcurrencyStamp) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class DeleteProductOptionValueCommandValidator : AbstractValidator<DeleteProductOptionValueCommand>
{
    public DeleteProductOptionValueCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.ValueId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class DeleteProductOptionValueCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<DeleteProductOptionValueCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        DeleteProductOptionValueCommand request,
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
            [x => x.ProductOptionId == request.OptionId]);
        var value = values.SingleOrDefault(x => x.Id == request.ValueId);
        if (value is null)
            return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var mappings = await unitOfWork.Repository<ProductVariantOptionValue>().FindAsync(
            [x => x.ProductOptionValueId == request.ValueId]);
        product.RemoveOptionValue(options, values, mappings, request.OptionId, request.ValueId);
        await unitOfWork.Repository<ProductOptionValue>().DeleteAsync(value, cancellationToken);

        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
