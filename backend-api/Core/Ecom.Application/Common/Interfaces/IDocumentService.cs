namespace Ecom.Application.Common.Interfaces;

public interface IDocumentService
{
    Task<Stream?> CreateWebPThumbnailAsync(Stream originalStream, int thumbnailWidth = 320,
        int thumbnailQuality = 75, CancellationToken cancellationToken = default);
}
