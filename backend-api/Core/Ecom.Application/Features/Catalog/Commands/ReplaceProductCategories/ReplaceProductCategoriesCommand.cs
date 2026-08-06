using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Products.Services;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Commands.ReplaceProductCategories;

public sealed record CategoryAssignmentRequest(Guid CategoryId, bool IsPrimary);
public sealed record ReplaceProductCategoriesCommand(Guid ProductId, Guid ConcurrencyStamp, IReadOnlyList<CategoryAssignmentRequest> Categories)
    : IRequest<TResult<ProductManagementResult>>, ITransactionalRequest;

public sealed class ReplaceProductCategoriesCommandValidator : AbstractValidator<ReplaceProductCategoriesCommand>
{
    public ReplaceProductCategoriesCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.Categories).NotEmpty();
        RuleForEach(x => x.Categories).ChildRules(x => x.RuleFor(y => y.CategoryId).NotEmpty());
        RuleFor(x => x.Categories.Count(y => y.IsPrimary)).Equal(1).WithMessage("Exactly one primary category is required.");
        RuleFor(x => x.Categories.Select(y => y.CategoryId).Distinct().Count()).Equal(x => x.Categories.Count)
            .WithMessage("Product categories must be unique.");
    }
}

public sealed class ReplaceProductCategoriesCommandHandler(IUnitOfWork unitOfWork, ICatalogProductMutationService mutation)
    : IRequestHandler<ReplaceProductCategoriesCommand, TResult<ProductManagementResult>>
{
    public async Task<TResult<ProductManagementResult>> Handle(ReplaceProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        var loaded = await mutation.LoadAsync(request.ProductId, request.ConcurrencyStamp,
            Permissions.CatalogProducts.Update, cancellationToken);
        if (!loaded.IsSuccess) return CatalogCommandSupport.Failure<ProductManagementResult>(loaded);
        var product = loaded.Data;
        var requestedIds = request.Categories.Select(x => x.CategoryId).ToArray();
        var actualCount = await unitOfWork.Repository<Category>().QueryNoTracking().CountAsync(x => requestedIds.Contains(x.Id), cancellationToken);
        if (actualCount != requestedIds.Length) return TResult<ProductManagementResult>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        product.ReturnToReviewIfPublished(DateTime.UtcNow);
        var existing = await unitOfWork.Repository<ProductCategory>().FindAsync([x => x.ProductId == product.Id]);
        var removed = existing.ToList();
        var replacements = product.ReplaceCategories(existing,
            request.Categories.Select(x => new ProductCategoryAssignment(x.CategoryId, x.IsPrimary)).ToArray());
        if (removed.Count > 0) await unitOfWork.Repository<ProductCategory>().DeleteRangeAsync(removed, cancellationToken);
        await unitOfWork.Repository<ProductCategory>().InsertRangeAsync(replacements, cancellationToken);
        var result = CatalogCommandSupport.RenewVersion(product);
        await unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
        return TResult<ProductManagementResult>.Success(result);
    }
}
