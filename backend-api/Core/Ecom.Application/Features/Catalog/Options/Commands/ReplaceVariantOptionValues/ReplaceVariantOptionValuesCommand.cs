using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Options;

public sealed record ReplaceVariantOptionValuesCommand(
    Guid ProductId,
    Guid VariantId,
    Guid ConcurrencyStamp,
    IReadOnlyList<Guid> OptionValueIds) : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class ReplaceVariantOptionValuesCommandValidator : AbstractValidator<ReplaceVariantOptionValuesCommand>
{
    public ReplaceVariantOptionValuesCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.OptionValueIds).NotEmpty();
        RuleFor(x => x.OptionValueIds.Distinct().Count()).Equal(x => x.OptionValueIds.Count);
    }
}

public sealed class ReplaceVariantOptionValuesCommandHandler(
    IUnitOfWork unitOfWork,
    ICatalogProductMutationService mutation)
    : IRequestHandler<ReplaceVariantOptionValuesCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(
        ReplaceVariantOptionValuesCommand request,
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
        var variants = await unitOfWork.Repository<ProductVariant>().FindAsync([x => x.ProductId == request.ProductId]);
        var mappings = await unitOfWork.Repository<ProductVariantOptionValue>().FindAsync(
            [x => x.ProductVariantId == request.VariantId]);
        var removed = mappings.ToList();
        var replacements = product.ReplaceVariantOptionValues(
            variants, options, values, mappings, request.VariantId, request.OptionValueIds);
        if (removed.Count > 0)
            await unitOfWork.Repository<ProductVariantOptionValue>().DeleteRangeAsync(removed, cancellationToken);
        await unitOfWork.Repository<ProductVariantOptionValue>().InsertRangeAsync(replacements, cancellationToken);

        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
