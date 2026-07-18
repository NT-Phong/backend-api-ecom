using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Services;

public sealed class ProductMediaReader(IUnitOfWork unitOfWork, IStorageService storage) : IProductMediaReader
{
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
            select new
            {
                media.Id,
                media.StorageKey,
                media.ContentType,
                media.AltText,
                link.Caption,
                link.DisplayOrder,
                link.IsPrimary
            };

        var rows = await query.ToListAsync(cancellationToken);
        return rows.Select(x => new ProductMediaDto(x.Id, storage.GetPublicFileUrl(x.StorageKey),
            x.ContentType, x.AltText, x.Caption, x.DisplayOrder, x.IsPrimary)).ToList();
    }
}
