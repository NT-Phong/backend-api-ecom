using Ecom.Application.Features.Catalog.Common;
using Ecom.Application.Features.Catalog.Queries.GetPublicCategories;
using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Catalog.Queries.GetProductBySlug;

public sealed class GetProductBySlugQueryHandler(
    IUnitOfWork unitOfWork,
    IEffectivePriceResolver effectivePriceResolver,
    IProductMediaReader productMediaReader,
    IProductAvailabilityReadService availabilityReadService)
    : IRequestHandler<GetProductBySlugQuery, TResult<PublicProductLookupDto>>
{
    public async Task<TResult<PublicProductLookupDto>> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim();
        var products = unitOfWork.Repository<Product>().QueryNoTracking();
        var producers = unitOfWork.Repository<Producer>().QueryNoTracking();
        var mappings = unitOfWork.Repository<ProductCategory>().QueryNoTracking();

        var product = await (
            from item in products
            join producer in producers on item.ProducerId equals producer.Id
            where item.Slug == slug
                  && item.Status == ProductStatus.Published
            select new ProductRow(item.Id, item.Slug, item.Name, item.ShortDescription, item.Description,
                item.UsageInstructions, item.StorageInstructions, item.WarningText, item.MetaTitle, item.MetaDescription,
                item.PublishedAt, item.Standard, producer.Id, producer.Code, producer.Name, producer.Description, producer.WebsiteUrl))
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            var history = await unitOfWork.Repository<ProductSlugHistory>().QueryNoTracking()
                .SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken);
            if (history is null)
                return TResult<PublicProductLookupDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);

            var canonicalSlug = await unitOfWork.Repository<Product>().QueryNoTracking()
                .Where(x => x.Id == history.ProductId && x.Status == ProductStatus.Published)
                .Select(x => x.Slug)
                .SingleOrDefaultAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(canonicalSlug)
                ? TResult<PublicProductLookupDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND)
                : TResult<PublicProductLookupDto>.Success(new PublicProductLookupDto(null, canonicalSlug));
        }

        var publicCategories = (await PublicCategoryVisibility.LoadAsync(unitOfWork, cancellationToken))
            .Where(category => category.Status == CatalogStatus.Published)
            .ToDictionary(category => category.Id);
        var productCategories = await mappings
            .Where(mapping => mapping.ProductId == product.Id)
            .Select(mapping => new { mapping.CategoryId, mapping.IsPrimary })
            .ToListAsync(cancellationToken);
        var responseCategories = productCategories
            .Where(mapping => publicCategories.TryGetValue(mapping.CategoryId, out var category)
                              && PublicCategoryVisibility.HasPublishedAncestors(category, publicCategories))
            .Select(mapping =>
            {
                var category = publicCategories[mapping.CategoryId];
                return new CategorySummaryDto(category.Id, category.Name, category.Slug, mapping.IsPrimary,
                    category.DisplayOrder);
            })
            .OrderByDescending(category => category.IsPrimary)
            .ThenBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToList();

        var variants = await unitOfWork.Repository<ProductVariant>().QueryNoTracking()
            .Where(x => x.ProductId == product.Id && x.Status == VariantStatus.Active)
            .Select(x => new VariantRow(x.Id, x.Sku, x.Name, x.WeightGrams, x.UnitLabel))
            .ToListAsync(cancellationToken);
        var asOfUtc = DateTime.UtcNow;
        var prices = await effectivePriceResolver.ResolveForVariantsAsync(variants.Select(x => x.Id).ToArray(), asOfUtc, cancellationToken);
        var availability = await availabilityReadService.ResolveForVariantsAsync(variants.Select(x => x.Id).ToArray(), asOfUtc,
            cancellationToken);

        var optionValues = await (
            from map in unitOfWork.Repository<ProductVariantOptionValue>().QueryNoTracking()
            join value in unitOfWork.Repository<ProductOptionValue>().QueryNoTracking() on map.ProductOptionValueId equals value.Id
            join option in unitOfWork.Repository<ProductOption>().QueryNoTracking() on value.ProductOptionId equals option.Id
            where variants.Select(x => x.Id).Contains(map.ProductVariantId) && option.ProductId == product.Id
            orderby option.DisplayOrder, value.DisplayOrder
            select new { map.ProductVariantId, Option = new VariantOptionValueDto(option.Id, option.Code, option.Name, value.Id, value.Value) })
            .ToListAsync(cancellationToken);

        var media = await productMediaReader.GetPublicMediaAsync(product.Id, cancellationToken);
        var responseVariants = variants
            .Where(x => prices.ContainsKey(x.Id))
            .Select(x =>
            {
                var price = prices[x.Id];
                return new ProductVariantDto(x.Id, x.Sku, x.Name, price.Amount, price.CurrencyCode, price.PriceType,
                    x.WeightGrams, availability.GetValueOrDefault(x.Id, CatalogAvailabilityStatus.Unavailable),
                    optionValues.Where(v => v.ProductVariantId == x.Id).Select(v => v.Option).ToList(), x.UnitLabel);
            })
            .ToList();

        return TResult<PublicProductLookupDto>.Success(new PublicProductLookupDto(new ProductDetailDto(product.Id, product.Slug, product.Name,
            product.ShortDescription, product.Description, product.UsageInstructions, product.StorageInstructions,
            product.WarningText, product.MetaTitle, product.MetaDescription,
            new ProducerSummaryDto(product.ProducerId, product.ProducerCode, product.ProducerName,
                product.ProducerDescription, product.ProducerWebsiteUrl),
            responseCategories, media, responseVariants, responseVariants.Count > 0,
            availabilityReadService.SummarizeProduct(availability.Values),
            product.PublishedAt ?? DateTime.MinValue, product.Standard,
            responseVariants.OrderBy(x => x.Price).ThenBy(x => x.Id).Select(x => x.UnitLabel).FirstOrDefault()), null));
    }

    private sealed record ProductRow(Guid Id, string Slug, string Name, string? ShortDescription, string? Description,
        string? UsageInstructions, string? StorageInstructions, string? WarningText, string? MetaTitle,
        string? MetaDescription, DateTime? PublishedAt, string? Standard, Guid ProducerId, string ProducerCode, string ProducerName,
        string? ProducerDescription, string? ProducerWebsiteUrl);
    private sealed record VariantRow(Guid Id, string Sku, string Name, decimal? WeightGrams, string? UnitLabel);
}
