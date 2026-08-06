using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Categories;

public sealed record GetCatalogCategoryByIdQuery(Guid CategoryId) : IRequest<TResult<CatalogCategoryManagementDto>>;

public sealed class GetCatalogCategoryByIdQueryValidator : AbstractValidator<GetCatalogCategoryByIdQuery>
{
    public GetCatalogCategoryByIdQueryValidator() => RuleFor(x => x.CategoryId).NotEmpty();
}

public sealed class GetCatalogCategoryByIdQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogCategoryByIdQuery, TResult<CatalogCategoryManagementDto>>
{
    public async Task<TResult<CatalogCategoryManagementDto>> Handle(
        GetCatalogCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogCategories.Read);
        if (!authorization.IsSuccess)
            return TResult<CatalogCategoryManagementDto>.Failure(authorization.Error!, authorization.ErrorCode);

        var category = await unitOfWork.Repository<Category>().QueryNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (category is null)
            return TResult<CatalogCategoryManagementDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var parent = category.ParentId.HasValue
            ? await unitOfWork.Repository<Category>().QueryNoTracking()
                .SingleOrDefaultAsync(x => x.Id == category.ParentId.Value, cancellationToken)
            : null;
        var childrenCount = await unitOfWork.Repository<Category>().QueryNoTracking()
            .CountAsync(x => x.ParentId == category.Id, cancellationToken);
        var productCount = await unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            .CountAsync(x => x.CategoryId == category.Id, cancellationToken);
        var publishedProductCount = await (
            from mapping in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join product in unitOfWork.Repository<Product>().QueryNoTracking() on mapping.ProductId equals product.Id
            where mapping.CategoryId == category.Id && product.Status == ProductStatus.Published
            select mapping.Id).CountAsync(cancellationToken);
        return TResult<CatalogCategoryManagementDto>.Success(CatalogCategoryManagementMapper.Map(
            category,
            parent is null ? null : new CatalogCategoryParentDto(parent.Id, parent.Name, parent.Slug),
            childrenCount,
            productCount,
            publishedProductCount));
    }
}
