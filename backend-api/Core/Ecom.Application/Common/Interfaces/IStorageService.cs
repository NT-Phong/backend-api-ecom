using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Interfaces;

/// <summary>Opaque storage boundary. Only public keys can be converted to anonymous URLs.</summary>
public interface IStorageService
{
    /// <summary>Verifies that the selected provider and its required areas are reachable.</summary>
    Task EnsureReadyAsync(CancellationToken cancellationToken = default);
    string GetPublicFileUrl(string storageKey);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<string> UploadToQuarantineAsync(Stream fileStream, string safeExtension, string contentType,
        CancellationToken cancellationToken = default);
    Task<string> UploadToPublicAsync(Stream fileStream, string safeExtension, string contentType,
        CancellationToken cancellationToken = default);
    Task<string> PromoteAsync(string quarantineKey, MediaVisibility targetVisibility, CancellationToken cancellationToken = default);
    Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default);
}
