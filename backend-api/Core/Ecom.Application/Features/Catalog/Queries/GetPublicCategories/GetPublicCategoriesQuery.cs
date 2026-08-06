using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetPublicCategories;

public sealed record GetPublicCategoriesQuery : IRequest<TResult<IReadOnlyList<PublicCategoryDto>>>;

public sealed class GetPublicCategoriesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPublicCategoriesQuery, TResult<IReadOnlyList<PublicCategoryDto>>>
{
    public async Task<TResult<IReadOnlyList<PublicCategoryDto>>> Handle(
        GetPublicCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await PublicCategoryVisibility.LoadAsync(unitOfWork, cancellationToken);
        var byId = categories.ToDictionary(category => category.Id);
        IReadOnlyList<PublicCategoryDto> result = categories
            .Where(category => category.Status == CatalogStatus.Published && PublicCategoryVisibility.HasPublishedAncestors(category, byId))
            .Select(category => new PublicCategoryDto(
                category.Id,
                category.ParentId,
                category.Name,
                category.Slug,
                category.Description,
                category.DisplayOrder))
            .ToList();
        return TResult<IReadOnlyList<PublicCategoryDto>>.Success(result);
    }
}
