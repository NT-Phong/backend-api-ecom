using Ecom.Application.Features.Media.Commands.CreatePendingMedia;

namespace Ecom.Application.Common.Services;

/// <summary>Writes the physical quarantine object before the transactional metadata command.</summary>
public sealed class MediaUploadOrchestrator(IMediaFileService mediaFiles, ISender sender)
{
    public async Task<TResult<MediaAssetResult>> UploadAsync(Stream stream, string fileName, string contentType,
        long sizeBytes, MediaUploadIntent intent, string? altText, CancellationToken cancellationToken)
    {
        StoredMediaUpload? stored = null;
        try
        {
            stored = await mediaFiles.StorePendingAsync(stream, fileName, contentType, sizeBytes, intent, cancellationToken);
            var result = await sender.Send(new CreatePendingMediaCommand(stored.StorageKey, stored.Metadata, intent, altText), cancellationToken);
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
