using Ecom.Domain.Enums;

namespace Ecom.Application.Common.Interfaces;

/// <summary>Opaque storage boundary. Only public keys can be converted to anonymous URLs.</summary>
public interface IStorageService
{
    string GetPublicFileUrl(string storageKey);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<string> UploadToQuarantineAsync(Stream fileStream, string safeExtension, CancellationToken cancellationToken = default);
    Task<string> PromoteAsync(string quarantineKey, MediaVisibility targetVisibility, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
