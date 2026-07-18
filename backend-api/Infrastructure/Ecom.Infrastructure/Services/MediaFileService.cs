using Ecom.Application.Common.Interfaces;
using Ecom.Application.Common.Models;
using Ecom.Domain.Enums;

namespace Ecom.Infrastructure.Services;

public sealed class MediaFileService(IFileUploadPolicy policy, IStorageService storage) : IMediaFileService
{
    public async Task<StoredMediaUpload> StorePendingAsync(Stream stream, string fileName, string claimedContentType,
        long sizeBytes, MediaUploadIntent intent, CancellationToken cancellationToken = default)
    {
        var metadata = await policy.ValidateAsync(stream, fileName, claimedContentType, sizeBytes, intent, cancellationToken);
        stream.Position = 0;
        var key = await storage.UploadToQuarantineAsync(stream, metadata.SafeExtension, cancellationToken);
        return new StoredMediaUpload(key, metadata);
    }

    public Task<string> PromoteAsync(string quarantineKey, MediaVisibility visibility,
        CancellationToken cancellationToken = default) => storage.PromoteAsync(quarantineKey, visibility, cancellationToken);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) =>
        storage.DeleteAsync(storageKey, cancellationToken);
}
