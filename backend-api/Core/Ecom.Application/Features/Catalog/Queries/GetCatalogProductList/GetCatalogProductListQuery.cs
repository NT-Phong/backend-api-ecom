using Ecom.Application.Features.Catalog.Common;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetCatalogProductList;

public sealed class GetCatalogProductListQuery : IRequest<TResult<PaginatedList<CatalogProductListItemDto>>>
{
    public string? Q { get; init; }
    public ProductStatus? Status { get; init; }
    public Guid? ProducerId { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Sku { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public DateTime? UpdatedFrom { get; init; }
    public DateTime? UpdatedTo { get; init; }
    public bool? HasActiveVariant { get; init; }
    public bool? HasEffectivePrice { get; init; }
    public bool? HasPrimaryMedia { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Skip() => (Page - 1) * PageSize;
}

public sealed class GetCatalogProductListQueryValidator : AbstractValidator<GetCatalogProductListQuery>
{
    public GetCatalogProductListQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(300);
        RuleFor(x => x.Sku).MaximumLength(100);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot exceed maximum price.");
        RuleFor(x => x).Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom <= x.CreatedTo)
            .WithMessage("Created date window is invalid.");
        RuleFor(x => x).Must(x => !x.UpdatedFrom.HasValue || !x.UpdatedTo.HasValue || x.UpdatedFrom <= x.UpdatedTo)
            .WithMessage("Updated date window is invalid.");
    }
}

public sealed class GetCatalogProductListQueryHandler(IUnitOfWork unitOfWork, ICatalogProductAccessService access,
    IEffectivePriceResolver effectivePriceResolver, IProductMediaReader productMediaReader)
    : IRequestHandler<GetCatalogProductListQuery, TResult<PaginatedList<CatalogProductListItemDto>>>
{
    public async Task<TResult<PaginatedList<CatalogProductListItemDto>>> Handle(GetCatalogProductListQuery request,
        CancellationToken cancellationToken)
    {
        var authorization = access.Ensure(Permissions.CatalogProducts.Read);
        if (!authorization.IsSuccess)
            return CatalogCommandSupport.Failure<PaginatedList<CatalogProductListItemDto>>(authorization);
        var inventoryAuthorization = access.Ensure(Permissions.Inventory.Read);
        if (!inventoryAuthorization.IsSuccess)
            return CatalogCommandSupport.Failure<PaginatedList<CatalogProductListItemDto>>(inventoryAuthorization);

        var query = unitOfWork.Repository<Product>().QueryNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var search = request.Q.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Slug.ToLower().Contains(search));
        }
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.ProducerId.HasValue) query = query.Where(x => x.ProducerId == request.ProducerId.Value);
        if (request.CategoryId.HasValue)
            query = query.Where(x => unitOfWork.Repository<ProductCategory>().QueryNoTracking()
                .Any(mapping => mapping.ProductId == x.Id && mapping.CategoryId == request.CategoryId.Value));
        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var sku = request.Sku.Trim().ToLower();
            query = query.Where(x => unitOfWork.Repository<ProductVariant>().QueryNoTracking()
                .Any(variant => variant.ProductId == x.Id && variant.Sku.ToLower().Contains(sku)));
        }
        if (request.CreatedFrom.HasValue) query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
        if (request.CreatedTo.HasValue) query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);
        if (request.UpdatedFrom.HasValue) query = query.Where(x => x.UpdatedAt >= request.UpdatedFrom.Value);
        if (request.UpdatedTo.HasValue) query = query.Where(x => x.UpdatedAt <= request.UpdatedTo.Value);

        var asOfUtc = DateTime.UtcNow;
        var productsWithEffectivePrice = effectivePriceResolver.QueryProductIdsWithEffectivePrice(asOfUtc);
        if (request.MinPrice.HasValue)
            query = query.Where(x => effectivePriceResolver.QueryProductIdsWithEffectivePrice(asOfUtc,
                request.MinPrice.Value, null).Contains(x.Id));
        if (request.MaxPrice.HasValue)
            query = query.Where(x => effectivePriceResolver.QueryProductIdsWithEffectivePrice(asOfUtc,
                null, request.MaxPrice.Value).Contains(x.Id));
        if (request.HasEffectivePrice.HasValue) query = request.HasEffectivePrice.Value
            ? query.Where(x => productsWithEffectivePrice.Contains(x.Id))
            : query.Where(x => !productsWithEffectivePrice.Contains(x.Id));
        if (request.HasActiveVariant.HasValue) query = request.HasActiveVariant.Value
            ? query.Where(x => unitOfWork.Repository<ProductVariant>().QueryNoTracking().Any(variant => variant.ProductId == x.Id && variant.Status == VariantStatus.Active))
            : query.Where(x => !unitOfWork.Repository<ProductVariant>().QueryNoTracking().Any(variant => variant.ProductId == x.Id && variant.Status == VariantStatus.Active));
        if (request.HasPrimaryMedia.HasValue) query = request.HasPrimaryMedia.Value
            ? query.Where(x => unitOfWork.Repository<ProductMedia>().QueryNoTracking().Any(media => media.ProductId == x.Id && media.IsPrimary))
            : query.Where(x => !unitOfWork.Repository<ProductMedia>().QueryNoTracking().Any(media => media.ProductId == x.Id && media.IsPrimary));

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return TResult<PaginatedList<CatalogProductListItemDto>>.Success(
                PaginatedList<CatalogProductListItemDto>.Create([], 0, request.Page, request.PageSize));

        var page = await query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ThenBy(x => x.Id)
            .Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new ProductRow(x.Id, x.ProducerId, x.Name, x.Slug, x.Status, x.CreatedAt, x.UpdatedAt, x.BrandName))
            .ToListAsync(cancellationToken);

        var productIds = page.Select(x => x.Id).ToArray();
        var primaryCategories = await (
            from mapping in unitOfWork.Repository<ProductCategory>().QueryNoTracking()
            join category in unitOfWork.Repository<Category>().QueryNoTracking() on mapping.CategoryId equals category.Id
            where productIds.Contains(mapping.ProductId) && mapping.IsPrimary
            select new { mapping.ProductId, Category = new CategorySummaryDto(category.Id, category.Name, category.Slug, true, category.DisplayOrder) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Category, cancellationToken);

        var prices = await effectivePriceResolver
            .QueryEffectiveProductPrices(asOfUtc, productIds)
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var trackedProductIds = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => productIds.Contains(x.ProductId) && x.InventoryMode == InventoryMode.Tracked)
            .Select(x => x.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var inventoryTotals = await (
            from level in unitOfWork.Repository<InventoryLevel>().QueryNoTracking()
            join item in unitOfWork.Repository<InventoryItem>().QueryNoTracking() on level.InventoryItemId equals item.Id
            join variant in unitOfWork.Repository<ProductVariant>().QueryNoTracking() on item.ProductVariantId equals variant.Id
            where productIds.Contains(variant.ProductId)
            group level by variant.ProductId into levels
            select new InventoryTotalsRow(levels.Key, levels.Sum(x => x.StockedQuantity), levels.Sum(x => x.ReservedQuantity),
                levels.Sum(x => x.IncomingQuantity)))
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var primaryMedia = await productMediaReader.GetPrimaryCleanPublicMediaAsync(productIds, cancellationToken);

        var trackedProductIdSet = trackedProductIds.ToHashSet();
        var items = page.Select(x => new CatalogProductListItemDto(x.Id, x.ProducerId, x.Name, x.Slug,
            x.Status, x.CreatedAt, x.UpdatedAt, primaryCategories.GetValueOrDefault(x.Id),
            prices.TryGetValue(x.Id, out var price)
                ? new CatalogProductPriceDto(price.Amount, price.CurrencyCode, true)
                : new CatalogProductPriceDto(null, null, false),
            inventoryTotals.TryGetValue(x.Id, out var inventory)
                ? new CatalogProductInventoryDto(inventory.StockedQuantity, inventory.ReservedQuantity,
                    inventory.StockedQuantity - inventory.ReservedQuantity, inventory.IncomingQuantity,
                    trackedProductIdSet.Contains(x.Id))
                : new CatalogProductInventoryDto(0m, 0m, 0m, 0m, trackedProductIdSet.Contains(x.Id)),
            primaryMedia.GetValueOrDefault(x.Id), x.BrandName)).ToList();
        return TResult<PaginatedList<CatalogProductListItemDto>>.Success(
            PaginatedList<CatalogProductListItemDto>.Create(items, totalCount, request.Page, request.PageSize));
    }

    private sealed record ProductRow(Guid Id, Guid ProducerId, string Name, string Slug, ProductStatus Status,
        DateTime CreatedAt, DateTime? UpdatedAt, string? BrandName);
    private sealed record InventoryTotalsRow(Guid ProductId, decimal StockedQuantity, decimal ReservedQuantity,
        decimal IncomingQuantity);
}
