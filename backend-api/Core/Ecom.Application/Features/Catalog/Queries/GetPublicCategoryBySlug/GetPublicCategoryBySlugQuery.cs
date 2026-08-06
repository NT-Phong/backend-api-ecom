using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetPublicCategories;

public sealed record GetPublicCategoryBySlugQuery(string Slug) : IRequest<TResult<PublicCategoryDto>>;

public sealed class GetPublicCategoryBySlugQueryValidator : AbstractValidator<GetPublicCategoryBySlugQuery>
{
    public GetPublicCategoryBySlugQueryValidator() => RuleFor(x => x.Slug).NotEmpty().MaximumLength(250);
}

public sealed class GetPublicCategoryBySlugQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPublicCategoryBySlugQuery, TResult<PublicCategoryDto>>
{
    public async Task<TResult<PublicCategoryDto>> Handle(
        GetPublicCategoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim();
        var categories = await PublicCategoryVisibility.LoadAsync(unitOfWork, cancellationToken);
        var byId = categories.ToDictionary(category => category.Id);
        var category = categories
            .Where(category => category.Slug == slug && category.Status == CatalogStatus.Published &&
                               PublicCategoryVisibility.HasPublishedAncestors(category, byId))
            .Select(category => new PublicCategoryDto(
                category.Id,
                category.ParentId,
                category.Name,
                category.Slug,
                category.Description,
                category.DisplayOrder))
            .SingleOrDefault();
        return category is null
            ? TResult<PublicCategoryDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND)
            : TResult<PublicCategoryDto>.Success(category);
    }
}
