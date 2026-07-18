using Ecom.Application.Common.Models;
using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Interfaces;

public interface IMediaFileService
{
    Task<StoredMediaUpload> StorePendingAsync(Stream stream, string fileName, string claimedContentType,
        long sizeBytes, MediaUploadIntent intent, CancellationToken cancellationToken = default);
    Task<string> PromoteAsync(string quarantineKey, MediaVisibility visibility, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
