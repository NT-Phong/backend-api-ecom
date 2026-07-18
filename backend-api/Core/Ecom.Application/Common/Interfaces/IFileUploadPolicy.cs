using Ecom.Application.Common.Models;

namespace Ecom.Application.Common.Interfaces;

public interface IFileUploadPolicy
{
    Task<ValidatedMediaUpload> ValidateAsync(Stream stream, string fileName, string claimedContentType,
        long sizeBytes, MediaUploadIntent intent, CancellationToken cancellationToken = default);
}
