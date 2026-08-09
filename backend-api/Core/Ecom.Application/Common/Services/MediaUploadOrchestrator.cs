using Ecom.Application.Common.Configuration;
using Ecom.Application.Features.Media.Commands.CreatePendingMedia;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Common.Services;

/// <summary>Writes media before the transactional metadata command.</summary>
public sealed class MediaUploadOrchestrator(IMediaFileService mediaFiles, ISender sender,
    IOptions<MediaProcessingOptions> mediaProcessing)
{
    public async Task<TResult<MediaAssetResult>> UploadAsync(Stream stream, string fileName, string contentType,
        long sizeBytes, MediaUploadIntent intent, string? altText, CancellationToken cancellationToken)
    {
        StoredMediaUpload? stored = null;
        var directPublicUpload = mediaProcessing.Value.DirectPublicUploadEnabled;
        try
        {
            stored = directPublicUpload
                ? await mediaFiles.StorePublicAsync(stream, fileName, contentType, sizeBytes, intent, cancellationToken)
                : await mediaFiles.StorePendingAsync(stream, fileName, contentType, sizeBytes, intent, cancellationToken);
            var result = await sender.Send(new CreatePendingMediaCommand(stored.StorageKey, stored.Metadata, intent, altText,
                directPublicUpload), cancellationToken);
            if (!result.IsSuccess)
                await mediaFiles.DeleteIfExistsAsync(stored.StorageKey, cancellationToken);
            return result;
        }
        catch
        {
            if (stored is not null)
                await mediaFiles.DeleteIfExistsAsync(stored.StorageKey, cancellationToken);
            throw;
        }
    }
}
