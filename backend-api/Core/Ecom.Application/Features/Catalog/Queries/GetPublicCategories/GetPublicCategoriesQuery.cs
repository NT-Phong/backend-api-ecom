using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetPublicCategories;

public sealed record GetPublicCategoriesQuery : IRequest<TResult<IReadOnlyList<PublicCategoryDto>>>;

public sealed class GetPublicCategoriesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPublicCategoriesQuery, TResult<IReadOnlyList<PublicCategoryDto>>>
{
    public async Task<TResult<IReadOnlyList<PublicCategoryDto>>> Handle(GetPublicCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.Repository<Category>().QueryNoTracking()
            .Where(x => x.Status == CatalogStatus.Published)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new PublicCategoryDto(x.Id, x.ParentId, x.Name, x.Slug, x.Description, x.DisplayOrder))
            .ToListAsync(cancellationToken);
        return TResult<IReadOnlyList<PublicCategoryDto>>.Success(categories);
    }
}
