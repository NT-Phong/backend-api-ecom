using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using Ecom.Application.Common.Configuration;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Ecom.Infrastructure.Services;

/// <summary>Azure production provider. ConnectionString is supplied by a secret store, never source control.</summary>
public sealed class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _client;
    private readonly MediaStorageOptions _options;

    public AzureBlobStorageService(IOptions<MediaStorageOptions> options)
    {
        _options = options.Value;
        if (!string.IsNullOrWhiteSpace(_options.AccountUrl))
            _client = new BlobServiceClient(new Uri(_options.AccountUrl), new DefaultAzureCredential());
        else if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            _client = new BlobServiceClient(_options.ConnectionString);
        else
            throw new InvalidOperationException("MediaStorage requires AccountUrl (Managed Identity) or a test-only connection string.");
    }

    public async Task EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        await _client.GetBlobContainerClient(_options.QuarantineContainer).GetPropertiesAsync(cancellationToken: cancellationToken);
        await _client.GetBlobContainerClient(_options.PublicContainer).GetPropertiesAsync(cancellationToken: cancellationToken);
        await _client.GetBlobContainerClient(_options.PrivateContainer).GetPropertiesAsync(cancellationToken: cancellationToken);
    }

    public string GetPublicFileUrl(string storageKey)
    {
        var (container, blobName) = Resolve(storageKey);
        if (!string.Equals(container, _options.PublicContainer, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only promoted public media can be exposed anonymously.");
        return string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? _client.GetBlobContainerClient(container).GetBlobClient(blobName).Uri.ToString()
            : $"{_options.PublicBaseUrl.TrimEnd('/')}/{blobName}";
    }

    public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var (container, blobName) = Resolve(storageKey);
        var response = await _client.GetBlobContainerClient(container).GetBlobClient(blobName)
            .DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task<string> UploadToQuarantineAsync(Stream fileStream, string safeExtension, string contentType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(safeExtension) || !safeExtension.StartsWith('.') || safeExtension[1..].Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException("A safe file extension is required.", nameof(safeExtension));
        var blobName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{safeExtension.ToLowerInvariant()}";
        var container = _client.GetBlobContainerClient(_options.QuarantineContainer);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        await container.GetBlobClient(blobName).UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);
        return $"quarantine/{blobName}";
    }

    public async Task<string> PromoteAsync(string quarantineKey, MediaVisibility targetVisibility, CancellationToken cancellationToken = default)
    {
        var (sourceContainer, blobName) = Resolve(quarantineKey);
        if (!string.Equals(sourceContainer, _options.QuarantineContainer, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only quarantined media can be promoted.");
        var targetContainerName = targetVisibility == MediaVisibility.Public ? _options.PublicContainer : _options.PrivateContainer;
        var target = _client.GetBlobContainerClient(targetContainerName);
        await target.CreateIfNotExistsAsync(targetVisibility == MediaVisibility.Public ? PublicAccessType.Blob : PublicAccessType.None, cancellationToken: cancellationToken);
        var destination = target.GetBlobClient(blobName);
        var operation = await destination.StartCopyFromUriAsync(
            _client.GetBlobContainerClient(sourceContainer).GetBlobClient(blobName).Uri,
            cancellationToken: cancellationToken);
        await operation.WaitForCompletionAsync(cancellationToken);
        var properties = await destination.GetPropertiesAsync(cancellationToken: cancellationToken);
        if (properties.Value.CopyStatus != CopyStatus.Success)
            throw new InvalidOperationException($"Media promotion failed with copy status {properties.Value.CopyStatus}.");
        return $"{(targetVisibility == MediaVisibility.Public ? "public" : "private")}/{blobName}";
    }

    public async Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var (container, blobName) = Resolve(storageKey);
        await _client.GetBlobContainerClient(container).DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken);
    }

    private (string Container, string BlobName) Resolve(string storageKey)
    {
        var key = storageKey?.Trim().Replace('\\', '/') ?? throw new ArgumentNullException(nameof(storageKey));
        var slash = key.IndexOf('/');
        if (slash <= 0 || slash == key.Length - 1 || key.Contains("..", StringComparison.Ordinal)) throw new InvalidOperationException("Invalid media storage key.");
        return key[..slash] switch
        {
            "quarantine" => (_options.QuarantineContainer, key[(slash + 1)..]),
            "public" => (_options.PublicContainer, key[(slash + 1)..]),
            "private" => (_options.PrivateContainer, key[(slash + 1)..]),
            _ => throw new InvalidOperationException("Unknown media storage area.")
        };
    }
}
