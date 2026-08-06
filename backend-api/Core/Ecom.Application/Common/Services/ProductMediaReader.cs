using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class ProductMediaReader(
    IUnitOfWork unitOfWork,
    IStorageService storage,
    ILogger<ProductMediaReader> logger) : IProductMediaReader
{
    public async Task<IReadOnlyDictionary<Guid, ProductMediaDto>> GetPrimaryPublicMediaAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0) return new Dictionary<Guid, ProductMediaDto>();
        var ids = productIds.Distinct().ToArray();
        var rows = await (
            from link in unitOfWork.Repository<ProductMedia>().QueryNoTracking()
            join product in unitOfWork.Repository<Product>().QueryNoTracking() on link.ProductId equals product.Id
            join media in unitOfWork.Repository<MediaAsset>().QueryNoTracking() on link.MediaAssetId equals media.Id
            where ids.Contains(product.Id)
                  && product.Status == ProductStatus.Published
                  && link.IsPrimary
                  && media.Visibility == MediaVisibility.Public
                  && media.ScanStatus == MediaScanStatus.Clean
            orderby link.DisplayOrder, link.Id
            select new PrimaryPublicMediaRow(
                link.ProductId,
                new PublicMediaRow(media.Id, media.StorageKey, media.ContentType, media.AltText,
                    link.Caption, link.DisplayOrder, link.IsPrimary)))
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.ProductId)
            .Select(group => new { ProductId = group.Key, Media = CreatePublicMedia(group.First().Media, group.Key) })
            .Where(x => x.Media is not null)
            .ToDictionary(x => x.ProductId, x => x.Media!);
    }

    public async Task<IReadOnlyList<ProductMediaDto>> GetPublicMediaAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty) return [];
        var query =
            from link in unitOfWork.Repository<ProductMedia>().QueryNoTracking()
            join product in unitOfWork.Repository<Product>().QueryNoTracking() on link.ProductId equals product.Id
            join media in unitOfWork.Repository<MediaAsset>().QueryNoTracking() on link.MediaAssetId equals media.Id
            where product.Id == productId
                  && product.Status == ProductStatus.Published
                  && media.Visibility == MediaVisibility.Public
                  && media.ScanStatus == MediaScanStatus.Clean
            orderby link.IsPrimary descending, link.DisplayOrder, link.Id
            select new PublicMediaRow(media.Id, media.StorageKey, media.ContentType, media.AltText,
                link.Caption, link.DisplayOrder, link.IsPrimary);

        var rows = await query.ToListAsync(cancellationToken);
        return rows
            .Select(x => CreatePublicMedia(x, productId))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    private ProductMediaDto? CreatePublicMedia(PublicMediaRow item, Guid productId)
    {
        try
        {
            return new ProductMediaDto(item.MediaAssetId, storage.GetPublicFileUrl(item.StorageKey), item.ContentType,
                item.AltText, item.Caption, item.DisplayOrder, item.IsPrimary);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception,
                "Ignoring invalid public-media storage key for ProductId {ProductId} and MediaAssetId {MediaAssetId}.",
                productId,
                item.MediaAssetId);
            return null;
        }
    }

    private sealed record PrimaryPublicMediaRow(Guid ProductId, PublicMediaRow Media);
    private sealed record PublicMediaRow(Guid MediaAssetId, string StorageKey, string ContentType, string? AltText,
        string? Caption, int DisplayOrder, bool IsPrimary);
}
