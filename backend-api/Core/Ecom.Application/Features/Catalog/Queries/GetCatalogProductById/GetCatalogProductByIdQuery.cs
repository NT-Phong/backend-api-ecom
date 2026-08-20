using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetCatalogProductById;

public sealed record GetCatalogProductByIdQuery(Guid ProductId) : IRequest<TResult<CatalogProductManagementDto>>;

public sealed class GetCatalogProductByIdQueryValidator : AbstractValidator<GetCatalogProductByIdQuery>
{
    public GetCatalogProductByIdQueryValidator() => RuleFor(x => x.ProductId).NotEmpty();
}

public sealed class GetCatalogProductByIdQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access)
    : IRequestHandler<GetCatalogProductByIdQuery, TResult<CatalogProductManagementDto>>
{
    public async Task<TResult<CatalogProductManagementDto>> Handle(GetCatalogProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Read);
        if (!authorization.IsSuccess) return CatalogCommandSupport.Failure<CatalogProductManagementDto>(authorization);

        var product = await unitOfWork.Repository<Product>().QueryNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);
        if (product is null)
            return TResult<CatalogProductManagementDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

        var categories = await (
            from mapping in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join category in unitOfWork.Repository<Category>().QueryNoTracking() on mapping.CategoryId equals category.Id
            where mapping.ProductId == product.Id
            orderby mapping.IsPrimary descending, category.DisplayOrder, category.Name
            select new CategorySummaryDto(category.Id, category.Name, category.Slug, mapping.IsPrimary, category.DisplayOrder))
            .ToListAsync(cancellationToken);

        var variants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => x.ProductId == product.Id)
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(x => new CatalogProductVariantDto(x.Id, x.Sku, x.Name, x.Status, x.InventoryMode,
                x.AllowBackorder, x.Barcode, x.WeightGrams, x.DisplayOrder))
            .ToListAsync(cancellationToken);

        var media = await (
            from link in unitOfWork.Repository<ProductMedia>().QueryNoTracking()
            join asset in unitOfWork.Repository<MediaAsset>().QueryNoTracking() on link.MediaAssetId equals asset.Id
            where link.ProductId == product.Id
            orderby link.IsPrimary descending, link.DisplayOrder, link.Id
            select new CatalogProductMediaDto(asset.Id, asset.OriginalFileName, asset.ContentType, asset.MediaType,
                asset.Visibility, asset.ScanStatus, link.DisplayOrder, link.IsPrimary, link.Caption))
            .ToListAsync(cancellationToken);

        var pricePeriods = await (
            from price in unitOfWork.Repository<VariantPrice>().QueryNoTracking()
            join variant in unitOfWork.Repository<ProductVariant>().QueryNoTracking() on price.ProductVariantId equals variant.Id
            where variant.ProductId == product.Id
            orderby variant.DisplayOrder, price.EffectiveFrom descending, price.Id
            select new CatalogVariantPriceDto(price.Id, price.ProductVariantId, price.Amount, price.CurrencyCode,
                price.PriceType, price.MinQuantity, price.EffectiveFrom, price.EffectiveTo, price.PriceListId))
            .ToListAsync(cancellationToken);

        return TResult<CatalogProductManagementDto>.Success(new CatalogProductManagementDto(product.Id,
            product.ProducerId, product.Name, product.Slug, product.ShortDescription, product.Description,
            product.UsageInstructions, product.StorageInstructions, product.WarningText, product.MetaTitle,
            product.MetaDescription, product.Status, product.PublishedAt, product.UnpublishedAt,
            product.ConcurrencyStamp, categories, media, variants, pricePeriods, product.BrandName));
    }
}
