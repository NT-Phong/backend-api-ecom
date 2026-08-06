using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Enums;
using Microsoft.AspNetCore.Hosting;

namespace Ecom.Infrastructure.Services;

public sealed class LocalStorageService(IWebHostEnvironment environment) : IStorageService
{
    private const string UploadRoot = "uploads";
    private const string QuarantineArea = "quarantine";
    private const string PublicArea = "public";
    private const string PrivateArea = "private";

    public Task EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public string GetPublicFileUrl(string storageKey)
    {
        var key = NormalizeKey(storageKey);
        var publicPrefix = Path.Combine(UploadRoot, PublicArea) + Path.DirectorySeparatorChar;
        if (!key.StartsWith(publicPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only public media can be exposed as an anonymous URL.");
        var relative = key[publicPrefix.Length..].Replace('\\', '/');
        return "/media/" + relative;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = File.Open(GetPhysicalPath(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public async Task<string> UploadToQuarantineAsync(Stream fileStream, string safeExtension, string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        if (string.IsNullOrWhiteSpace(safeExtension) || !safeExtension.StartsWith('.') ||
            safeExtension.Length < 2 || safeExtension[1..].Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException("A safe file extension is required.", nameof(safeExtension));

        var key = Path.Combine(UploadRoot, QuarantineArea, DateTime.UtcNow.ToString("yyyy/MM/dd"),
            $"{Guid.NewGuid():N}{safeExtension.ToLowerInvariant()}");
        var path = GetPhysicalPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous);
        await fileStream.CopyToAsync(destination, cancellationToken);
        return key;
    }

    public Task<string> PromoteAsync(string quarantineKey, MediaVisibility targetVisibility,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sourceKey = NormalizeKey(quarantineKey);
        var quarantinePrefix = Path.Combine(UploadRoot, QuarantineArea) + Path.DirectorySeparatorChar;
        if (!sourceKey.StartsWith(quarantinePrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only quarantined media can be promoted.");

        var targetArea = targetVisibility == MediaVisibility.Public ? PublicArea : PrivateArea;
        var relative = sourceKey[quarantinePrefix.Length..];
        var targetKey = Path.Combine(UploadRoot, targetArea, relative);
        var sourcePath = GetPhysicalPath(sourceKey);
        var targetPath = GetPhysicalPath(targetKey);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(sourcePath, targetPath, false);
        return Task.FromResult(targetKey);
    }

    public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = GetPhysicalPath(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string GetPhysicalPath(string storageKey)
    {
        var webRoot = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        var root = Path.GetFullPath(Path.Combine(webRoot, UploadRoot));
        var path = Path.GetFullPath(Path.Combine(webRoot, NormalizeKey(storageKey)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage key is outside the uploads root.");
        return path;
    }

    private static string NormalizeKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        var key = storageKey.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        if (!key.StartsWith(UploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage key must be rooted under uploads.");
        return key;
    }
}
