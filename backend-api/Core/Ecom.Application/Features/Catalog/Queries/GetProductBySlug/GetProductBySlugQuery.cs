using Ecom.Application.Features.Catalog.Common;

namespace Ecom.Application.Features.Catalog.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string Slug) : IRequest<TResult<ProductDetailDto>>;

public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugQueryValidator() => RuleFor(x => x.Slug).NotEmpty().MaximumLength(350);
}
