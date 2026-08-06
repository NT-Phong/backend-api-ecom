using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetProductList;

public sealed class GetProductListQueryHandler(
    IUnitOfWork unitOfWork,
    IEffectivePriceResolver effectivePriceResolver,
    IProductMediaReader productMediaReader)
    : IRequestHandler<GetProductListQuery, TResult<PaginatedList<ProductListItemDto>>>
{
    public async Task<TResult<PaginatedList<ProductListItemDto>>> Handle(GetProductListQuery request,
        CancellationToken cancellationToken)
    {
        var products = unitOfWork.Repository<Product>().QueryNoTracking();
        var producers = unitOfWork.Repository<Producer>().QueryNoTracking();
        var mappings = unitOfWork.Repository<ProductCategory>().QueryNoTracking();
        var categories = unitOfWork.Repository<Category>().QueryNoTracking();

        var asOfUtc = DateTime.UtcNow;
        var productPrices = effectivePriceResolver.QueryEffectiveProductPrices(asOfUtc);

        var query =
            from product in products
            join producer in producers on product.ProducerId equals producer.Id
            join mapping in mappings.Where(x => x.IsPrimary) on product.Id equals mapping.ProductId
            join category in categories on mapping.CategoryId equals category.Id
            join price in productPrices on product.Id equals price.ProductId
            where product.Status == ProductStatus.Published
                  && producer.PublicStatus == PublicStatus.Published
                  && producer.IsVerified
                  && category.Status == CatalogStatus.Published
            select new ProductRow(product.Id, product.Slug, product.Name, product.ShortDescription, product.PublishedAt,
                producer.Id, producer.Code, producer.Name, producer.Description, producer.WebsiteUrl,
                category.Id, category.Name, category.Slug, category.DisplayOrder, price.Amount);

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var search = request.Q.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) ||
                                     (x.ShortDescription != null && x.ShortDescription.ToLower().Contains(search)));
        }
        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var slug = request.CategorySlug.Trim();
            var matchingProductIds =
                from mapping in mappings
                join category in categories on mapping.CategoryId equals category.Id
                where category.Slug == slug && category.Status == CatalogStatus.Published
                select mapping.ProductId;
            query = query.Where(x => matchingProductIds.Contains(x.Id));
        }
        if (request.ProducerId.HasValue)
            query = query.Where(x => x.ProducerId == request.ProducerId.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(x => x.FromPrice >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(x => x.FromPrice <= request.MaxPrice.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return TResult<PaginatedList<ProductListItemDto>>.Success(PaginatedList<ProductListItemDto>.Create([], 0, request.Page, request.PageSize));

        var ordered = request.Sort switch
        {
            ProductSort.NameAscending => query.OrderBy(x => x.Name).ThenBy(x => x.Id),
            ProductSort.PriceAscending => query.OrderBy(x => x.FromPrice).ThenBy(x => x.Id),
            ProductSort.PriceDescending => query.OrderByDescending(x => x.FromPrice).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.PublishedAt).ThenBy(x => x.Id)
        };

        var page = await ordered.Skip(request.Skip()).Take(request.PageSize).ToListAsync(cancellationToken);
        var media = await productMediaReader.GetPrimaryPublicMediaAsync(page.Select(x => x.Id).ToArray(), cancellationToken);
        var items = page.Select(x => new ProductListItemDto(x.Id, x.Slug, x.Name, x.ShortDescription,
            new ProducerSummaryDto(x.ProducerId, x.ProducerCode, x.ProducerName, x.ProducerDescription, x.ProducerWebsiteUrl),
            new CategorySummaryDto(x.CategoryId, x.CategoryName, x.CategorySlug, true, x.CategoryDisplayOrder),
            media.GetValueOrDefault(x.Id), x.FromPrice, CommerceConstants.DefaultCurrency,
            x.PublishedAt ?? DateTime.MinValue)).ToList();

        return TResult<PaginatedList<ProductListItemDto>>.Success(
            PaginatedList<ProductListItemDto>.Create(items, totalCount, request.Page, request.PageSize));
    }

    private sealed record ProductRow(Guid Id, string Slug, string Name, string? ShortDescription, DateTime? PublishedAt,
        Guid ProducerId, string ProducerCode, string ProducerName, string? ProducerDescription, string? ProducerWebsiteUrl,
        Guid CategoryId, string CategoryName, string CategorySlug, int CategoryDisplayOrder, decimal FromPrice);
}
